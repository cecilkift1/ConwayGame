using GameOfLife.Api.Data;
using GameOfLife.Api.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTP-only local/dev serving: HTTPS redirect breaks when no HTTPS
// port is configured and can blank the page behind port forwards.
app.UseDefaultFiles();
app.UseStaticFiles();
app.MapControllers();

app.Run();

public partial class Program { }
