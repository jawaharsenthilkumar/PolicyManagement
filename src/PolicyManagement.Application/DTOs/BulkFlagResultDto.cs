namespace PolicyManagement.Application.DTOs;

public class BulkFlagResultDto
{
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public List<Guid> FailedIds { get; init; } = [];
}
