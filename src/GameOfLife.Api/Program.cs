using GameOfLife.Api.Data;
using GameOfLife.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var extraOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri)) return false;
                if (uri.Host is "localhost" or "127.0.0.1") return true;
                if (uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)) return true;
                return extraOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
            })
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("GameOfLife")
    ?? "Data Source=data/gameoflife.db";

builder.Services.AddDbContext<GameOfLifeDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddSingleton<IGameOfLifeEngine, GameOfLifeEngine>();
builder.Services.Configure<GameOfLifeOptions>(builder.Configuration.GetSection("GameOfLife"));
builder.Services.AddScoped<IGameOfLifeService, GameOfLifeService>();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

app.UseSwagger();
app.UseSwaggerUI();

if (!string.IsNullOrEmpty(app.Configuration["ASPNETCORE_HTTPS_PORTS"])
    || !string.IsNullOrEmpty(app.Configuration["HTTPS_PORT"]))
{
    app.UseHttpsRedirection();
}

app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program { }
