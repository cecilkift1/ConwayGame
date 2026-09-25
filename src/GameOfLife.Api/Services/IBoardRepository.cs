using GameOfLife.Api.Domain;

namespace GameOfLife.Api.Services;

public interface IBoardRepository
{
    Task AddAsync(Board board, CancellationToken cancellationToken);
    Task<Board?> GetAsync(Guid id, CancellationToken cancellationToken);
}
