namespace PolicyManagement.Application.DTOs;

public class BulkFlagPoliciesRequest
{
    public List<Guid> PolicyIds { get; init; } = [];
}
