# Plagiarism Check Service

Python microservice for plagiarism detection using VectorDB (ChromaDB)

## Setup

1. Install Python 3.8+
2. Run: `start.bat`

Or manually:
```bash
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
python app.py
```

## API Endpoints

- POST /check-plagiarism - Check if submission is plagiarized
- POST /store-submission - Store submission in VectorDB
- GET /health - Health check
- DELETE /delete-submission/{id} - Delete submission from VectorDB

Service runs on: http://localhost:5001
