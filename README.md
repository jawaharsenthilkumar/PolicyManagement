# PolicyManagement

Insurance Policy Management System built with **.NET 8** using **Clean Architecture** and **spec-driven development**.

## Architecture

```
PolicyManagement/
├── src/
│   ├── PolicyManagement.Domain/          # Entities, value objects, domain interfaces
│   ├── PolicyManagement.Application/     # Use cases, commands, queries, DTOs
│   ├── PolicyManagement.Infrastructure/  # EF Core + SQLite, repositories
│   └── PolicyManagement.API/             # ASP.NET Core Web API, Swagger
└── tests/
    ├── PolicyManagement.Domain.Tests/
    └── PolicyManagement.Application.Tests/
```

## Tech Stack

| Layer          | Technology                              |
|----------------|-----------------------------------------|
| API            | ASP.NET Core 8, Swashbuckle (Swagger)   |
| Logging        | Serilog + Console sink                  |
| Application    | AutoMapper                              |
| Persistence    | Entity Framework Core 8 + SQLite        |
| Testing        | xUnit (planned)                         |

## Project Dependencies

```
API  →  Application  →  Domain
API  →  Infrastructure
Infrastructure  →  Application  →  Domain
```

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
dotnet build
```

### Run

```bash
cd src/PolicyManagement.API
dotnet run
```

The API will be available at `https://localhost:5001` with Swagger UI at `/swagger`.

### Database

SQLite database file `PolicyManagement.db` is created automatically on first run in the API project directory.

## Development

This project follows **spec-driven development** — each feature is implemented from a specification file in `.claude/specs/`.

| Spec | Description |
|------|-------------|
| `00-project-creation.spec.md` | Solution structure and project scaffolding |
