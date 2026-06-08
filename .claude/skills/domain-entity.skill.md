# Skill: DomainEntity

## Purpose

Generate clean domain entities with no framework dependencies.

## Key Rules

- No EF Core attributes ([Column], [Table], etc.)
- Private setters for immutability
- Constructor accepts only required fields
- Private parameterless constructor for EF Core
- Business logic methods are public
- UpdatedAt field updated when domain methods called
- Namespace: PolicyManagement.Domain.Entities

## File Location

`src/PolicyManagement.Domain/Entities/[EntityName].cs`
