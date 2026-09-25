using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameOfLifeDbContext>();
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }
}
