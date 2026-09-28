namespace GameOfLife.Api.Interfaces;

public interface ICreateBoardRequest
{
    IReadOnlyList<string> Rows { get; }
}
