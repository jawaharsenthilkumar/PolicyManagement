namespace PolicyManagement.Application.DTOs;

public class PolicyDto
{
    public Guid Id { get; init; }
    public string PolicyNumber { get; init; } = string.Empty;
    public string PolicyholderName { get; init; } = string.Empty;
    public string LineOfBusiness { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public decimal PremiumAmount { get; init; }
    public string Currency { get; init; } = string.Empty;
    public DateTime EffectiveDate { get; init; }
    public DateTime ExpiryDate { get; init; }
    public string Region { get; init; } = string.Empty;
    public string Underwriter { get; init; } = string.Empty;
    public bool FlaggedForReview { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
