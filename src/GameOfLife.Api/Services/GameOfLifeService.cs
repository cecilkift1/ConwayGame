using GameOfLife.Api.Domain;
using GameOfLife.Api.DTOs;
using GameOfLife.Api.Interfaces;
using Microsoft.Extensions.Options;

namespace GameOfLife.Api.Services;

public sealed class GameOfLifeService(
    IBoardRepository repository,
    IGameOfLifeEngine engine,
    IOptions<GameOfLifeOptions> options) : IGameOfLifeService
{
    private readonly GameOfLifeOptions _options = options.Value;

    public async Task<Guid> CreateBoardAsync(ICreateBoardRequest request, CancellationToken cancellationToken)
    {
        ValidateDimensions(request.Rows);
        var state = BoardState.Parse(request.Rows);

        var board = new Board
        {
            Id = Guid.NewGuid(),
            Rows = state.Rows,
            Columns = state.Columns,
            State = string.Join('\n', state.ToRows()),
            CreatedUtc = DateTime.UtcNow
        };

        await repository.AddAsync(board, cancellationToken);
        return board.Id;
    }

    public async Task<IBoardResponse?> GetNextStateAsync(Guid id, CancellationToken cancellationToken)
    {
        var board = await repository.GetAsync(id, cancellationToken);
        if (board is null) return null;

        var current = ParseStored(board);
        var next = engine.Next(current);
        return ToResponse(id, next, 1, current.IsStableWith(next));
    }

    public async Task<IStatesAwayResponse?> GetStatesAwayAsync(Guid id, int generations, CancellationToken cancellationToken)
    {
        if (generations < 0) throw new ArgumentOutOfRangeException(nameof(generations));
        var board = await repository.GetAsync(id, cancellationToken);
        if (board is null) return null;

        var state = ParseStored(board);
        for (var generation = 0; generation < generations; generation++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            state = engine.Next(state);
        }

        var stable = state.IsStableWith(engine.Next(state));
        return new StatesAwayResponse(id, state.ToRows(), generations, generations, stable, state.IsEmpty);
    }

    public async Task<IFinalStateResult> GetFinalStateAsync(Guid id, int maxAttempts, CancellationToken cancellationToken)
    {
        if (maxAttempts <= 0 || maxAttempts > _options.MaxFinalStateAttempts)
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));

        var board = await repository.GetAsync(id, cancellationToken);
        if (board is null) return new FinalStateResult(false, false, null, null);

        var current = ParseStored(board);
        if (current.IsEmpty)
        {
            return new FinalStateResult(true, true, new FinalStateResponse(id, current.ToRows(), 0, 0, true, true), null);
        }

        var seen = new HashSet<string>(StringComparer.Ordinal) { current.Fingerprint() };
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var next = engine.Next(current);

            if (current.IsStableWith(next) || next.IsEmpty)
            {
                return new FinalStateResult(true, true, new FinalStateResponse(id, next.ToRows(), attempt, attempt, true, next.IsEmpty), null);
            }

            if (!seen.Add(next.Fingerprint()))
            {
                return new FinalStateResult(true, false, null, "The board entered a cycle and therefore has no final stable state.");
            }

            current = next;
        }

        return new FinalStateResult(true, false, null, $"The board did not reach a stable or empty state within {maxAttempts} attempts.");
    }

    private static IBoardState ParseStored(IBoard board) =>
        BoardState.Parse(board.State.Split('\n'));

    private void ValidateDimensions(IReadOnlyList<string> rows)
    {
        if (rows is null || rows.Count == 0)
            throw new ArgumentException("Board must contain at least one row.");
        if (rows.Count > _options.MaxBoardDimension)
            throw new ArgumentException($"Board cannot exceed {_options.MaxBoardDimension} rows.");
        if (rows[0] is null || rows[0].Length == 0)
            throw new ArgumentException("Board must contain at least one column.");
        if (rows[0].Length > _options.MaxBoardDimension)
            throw new ArgumentException($"Board cannot exceed {_options.MaxBoardDimension} columns.");
    }

    private static IBoardResponse ToResponse(Guid id, IBoardState state, int generation, bool stable) =>
        new BoardResponse(id, state.ToRows(), generation, stable, state.IsEmpty);
}
