namespace BMSBTRwp_API.Models;

/// <summary>
/// Generic wrapper returned by all paginated endpoints.
/// Designed for React Native clients that insert chunks into local SQLite.
/// </summary>
public class PagedResult
{
    public string TableName { get; set; } = string.Empty;
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public List<string> Columns { get; set; } = new();
    public List<Dictionary<string, object?>> Data { get; set; } = new();
}
