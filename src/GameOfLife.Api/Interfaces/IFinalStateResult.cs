namespace GameOfLife.Api.Interfaces;

public interface IFinalStateResult
{
    bool Found { get; }
    bool Concluded { get; }
    IFinalStateResponse? Response { get; }
    string? Error { get; }
}
