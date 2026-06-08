# Spec 2: Feature 2 - Get Single Policy

## Reuse

- Policy entity (no changes)
- PolicyDto (no changes)
- AutoMapper (no changes)
- ExceptionHandlingMiddleware (KeyNotFoundException → 404)

## New Components

### Repository

- ADD method: GetByIdAsync(Guid id, CancellationToken) → Policy or null

### Service

- ADD interface method: GetPolicyByIdAsync(Guid id, CancellationToken) → PolicyDto
- ADD implementation: Log entry, call repository.GetByIdAsync(id), if null throw KeyNotFoundException, map to PolicyDto, return

### Controller

- ADD endpoint: [HttpGet("{id:guid}")]
  - Param: [FromRoute] Guid id
  - ProducesResponseType: 200, 404, 500
  - Call \_service.GetPolicyByIdAsync(id)
  - Return Ok(result)
  - Include XML doc comments

## Tests (5 new)

- PolicyRepositoryTests: ADD 1 (GetByIdAsync_WithValidId_ReturnsPolicy)
- PolicyServiceTests: ADD 2 (GetPolicyByIdAsync_WithValidId_ReturnsMappedPolicy, GetPolicyByIdAsync_WithInvalidId_ThrowsKeyNotFoundException)
- PoliciesControllerIntegrationTests: ADD 2 (GET /{valid-id} returns 200, GET /{invalid-id} returns 404)

## Validation

- dotnet build
- dotnet test (20 total: 15 existing + 5 new)
- GET /api/v1/policies/{valid-guid} → 200
- GET /api/v1/policies/{invalid-guid} → 404
