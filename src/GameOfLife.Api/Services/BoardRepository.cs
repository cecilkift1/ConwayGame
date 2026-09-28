using GameOfLife.Api.Data;
using GameOfLife.Api.Domain;
using GameOfLife.Api.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Services;

public sealed class BoardRepository(GameOfLifeDbContext db) : IBoardRepository
{
    public async Task AddAsync(IBoard board, CancellationToken cancellationToken)
    {
        if (board is not Board entity)
        {
            throw new ArgumentException("A board must be a Board entity to be stored.", nameof(board));
        }

        db.Boards.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IBoard?> GetAsync(Guid id, CancellationToken cancellationToken) =>
        await db.Boards.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
