using GameOfLife.Api.DTOs;
using GameOfLife.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.Api.Controllers;

[ApiController]
[Route("api/boards")]
public sealed class BoardsController(IGameOfLifeService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBoardRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var id = await service.CreateBoardAsync(request, cancellationToken);
            return CreatedAtAction(nameof(GetNextState), new { id }, new { id });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id:guid}/next")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextState(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetNextStateAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("{id:guid}/states/{generations:int}")]
    [ProducesResponseType(typeof(StatesAwayResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStates(Guid id, int generations, CancellationToken cancellationToken)
    {
        if (generations < 0) return BadRequest(new ErrorResponse("Generations must be zero or greater."));
        try
        {
            var result = await service.GetStatesAwayAsync(id, generations, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new ErrorResponse(ex.Message));
        }
    }

    [HttpGet("{id:guid}/final")]
    [ProducesResponseType(typeof(FinalStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinalState(
        Guid id,
        [FromQuery] int maxAttempts = 1000,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await service.GetFinalStateAsync(id, maxAttempts, cancellationToken);
            if (!result.Found) return NotFound();
            if (!result.Concluded) return UnprocessableEntity(new ErrorResponse(result.Error!));
            return Ok(result.Response);
        }
        catch (ArgumentOutOfRangeException)
        {
            return BadRequest(new ErrorResponse("maxAttempts must be positive and within the configured limit."));
        }
    }
}
