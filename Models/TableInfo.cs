namespace BMSBTRwp_API.Models;

public class TableInfo
{
    public string Schema { get; set; } = "dbo";
    public string TableName { get; set; } = string.Empty;
    public List<ColumnInfo> Columns { get; set; } = new();

    public string FullName => $"[{Schema}].[{TableName}]";
}

public class ColumnInfo
{
    public string Name { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? MaxLength { get; set; }
    public bool IsNullable { get; set; }
}
