using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.Services;

public sealed class BoardState : IBoardState
{
    private readonly bool[,] _cells;

    public BoardState(int rows, int columns)
    {
        _cells = new bool[rows, columns];
    }

    private BoardState(bool[,] cells) => _cells = cells;

    public int Rows => _cells.GetLength(0);
    public int Columns => _cells.GetLength(1);

    public bool this[int row, int column] => _cells[row, column];

    public static IBoardState Parse(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0)
            throw new ArgumentException("Board must contain at least one row.");

        var columns = rows[0]?.Length ?? 0;
        if (columns == 0)
            throw new ArgumentException("Board rows must not be empty.");

        var cells = new bool[rows.Count, columns];
        for (var rowIndex = 0; rowIndex < rows.Count; rowIndex++)
        {
            if (rows[rowIndex] is null || rows[rowIndex].Length != columns)
                throw new ArgumentException("All board rows must have the same length.");

            for (var columnIndex = 0; columnIndex < columns; columnIndex++)
            {
                cells[rowIndex, columnIndex] = rows[rowIndex][columnIndex] switch
                {
                    '#' or '1' or 'X' or 'x' or 'O' or 'o' => true,
                    '.' or '0' or '_' or ' ' => false,
                    _ => throw new ArgumentException($"Invalid cell '{rows[rowIndex][columnIndex]}' at row {rowIndex}, column {columnIndex}. Use '#' for alive and '.' for dead.")
                };
            }
        }

        return new BoardState(cells);
    }

    public IBoardState Next()
    {
        var nextCells = new bool[Rows, Columns];
        for (var rowIndex = 0; rowIndex < Rows; rowIndex++)
        {
            for (var columnIndex = 0; columnIndex < Columns; columnIndex++)
            {
                var livingNeighborCount = CountNeighbors(rowIndex, columnIndex);
                nextCells[rowIndex, columnIndex] = this[rowIndex, columnIndex]
                    ? livingNeighborCount is 2 or 3
                    : livingNeighborCount == 3;
            }
        }

        return new BoardState(nextCells);
    }

    public bool IsStableWith(IBoardState other)
    {
        if (Rows != other.Rows || Columns != other.Columns) return false;
        for (var rowIndex = 0; rowIndex < Rows; rowIndex++)
            for (var columnIndex = 0; columnIndex < Columns; columnIndex++)
                if (this[rowIndex, columnIndex] != other[rowIndex, columnIndex]) return false;
        return true;
    }

    public bool IsEmpty
    {
        get
        {
            for (var rowIndex = 0; rowIndex < Rows; rowIndex++)
                for (var columnIndex = 0; columnIndex < Columns; columnIndex++)
                    if (_cells[rowIndex, columnIndex]) return false;
            return true;
        }
    }

    public IReadOnlyList<string> ToRows()
    {
        var rows = new string[Rows];
        for (var rowIndex = 0; rowIndex < Rows; rowIndex++)
        {
            var rowCharacters = new char[Columns];
            for (var columnIndex = 0; columnIndex < Columns; columnIndex++)
                rowCharacters[columnIndex] = _cells[rowIndex, columnIndex] ? '#' : '.';
            rows[rowIndex] = new string(rowCharacters);
        }
        return rows;
    }

    private int CountNeighbors(int row, int column)
    {
        var livingNeighborCount = 0;
        for (var rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (var columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                if (rowOffset == 0 && columnOffset == 0) continue;
                var neighborRow = row + rowOffset;
                var neighborColumn = column + columnOffset;
                if (neighborRow >= 0 && neighborRow < Rows && neighborColumn >= 0 && neighborColumn < Columns && _cells[neighborRow, neighborColumn])
                    livingNeighborCount++;
            }
        }
        return livingNeighborCount;
    }

    public string Fingerprint() => string.Join('\n', ToRows());
}
