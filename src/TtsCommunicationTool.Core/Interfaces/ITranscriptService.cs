namespace TtsCommunicationTool.Core.Interfaces;

/// <summary>
/// Appends spoken messages to a persistent transcript file.
/// </summary>
public interface ITranscriptService
{
    /// <summary>
    /// Appends <paramref name="text"/> with a UTC timestamp to the transcript file,
    /// if transcript logging is currently enabled in settings. No-op otherwise.
    /// </summary>
    Task LogAsync(string text);
}
