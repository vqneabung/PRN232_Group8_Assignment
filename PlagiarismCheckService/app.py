from fastapi import FastAPI, UploadFile, File, Form, HTTPException
from fastapi.responses import JSONResponse
from typing import List, Optional
import hashlib
import zipfile
import os
import tempfile
import shutil
from sentence_transformers import SentenceTransformer
import uvicorn
from pydantic import BaseModel
import pickle
import numpy as np
from sklearn.metrics.pairwise import cosine_similarity

app = FastAPI(title="Plagiarism Check Service")

model = SentenceTransformer('all-MiniLM-L6-v2')

STORAGE_FILE = "./vectordb_storage/submissions.pkl"
os.makedirs("./vectordb_storage", exist_ok=True)

def load_storage():
    if os.path.exists(STORAGE_FILE):
        with open(STORAGE_FILE, 'rb') as f:
            return pickle.load(f)
    return []

def save_storage(data):
    with open(STORAGE_FILE, 'wb') as f:
        pickle.dump(data, f)

storage = load_storage()
print(f"[STARTUP] Loaded {len(storage)} submissions from storage")

class PlagiarismResult(BaseModel):
    isPlagiarized: bool
    similarityScore: float
    matchedSubmissionId: Optional[str]
    matchedFiles: List[dict]
    totalFilesChecked: int
    
    class Config:
        populate_by_name = True
        alias_generator = None

class StoreRequest(BaseModel):
    submission_id: str
    files_content: dict

def extract_code_content(file_path: str) -> str:
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            return f.read()
    except:
        try:
            with open(file_path, 'r', encoding='latin-1') as f:
                return f.read()
        except:
            return ""

def calculate_file_hash(content: str) -> str:
    return hashlib.md5(content.encode()).hexdigest()

@app.post("/check-plagiarism")
async def check_plagiarism(
    file: UploadFile = File(...),
    submission_id: str = Form(...),
    threshold: float = Form(0.85)
):
    global storage
    current_storage = load_storage()
    
    temp_folder = os.path.join(tempfile.gettempdir(), f"Submission_{submission_id}_{os.urandom(4).hex()}")
    os.makedirs(temp_folder, exist_ok=True)
    
    try:
        zip_path = os.path.join(temp_folder, file.filename or "submission.zip")
        with open(zip_path, 'wb') as buffer:
            shutil.copyfileobj(file.file, buffer)
        
        print(f"[DEBUG] Uploaded file size: {os.path.getsize(zip_path)} bytes")
        
        extract_path = os.path.join(temp_folder, "Extracted")
        os.makedirs(extract_path, exist_ok=True)
        
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            zip_ref.extractall(extract_path)
        
        all_files = []
        for root, dirs, files in os.walk(extract_path):
            for f in files:
                all_files.append(os.path.join(root, f))
        
        print(f"[DEBUG] Total files extracted: {len(all_files)}")
        
        code_files = []
        for file_path in all_files:
            filename = os.path.basename(file_path)
            
            if 'postman' in filename.lower() or 'postman' in file_path.lower():
                continue
            
            if filename.endswith(('.cs', '.py', '.java', '.cpp', '.c', '.js', '.ts', '.txt', '.csproj', '.sln', '.json')):
                if filename.endswith(('.json')) and ('nuget' in filename.lower() or 'project.assets' in filename.lower() or 'dgspec' in filename.lower()):
                    continue
                    
                content = extract_code_content(file_path)
                if content and len(content.strip()) > 50:
                    relative_path = file_path.replace(extract_path, "").lstrip(os.sep).lstrip('/')
                    code_files.append({
                        'filename': filename,
                        'content': content,
                        'path': relative_path
                    })
        
        print(f"[DEBUG] Code files found: {len(code_files)}")
        
        if not code_files:
            return JSONResponse({
                "isPlagiarized": False,
                "similarityScore": 0.0,
                "matchedSubmissionId": None,
                "matchedFiles": [],
                "totalFilesChecked": 0,
                "message": "No valid code files found"
            })
        
        current_storage = load_storage()
        print(f"[CHECK] Loaded {len(current_storage)} submissions from storage")
        
        plagiarism_detected = False
        max_similarity = 0.0
        matched_submission = None
        matched_details = []
        
        for code_file in code_files:
            content = code_file['content']
            
            embedding = model.encode(content).reshape(1, -1)
            
            for stored_item in current_storage:
                if stored_item['submission_id'] == submission_id:
                    continue
                
                stored_embedding = np.array(stored_item['embedding']).reshape(1, -1)
                similarity = cosine_similarity(embedding, stored_embedding)[0][0]
                
                if similarity >= threshold:
                    plagiarism_detected = True
                    if similarity > max_similarity:
                        max_similarity = similarity
                        matched_submission = stored_item['submission_id']
                    
                    matched_details.append({
                        'currentFile': code_file['filename'],
                        'matchedFile': stored_item.get('filename', 'unknown'),
                        'similarity': round(float(similarity), 4),
                        'matchedSubmissionId': stored_item['submission_id']
                    })
        
        if max_similarity < threshold:
            storage = load_storage()
            stored_count = 0
            
            for code_file in code_files:
                content = code_file['content']
                file_hash = calculate_file_hash(content)
                embedding = model.encode(content).tolist()
                
                storage.append({
                    'submission_id': submission_id,
                    'filename': code_file['filename'],
                    'file_hash': file_hash,
                    'path': code_file['path'],
                    'embedding': embedding
                })
                stored_count += 1
            
            save_storage(storage)
            print(f"[AUTO-STORE] Stored {stored_count} files for submission {submission_id}")
        
        return JSONResponse({
            "isPlagiarized": plagiarism_detected,
            "similarityScore": round(float(max_similarity), 4),
            "matchedSubmissionId": matched_submission,
            "matchedFiles": matched_details,
            "totalFilesChecked": len(code_files)
        })
    
    finally:
        try:
            shutil.rmtree(temp_folder)
        except:
            pass

