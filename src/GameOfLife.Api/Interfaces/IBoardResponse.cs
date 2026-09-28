namespace GameOfLife.Api.Interfaces;

public interface IBoardResponse
{
    Guid Id { get; }
    IReadOnlyList<string> Rows { get; }
    long Generation { get; }
    bool IsStable { get; }
    bool IsEmpty { get; }
}
