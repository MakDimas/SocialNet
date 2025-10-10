namespace SocialNet.Core.Dtos.Models;

public class QueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 25;
    public string? SortBy { get; set; } = "CreatedAt";
    public string? SortDirection { get; set; } = "Desc";
}
