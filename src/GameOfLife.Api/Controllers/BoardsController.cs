using GameOfLife.Api.DTOs;
using GameOfLife.Api.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GameOfLife.Api.Controllers;

/// <summary>Create a Game of Life board and advance it.</summary>
[ApiController]
[Route("api/boards")]
public sealed class BoardsController(IGameOfLifeService service) : ControllerBase
{
    /// <summary>Stores a new board.</summary>
    /// <remarks>
    /// Each string in rows is one row of the board. Use # for a living cell and . for a dead cell.
    /// Every row must have the same length. The response id is used by the other board calls.
    /// </remarks>
    /// <response code="201">The board was stored. The body contains its id.</response>
    /// <response code="400">The board is missing, empty, uneven, or contains a character other than a living or dead cell.</response>
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

    /// <summary>Returns the board one generation later.</summary>
    /// <param name="id">Id returned when the board was stored.</param>
    /// <param name="cancellationToken">Stops the call when the client disconnects.</param>
    /// <remarks>Applies Conway's rules once. The stored board is left unchanged.</remarks>
    /// <response code="200">The next generation, including whether that generation is stable or empty.</response>
    /// <response code="404">No board exists with this id.</response>
    [HttpGet("{id:guid}/next")]
    [ProducesResponseType(typeof(BoardResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextState(Guid id, CancellationToken cancellationToken)
    {
        var result = await service.GetNextStateAsync(id, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Returns the board after a chosen number of generations.</summary>
    /// <param name="id">Id returned when the board was stored.</param>
    /// <param name="generations">How many generations to advance. Zero returns the stored board.</param>
    /// <param name="cancellationToken">Stops the call when the client disconnects.</param>
    /// <remarks>The stored board is left unchanged.</remarks>
    /// <response code="200">The board after the requested number of generations.</response>
    /// <response code="400">Generations is negative.</response>
    /// <response code="404">No board exists with this id.</response>
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

    /// <summary>Runs the board until it is stable or empty.</summary>
    /// <param name="id">Id returned when the board was stored.</param>
    /// <param name="maxAttempts">Maximum generations to simulate. Must be from 1 through the configured limit. Defaults to 1000.</param>
    /// <param name="cancellationToken">Stops the call when the client disconnects.</param>
    /// <remarks>
    /// A stable board does not change on the next generation. An empty board has no living cells.
    /// If the pattern loops, or the attempt limit is reached first, the call reports that no final state was found.
    /// The stored board is left unchanged.
    /// </remarks>
    /// <response code="200">A stable or empty board, including how many generations were simulated.</response>
    /// <response code="400">maxAttempts is not positive or is above the configured limit.</response>
    /// <response code="404">No board exists with this id.</response>
    /// <response code="422">The board entered a repeating cycle or did not settle within maxAttempts.</response>
    [HttpGet("{id:guid}/final")]
    [ProducesResponseType(typeof(FinalStateResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
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
