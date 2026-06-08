using Microsoft.EntityFrameworkCore;
using PolicyManagement.Domain.Entities;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Infrastructure.Persistence;

namespace PolicyManagement.Infrastructure.Seeders;

public static class PolicySeeder
{
    private static readonly string[] FirstNames =
        ["Hiroshi", "Amara", "Wei", "Niran", "Kenji", "Priya", "Siti", "Carlos", "Yuki", "Hana"];

    private static readonly string[] LastNames =
        ["Tanaka", "Kumar", "Wong", "Silva", "Yoshida", "Chen", "Ahmad", "Santos", "Kim", "Nguyen"];

    private static readonly string[] Currencies = ["USD", "SGD", "HKD", "AUD", "JPY", "THB"];

    private static readonly string[] Regions =
        ["Singapore", "Hong Kong", "Japan", "Australia", "Thailand", "Malaysia", "Indonesia", "Philippines"];

    private static readonly string[] Underwriters =
        ["Alice Tan", "Bob Lee", "Carol Wang", "David Sharma", "Emma Rodriguez", "Frank Nakamura", "Grace Liu", "Henry Park"];

    public static async Task SeedAsync(PolicyDbContext context)
    {
        if (await context.Policies.AnyAsync())
            return;

        var rng = new Random(42);
        var policies = new List<Policy>(210);
        var baseDate = DateTime.UtcNow.Date;
        var statuses = Enum.GetValues<PolicyStatus>();
        var lobs = Enum.GetValues<LineOfBusiness>();

        for (var i = 1; i <= 210; i++)
        {
            var effectiveDate = baseDate.AddDays(rng.Next(-365, 30));
            var premiumAmount = Math.Round(1000m + (decimal)(rng.NextDouble() * 4_999_000), 2);

            policies.Add(new Policy(
                policyNumber:     $"POL-{i:D6}",
                policyholderName: $"{FirstNames[(i - 1) % FirstNames.Length]} {LastNames[i % LastNames.Length]}",
                lineOfBusiness:   lobs[(i - 1) % lobs.Length],
                status:           statuses[(i - 1) % statuses.Length],
                premiumAmount:    premiumAmount,
                currency:         Currencies[(i - 1) % Currencies.Length],
                effectiveDate:    effectiveDate,
                expiryDate:       effectiveDate.AddYears(1),
                region:           Regions[(i - 1) % Regions.Length],
                underwriter:      Underwriters[(i - 1) % Underwriters.Length]));
        }

        await context.Policies.AddRangeAsync(policies);
        await context.SaveChangesAsync();
    }
}
