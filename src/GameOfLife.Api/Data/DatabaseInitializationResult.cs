using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.Data;

public sealed class DatabaseInitializationResult(bool succeeded, string? error = null) : IDatabaseInitializationResult
{
    public bool Succeeded { get; } = succeeded;
    public string? Error { get; } = error;
}
