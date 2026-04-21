namespace TtsCommunicationTool.Core.Models;

public sealed class PhraseImportResult
{
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public int AddedCount { get; init; }
    public int NamesChangedCount { get; init; }
    public int HotkeysCleared { get; init; }
    public IReadOnlyList<string> AddedPhraseIds { get; init; } = Array.Empty<string>();

    public static PhraseImportResult Fail(string error) =>
        new() { Success = false, ErrorMessage = error };
}
