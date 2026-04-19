namespace TtsCommunicationTool.Core.Models;

public sealed class OperationResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }

    public static OperationResult Ok() => new() { Success = true };
    public static OperationResult Fail(string error) => new() { Success = false, ErrorMessage = error };
}
