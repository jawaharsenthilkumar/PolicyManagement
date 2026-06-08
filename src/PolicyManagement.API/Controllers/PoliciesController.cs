using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.DTOs;
using PolicyManagement.Application.Interfaces;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.API.Controllers;

[ApiController]
[Route("api/v1/policies")]
[Produces("application/json")]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _service;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(IPolicyService service, ILogger<PoliciesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of policies with optional filtering and sorting.
    /// </summary>
    /// <param name="page">Page number (1-based).</param>
    /// <param name="size">Number of results per page.</param>
    /// <param name="status">Filter by policy status.</param>
    /// <param name="lineOfBusiness">Filter by line of business.</param>
    /// <param name="region">Filter by APAC region.</param>
    /// <param name="effectiveDateFrom">Filter policies effective on or after this date.</param>
    /// <param name="effectiveDateTo">Filter policies effective on or before this date.</param>
    /// <param name="search">Free-text search across policy number, holder name, and underwriter.</param>
    /// <param name="sortBy">Field to sort by (default: createdAt).</param>
    /// <param name="sortDirection">Sort direction: asc or desc (default: desc).</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<PolicyDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPolicies(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] PolicyStatus? status = null,
        [FromQuery] LineOfBusiness? lineOfBusiness = null,
        [FromQuery] string? region = null,
        [FromQuery] DateTime? effectiveDateFrom = null,
        [FromQuery] DateTime? effectiveDateTo = null,
        [FromQuery] string? search = null,
        [FromQuery] string sortBy = "createdAt",
        [FromQuery] string sortDirection = "desc",
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetPoliciesAsync(
            page, size, status, lineOfBusiness, region,
            effectiveDateFrom, effectiveDateTo, search,
            sortBy, sortDirection, cancellationToken);

        return Ok(result);
    }
}
