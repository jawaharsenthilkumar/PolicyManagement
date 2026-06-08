using Microsoft.EntityFrameworkCore;
using Xunit;
using PolicyManagement.Domain.Entities;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Infrastructure.Persistence;
using PolicyManagement.Infrastructure.Persistence.Repositories;

namespace PolicyManagement.UnitTests;

public class PolicyRepositoryTests
{
    private static PolicyDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<PolicyDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new PolicyDbContext(options);
    }

    private static Policy MakePolicy(string number, PolicyStatus status = PolicyStatus.Active,
        string name = "Test User") =>
        new(number, name, LineOfBusiness.Property, status, 10_000m, "USD",
            DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "Singapore", "Alice Tan");

    // ── 1 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPagedAsync_ReturnsPaginatedResult()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        for (var i = 1; i <= 5; i++) ctx.Policies.Add(MakePolicy($"POL-{i:D6}"));
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetPagedAsync(page: 1, size: 3);

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(5, result.Total);
        Assert.Equal(1, result.Page);
        Assert.Equal(3, result.Size);
    }

    // ── 2 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPagedAsync_WithFilters_ReturnsFilteredResults()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        ctx.Policies.Add(MakePolicy("POL-000001", PolicyStatus.Active));
        ctx.Policies.Add(MakePolicy("POL-000002", PolicyStatus.Active));
        ctx.Policies.Add(MakePolicy("POL-000003", PolicyStatus.Expired));
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetPagedAsync(page: 1, size: 10, statusFilter: PolicyStatus.Active);

        // Assert
        Assert.Equal(2, result.Total);
        Assert.All(result.Items, p => Assert.Equal(PolicyStatus.Active, p.Status));
    }

    // ── 3 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPagedAsync_WithSearch_ReturnsMatchingResults()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        ctx.Policies.Add(MakePolicy("POL-000001", name: "Hiroshi Tanaka"));
        ctx.Policies.Add(MakePolicy("POL-000002", name: "Wei Wong"));
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetPagedAsync(page: 1, size: 10, searchTerm: "Hiroshi");

        // Assert
        Assert.Equal(1, result.Total);
        Assert.Equal("POL-000001", result.Items[0].PolicyNumber);
    }

    // ── 4 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_WithValidId_ReturnsPolicy()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        var policy = MakePolicy("POL-000001");
        ctx.Policies.Add(policy);
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetByIdAsync(policy.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(policy.Id, result.Id);
        Assert.Equal("POL-000001", result.PolicyNumber);
    }

    // ── 6 (Feature 2) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_ReturnsAllFieldsCorrectly()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        var policy = new Policy(
            "POL-F2-001", "Wei Wong", LineOfBusiness.Marine, PolicyStatus.Expired,
            75_000m, "SGD",
            new DateTime(2023, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc),
            "Hong Kong", "Bob Lee");
        ctx.Policies.Add(policy);
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetByIdAsync(policy.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(policy.Id, result.Id);
        Assert.Equal("POL-F2-001", result.PolicyNumber);
        Assert.Equal("Wei Wong", result.PolicyholderName);
        Assert.Equal(LineOfBusiness.Marine, result.LineOfBusiness);
        Assert.Equal(PolicyStatus.Expired, result.Status);
        Assert.Equal(75_000m, result.PremiumAmount);
        Assert.Equal("SGD", result.Currency);
        Assert.Equal("Hong Kong", result.Region);
        Assert.Equal("Bob Lee", result.Underwriter);
    }

    // ── 5 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    // ── 8 (Feature 4) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task FlagPoliciesAsync_WithValidIds_FlagsAllPolicies()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        var p1 = MakePolicy("POL-F001");
        var p2 = MakePolicy("POL-F002");
        ctx.Policies.AddRange(p1, p2);
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        await repo.FlagPoliciesAsync([p1.Id, p2.Id]);

        // Assert — open a fresh context to confirm persistence
        await using var verify = CreateContext(db);
        var flagged = await verify.Policies
            .Where(p => p.PolicyNumber == "POL-F001" || p.PolicyNumber == "POL-F002")
            .ToListAsync();
        Assert.Equal(2, flagged.Count);
        Assert.All(flagged, p => Assert.True(p.FlaggedForReview));
    }

    // ── 7 (Feature 3) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetSummaryAsync_ReturnsCorrectAggregates()
    {
        // Arrange
        var db = Guid.NewGuid().ToString();
        await using var ctx = CreateContext(db);
        var today = DateTime.UtcNow.Date;

        ctx.Policies.Add(MakePolicy("POL-S001", PolicyStatus.Active));
        ctx.Policies.Add(MakePolicy("POL-S002", PolicyStatus.Active));
        ctx.Policies.Add(MakePolicy("POL-S003", PolicyStatus.Expired));
        // One policy expiring within 30 days (Marine LOB to distinguish from MakePolicy's Property)
        ctx.Policies.Add(new Policy(
            "POL-S004", "User D", LineOfBusiness.Marine, PolicyStatus.Active,
            5_000m, "USD", today.AddDays(-30), today.AddDays(10), "Singapore", "Alice Tan"));
        await ctx.SaveChangesAsync();
        var repo = new PolicyRepository(ctx);

        // Act
        var result = await repo.GetSummaryAsync();

        // Assert
        Assert.Equal(4, result.TotalPolicies);
        Assert.Equal(3, result.CountByStatus["Active"]);
        Assert.Equal(1, result.CountByStatus["Expired"]);
        Assert.Equal(30_000m, result.TotalPremiumByLOB["Property"]);
        Assert.Equal(5_000m, result.TotalPremiumByLOB["Marine"]);
        Assert.Equal(1, result.ExpiringWithin30Days);
    }
}
