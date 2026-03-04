using Microsoft.AspNetCore.Mvc;
using BMSBTRwp_API.Services;

namespace BMSBTRwp_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly DbTextFileService _dbTextService;
    private readonly DataFetchService _dataFetchService;
    private readonly ILogger<DataController> _logger;

    public DataController(
        DbTextFileService dbTextService,
        DataFetchService dataFetchService,
        ILogger<DataController> logger)
    {
        _dbTextService = dbTextService;
        _dataFetchService = dataFetchService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the list of tables defined in db.txt.
    /// Useful for React Native clients to know which tables to sync.
    /// GET /api/data/tables
    /// </summary>
    [HttpGet("tables")]
    public IActionResult GetTables()
    {
        try
        {
            var tables = _dbTextService.GetTables();
            var summary = tables.Select(t => new
            {
                t.Schema,
                t.TableName,
                ColumnCount = t.Columns.Count,
                Columns = t.Columns.Select(c => c.Name)
            });
            return Ok(summary);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "db.txt not found");
            return NotFound(new { error = "db.txt file not found on server." });
        }
    }

    /// <summary>
    /// Fetches paginated data from a table listed in db.txt.
    /// GET /api/data/{tableName}?page=1&pageSize=1000
    ///
    /// PAGE SIZE GUIDE (adjust for your React Native SQLite inserts):
    ///   500  — lightweight, good for slow networks
    ///  1000  — balanced default
    ///  5000  — fast networks / large tables
    /// </summary>
    [HttpGet("{tableName}")]
    public async Task<IActionResult> GetData(
        string tableName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 1000)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 1;
        if (pageSize > 10000) pageSize = 10000; // safety cap

        try
        {
            var table = _dbTextService.FindTable(tableName);
            if (table == null)
            {
                return NotFound(new
                {
                    error = $"Table '{tableName}' not found in db.txt.",
                    availableTables = _dbTextService.GetTables().Select(t => t.TableName)
                });
            }

            var result = await _dataFetchService.FetchPageAsync(table, page, pageSize);
            return Ok(result);
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogError(ex, "db.txt not found");
            return NotFound(new { error = "db.txt file not found on server." });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            _logger.LogError(ex, "SQL Server error while fetching {Table}", tableName);
            return StatusCode(500, new
            {
                error = "Database error occurred.",
                detail = ex.Message
            });
        }
    }
}
