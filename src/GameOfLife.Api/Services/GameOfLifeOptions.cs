namespace GameOfLife.Api.Services;

public sealed class GameOfLifeOptions
{
    public int MaxBoardDimension { get; init; } = 50;
    public int MaxFinalStateAttempts { get; init; } = 100;
}
