using System.Data;
using BMSBTRwp_API.Models;
using Microsoft.Data.SqlClient;

namespace BMSBTRwp_API.Services;

/// <summary>
/// Simple ADO.NET repository that reads all rows from dbo.MultiValueConfigurations.
/// It validates the table definition from db.txt via DbTextFileService.
/// </summary>
public class MultiValueConfigurationsRepository
{
    private const string TableName = "MultiValueConfigurations";

    private readonly string _connectionString;
    private readonly DbTextFileService _dbTextFileService;
    private readonly ILogger<MultiValueConfigurationsRepository> _logger;

    public MultiValueConfigurationsRepository(
        IConfiguration configuration,
        DbTextFileService dbTextFileService,
        ILogger<MultiValueConfigurationsRepository> logger)
    {
        _connectionString = configuration.GetConnectionString("ConnectionBMSBT")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionBMSBT' not found. " +
                "Add it to appsettings.json under ConnectionStrings.");

        _dbTextFileService = dbTextFileService;
        _logger = logger;
    }

    private TableInfo GetTableInfo()
    {
        var table = _dbTextFileService.FindTable(TableName);
        if (table is null)
        {
            throw new InvalidOperationException($"Table '{TableName}' not defined in db.txt.");
        }

        return table;
    }

    public async Task<List<MultiValueConfiguration>> GetAllAsync()
    {
        GetTableInfo(); // validate presence in db.txt

        var list = new List<MultiValueConfiguration>();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            SELECT uid, ConfigKey, ConfigValue
            FROM dbo.MultiValueConfigurations
            """;

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);

        while (await reader.ReadAsync())
        {
            list.Add(new MultiValueConfiguration
            {
                Uid = reader.GetInt32(reader.GetOrdinal("uid")),
                ConfigKey = reader.GetString(reader.GetOrdinal("ConfigKey")),
                ConfigValue = reader.GetString(reader.GetOrdinal("ConfigValue"))
            });
        }

        _logger.LogInformation("Fetched {Count} MultiValueConfigurations rows", list.Count);
        return list;
    }

    public async Task<int> CreateAsync(MultiValueConfiguration item)
    {
        GetTableInfo();

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = """
            INSERT INTO dbo.MultiValueConfigurations (ConfigKey, ConfigValue)
            OUTPUT INSERTED.uid
            VALUES (@ConfigKey, @ConfigValue)
            """;

        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@ConfigKey", item.ConfigKey);
        cmd.Parameters.AddWithValue("@ConfigValue", item.ConfigValue);

        var insertedId = (int)(await cmd.ExecuteScalarAsync())!;
        _logger.LogInformation("Created MultiValueConfiguration with uid {Id}", insertedId);
        return insertedId;
    }
}

