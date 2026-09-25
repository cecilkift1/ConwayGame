using GameOfLife.Api.Services;

namespace GameOfLife.Api.Tests;

public sealed class GameOfLifeEngineTests
{
    private readonly GameOfLifeEngine _engine = new();

    [Fact]
    public void Blinker_rotates_one_generation()
    {
        var state = BoardState.Parse([
            ".....",
            ".....",
            ".###.",
            ".....",
            "....."]);

        var next = _engine.Next(state);

        Assert.Equal([
            ".....",
            "..#..",
            "..#..",
            "..#..",
            "....."], next.ToRows());
    }

    [Fact]
    public void Underpopulation_kills_cell()
    {
        var state = BoardState.Parse([
            "#..",
            "...",
            "..."]);

        Assert.False(_engine.Next(state)[0, 0]);
    }

    [Fact]
    public void Birth_occurs_with_three_neighbors()
    {
        var state = BoardState.Parse([
            "##.",
            "#..",
            "..."]);

        Assert.True(_engine.Next(state)[1, 1]);
    }

    [Fact]
    public void Overpopulation_kills_cell()
    {
        var state = BoardState.Parse([
            "###",
            "###",
            "..."]);

        Assert.False(_engine.Next(state)[1, 1]);
    }
}
