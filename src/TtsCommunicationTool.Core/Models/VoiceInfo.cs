namespace TtsCommunicationTool.Core.Models;

public sealed class VoiceInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string EngineName { get; set; } = string.Empty;

    /// <summary>
    /// True for ElevenLabs voices that are user-cloned or AI-generated (not premade library voices).
    /// Highlighted in red in the UI because they require Creator or higher subscription.
    /// </summary>
    public bool IsCustomVoice { get; set; }
}
