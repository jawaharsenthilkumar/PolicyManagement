using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Domain.Entities;

public sealed class Policy
{
    public Guid Id { get; private set; }
    public string PolicyNumber { get; private set; } = string.Empty;
    public string PolicyholderName { get; private set; } = string.Empty;
    public LineOfBusiness LineOfBusiness { get; private set; }
    public PolicyStatus Status { get; private set; }
    public decimal PremiumAmount { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public DateTime EffectiveDate { get; private set; }
    public DateTime ExpiryDate { get; private set; }
    public string Region { get; private set; } = string.Empty;
    public string Underwriter { get; private set; } = string.Empty;
    public bool FlaggedForReview { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Required by EF Core
    private Policy() { }

    public Policy(
        string policyNumber,
        string policyholderName,
        LineOfBusiness lineOfBusiness,
        PolicyStatus status,
        decimal premiumAmount,
        string currency,
        DateTime effectiveDate,
        DateTime expiryDate,
        string region,
        string underwriter)
    {
        Id = Guid.NewGuid();
        PolicyNumber = policyNumber;
        PolicyholderName = policyholderName;
        LineOfBusiness = lineOfBusiness;
        Status = status;
        PremiumAmount = premiumAmount;
        Currency = currency;
        EffectiveDate = effectiveDate;
        ExpiryDate = expiryDate;
        Region = region;
        Underwriter = underwriter;
        FlaggedForReview = false;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void FlagForReview()
    {
        FlaggedForReview = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UnflagForReview()
    {
        FlaggedForReview = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
