using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api.Data;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<GameOfLifeDbContext>();
        var connection = db.Database.GetConnectionString();
        EnsureSqliteDirectoryExists(connection);
        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    private static void EnsureSqliteDirectoryExists(string? connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString)) return;

        const string prefix = "Data Source=";
        var idx = connectionString.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
        if (idx < 0) return;

        var path = connectionString[(idx + prefix.Length)..].Trim().Trim('"');
        var semicolon = path.IndexOf(';');
        if (semicolon >= 0) path = path[..semicolon];

        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
