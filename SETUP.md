# TaskHub - Complete Setup Guide

This guide will help you set up and run both the frontend (Angular) and backend (ASP.NET Core) of the TaskHub application.

## Prerequisites

### Required Software
1. **Node.js** (v18 or later) - for Angular frontend
   - Download from: https://nodejs.org/
   
2. **.NET 9 SDK** - for ASP.NET Core backend
   - Download from: https://dotnet.microsoft.com/download
   
3. **SQL Server LocalDB** (comes with Visual Studio) or SQL Server
   - Alternatively, install SQL Server Express: https://www.microsoft.com/sql-server/sql-server-downloads

### Verify Installation
Open a terminal/command prompt and run:
```bash
node --version
npm --version
dotnet --version
```

## Project Structure

```
taskhub/
├── src/              # Angular frontend
│   ├── app/
│   └── ...
├── TaskHub.API/      # ASP.NET Core backend
│   ├── Controllers/
│   ├── Models/
│   └── ...
├── package.json
└── SETUP.md (this file)
```

## Setup Instructions

### 1. Backend Setup (ASP.NET Core)

#### Step 1: Navigate to the API folder
```bash
cd TaskHub.API
```

#### Step 2: Restore packages (if needed)
```bash
dotnet restore
```

#### Step 3: Build the project
```bash
dotnet build
```

#### Step 4: Run the backend
```bash
dotnet run
```

The backend will start on:
- **HTTPS**: https://localhost:7052
- **HTTP**: http://localhost:5250

**Note**: The database will be automatically created and seeded with sample data on first run.

### 2. Frontend Setup (Angular)

#### Step 1: Navigate to the project root (if you're in TaskHub.API folder)
```bash
cd ..
```

#### Step 2: Install dependencies
```bash
npm install
```

#### Step 3: Start the Angular development server
```bash
npm start
```
or
```bash
ng serve
```

The frontend will start on:
- **URL**: http://localhost:4200

## Running the Full Application

### Option 1: Two Separate Terminals (Recommended)

**Terminal 1 - Backend:**
```bash
cd TaskHub.API
dotnet run
```

**Terminal 2 - Frontend:**
```bash
npm start
```

### Option 2: PowerShell (Windows)
You can create a simple PowerShell script to run both:

Create `start-app.ps1`:
```powershell
# Start backend in background
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd TaskHub.API; dotnet run"

# Wait a moment for backend to start
Start-Sleep -Seconds 5

# Start frontend
npm start
```

Run it:
```bash
./start-app.ps1
```

## Accessing the Application

1. Ensure both backend and frontend are running
2. Open your browser to: **http://localhost:4200**
3. You should see the TaskHub application

## Features

- **View Requests**: See all IT support requests in the system
- **Create Requests**: Submit new requests by category
- **View Details**: Click on a request to see full details, participants, and comments
- **Search**: Filter requests by description or category

## Sample Data

The application comes pre-loaded with:
- 5 sample requests
- 3 users (გიორგი მაისურაძე, ნინო ბერიძე, დავით კვარაცხელია)
- 6 categories with multiple subcategories
- Various request statuses

## Troubleshooting

### Backend Issues

**Problem**: Database connection errors
- **Solution**: Ensure SQL Server LocalDB is installed. Try changing the connection string in `TaskHub.API/appsettings.json`

**Problem**: Port 7052 already in use
- **Solution**: Stop other applications using that port, or modify `Properties/launchSettings.json`

**Problem**: CORS errors in browser console
- **Solution**: Ensure backend is running on https://localhost:7052 and frontend on http://localhost:4200

### Frontend Issues

**Problem**: `npm install` fails
- **Solution**: Clear npm cache: `npm cache clean --force`, then try again

**Problem**: Angular CLI not found
- **Solution**: Install globally: `npm install -g @angular/cli`

**Problem**: API connection errors
- **Solution**: 
  1. Verify backend is running on https://localhost:7052
  2. Check browser console for CORS errors
  3. Ensure `request.service.ts` has correct API URL

### SSL Certificate Issues

If you get SSL certificate warnings when accessing https://localhost:7052:

**Windows:**
```bash
dotnet dev-certs https --trust
```

Then restart the backend.

## Development Notes

### Backend (ASP.NET Core)
- Built with .NET 9
- Uses Entity Framework Core for database access
- SQL Server LocalDB for data storage
- Automatic database seeding on startup
- CORS enabled for localhost:4200

### Frontend (Angular)
- Angular 19
- Standalone components
- HttpClient for API communication
- Reactive forms and routing

## API Documentation

Once the backend is running, you can access API documentation at:
- **OpenAPI**: https://localhost:7052/openapi/v1.json

## Next Steps

After successful setup:
1. Explore the existing requests
2. Create a new request by clicking "ახალი მოთხოვნა"
3. View request details by clicking on any request
4. Check the backend API endpoints at https://localhost:7052/api/requests

## Need Help?

- Check the browser console (F12) for frontend errors
- Check the terminal running `dotnet run` for backend errors
- Ensure all prerequisites are installed correctly
- Verify both frontend and backend are running simultaneously