@app.post("/store-submission")
async def store_submission(
    file: UploadFile = File(...),
    submission_id: str = Form(...)
):
    global storage
    temp_folder = os.path.join(tempfile.gettempdir(), f"Store_{submission_id}")
    os.makedirs(temp_folder, exist_ok=True)
    
    try:
        zip_path = os.path.join(temp_folder, file.filename or "submission.zip")
        with open(zip_path, 'wb') as buffer:
            shutil.copyfileobj(file.file, buffer)
        
        extract_path = os.path.join(temp_folder, "Extracted")
        os.makedirs(extract_path, exist_ok=True)
        
        with zipfile.ZipFile(zip_path, 'r') as zip_ref:
            zip_ref.extractall(extract_path)
        
        all_files = []
        for root, dirs, files in os.walk(extract_path):
            for f in files:
                all_files.append(os.path.join(root, f))
        
        stored_count = 0
        
        for file_path in all_files:
            filename = os.path.basename(file_path)
            if filename.endswith(('.cs', '.py', '.java', '.cpp', '.c', '.js', '.ts', '.txt')):
                content = extract_code_content(file_path)
                
                if content and len(content.strip()) > 50:
                    file_hash = calculate_file_hash(content)
                    embedding = model.encode(content).tolist()
                    relative_path = file_path.replace(extract_path, "").lstrip(os.sep).lstrip('/')
                    
                    storage.append({
                        'submission_id': submission_id,
                        'filename': filename,
                        'file_hash': file_hash,
                        'path': relative_path,
                        'embedding': embedding
                    })
                    stored_count += 1
        
        save_storage(storage)
        
        return JSONResponse({
            "message": "Submission stored successfully",
            "submission_id": submission_id,
            "files_stored": stored_count
        })
    
    finally:
        try:
            shutil.rmtree(temp_folder)
        except:
            pass

@app.get("/health")
async def health_check():
    return {"status": "healthy", "service": "Plagiarism Check Service"}

@app.delete("/delete-submission/{submission_id}")
async def delete_submission(submission_id: str):
    global storage
    original_count = len(storage)
    storage = [item for item in storage if item['submission_id'] != submission_id]
    deleted_count = original_count - len(storage)
    save_storage(storage)
    return {"message": f"Deleted {deleted_count} files from submission {submission_id}"}

if __name__ == "__main__":
    uvicorn.run(app, host="0.0.0.0", port=5001)
