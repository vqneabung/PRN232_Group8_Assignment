Add-Type -AssemblyName System.Net.Http

function Test-PlagiarismCheck {
    param(
        [string]$FilePath = "e:\S9\PRN232_Group8_Assignment\AnhNASE183208\0\solution.zip",
        [string]$SubmissionId = "test001",
        [double]$Threshold = 0.85
    )
    
    Write-Host "Testing plagiarism check for submission: $SubmissionId" -ForegroundColor Cyan
    
    $fileBytes = [System.IO.File]::ReadAllBytes($FilePath)
    $fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/zip")
    
    $multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
    $multipartContent.Add($fileContent, "file", "solution.zip")
    $multipartContent.Add([System.Net.Http.StringContent]::new($SubmissionId), "submissionId")
    $multipartContent.Add([System.Net.Http.StringContent]::new($Threshold.ToString()), "threshold")
    
    $httpClient = [System.Net.Http.HttpClient]::new()
    $httpClient.Timeout = [TimeSpan]::FromMinutes(5)
    
    try {
        Write-Host "Sending request..." -ForegroundColor Yellow
        $result = $httpClient.PostAsync("http://localhost:5268/api/plagiarism/check", $multipartContent).Result
        $content = $result.Content.ReadAsStringAsync().Result
        
        Write-Host "Response:" -ForegroundColor Green
        $json = $content | ConvertFrom-Json
        $json | ConvertTo-Json -Depth 10
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
    finally {
        $httpClient.Dispose()
    }
}

function Store-Submission {
    param(
        [string]$FilePath = "e:\S9\PRN232_Group8_Assignment\AnhNASE183208\0\solution.zip",
        [string]$SubmissionId = "stored001"
    )
    
    Write-Host "Storing submission: $SubmissionId" -ForegroundColor Cyan
    
    $fileBytes = [System.IO.File]::ReadAllBytes($FilePath)
    $fileContent = [System.Net.Http.ByteArrayContent]::new($fileBytes)
    $fileContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("application/zip")
    
    $multipartContent = [System.Net.Http.MultipartFormDataContent]::new()
    $multipartContent.Add($fileContent, "file", "solution.zip")
    $multipartContent.Add([System.Net.Http.StringContent]::new($SubmissionId), "submissionId")
    
    $httpClient = [System.Net.Http.HttpClient]::new()
    $httpClient.Timeout = [TimeSpan]::FromMinutes(5)
    
    try {
        Write-Host "Sending request..." -ForegroundColor Yellow
        $result = $httpClient.PostAsync("http://localhost:5268/api/plagiarism/store", $multipartContent).Result
        $content = $result.Content.ReadAsStringAsync().Result
        
        Write-Host "Response:" -ForegroundColor Green
        $json = $content | ConvertFrom-Json
        $json | ConvertTo-Json -Depth 10
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
    finally {
        $httpClient.Dispose()
    }
}

function Test-Health {
    Write-Host "Checking service health..." -ForegroundColor Cyan
    
    try {
        $response = Invoke-RestMethod -Uri "http://localhost:5268/api/plagiarism/health"
        Write-Host "C# API Health:" -ForegroundColor Green
        $response | ConvertTo-Json
        
        $pyResponse = Invoke-RestMethod -Uri "http://localhost:5001/health"
        Write-Host "`nPython Service Health:" -ForegroundColor Green
        $pyResponse | ConvertTo-Json
    }
    catch {
        Write-Host "Error: $_" -ForegroundColor Red
    }
}

Write-Host @"
==============================================
Plagiarism Check Test Script
==============================================

Commands:
  Test-Health                              - Check both services
  Test-PlagiarismCheck                     - Check plagiarism (default params)
  Test-PlagiarismCheck -SubmissionId "id"  - Check with custom ID
  Store-Submission                         - Store submission (default params)
  Store-Submission -SubmissionId "id"      - Store with custom ID

==============================================
"@ -ForegroundColor Cyan
