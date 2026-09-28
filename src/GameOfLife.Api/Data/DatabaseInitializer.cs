using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.Data;

public static class DatabaseInitializer
{
    public static async Task<IDatabaseInitializationResult> InitializeAsync(
        IServiceProvider services,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        var dataDirectoryName = services.GetRequiredService<IConfiguration>()["DataDirectory"];
        if (string.IsNullOrWhiteSpace(dataDirectoryName))
        {
            return CreateDatabaseInitializationResult(logger, false, "Configuration value 'DataDirectory' is required.");
        }

        var dataDirectory = CreateDataDirectory(logger, dataDirectoryName);
        if (!dataDirectory.Succeeded)
        {
            return dataDirectory;
        }
      
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GameOfLifeDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            return CreateDatabaseInitializationResult(logger, true, "The database was created successfully.");
        }
        catch (Exception ex)
        {
            return CreateDatabaseInitializationResult(logger, false, $"Failed to create the database: {ex.Message}");
        }
    }

    private static IDatabaseInitializationResult CreateDatabaseInitializationResult(ILogger logger, bool succeeded, string message)
    {
        if (succeeded)
        {
            logger.LogInformation(message);
        }
        else
        {
            logger.LogError(message);
        }
        
        return new DatabaseInitializationResult(succeeded, message);
    }

    private static IDatabaseInitializationResult CreateDataDirectory(ILogger logger, string dataDirectoryName)
    {
        var dataDirectory = Path.GetFullPath(dataDirectoryName);

        if (Directory.Exists(dataDirectory))
        {
            return CreateDatabaseInitializationResult(logger, true, $"The database directory '{dataDirectory}' already exists.");
        }

        DirectoryInfo directoryInformation;
        try
        {
            directoryInformation = Directory.CreateDirectory(dataDirectory);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create database directory {DataDirectory}", dataDirectory);
            return CreateDatabaseInitializationResult(logger, false, $"Failed to create database directory '{dataDirectory}': {ex.Message}");
        }

        if (!directoryInformation.Exists)
        {
            return CreateDatabaseInitializationResult(logger, false, $"The database directory '{dataDirectory}' does not exist.");
        }

        return CreateDatabaseInitializationResult(logger, true, $"The database directory '{dataDirectory}' was created successfully.");
    }
}
