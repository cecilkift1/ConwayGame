namespace GameOfLife.Api.Interfaces;

public interface IGameOfLifeEngine
{
    IBoardState Next(IBoardState state);
}
