using BMSBTRwp_API.Models;
using Microsoft.Data.SqlClient;

namespace BMSBTRwp_API.Services;

public class MeterReadingsStatsService
{
    private readonly string _connectionString;
    private readonly DbTextFileService _dbTextFileService;
    private readonly ILogger<MeterReadingsStatsService> _logger;

    public MeterReadingsStatsService(
        IConfiguration configuration,
        DbTextFileService dbTextFileService,
        ILogger<MeterReadingsStatsService> logger)
    {
        _connectionString = configuration.GetConnectionString("ConnectionBMSBT")
            ?? throw new InvalidOperationException(
                "Connection string 'ConnectionBMSBT' not found. Add it to appsettings.json under ConnectionStrings.");
        _dbTextFileService = dbTextFileService;
        _logger = logger;
    }

    public async Task<MeterReadingsSummary> GetSummaryAsync()
    {
        // Ensure table exists in db.txt and has Project column
        var table = _dbTextFileService.FindTable("MeterReadings")
                    ?? throw new InvalidOperationException("Table 'MeterReadings' not defined in db.txt.");

        if (!table.Columns.Any(c => c.Name.Equals("Project", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException("Column 'Project' not defined for MeterReadings in db.txt.");
        }

        const string sql = @"SELECT
    COUNT(*) AS TotalRecords,
    SUM(CASE WHEN Project LIKE 'Phase 1-6%' THEN 1 ELSE 0 END) AS RecordsPhase1To6,
    SUM(CASE WHEN Project = 'Phase 8' THEN 1 ELSE 0 END) AS RecordsPhase8,
    SUM(CASE WHEN Project = 'Bahria Enclave' THEN 1 ELSE 0 END) AS RecordsBahriaEnclave
FROM dbo.MeterReadings";

        await using var conn = new SqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn);
        await using var reader = await cmd.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new MeterReadingsSummary
            {
                TotalRecords = reader.GetInt32(reader.GetOrdinal("TotalRecords")),
                RecordsPhase1To6 = reader.IsDBNull(reader.GetOrdinal("RecordsPhase1To6")) ? 0 : reader.GetInt32(reader.GetOrdinal("RecordsPhase1To6")),
                RecordsPhase8 = reader.IsDBNull(reader.GetOrdinal("RecordsPhase8")) ? 0 : reader.GetInt32(reader.GetOrdinal("RecordsPhase8")),
                RecordsBahriaEnclave = reader.IsDBNull(reader.GetOrdinal("RecordsBahriaEnclave")) ? 0 : reader.GetInt32(reader.GetOrdinal("RecordsBahriaEnclave"))
            };
        }

        _logger.LogWarning("MeterReadings summary query returned no rows.");
        return new MeterReadingsSummary();
    }
}

