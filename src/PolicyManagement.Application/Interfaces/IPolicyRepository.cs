using PolicyManagement.Application.DTOs;
using PolicyManagement.Domain.Entities;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Application.Interfaces;

public interface IPolicyRepository
{
    Task<PagedResult<Policy>> GetPagedAsync(
        int page,
        int size,
        PolicyStatus? statusFilter = null,
        LineOfBusiness? lineOfBusinessFilter = null,
        string? regionFilter = null,
        DateTime? effectiveDateFrom = null,
        DateTime? effectiveDateTo = null,
        string? searchTerm = null,
        string sortBy = "createdAt",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default);

    Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task CreateAsync(Policy policy, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PolicySummaryDto> GetSummaryAsync(
        PolicyStatus? status = null,
        LineOfBusiness? lineOfBusiness = null,
        string? region = null,
        CancellationToken cancellationToken = default);

    Task FlagPoliciesAsync(List<Guid> policyIds, CancellationToken cancellationToken = default);
}
