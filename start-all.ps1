Write-Host "=== Starting Plagiarism Detection Services ===" -ForegroundColor Green
Write-Host ""

$pythonPath = ".\PlagiarismCheckService"
$apiPath = ".\API"

Write-Host "Starting Python Service..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$pythonPath'; .\venv\Scripts\activate; python app.py"

Start-Sleep -Seconds 3

Write-Host "Starting C# API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$apiPath'; dotnet run"

Write-Host ""
Write-Host "Services are starting..." -ForegroundColor Green
Write-Host "Please wait 10-15 seconds for services to be ready" -ForegroundColor Yellow
Write-Host ""
Write-Host "Then run: .\test-services.ps1 to verify" -ForegroundColor Cyan
