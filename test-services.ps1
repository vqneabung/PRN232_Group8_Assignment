Write-Host "=== Plagiarism Detection System - Test Script ===" -ForegroundColor Green
Write-Host ""

$pythonServiceUrl = "http://localhost:5001"
$apiUrl = "http://localhost:5000"

Write-Host "Step 1: Checking Python Service..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$pythonServiceUrl/health" -Method GET
    $result = $response.Content | ConvertFrom-Json
    Write-Host "  Status: $($result.status)" -ForegroundColor Green
    Write-Host "  Service: $($result.service)" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: Python service is not running!" -ForegroundColor Red
    Write-Host "  Please start it with: cd PlagiarismCheckService; python app.py" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Step 2: Checking C# API connection to Python..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$apiUrl/api/plagiarism/health" -Method GET
    $result = $response.Content | ConvertFrom-Json
    Write-Host "  Available: $($result.available)" -ForegroundColor Green
    Write-Host "  Message: $($result.message)" -ForegroundColor Green
} catch {
    Write-Host "  ERROR: Cannot connect to C# API!" -ForegroundColor Red
    Write-Host "  Please start it with: cd API; dotnet run" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== All services are running! ===" -ForegroundColor Green
Write-Host ""
Write-Host "You can now test the submission upload:" -ForegroundColor Cyan
Write-Host "  POST $apiUrl/api/submission/upload" -ForegroundColor White
Write-Host ""
Write-Host "Available endpoints:" -ForegroundColor Cyan
Write-Host "  Python Service:" -ForegroundColor Yellow
Write-Host "    GET    $pythonServiceUrl/health" -ForegroundColor White
Write-Host "    POST   $pythonServiceUrl/check-plagiarism" -ForegroundColor White
Write-Host "    POST   $pythonServiceUrl/store-submission" -ForegroundColor White
Write-Host "    DELETE $pythonServiceUrl/delete-submission/{id}" -ForegroundColor White
Write-Host ""
Write-Host "  C# API:" -ForegroundColor Yellow
Write-Host "    GET    $apiUrl/api/plagiarism/health" -ForegroundColor White
Write-Host "    POST   $apiUrl/api/plagiarism/check" -ForegroundColor White
Write-Host "    POST   $apiUrl/api/plagiarism/store" -ForegroundColor White
Write-Host "    POST   $apiUrl/api/submission/upload" -ForegroundColor White
Write-Host ""
