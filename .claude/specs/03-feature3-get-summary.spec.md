# Spec 3: Feature 3 - Get Summary Statistics

## New DTOs

- PolicySummaryDto: CountByStatus (Dict<string, int>), TotalPremiumByLOB (Dict<string, decimal>), ExpiringWithin30Days (int), TotalPolicies (int)

## New Components

### Repository

- ADD method: GetSummaryAsync(status?, lineOfBusiness?, region?, CancellationToken) → PolicySummaryDto
  - Build IQueryable with all filters applied
  - CountByStatus: GroupBy(Status).ToDictionary() with count
  - TotalPremiumByLOB: GroupBy(LineOfBusiness).Select(Sum).ToDictionary()
  - ExpiringWithin30Days: Count where ExpiryDate <= today+30days AND >= today
  - TotalPolicies: Total count
  - Return populated PolicySummaryDto

### Service

- ADD interface method: GetSummaryAsync(status?, lineOfBusiness?, region?, CancellationToken) → PolicySummaryDto
- ADD implementation: Log entry, call repository.GetSummaryAsync(filters), return result

### Controller

- ADD endpoint: [HttpGet("summary")]
  - Params: [FromQuery] status?, lineOfBusiness?, region?
  - ProducesResponseType: 200, 400, 500
  - Call \_service.GetSummaryAsync(filters)
  - Return Ok(result)

## Tests (3 new)

- PolicyRepositoryTests: ADD 1 (GetSummaryAsync_ReturnsCorrectAggregates)
- PolicyServiceTests: ADD 1 (GetSummaryAsync_CallsRepositoryAndReturnsResult)
- PoliciesControllerIntegrationTests: ADD 1 (GET /summary returns 200 with stats)

## Validation

- dotnet build
- dotnet test (23 total: 20 existing + 3 new)
- GET /api/v1/policies/summary → 200 with aggregated stats
