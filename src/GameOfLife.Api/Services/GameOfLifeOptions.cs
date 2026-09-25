namespace GameOfLife.Api.Services;

public sealed class GameOfLifeOptions
{
    public int MaxBoardDimension { get; init; } = 500;
    public int MaxFinalStateAttempts { get; init; } = 10_000;
}
