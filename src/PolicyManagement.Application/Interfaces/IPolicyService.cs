using PolicyManagement.Application.DTOs;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Application.Interfaces;

public interface IPolicyService
{
    Task<PagedResult<PolicyDto>> GetPoliciesAsync(
        int page,
        int size,
        PolicyStatus? status = null,
        LineOfBusiness? lineOfBusiness = null,
        string? region = null,
        DateTime? effectiveDateFrom = null,
        DateTime? effectiveDateTo = null,
        string? search = null,
        string sortBy = "createdAt",
        string sortDirection = "desc",
        CancellationToken cancellationToken = default);

    Task<PolicyDto> GetPolicyByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PolicySummaryDto> GetSummaryAsync(
        PolicyStatus? status = null,
        LineOfBusiness? lineOfBusiness = null,
        string? region = null,
        CancellationToken cancellationToken = default);
}
