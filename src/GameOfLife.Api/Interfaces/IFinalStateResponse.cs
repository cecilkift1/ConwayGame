namespace GameOfLife.Api.Interfaces;

public interface IFinalStateResponse
{
    Guid Id { get; }
    IReadOnlyList<string> Rows { get; }
    long Generation { get; }
    int Attempts { get; }
    bool IsStable { get; }
    bool IsEmpty { get; }
}
