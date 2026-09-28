using GameOfLife.Api.Data;
using GameOfLife.Api.DTOs;
using GameOfLife.Api.Interfaces;
using GameOfLife.Api.Services;
using GameOfLife.Api.Swagger;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace GameOfLife.Api;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var applicationBuilder = CreateApplicationBuilder(args);
        if (!applicationBuilder.Succeeded)
        {
            await Console.Error.WriteLineAsync($"Startup aborted: {applicationBuilder.Error}");
            return 1;
        }

        if (applicationBuilder.Builder is null)
        {
            await Console.Error.WriteLineAsync("Startup aborted: Application builder is null.");
            return 1;
        }

        WebApplication? app;
        try
        {
            app = applicationBuilder.Builder.Build();
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Failed to build the application host: {ex}");
            return 1;
        }

        try
        {
            var initialization = await DatabaseInitializer.InitializeAsync(app.Services, app.Logger);
            if (!initialization.Succeeded)
            {
                app.Logger.LogCritical("Startup aborted: {Error}", initialization.Error);
                return 1;
            }

            app.UseExceptionHandler(handler =>
            {
                handler.Run(async context =>
                {
                    var error = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                    var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GameOfLife.Api");
                    logger.LogError(error, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

                    if (context.Response.HasStarted)
                    {
                        return;
                    }

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsJsonAsync(new ErrorResponse("An unexpected error occurred."));
                });
            });

            app.UseSwagger();
            app.UseSwaggerUI();

            app.Use(async (context, next) =>
            {
                context.Response.OnStarting(() =>
                {
                    var request = context.Request;
                    if (request.Headers.ContainsKey("Access-Control-Request-Private-Network")
                        || request.Headers.ContainsKey("Access-Control-Request-Local-Network"))
                    {
                        context.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
                        context.Response.Headers["Access-Control-Allow-Local-Network"] = "true";
                    }

                    return Task.CompletedTask;
                });
                await next();
            });
            app.UseCors();
            app.UseDefaultFiles();
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    if (ctx.File.Name.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                        ctx.Context.Response.Headers.CacheControl = "no-cache, no-store";
                }
            });
            app.MapControllers();

            await app.RunAsync();
            return 0;
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            app.Logger.LogCritical(ex, "Startup aborted because of an unhandled error.");
            return 1;
        }
    }

    internal static readonly string GameName = nameof(GameOfLife);

    private static IApplicationBuilderResult CreateApplicationBuilder(string[] args)
    {
        WebApplicationBuilder builder;
        try
        {
            builder = WebApplication.CreateBuilder(args);
        }
        catch (Exception ex) when (ex is not HostAbortedException)
        {
            return new ApplicationBuilderResult(false, error: $"Failed to create the application builder: {ex.Message}");
        }

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            var xmlPath = Path.Combine(AppContext.BaseDirectory, $"{typeof(Program).Assembly.GetName().Name}.xml");
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            options.OperationFilter<BoardApiExampleOperationFilter>();
        });

        var extraOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [];
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.SetIsOriginAllowed(origin => IsAllowedCorsOrigin(origin, extraOrigins))
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var gameName = builder.Configuration[nameof(GameName)];
        if (string.IsNullOrWhiteSpace(gameName))
        {
            return new ApplicationBuilderResult(false, error: "Configuration value 'GameName' is required.");
        }

        var connectionString = builder.Configuration.GetConnectionString(gameName)
                               ?? "Data Source=data/gameoflife.db";

        builder.Services.AddDbContext<GameOfLifeDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddScoped<IBoardRepository, BoardRepository>();
        builder.Services.AddSingleton<IGameOfLifeEngine, GameOfLifeEngine>();
        builder.Services.Configure<GameOfLifeOptions>(builder.Configuration.GetSection(gameName));
        builder.Services.AddScoped<IGameOfLifeService, GameOfLifeService>();

        return new ApplicationBuilderResult(true, builder);
    }

    internal static bool IsAllowedCorsOrigin(string? origin, IReadOnlyCollection<string> extraOrigins)
    {
        if (string.IsNullOrWhiteSpace(origin) || !Uri.TryCreate(origin, UriKind.Absolute, out var uri))
            return false;

        if (uri.Host is "localhost" or "127.0.0.1" or "[::1]")
            return true;

        if (uri.Host.EndsWith(".vercel.app", StringComparison.OrdinalIgnoreCase)
            || uri.Host.Equals("vercel.app", StringComparison.OrdinalIgnoreCase))
            return true;

        return extraOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase);
    }
}
