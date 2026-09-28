using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.Services;

public sealed class GameOfLifeEngine : IGameOfLifeEngine
{
    public IBoardState Next(IBoardState state) => state.Next();
}
