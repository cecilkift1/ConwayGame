using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.Domain;

public sealed class Board : IBoard
{
    public Guid Id { get; set; }
    public int Rows { get; set; }
    public int Columns { get; set; }
    public string State { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
