# Event Parking Reservation System

Full-stack team project built with ASP.NET Core .NET 8, EF Core, SQL Server, Angular and Tailwind CSS.

## Project Structure

- `backend/src/EventParking.API` - ASP.NET Core Web API
- `backend/tests/EventParking.Tests` - xUnit tests
- `frontend/event-parking-ui` - Angular application
- `docs/api` - API contract documentation
- `.config/dotnet-tools.json` - repository-local EF Core CLI
- `global.json` - .NET SDK pinning

## Local Requirements

- .NET SDK 8
- SQL Server Express or Developer
- Node.js and npm
- Angular CLI
- Git

## Backend Setup

From the repository root, restore and verify the backend using:

`dotnet restore backend\EventParking.sln`

`dotnet tool restore`

`dotnet build backend\EventParking.sln`

`dotnet test backend\EventParking.sln`

Local sensitive configuration must use ASP.NET Core User Secrets.

Each developer must configure their own `ConnectionStrings:DefaultConnection` value using their local SQL Server instance.

Real connection strings, JWT signing keys, passwords and credentials must never be committed.

## Backend Development URLs

- API: `https://localhost:7239`
- Swagger: `https://localhost:7239/swagger`
- Health: `https://localhost:7239/api/health`

Expected health response: `Healthy`

## Frontend Setup

Angular application:

`frontend/event-parking-ui`

Install dependencies with:

`npm ci`

Run locally with:

`ng serve`

Build with:

`ng build`

Development frontend URL:

`http://localhost:4200`

Development API base URL:

`https://localhost:7239/api`

## Current Platform Foundation

The current foundation includes:

- .NET 8 Web API
- EF Core SQL Server support
- repository-local EF CLI
- Swagger / OpenAPI
- global ProblemDetails error handling
- configured CORS
- health endpoint
- strongly typed configuration
- Angular environment configuration
- Angular HttpClient integration
- reusable Light Liquid Glass shared UI components
- frontend API error handling
- real Angular-to-API health verification
- xUnit foundation smoke test

## Git Workflow

Normal development must not be performed directly on `main`.

Each work package uses its assigned feature branch, meaningful commits, pull request, reviewer approval and merge.

## Team

- Fahim
- Shankavy
- Tharmithan
- Reshmina

Each member contributes across implementation, testing, Git workflow, pull requests, reviews, integration and viva preparation.