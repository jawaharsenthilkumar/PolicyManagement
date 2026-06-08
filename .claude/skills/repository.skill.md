# Skill: Repository

## Purpose

Generate repository interface and implementation following Repository Pattern.

## File Locations

- Interface: `src/PolicyManagement.Application/Interfaces/I[EntityName]Repository.cs`
- Implementation: `src/PolicyManagement.Infrastructure/Persistence/Repositories/[EntityName]Repository.cs`
- Configuration: `src/PolicyManagement.Infrastructure/Persistence/Configurations/[EntityName]Configuration.cs`

## Interface Rules

- GetByIdAsync(Guid id, CancellationToken)
- GetPagedAsync(filters, CancellationToken) → PagedResult<T>
- CreateAsync(Entity, CancellationToken)
- UpdateAsync(Entity, CancellationToken)
- ExistsAsync(Guid id, CancellationToken)
- All methods async, all accept CancellationToken = default

## Implementation Rules

- Use DbContext for data access
- Use .AsNoTracking() for read operations
- Return PagedResult<T> for paginated methods
- Support filtering, sorting, pagination
- Free-text search on multiple fields

## EF Configuration

- HasKey(x => x.Id)
- Property constraints (IsRequired, HasMaxLength, etc.)
- Unique indexes and composite indexes
- Enum conversions
