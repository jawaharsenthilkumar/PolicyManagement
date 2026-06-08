# Spec 4: Feature 4 - Bulk Flag Policies

## New DTOs

- BulkFlagPoliciesRequest: PolicyIds (List<Guid>)
- BulkFlagResultDto: SuccessCount (int), FailedCount (int), FailedIds (List<Guid>)

## New Components

### Repository

- ADD method: FlagPoliciesAsync(List<Guid> policyIds, CancellationToken)
  - Get all policies where Id in policyIds
  - Call policy.FlagForReview() on each (uses existing domain method)
  - SaveChangesAsync

### Service

- ADD interface method: BulkFlagPoliciesAsync(List<Guid> policyIds, CancellationToken) → BulkFlagResultDto
- ADD implementation:
  - Log entry: "Flagging {Count} policies"
  - If empty: log warning, return BulkFlagResultDto(0, 0, empty)
  - Try: call repository.FlagPoliciesAsync(policyIds), log success, return BulkFlagResultDto(policyIds.Count, 0, empty)
  - Catch Exception: log error, re-throw

### Controller

- ADD endpoint: [HttpPatch("flag")]
  - Param: [FromBody] BulkFlagPoliciesRequest request
  - ProducesResponseType: 200, 400, 500
  - Call \_service.BulkFlagPoliciesAsync(request.PolicyIds)
  - Return Ok(result)

## Tests (4 new)

- PolicyRepositoryTests: ADD 1 (FlagPoliciesAsync_WithValidIds_FlagsAllPolicies)
- PolicyServiceTests: ADD 2 (BulkFlagPoliciesAsync_WithValidIds_ReturnsSuccess, BulkFlagPoliciesAsync_WithEmptyList_ReturnsZero)
- PoliciesControllerIntegrationTests: ADD 1 (PATCH /flag with valid IDs returns 200)

## Validation

- dotnet build
- dotnet test (27 total: 23 existing + 4 new)
- PATCH /api/v1/policies/flag with body {"policyIds": ["guid1", "guid2"]} → 200
