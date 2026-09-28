namespace GameOfLife.Api.Interfaces;

public interface IBoardRepository
{
    Task AddAsync(IBoard board, CancellationToken cancellationToken);
    Task<IBoard?> GetAsync(Guid id, CancellationToken cancellationToken);
}
