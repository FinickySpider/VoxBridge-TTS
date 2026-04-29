namespace TtsCommunicationTool.Core.Models;

public sealed class ElevenLabsSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SelectedVoiceId { get; set; } = string.Empty;
    public string SelectedVoiceName { get; set; } = string.Empty;
    public string ModelId { get; set; } = "eleven_multilingual_v2";
}
