using Microsoft.Data.SqlClient;
using BMSBTRwp_API.Models;

namespace BMSBTRwp_API.Services;

/// <summary>
/// Uses ADO.NET to fetch paginated data from SQL Server.
/// ADO.NET is chosen over EF Core because the table structures are dynamic
/// (discovered at runtime from db.txt) — no compile-time DbContext models needed.
/// </summary>
public class DataFetchService
{
    // ─── CONNECTION STRING ───────────────────────────────────────────────
    // Pulled from appsettings.json → ConnectionStrings:ConnectionBMSBT
    // To change it, edit appsettings.json or set the environment variable:
    //   ConnectionStrings__ConnectionBMSBT=Server=...;Database=...;...
    // ─────────────────────────────────────────────────────────────────────
    private readonly string _connectionString;
    private readonly ILogger<DataFetchService> _logger;

    public DataFetchService(IConfiguration config, ILogger<DataFetchService> logger)
    {
        _connectionString = config.GetConnectionString("ConnectionBMSBT")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionBMSBT' not found. " +
                "Add it to appsettings.json under ConnectionStrings.");
        _logger = logger;
    }

    /// <summary>
    /// Fetches a page of rows from the given table.
    /// </summary>
    /// <param name="table">Table metadata parsed from db.txt</param>
    /// <param name="page">1-based page number</param>
    /// <param name="pageSize">
    /// Number of rows per page. Default 1000.
    /// Adjust this based on your React Native SQLite batch insert capacity.
    /// Smaller values = more requests but less memory per request.
    /// Larger values  = fewer requests but heavier payloads.
    /// </param>
    public async Task<PagedResult> FetchPageAsync(TableInfo table, int page, int pageSize)
    {
        var result = new PagedResult
        {
            TableName = table.TableName,
            Page = page,
            PageSize = pageSize,
            Columns = table.Columns.Select(c => c.Name).ToList()
        };

        var columnList = string.Join(", ",
            table.Columns.Select(c => $"[{c.Name}]"));

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        result.TotalRecords = await GetRowCountAsync(conn, table);
        result.TotalPages = (int)Math.Ceiling((double)result.TotalRecords / pageSize);

        // OFFSET/FETCH requires an ORDER BY — use the first column (typically the PK)
        var orderColumn = $"[{table.Columns[0].Name}]";
        var offset = (page - 1) * pageSize;

        var sql = $"""
            SELECT {columnList}
            FROM {table.FullName}
            ORDER BY {orderColumn}
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@Offset", offset);
        cmd.Parameters.AddWithValue("@PageSize", pageSize);

        _logger.LogInformation(
            "Fetching {Table} — page {Page}, pageSize {Size}, offset {Offset}",
            table.TableName, page, pageSize, offset);

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            result.Data.Add(row);
        }

        return result;
    }

    private static async Task<int> GetRowCountAsync(SqlConnection conn, TableInfo table)
    {
        var sql = $"SELECT COUNT(*) FROM {table.FullName}";
        await using var cmd = new SqlCommand(sql, conn);
        return (int)(await cmd.ExecuteScalarAsync())!;
    }
}
