namespace GameOfLife.Api.Services;

public sealed class GameOfLifeEngine : IGameOfLifeEngine
{
    public BoardState Next(BoardState state) => state.Next();
}
