using System.Text.RegularExpressions;
using BMSBTRwp_API.Models;

namespace BMSBTRwp_API.Services;

/// <summary>
/// Reads and parses db.txt to discover which tables/columns the API is allowed to serve.
/// The file is re-read on each request so you can update it without restarting the API.
/// </summary>
public class DbTextFileService
{
    private readonly string _filePath;
    private readonly ILogger<DbTextFileService> _logger;

    // Matches lines like:  TABLE: dbo.MeterReadings   (27 columns)
    private static readonly Regex TableHeaderRegex =
        new(@"TABLE:\s+(?:(\w+)\.)?(\w+)", RegexOptions.Compiled);

    // Matches column rows like:   1  Uid   int   —   NO   —
    private static readonly Regex ColumnRowRegex =
        new(@"^\s*\d+\s{2,}(\S+)\s{2,}(\S+)\s{2,}(\S+)\s{2,}(YES|NO)\s{2,}", RegexOptions.Compiled);

    public DbTextFileService(IWebHostEnvironment env, ILogger<DbTextFileService> logger)
    {
        _filePath = Path.Combine(env.ContentRootPath, "db.txt");
        _logger = logger;
    }

    public List<TableInfo> GetTables()
    {
        if (!File.Exists(_filePath))
            throw new FileNotFoundException($"db.txt not found at {_filePath}");

        var lines = File.ReadAllLines(_filePath);
        var tables = new List<TableInfo>();
        TableInfo? current = null;

        foreach (var line in lines)
        {
            var headerMatch = TableHeaderRegex.Match(line);
            if (headerMatch.Success)
            {
                current = new TableInfo
                {
                    Schema = headerMatch.Groups[1].Success ? headerMatch.Groups[1].Value : "dbo",
                    TableName = headerMatch.Groups[2].Value
                };
                tables.Add(current);
                continue;
            }

            if (current == null) continue;

            var colMatch = ColumnRowRegex.Match(line);
            if (colMatch.Success)
            {
                current.Columns.Add(new ColumnInfo
                {
                    Name = colMatch.Groups[1].Value,
                    DataType = colMatch.Groups[2].Value,
                    MaxLength = colMatch.Groups[3].Value == "—" ? null : colMatch.Groups[3].Value,
                    IsNullable = colMatch.Groups[4].Value == "YES"
                });
            }
        }

        _logger.LogInformation("Parsed {Count} table(s) from db.txt", tables.Count);
        return tables;
    }

    public TableInfo? FindTable(string tableName)
    {
        return GetTables().FirstOrDefault(t =>
            t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase));
    }
}
