using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.Services;

public sealed class FinalStateResult(
    bool found,
    bool concluded,
    IFinalStateResponse? response,
    string? error) : IFinalStateResult
{
    public bool Found { get; } = found;
    public bool Concluded { get; } = concluded;
    public IFinalStateResponse? Response { get; } = response;
    public string? Error { get; } = error;
}
