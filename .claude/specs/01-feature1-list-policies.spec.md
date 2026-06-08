# Spec 1: Feature 1 - List Policies with Pagination

Build complete: Entity → DbContext → Repository → DTO → Service → Endpoint → Tests

## Domain Layer

### Enums
- PolicyStatus: Active(1), Expired(2), Pending(3), Cancelled(4)
- LineOfBusiness: Property(1), Casualty(2), AH(3), Marine(4)

### Policy Entity
Properties: Id (Guid, auto), PolicyNumber (unique, POL-XXXXXX), PolicyholderName, LineOfBusiness, Status, PremiumAmount (decimal 1k-5M), Currency (USD/SGD/HKD/AUD/JPY/THB), EffectiveDate, ExpiryDate, Region (8 APAC regions), Underwriter, FlaggedForReview (bool, default false), CreatedAt (UTC), UpdatedAt (UTC)

Methods: Constructor(required fields), FlagForReview(), UnflagForReview(), private parameterless constructor for EF

## Infrastructure Layer

### DbContext
- DbSet<Policy> Policies
- OnModelCreating applies PolicyConfiguration

### PolicyConfiguration
- HasKey(x => x.Id)
- Property constraints (IsRequired, HasMaxLength, etc.)
- Indexes: PolicyNumber (unique), Status, Region, LineOfBusiness, (EffectiveDate, ExpiryDate)
- ToTable("policies")

### IPolicyRepository Interface
Methods:
- GetPagedAsync(page, size, statusFilter?, lineOfBusinessFilter?, regionFilter?, effectiveDateFrom?, effectiveDateTo?, searchTerm?, sortBy, sortDirection, CancellationToken) → PagedResult<Policy>
- GetByIdAsync(Guid id, CancellationToken) → Policy or null
- CreateAsync(Policy, CancellationToken)
- ExistsAsync(Guid id, CancellationToken) → bool

### PolicyRepository Implementation
- GetPagedAsync: Apply all filters, search (policyNumber/policyholderName/underwriter), sort, paginate, return PagedResult<Policy>
- GetByIdAsync: AsNoTracking query, FirstOrDefaultAsync
- CreateAsync: Add and SaveChanges
- ExistsAsync: Check existence

### PolicySeeder
- SeedAsync(context) static method
- Creates 200+ realistic policies (POL-000001 to POL-000200+)
- APAC names (Hiroshi, Amara, Wei, Niran, Kenji, Priya, Siti, Carlos, Yuki, Hana, Tanaka, Kumar, Wong, Silva, Yoshida, Chen, Ahmad, Santos, Kim, Nguyen)
- All statuses, LOBs, regions, currencies represented
- PremiumAmount: random 1k-5M
- Dates spread across past year
- Idempotent (check if data exists first)

## Application Layer

### DTOs
- PagedResult<T>: Items (List<T>), Total (int), Page (int), Size (int), TotalPages (computed property)
- PolicyDto: All Policy properties with enums as strings

### AutoMapper
- PolicyMappingProfile: Policy → PolicyDto (convert Status and LineOfBusiness to strings)

### IPolicyService Interface
- GetPoliciesAsync(page, size, status?, lineOfBusiness?, region?, effectiveDateFrom?, effectiveDateTo?, search?, sortBy, sortDirection, CancellationToken) → PagedResult<PolicyDto>

### PolicyService Implementation
- Constructor: IPolicyRepository, IMapper, ILogger<PolicyService>
- GetPoliciesAsync: Log entry, call repository.GetPagedAsync(), map items to PolicyDto, return PagedResult<PolicyDto>

## API Layer

### PoliciesController
- [ApiController], [Route("api/v1/policies")], [Produces("application/json")]
- Constructor: IPolicyService, ILogger<PoliciesController>
- [HttpGet] GetPolicies(page, size, status?, lineOfBusiness?, region?, effectiveDateFrom?, effectiveDateTo?, search?, sortBy, sortDirection)
  - All params [FromQuery]
  - ProducesResponseType: 200, 400, 500
  - Call _service, return Ok(result)

### ExceptionHandlingMiddleware
- Catch KeyNotFoundException → response.StatusCode = 404, write JSON error
- Catch generic Exception → response.StatusCode = 500, write JSON error
- Log all exceptions

### Program.cs
- Serilog: MinimumLevel Information, WriteTo Console
- DbContext: UseSqlite("Data Source=PolicyManagement.db")
- AutoMapper: scan PolicyManagement.Application
- DI: AddScoped<IPolicyRepository, PolicyRepository>(), AddScoped<IPolicyService, PolicyService>()
- Swagger: title "Policy Management API", version "v1"
- CORS: AllowAnyOrigin, AllowAnyMethod, AllowAnyHeader
- On startup: dbContext.Database.Migrate(), await PolicySeeder.SeedAsync(dbContext)
- Middleware: ExceptionHandling
- Map: Swagger, HTTPS, CORS, Authorization, Controllers

## Tests

### PolicyRepositoryTests (5 tests, in-memory DB)
1. GetPagedAsync returns paginated result
2. GetPagedAsync with filters returns filtered results
3. GetPagedAsync with search returns matching results
4. GetByIdAsync with valid ID returns policy
5. GetByIdAsync with invalid ID returns null

### PolicyServiceTests (5 tests, mocked repository)
1. GetPoliciesAsync returns mapped PagedResult
2. GetPoliciesAsync calls repository once
3. GetPoliciesAsync maps Policy to PolicyDto
4. GetPoliciesAsync preserves pagination info
5. GetPoliciesAsync handles empty results

### PoliciesControllerIntegrationTests (5 tests, WebApplicationFactory + in-memory DB)
1. GET /policies returns 200 with paginated result
2. GET /policies?status=Active returns filtered results
3. GET /policies?search=term returns matching results
4. GET /policies?sortBy=id&sortDirection=desc returns sorted
5. GET /policies with all parameters works correctly

## Validation
- dotnet build
- dotnet ef migrations add InitialCreatePolicySchema
- dotnet ef database update (PolicyManagement.db created)
- dotnet test (15 tests pass)
- dotnet run (API starts)
- https://localhost:5001/swagger (GET /api/v1/policies endpoint visible)