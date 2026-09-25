using System.Net;
using System.Net.Http.Json;
using GameOfLife.Api.DTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace GameOfLife.Api.Tests;

public sealed class ApiTests : IAsyncLifetime
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"gol-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:GameOfLife"] = $"Data Source={_databasePath}",
                    ["GameOfLife:MaxBoardDimension"] = "100",
                    ["GameOfLife:MaxFinalStateAttempts"] = "1000"
                }));
        });
        _client = _factory.CreateClient();
        return Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        try { File.Delete(_databasePath); } catch { }
        await Task.CompletedTask;
    }

    [Fact]
    public async Task Upload_next_and_states_endpoints_work()
    {
        var create = await _client.PostAsJsonAsync("/api/boards", new CreateBoardRequest([
            ".....",
            "..#..",
            "..#..",
            "..#..",
            "....."]));

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        Assert.NotEqual(Guid.Empty, created!.Id);

        var next = await _client.GetFromJsonAsync<BoardResponse>($"/api/boards/{created.Id}/next");
        Assert.Equal([".....", ".....", ".###.", ".....", "....."], next!.Rows);

        var two = await _client.GetFromJsonAsync<StatesAwayResponse>($"/api/boards/{created.Id}/states/2");
        Assert.Equal([".....", "..#..", "..#..", "..#..", "....."], two!.Rows);
        Assert.Equal(2, two.Generation);
    }

    [Fact]
    public async Task Final_state_returns_stable_state()
    {
        var create = await _client.PostAsJsonAsync("/api/boards", new CreateBoardRequest(["#"]));
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();

        var response = await _client.GetFromJsonAsync<FinalStateResponse>($"/api/boards/{created!.Id}/final?maxAttempts=10");

        Assert.True(response!.IsEmpty);
        Assert.Equal(1, response.Attempts);
    }

    [Fact]
    public async Task Final_state_reports_cycle()
    {
        var create = await _client.PostAsJsonAsync("/api/boards", new CreateBoardRequest([
            ".....",
            ".....",
            ".###.",
            ".....",
            "....."]));
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}/final?maxAttempts=10");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Final_state_honors_attempt_limit()
    {
        var create = await _client.PostAsJsonAsync("/api/boards", new CreateBoardRequest([
            ".....",
            ".....",
            ".###.",
            ".....",
            "....."]));
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}/final?maxAttempts=1");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Missing_board_returns_404()
    {
        var response = await _client.GetAsync($"/api/boards/{Guid.NewGuid()}/next");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Board_survives_application_restart()
    {
        var create = await _client.PostAsJsonAsync("/api/boards", new CreateBoardRequest([
            "...",
            ".#.",
            "..."]));
        var created = await create.Content.ReadFromJsonAsync<IdResponse>();
        _client.Dispose();
        _factory.Dispose();

        using var restarted = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:GameOfLife"] = $"Data Source={_databasePath}"
                }));
        });
        using var client = restarted.CreateClient();

        var response = await client.GetAsync($"/api/boards/{created!.Id}/next");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Invalid_board_is_rejected()
    {
        var response = await _client.PostAsJsonAsync("/api/boards", new CreateBoardRequest([
            "...",
            ".."]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private sealed record IdResponse(Guid Id);
}
