namespace PolicyManagement.Application.DTOs;

public class PolicySummaryDto
{
    public int TotalPolicies { get; init; }
    public Dictionary<string, int> CountByStatus { get; init; } = [];
    public Dictionary<string, decimal> TotalPremiumByLOB { get; init; } = [];
    public int ExpiringWithin30Days { get; init; }
}
