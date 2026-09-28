namespace GameOfLife.Api.Interfaces;

public interface IGameOfLifeService
{
    Task<Guid> CreateBoardAsync(ICreateBoardRequest request, CancellationToken cancellationToken);
    Task<IBoardResponse?> GetNextStateAsync(Guid id, CancellationToken cancellationToken);
    Task<IStatesAwayResponse?> GetStatesAwayAsync(Guid id, int generations, CancellationToken cancellationToken);
    Task<IFinalStateResult> GetFinalStateAsync(Guid id, int maxAttempts, CancellationToken cancellationToken);
}
