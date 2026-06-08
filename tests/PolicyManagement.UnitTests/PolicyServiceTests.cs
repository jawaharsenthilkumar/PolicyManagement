using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using PolicyManagement.Application.DTOs;
using PolicyManagement.Application.Interfaces;
using PolicyManagement.Application.Mappings;
using PolicyManagement.Application.Services;
using PolicyManagement.Domain.Entities;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.UnitTests;

public class PolicyServiceTests
{
    private readonly Mock<IPolicyRepository> _repo = new();
    private readonly IMapper _mapper;
    private readonly Mock<ILogger<PolicyService>> _logger = new();
    private readonly PolicyService _sut;

    public PolicyServiceTests()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAutoMapper(cfg => cfg.AddProfile<PolicyMappingProfile>());
        _mapper = services.BuildServiceProvider().GetRequiredService<IMapper>();
        _sut = new PolicyService(_repo.Object, _mapper, _logger.Object);
    }

    private static Policy MakePolicy(string number = "POL-000001",
        PolicyStatus status = PolicyStatus.Active,
        LineOfBusiness lob = LineOfBusiness.Property) =>
        new(number, "Test User", lob, status, 10_000m, "USD",
            DateTime.UtcNow, DateTime.UtcNow.AddYears(1), "Singapore", "Alice Tan");

    private void SetupRepo(List<Policy> items, int total = -1, int page = 1, int size = 10) =>
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<PolicyStatus?>(), It.IsAny<LineOfBusiness?>(),
                It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Policy>
            {
                Items = items,
                Total = total < 0 ? items.Count : total,
                Page = page,
                Size = size
            });

    // ── 1 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPoliciesAsync_ReturnsMappedPagedResult()
    {
        // Arrange
        SetupRepo([MakePolicy()]);

        // Act
        var result = await _sut.GetPoliciesAsync(1, 10);

        // Assert
        Assert.NotNull(result);
        Assert.IsType<PagedResult<PolicyDto>>(result);
        Assert.Single(result.Items);
    }

    // ── 2 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPoliciesAsync_CallsRepositoryOnce()
    {
        // Arrange
        SetupRepo([]);

        // Act
        await _sut.GetPoliciesAsync(1, 10);

        // Assert
        _repo.Verify(r => r.GetPagedAsync(
            It.IsAny<int>(), It.IsAny<int>(),
            It.IsAny<PolicyStatus?>(), It.IsAny<LineOfBusiness?>(),
            It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
            It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── 3 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPoliciesAsync_MapsPolicyToPolicyDto()
    {
        // Arrange
        SetupRepo([MakePolicy("POL-999999", PolicyStatus.Active, LineOfBusiness.Marine)]);

        // Act
        var result = await _sut.GetPoliciesAsync(1, 10);

        // Assert
        var dto = Assert.Single(result.Items);
        Assert.Equal("POL-999999", dto.PolicyNumber);
        Assert.Equal("Active", dto.Status);
        Assert.Equal("Marine", dto.LineOfBusiness);
    }

    // ── 4 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPoliciesAsync_PreservesPaginationInfo()
    {
        // Arrange
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<PolicyStatus?>(), It.IsAny<LineOfBusiness?>(),
                It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                It.IsAny<string?>(), It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Policy> { Items = [], Total = 42, Page = 3, Size = 10 });

        // Act
        var result = await _sut.GetPoliciesAsync(3, 10);

        // Assert
        Assert.Equal(42, result.Total);
        Assert.Equal(3, result.Page);
        Assert.Equal(10, result.Size);
        Assert.Equal(5, result.TotalPages); // ceil(42/10) = 5
    }

    // ── 5 ─────────────────────────────────────────────────────────────────────
    [Fact]
    public async Task GetPoliciesAsync_HandlesEmptyResults()
    {
        // Arrange
        SetupRepo([]);

        // Act
        var result = await _sut.GetPoliciesAsync(1, 10);

        // Assert
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.TotalPages);
    }
}
