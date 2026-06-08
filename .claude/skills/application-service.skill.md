# Skill: ApplicationService

## Purpose

Generate application services for business logic orchestration.

## File Locations

- Interface: `src/PolicyManagement.Application/Interfaces/I[ServiceName]Service.cs`
- Implementation: `src/PolicyManagement.Application/Services/[ServiceName]Service.cs`

## Implementation Rules

- Constructor injects: I[EntityName]Repository, IMapper, ILogger<[ServiceName]>
- All fields are private readonly
- All methods async with CancellationToken = default
- Log entry point: \_logger.LogInformation()
- Call repository for data access
- Use AutoMapper for DTO conversions
- Throw KeyNotFoundException if needed
- Return DTOs, never entities
