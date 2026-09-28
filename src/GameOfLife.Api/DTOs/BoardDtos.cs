using System.ComponentModel.DataAnnotations;
using GameOfLife.Api.Interfaces;

namespace GameOfLife.Api.DTOs;

public sealed class CreateBoardRequest(IReadOnlyList<string> rows) : ICreateBoardRequest
{
    [Required]
    public IReadOnlyList<string> Rows { get; } = rows;
}

public sealed class BoardResponse(
    Guid id,
    IReadOnlyList<string> rows,
    long generation,
    bool isStable,
    bool isEmpty) : IBoardResponse
{
    public Guid Id { get; } = id;
    public IReadOnlyList<string> Rows { get; } = rows;
    public long Generation { get; } = generation;
    public bool IsStable { get; } = isStable;
    public bool IsEmpty { get; } = isEmpty;
}

public sealed class StatesAwayResponse(
    Guid id,
    IReadOnlyList<string> rows,
    long generation,
    int requestedGenerations,
    bool isStable,
    bool isEmpty) : IStatesAwayResponse
{
    public Guid Id { get; } = id;
    public IReadOnlyList<string> Rows { get; } = rows;
    public long Generation { get; } = generation;
    public int RequestedGenerations { get; } = requestedGenerations;
    public bool IsStable { get; } = isStable;
    public bool IsEmpty { get; } = isEmpty;
}

public sealed class FinalStateResponse(
    Guid id,
    IReadOnlyList<string> rows,
    long generation,
    int attempts,
    bool isStable,
    bool isEmpty) : IFinalStateResponse
{
    public Guid Id { get; } = id;
    public IReadOnlyList<string> Rows { get; } = rows;
    public long Generation { get; } = generation;
    public int Attempts { get; } = attempts;
    public bool IsStable { get; } = isStable;
    public bool IsEmpty { get; } = isEmpty;
}

public sealed class ErrorResponse(string error) : IErrorResponse
{
    public string Error { get; } = error;
}
