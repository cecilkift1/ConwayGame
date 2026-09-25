namespace GameOfLife.Api.Services;

public sealed class BoardState
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

    public static BoardState Parse(IReadOnlyList<string> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count == 0)
            throw new ArgumentException("Board must contain at least one row.");

        var columns = rows[0]?.Length ?? 0;
        if (columns == 0)
            throw new ArgumentException("Board rows must not be empty.");

        var cells = new bool[rows.Count, columns];
        for (var r = 0; r < rows.Count; r++)
        {
            if (rows[r] is null || rows[r].Length != columns)
                throw new ArgumentException("All board rows must have the same length.");

            for (var c = 0; c < columns; c++)
            {
                cells[r, c] = rows[r][c] switch
                {
                    '#' or '1' or 'X' or 'x' or 'O' or 'o' => true,
                    '.' or '0' or '_' or ' ' => false,
                    _ => throw new ArgumentException($"Invalid cell '{rows[r][c]}' at row {r}, column {c}. Use '#' for alive and '.' for dead.")
                };
            }
        }

        return new BoardState(cells);
    }

    public BoardState Next()
    {
        var next = new bool[Rows, Columns];
        for (var r = 0; r < Rows; r++)
        {
            for (var c = 0; c < Columns; c++)
            {
                var neighbors = CountNeighbors(r, c);
                next[r, c] = this[r, c]
                    ? neighbors is 2 or 3
                    : neighbors == 3;
            }
        }

        return new BoardState(next);
    }

    public bool IsStableWith(BoardState other)
    {
        if (Rows != other.Rows || Columns != other.Columns) return false;
        for (var r = 0; r < Rows; r++)
            for (var c = 0; c < Columns; c++)
                if (_cells[r, c] != other._cells[r, c]) return false;
        return true;
    }

    public bool IsEmpty
    {
        get
        {
            for (var r = 0; r < Rows; r++)
                for (var c = 0; c < Columns; c++)
                    if (_cells[r, c]) return false;
            return true;
        }
    }

    public IReadOnlyList<string> ToRows()
    {
        var rows = new string[Rows];
        for (var r = 0; r < Rows; r++)
        {
            var chars = new char[Columns];
            for (var c = 0; c < Columns; c++) chars[c] = _cells[r, c] ? '#' : '.';
            rows[r] = new string(chars);
        }
        return rows;
    }

    private int CountNeighbors(int row, int column)
    {
        var count = 0;
        for (var dr = -1; dr <= 1; dr++)
        {
            for (var dc = -1; dc <= 1; dc++)
            {
                if (dr == 0 && dc == 0) continue;
                var r = row + dr;
                var c = column + dc;
                if (r >= 0 && r < Rows && c >= 0 && c < Columns && _cells[r, c]) count++;
            }
        }
        return count;
    }

    public string Fingerprint() => string.Join('\n', ToRows());
}
