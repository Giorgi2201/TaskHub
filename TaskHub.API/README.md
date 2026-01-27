# TaskHub Backend API

ASP.NET Core Web API backend for the TaskHub application.

## Features

- RESTful API for managing IT support requests
- Entity Framework Core with SQL Server
- Automatic database creation and seeding
- CORS configured for Angular frontend
- Category and subcategory management
- Request tracking with participants and comments
- Status workflow management

## Prerequisites

- .NET 9 SDK
- SQL Server LocalDB (comes with Visual Studio) or SQL Server
- Angular CLI (for running the frontend)

## Database

The application uses SQL Server LocalDB by default. The connection string is configured in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskHubDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

The database will be automatically created and seeded with initial data when the application first runs.

## Running the Backend

1. Navigate to the TaskHub.API folder:
   ```bash
   cd TaskHub.API
   ```

2. Run the application:
   ```bash
   dotnet run
   ```

The API will start on:
- HTTPS: `https://localhost:7052`
- HTTP: `http://localhost:5250`

## API Endpoints

### Requests
- `GET /api/requests` - Get all requests
- `GET /api/requests/{id}` - Get request by ID with full details
- `POST /api/requests` - Create new request
- `PUT /api/requests/{id}` - Update request
- `DELETE /api/requests/{id}` - Delete request

### Categories
- `GET /api/categories` - Get all categories with subcategories
- `GET /api/categories/{id}` - Get category by ID

## Seed Data

The database is seeded with:
- 6 Statuses (ახალი, დამტკიცების მოლოდინში, დამტკიცებული, შესრულების პროცესში, შესრულებული, უარყოფილი)
- 3 Users (გიორგი მაისურაძე, ნინო ბერიძე, დავით კვარაცხელია)
- 6 Categories (კომპიუტერული ტექნიკა, პრინტერი, ქსელი, პროგრამული უზრუნველყოფა, ტელეფონი, სხვა)
- 16 Subcategories
- 5 Sample Requests with participants and comments

## Project Structure

```
TaskHub.API/
├── Controllers/        # API Controllers
├── Data/              # DbContext and database configuration
├── DTOs/              # Data Transfer Objects
├── Models/            # Entity models
├── Properties/        # Launch settings
├── appsettings.json   # Application configuration
└── Program.cs         # Application entry point
```

## Running with the Frontend

1. Start the backend (this project) on port 7052
2. Navigate to the Angular frontend folder and run:
   ```bash
   cd ../
   npm install
   ng serve
   ```
3. Open browser to `http://localhost:4200`

The frontend is configured to connect to the backend at `https://localhost:7052`.
