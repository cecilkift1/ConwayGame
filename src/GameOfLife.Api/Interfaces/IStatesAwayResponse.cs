namespace GameOfLife.Api.Interfaces;

public interface IStatesAwayResponse
{
    Guid Id { get; }
    IReadOnlyList<string> Rows { get; }
    long Generation { get; }
    int RequestedGenerations { get; }
    bool IsStable { get; }
    bool IsEmpty { get; }
}
