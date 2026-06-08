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
}
