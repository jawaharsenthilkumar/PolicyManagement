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
}
