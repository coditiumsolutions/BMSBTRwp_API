using BMSBTRwp_API.Models;
using BMSBTRwp_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BMSBTRwp_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeterReadingsController : ControllerBase
{
    private readonly MeterReadingsStatsService _statsService;
    private readonly ILogger<MeterReadingsController> _logger;

    public MeterReadingsController(
        MeterReadingsStatsService statsService,
        ILogger<MeterReadingsController> logger)
    {
        _statsService = statsService;
        _logger = logger;
    }

    /// <summary>
    /// Returns total counts for MeterReadings, including breakdown by Project.
    /// TotalRecords, RecordsPhase1To6, RecordsPhase8, RecordsBahriaEnclave.
    /// GET /api/meterreadings/summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<MeterReadingsSummary>> GetSummary()
    {
        try
        {
            var summary = await _statsService.GetSummaryAsync();
            return Ok(summary);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error while getting MeterReadings summary");
            return BadRequest(new { error = ex.Message });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            _logger.LogError(ex, "SQL error while getting MeterReadings summary");
            return StatusCode(500, new { error = "Database error.", detail = ex.Message });
        }
    }
}

