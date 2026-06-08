using Microsoft.EntityFrameworkCore;
using PolicyManagement.Application.DTOs;
using PolicyManagement.Application.Interfaces;
using PolicyManagement.Domain.Entities;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Infrastructure.Persistence.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly PolicyDbContext _context;

    public PolicyRepository(PolicyDbContext context) => _context = context;

    public async Task<PagedResult<Policy>> GetPagedAsync(
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
        CancellationToken cancellationToken = default)
    {
        var query = _context.Policies.AsNoTracking();

        if (statusFilter.HasValue)
            query = query.Where(p => p.Status == statusFilter.Value);

        if (lineOfBusinessFilter.HasValue)
            query = query.Where(p => p.LineOfBusiness == lineOfBusinessFilter.Value);

        if (!string.IsNullOrWhiteSpace(regionFilter))
            query = query.Where(p => p.Region == regionFilter);

        if (effectiveDateFrom.HasValue)
            query = query.Where(p => p.EffectiveDate >= effectiveDateFrom.Value);

        if (effectiveDateTo.HasValue)
            query = query.Where(p => p.EffectiveDate <= effectiveDateTo.Value);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p =>
                p.PolicyNumber.ToLower().Contains(term) ||
                p.PolicyholderName.ToLower().Contains(term) ||
                p.Underwriter.ToLower().Contains(term));
        }

        var total = await query.CountAsync(cancellationToken);

        query = (sortBy.ToLower(), sortDirection.ToLower()) switch
        {
            ("policynumber",     "asc") => query.OrderBy(p => p.PolicyNumber),
            ("policynumber",      _   ) => query.OrderByDescending(p => p.PolicyNumber),
            ("policyholdername", "asc") => query.OrderBy(p => p.PolicyholderName),
            ("policyholdername",  _   ) => query.OrderByDescending(p => p.PolicyholderName),
            ("premiumamount",    "asc") => query.OrderBy(p => p.PremiumAmount),
            ("premiumamount",     _   ) => query.OrderByDescending(p => p.PremiumAmount),
            ("effectivedate",    "asc") => query.OrderBy(p => p.EffectiveDate),
            ("effectivedate",     _   ) => query.OrderByDescending(p => p.EffectiveDate),
            ("expirydate",       "asc") => query.OrderBy(p => p.ExpiryDate),
            ("expirydate",        _   ) => query.OrderByDescending(p => p.ExpiryDate),
            ("status",           "asc") => query.OrderBy(p => p.Status),
            ("status",            _   ) => query.OrderByDescending(p => p.Status),
            ("createdat",        "asc") => query.OrderBy(p => p.CreatedAt),
            _                          => query.OrderByDescending(p => p.CreatedAt)
        };

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return new PagedResult<Policy> { Items = items, Total = total, Page = page, Size = size };
    }

    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task CreateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        _context.Policies.Add(policy);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default) =>
        await _context.Policies
            .AsNoTracking()
            .AnyAsync(p => p.Id == id, cancellationToken);

    public async Task<PolicySummaryDto> GetSummaryAsync(
        PolicyStatus? status = null,
        LineOfBusiness? lineOfBusiness = null,
        string? region = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Policies.AsNoTracking();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        if (lineOfBusiness.HasValue)
            query = query.Where(p => p.LineOfBusiness == lineOfBusiness.Value);

        if (!string.IsNullOrWhiteSpace(region))
            query = query.Where(p => p.Region == region);

        var total = await query.CountAsync(cancellationToken);

        // Cast enum key to int before projection so EF Core can translate to SQL,
        // then convert back to string in memory after materialisation.
        var statusGroups = await query
            .GroupBy(p => p.Status)
            .Select(g => new { Key = (int)g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var lobGroups = await query
            .GroupBy(p => p.LineOfBusiness)
            .Select(g => new { Key = (int)g.Key, Total = g.Sum(p => p.PremiumAmount) })
            .ToListAsync(cancellationToken);

        var today = DateTime.UtcNow.Date;
        var cutoff = today.AddDays(30);
        var expiring = await query
            .CountAsync(p => p.ExpiryDate >= today && p.ExpiryDate <= cutoff, cancellationToken);

        return new PolicySummaryDto
        {
            TotalPolicies = total,
            CountByStatus = statusGroups.ToDictionary(
                x => ((PolicyStatus)x.Key).ToString(), x => x.Count),
            TotalPremiumByLOB = lobGroups.ToDictionary(
                x => ((LineOfBusiness)x.Key).ToString(), x => x.Total),
            ExpiringWithin30Days = expiring
        };
    }
}
