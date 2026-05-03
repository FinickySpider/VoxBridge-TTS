namespace TtsCommunicationTool.Core.Models;

public sealed class TtsRequest
{
    public required string Text { get; init; }
    public string VoiceId { get; init; } = string.Empty;
    /// <summary>Optional correlation ID for structured diagnostic logging. Propagated through the TTS pipeline.</summary>
    public string? RequestId { get; init; }
}
