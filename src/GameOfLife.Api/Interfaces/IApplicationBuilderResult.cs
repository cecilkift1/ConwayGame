namespace GameOfLife.Api.Interfaces;

public interface IApplicationBuilderResult
{
    bool Succeeded { get; }
    WebApplicationBuilder? Builder { get; }
    string? Error { get; }
}
