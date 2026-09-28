namespace GameOfLife.Api.Interfaces;

public interface IBoardState
{
    int Rows { get; }
    int Columns { get; }
    bool this[int row, int column] { get; }
    IBoardState Next();
    bool IsStableWith(IBoardState other);
    bool IsEmpty { get; }
    IReadOnlyList<string> ToRows();
    string Fingerprint();
}
