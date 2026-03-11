using BMSBTRwp_API.Models;
using BMSBTRwp_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BMSBTRwp_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MultiValueConfigurationsController : ControllerBase
{
    private readonly MultiValueConfigurationsRepository _repository;
    private readonly ILogger<MultiValueConfigurationsController> _logger;

    public MultiValueConfigurationsController(
        MultiValueConfigurationsRepository repository,
        ILogger<MultiValueConfigurationsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Returns all records from dbo.MultiValueConfigurations.
    /// This uses db.txt to validate that the table exists and is allowed.
    /// GET /api/multivalueconfigurations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<MultiValueConfiguration>>> GetAll()
    {
        try
        {
            var rows = await _repository.GetAllAsync();
            return Ok(rows);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration/db.txt error while fetching MultiValueConfigurations");
            return BadRequest(new { error = ex.Message });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            _logger.LogError(ex, "SQL error while fetching MultiValueConfigurations");
            return StatusCode(500, new { error = "Database error.", detail = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new configuration row in dbo.MultiValueConfigurations.
    /// POST /api/multivalueconfigurations
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MultiValueConfiguration>> Create([FromBody] MultiValueConfiguration item)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var uid = await _repository.CreateAsync(item);
            item.Uid = uid;
            return CreatedAtAction(nameof(GetAll), new { id = uid }, item);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration/db.txt error while creating MultiValueConfiguration");
            return BadRequest(new { error = ex.Message });
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            _logger.LogError(ex, "SQL error while creating MultiValueConfiguration");
            return StatusCode(500, new { error = "Database error.", detail = ex.Message });
        }
    }
}

