using GameOfLife.Api.Data;
using GameOfLife.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Services;

public sealed class BoardRepository(GameOfLifeDbContext db) : IBoardRepository
{
    public async Task AddAsync(Board board, CancellationToken cancellationToken)
    {
        db.Boards.Add(board);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<Board?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        db.Boards.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
