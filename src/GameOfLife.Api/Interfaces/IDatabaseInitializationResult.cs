namespace GameOfLife.Api.Interfaces;

public interface IDatabaseInitializationResult
{
    bool Succeeded { get; }
    string? Error { get; }
}
