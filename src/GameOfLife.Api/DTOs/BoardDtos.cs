using System.ComponentModel.DataAnnotations;

namespace GameOfLife.Api.DTOs;

public sealed record CreateBoardRequest(
    [Required] IReadOnlyList<string> Rows);

public sealed record BoardResponse(
    Guid Id,
    IReadOnlyList<string> Rows,
    long Generation,
    bool IsStable,
    bool IsEmpty);

public sealed record StatesAwayResponse(
    Guid Id,
    IReadOnlyList<string> Rows,
    long Generation,
    int RequestedGenerations,
    bool IsStable,
    bool IsEmpty);

public sealed record FinalStateResponse(
    Guid Id,
    IReadOnlyList<string> Rows,
    long Generation,
    int Attempts,
    bool IsStable,
    bool IsEmpty);

public sealed record ErrorResponse(string Error);
