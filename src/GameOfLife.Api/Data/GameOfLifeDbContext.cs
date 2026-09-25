using GameOfLife.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data;

public sealed class GameOfLifeDbContext(DbContextOptions<GameOfLifeDbContext> options) : DbContext(options)
{
    public DbSet<Board> Boards => Set<Board>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Board>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).ValueGeneratedNever();
            entity.Property(x => x.CreatedUtc).IsRequired();
            entity.Property(x => x.Rows).IsRequired();
            entity.Property(x => x.Columns).IsRequired();
            entity.Property(x => x.State).IsRequired();
            entity.HasIndex(x => x.CreatedUtc);
        });
    }
}
