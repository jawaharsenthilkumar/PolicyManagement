# Spec 0: Project Creation

## Objective

Create solution structure with 4 projects and folders. NO code files.

## Deliverables

- PolicyManagement.sln
- src/PolicyManagement.Domain/PolicyManagement.Domain.csproj
- src/PolicyManagement.Application/PolicyManagement.Application.csproj
- src/PolicyManagement.Infrastructure/PolicyManagement.Infrastructure.csproj
- src/PolicyManagement.API/PolicyManagement.API.csproj
- Complete folder structure (src/_, tests/_)
- appsettings.json with SQLite connection string
- appsettings.Development.json with debug logging

## Project Dependencies

- Domain: NO NuGet packages, NO references
- Application: References Domain; NuGet: AutoMapper
- Infrastructure: References Application + Domain; NuGet: EntityFrameworkCore, EntityFrameworkCore.Sqlite
- API: References Application + Infrastructure; NuGet: Swashbuckle.AspNetCore, Serilog, Serilog.Sinks.Console

## Configuration

- appsettings.json: ConnectionString "Data Source=PolicyManagement.db"
- appsettings.Development.json: Debug logging enabled

## Validation

- dotnet build (must succeed)
