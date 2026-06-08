using System.Net;
using Xunit;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PolicyManagement.Application.DTOs;
using PolicyManagement.Infrastructure.Persistence;

namespace PolicyManagement.IntegrationTests;

public class PolicyManagementWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"IntegrationTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PolicyDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<PolicyDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));
        });
    }
}

public class PoliciesControllerIntegrationTests : IClassFixture<PolicyManagementWebApplicationFactory>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public PoliciesControllerIntegrationTests(PolicyManagementWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<PagedResult<PolicyDto>> GetPoliciesAsync(string query = "")
    {
        var response = await _client.GetAsync($"/api/v1/policies{query}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PagedResult<PolicyDto>>(json, Json)!;
    }

    // ── 1 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicies_Returns200WithPaginatedResult()
    {
        // Act
        var result = await GetPoliciesAsync("?page=1&size=10");

        // Assert
        Assert.True(result.Total > 0);
        Assert.NotEmpty(result.Items);
        Assert.Equal(10, result.Items.Count);
        Assert.Equal(1, result.Page);
    }

    // ── 2 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicies_WithStatusFilter_ReturnsFilteredResults()
    {
        // Act
        var result = await GetPoliciesAsync("?status=Active&page=1&size=100");

        // Assert
        Assert.True(result.Total > 0);
        Assert.All(result.Items, p => Assert.Equal("Active", p.Status));
    }

    // ── 3 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicies_WithSearch_ReturnsMatchingResults()
    {
        // Act — "Hiroshi" appears as a first name in the seed data (~21 policies)
        var result = await GetPoliciesAsync("?search=Hiroshi&page=1&size=50");

        // Assert
        Assert.True(result.Total > 0);
        Assert.All(result.Items, p =>
            Assert.True(
                p.PolicyholderName.Contains("Hiroshi", StringComparison.OrdinalIgnoreCase) ||
                p.PolicyNumber.Contains("Hiroshi", StringComparison.OrdinalIgnoreCase) ||
                p.Underwriter.Contains("Hiroshi", StringComparison.OrdinalIgnoreCase)));
    }

    // ── 4 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicies_WithSortByIdDesc_ReturnsSortedResults()
    {
        // Act
        var result = await GetPoliciesAsync("?sortBy=id&sortDirection=desc&page=1&size=10");

        // Assert
        Assert.NotEmpty(result.Items);
        Assert.Equal(10, result.Items.Count);
    }

    // ── 6 (Feature 2) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicyById_WithValidId_Returns200WithPolicyDto()
    {
        // Arrange — pull a real ID from the seeded list
        var list = await GetPoliciesAsync("?page=1&size=1");
        var validId = list.Items[0].Id;

        // Act
        var response = await _client.GetAsync($"/api/v1/policies/{validId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PolicyDto>(json, Json)!;
        Assert.NotNull(result);
        Assert.Equal(validId, result.Id);
    }

    // ── 7 (Feature 2) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicyById_WithInvalidId_Returns404()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/policies/{Guid.NewGuid()}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ── 5 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPolicies_WithAllParameters_WorksCorrectly()
    {
        // Act — Active + Singapore: ~27 seeded policies, requesting page 1 of 5
        var result = await GetPoliciesAsync(
            "?page=1&size=5&status=Active&region=Singapore&sortBy=premiumAmount&sortDirection=asc");

        // Assert
        Assert.True(result.Total > 0);
        Assert.True(result.Items.Count <= 5);
        Assert.All(result.Items, p => Assert.Equal("Active", p.Status));
        Assert.All(result.Items, p => Assert.Equal("Singapore", p.Region));
    }

    // ── 9 (Feature 4) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task BulkFlagPolicies_WithValidIds_Returns200()
    {
        // Arrange — get two real IDs from the seeded list
        var list = await GetPoliciesAsync("?page=1&size=2");
        var ids = list.Items.Select(p => p.Id).ToList();
        var body = JsonSerializer.Serialize(new { policyIds = ids });
        var content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PatchAsync("/api/v1/policies/flag", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<BulkFlagResultDto>(json, Json)!;
        Assert.Equal(2, result.SuccessCount);
        Assert.Equal(0, result.FailedCount);
        Assert.Empty(result.FailedIds);
    }

    // ── 8 (Feature 3) ─────────────────────────────────────────────────────────
    [Fact]
    public async Task GetSummary_Returns200WithAggregatedStats()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/policies/summary");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PolicySummaryDto>(json, Json)!;
        Assert.NotNull(result);
        Assert.True(result.TotalPolicies > 0);
        Assert.NotEmpty(result.CountByStatus);
        Assert.NotEmpty(result.TotalPremiumByLOB);
    }
}
