# TaskHub

A full-stack IT support request management system built with Angular (frontend) and ASP.NET Core (backend).

## Project Structure

```
taskhub/
├── src/              # Angular frontend application
├── TaskHub.API/      # ASP.NET Core backend API
├── SETUP.md          # Complete setup guide
└── README.md         # This file
```

## Quick Start

### Prerequisites
- Node.js (v18+)
- .NET 9 SDK
- SQL Server LocalDB or SQL Server

### Running the Application

**Step 1: Start the Backend**
```bash
cd TaskHub.API
dotnet run
```
Backend will run on https://localhost:7052

**Step 2: Start the Frontend** (in a new terminal)
```bash
npm install
ng serve
```
Frontend will run on http://localhost:4200

**Step 3: Open the Application**
Navigate to http://localhost:4200 in your browser.

For detailed setup instructions, see [SETUP.md](SETUP.md).

## Features

- ✅ IT support request management
- ✅ Request categorization and subcategorization
- ✅ Request status tracking
- ✅ User roles and participants
- ✅ Comment system
- ✅ RESTful API backend
- ✅ SQL Server database with Entity Framework Core
- ✅ Responsive Angular UI

## Development

### Frontend Development Server

To start the Angular development server:

```bash
ng serve
```

Once the server is running, open your browser and navigate to `http://localhost:4200/`. The application will automatically reload whenever you modify any of the source files.

### Backend Development

The backend API is located in the `TaskHub.API` folder. To run:

```bash
cd TaskHub.API
dotnet run
```

## Code scaffolding

Angular CLI includes powerful code scaffolding tools. To generate a new component, run:

```bash
ng generate component component-name
```

For a complete list of available schematics (such as `components`, `directives`, or `pipes`), run:

```bash
ng generate --help
```

## Building

To build the project run:

```bash
ng build
```

This will compile your project and store the build artifacts in the `dist/` directory. By default, the production build optimizes your application for performance and speed.

## Running unit tests

To execute unit tests with the [Karma](https://karma-runner.github.io) test runner, use the following command:

```bash
ng test
```

## Running end-to-end tests

For end-to-end (e2e) testing, run:

```bash
ng e2e
```

Angular CLI does not come with an end-to-end testing framework by default. You can choose one that suits your needs.

## Additional Resources

For more information on using the Angular CLI, including detailed command references, visit the [Angular CLI Overview and Command Reference](https://angular.dev/tools/cli) page.
