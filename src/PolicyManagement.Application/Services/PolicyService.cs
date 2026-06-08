using AutoMapper;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs;
using PolicyManagement.Application.Interfaces;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Application.Services;

public class PolicyService : IPolicyService
{
    private readonly IPolicyRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(IPolicyRepository repository, IMapper mapper, ILogger<PolicyService> logger)
    {
        _repository = repository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<PolicyDto>> GetPoliciesAsync(
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
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetPoliciesAsync: page={Page}, size={Size}, status={Status}, lob={LineOfBusiness}, region={Region}",
            page, size, status, lineOfBusiness, region);

        var result = await _repository.GetPagedAsync(
            page, size, status, lineOfBusiness, region,
            effectiveDateFrom, effectiveDateTo, search,
            sortBy, sortDirection, cancellationToken);

        return new PagedResult<PolicyDto>
        {
            Items = _mapper.Map<List<PolicyDto>>(result.Items),
            Total = result.Total,
            Page = result.Page,
            Size = result.Size
        };
    }

    public async Task<PolicyDto> GetPolicyByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("GetPolicyByIdAsync: id={Id}", id);

        var policy = await _repository.GetByIdAsync(id, cancellationToken);

        if (policy is null)
            throw new KeyNotFoundException($"Policy with id '{id}' was not found.");

        return _mapper.Map<PolicyDto>(policy);
    }

    public async Task<PolicySummaryDto> GetSummaryAsync(
        PolicyStatus? status = null,
        LineOfBusiness? lineOfBusiness = null,
        string? region = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "GetSummaryAsync: status={Status}, lob={LineOfBusiness}, region={Region}",
            status, lineOfBusiness, region);

        return await _repository.GetSummaryAsync(status, lineOfBusiness, region, cancellationToken);
    }

    public async Task<BulkFlagResultDto> BulkFlagPoliciesAsync(
        List<Guid> policyIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("BulkFlagPoliciesAsync: flagging {Count} policies", policyIds.Count);

        if (policyIds.Count == 0)
        {
            _logger.LogWarning("BulkFlagPoliciesAsync called with empty list");
            return new BulkFlagResultDto { SuccessCount = 0, FailedCount = 0, FailedIds = [] };
        }

        try
        {
            await _repository.FlagPoliciesAsync(policyIds, cancellationToken);
            _logger.LogInformation("BulkFlagPoliciesAsync: successfully flagged {Count} policies", policyIds.Count);
            return new BulkFlagResultDto { SuccessCount = policyIds.Count, FailedCount = 0, FailedIds = [] };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BulkFlagPoliciesAsync failed for {Count} policies", policyIds.Count);
            throw;
        }
    }
}
