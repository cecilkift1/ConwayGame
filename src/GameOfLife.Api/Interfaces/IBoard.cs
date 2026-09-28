namespace GameOfLife.Api.Interfaces;

public interface IBoard
{
    Guid Id { get; }
    int Rows { get; }
    int Columns { get; }
    string State { get; }
    DateTime CreatedUtc { get; }
}
