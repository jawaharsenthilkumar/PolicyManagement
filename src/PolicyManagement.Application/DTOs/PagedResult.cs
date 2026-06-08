namespace PolicyManagement.Application.DTOs;

public class PagedResult<T>
{
    public List<T> Items { get; init; } = [];
    public int Total { get; init; }
    public int Page { get; init; }
    public int Size { get; init; }
    public int TotalPages => Size > 0 ? (int)Math.Ceiling((double)Total / Size) : 0;
}
