using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace GameOfLife.Api.Swagger;

public sealed class BoardApiExampleOperationFilter : IOperationFilter
{
    private const string ExampleId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath;
        var method = context.ApiDescription.HttpMethod;

        if (path == "api/boards" && method == "POST")
        {
            SetRequestExample(operation, new OpenApiObject
            {
                ["rows"] = Rows(".....", "..#..", "..#..", "..#..", ".....")
            });
            SetResponseExample(operation, "201", new OpenApiObject
            {
                ["id"] = new OpenApiString(ExampleId)
            });
            SetResponseExample(operation, "400", Error("All board rows must have the same length."));
            return;
        }

        SetParameterExample(operation, "id", new OpenApiString(ExampleId));

        if (path == "api/boards/{id}/next" && method == "GET")
        {
            SetResponseExample(operation, "200", Board(
                [".....", ".....", ".###.", ".....", "....."],
                generation: 1,
                isStable: false,
                isEmpty: false));
            return;
        }

        if (path == "api/boards/{id}/states/{generations}" && method == "GET")
        {
            SetParameterExample(operation, "generations", new OpenApiInteger(2));
            SetResponseExample(operation, "200", Board(
                [".....", "..#..", "..#..", "..#..", "....."],
                generation: 2,
                isStable: false,
                isEmpty: false,
                requestedGenerations: 2));
            SetResponseExample(operation, "400", Error("Generations must be zero or greater."));
            return;
        }

        if (path == "api/boards/{id}/final" && method == "GET")
        {
            SetParameterExample(operation, "maxAttempts", new OpenApiInteger(10));
            SetResponseExample(operation, "200", Board(
                ["."],
                generation: 1,
                isStable: true,
                isEmpty: true,
                attempts: 1));
            SetResponseExample(operation, "400", Error("maxAttempts must be positive and within the configured limit."));
            SetResponseExample(operation, "422", Error("The board entered a cycle and therefore has no final stable state."));
        }
    }

    private static void SetParameterExample(OpenApiOperation operation, string name, IOpenApiAny example)
    {
        var parameter = operation.Parameters.FirstOrDefault(candidate => candidate.Name == name);
        if (parameter is not null)
            parameter.Example = example;
    }

    private static void SetRequestExample(OpenApiOperation operation, IOpenApiAny example)
    {
        if (operation.RequestBody?.Content.TryGetValue("application/json", out var media) == true)
            media.Example = example;
    }

    private static void SetResponseExample(OpenApiOperation operation, string statusCode, IOpenApiAny example)
    {
        if (operation.Responses.TryGetValue(statusCode, out var response)
            && response.Content.TryGetValue("application/json", out var media))
        {
            media.Example = example;
        }
    }

    private static OpenApiObject Board(
        string[] rows,
        long generation,
        bool isStable,
        bool isEmpty,
        int? requestedGenerations = null,
        int? attempts = null)
    {
        var body = new OpenApiObject
        {
            ["id"] = new OpenApiString(ExampleId),
            ["rows"] = Rows(rows),
            ["generation"] = new OpenApiLong(generation),
            ["isStable"] = new OpenApiBoolean(isStable),
            ["isEmpty"] = new OpenApiBoolean(isEmpty)
        };

        if (requestedGenerations is int requested)
            body["requestedGenerations"] = new OpenApiInteger(requested);
        if (attempts is int attemptCount)
            body["attempts"] = new OpenApiInteger(attemptCount);

        return body;
    }

    private static OpenApiArray Rows(params string[] rows)
    {
        var values = new OpenApiArray();
        foreach (var row in rows)
            values.Add(new OpenApiString(row));
        return values;
    }

    private static OpenApiObject Error(string message) => new()
    {
        ["error"] = new OpenApiString(message)
    };
}
