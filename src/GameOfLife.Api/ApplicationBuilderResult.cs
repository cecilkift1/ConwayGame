using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api;

public sealed class ApplicationBuilderResult(
    bool succeeded,
    WebApplicationBuilder? builder = null,
    string? error = null) : IApplicationBuilderResult
{
    public bool Succeeded { get; } = succeeded;
    public WebApplicationBuilder? Builder { get; } = builder;
    public string? Error { get; } = error;
}
