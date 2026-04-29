namespace TtsCommunicationTool.Core.Models;

public sealed class VoiceSettings
{
    public VoiceEngine Engine { get; set; } = VoiceEngine.Kokoro;
    /// <summary>Kept for back-compat; mirrors Engine.ToString()</summary>
    public string EngineName { get; set; } = "Kokoro";
    public string SelectedVoiceId { get; set; } = "af_heart";
    public string SelectedVoiceDisplayName { get; set; } = "AF Heart";
}
