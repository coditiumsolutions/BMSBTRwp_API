using BMSBTRwp_API.Models;
using BMSBTRwp_API.Services;
using Microsoft.AspNetCore.Mvc;

namespace BMSBTRwp_API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OperatorsController : ControllerBase
{
    private readonly OperatorsRepository _repository;
    private readonly ILogger<OperatorsController> _logger;

    public OperatorsController(OperatorsRepository repository, ILogger<OperatorsController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Returns all Operators records.
    /// This is your first API to fetch all rows from dbo.Operators.
    /// GET /api/operators
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<Operator>>> GetAll()
    {
        try
        {
            var ops = await _repository.GetAllAsync();
            return Ok(ops);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching all operators");
            return StatusCode(500, new { error = "Failed to fetch operators." });
        }
    }

    /// <summary>
    /// Gets a single operator by Id.
    /// GET /api/operators/{id}
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Operator>> GetById(int id)
    {
        try
        {
            var op = await _repository.GetByIdAsync(id);
            if (op == null)
            {
                return NotFound(new { error = $"Operator with Id {id} not found." });
            }

            return Ok(op);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while fetching operator {Id}", id);
            return StatusCode(500, new { error = "Failed to fetch operator." });
        }
    }

    /// <summary>
    /// Creates a new operator.
    /// POST /api/operators
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Operator>> Create([FromBody] Operator op)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var newId = await _repository.CreateAsync(op);
            var created = await _repository.GetByIdAsync(newId);
            return CreatedAtAction(nameof(GetById), new { id = newId }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while creating operator");
            return StatusCode(500, new { error = "Failed to create operator." });
        }
    }

    /// <summary>
    /// Updates an existing operator.
    /// PUT /api/operators/{id}
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Operator op)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var exists = await _repository.GetByIdAsync(id);
            if (exists == null)
            {
                return NotFound(new { error = $"Operator with Id {id} not found." });
            }

            var ok = await _repository.UpdateAsync(id, op);
            if (!ok)
            {
                return StatusCode(500, new { error = "Update failed (no rows affected)." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while updating operator {Id}", id);
            return StatusCode(500, new { error = "Failed to update operator." });
        }
    }

    /// <summary>
    /// Deletes an operator.
    /// DELETE /api/operators/{id}
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var exists = await _repository.GetByIdAsync(id);
            if (exists == null)
            {
                return NotFound(new { error = $"Operator with Id {id} not found." });
            }

            var ok = await _repository.DeleteAsync(id);
            if (!ok)
            {
                return StatusCode(500, new { error = "Delete failed (no rows affected)." });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while deleting operator {Id}", id);
            return StatusCode(500, new { error = "Failed to delete operator." });
        }
    }
}

