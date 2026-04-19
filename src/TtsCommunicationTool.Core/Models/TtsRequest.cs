namespace TtsCommunicationTool.Core.Models;

public sealed class TtsRequest
{
    public required string Text { get; init; }
    public string VoiceId { get; init; } = string.Empty;
}
