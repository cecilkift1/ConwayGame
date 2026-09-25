using GameOfLife.Api.DTOs;

namespace GameOfLife.Api.Services;

public interface IGameOfLifeService
{
    Task<Guid> CreateBoardAsync(CreateBoardRequest request, CancellationToken cancellationToken);
    Task<BoardResponse?> GetNextStateAsync(Guid id, CancellationToken cancellationToken);
    Task<StatesAwayResponse?> GetStatesAwayAsync(Guid id, int generations, CancellationToken cancellationToken);
    Task<FinalStateResult> GetFinalStateAsync(Guid id, int maxAttempts, CancellationToken cancellationToken);
}

public sealed record FinalStateResult(
    bool Found,
    bool Concluded,
    FinalStateResponse? Response,
    string? Error);
