# TaskHub Startup Script
Write-Host "Starting TaskHub Application..." -ForegroundColor Green

# Start backend in a new PowerShell window
Write-Host "Starting Backend API..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\TaskHub.API'; Write-Host 'Backend API Starting...' -ForegroundColor Yellow; dotnet run"

# Wait for backend to initialize
Write-Host "Waiting for backend to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

# Start frontend in a new PowerShell window
Write-Host "Starting Frontend..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot'; Write-Host 'Frontend Starting...' -ForegroundColor Yellow; npm start"

Write-Host ""
Write-Host "TaskHub is starting up!" -ForegroundColor Green
Write-Host "Backend: https://localhost:7052" -ForegroundColor Cyan
Write-Host "Frontend: http://localhost:4200" -ForegroundColor Cyan
Write-Host ""
Write-Host "Please wait a few seconds for both servers to fully start." -ForegroundColor Yellow
Write-Host "Your browser should automatically open to http://localhost:4200" -ForegroundColor Yellow
