namespace GameOfLife.Api.Services;

public interface IGameOfLifeEngine
{
    BoardState Next(BoardState state);
}
