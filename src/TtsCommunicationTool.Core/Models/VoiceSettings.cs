namespace TtsCommunicationTool.Core.Models;

public sealed class VoiceSettings
{
    public VoiceEngine Engine { get; set; } = VoiceEngine.Kokoro;
    /// <summary>Kept for back-compat; mirrors Engine.ToString()</summary>
    public string EngineName { get; set; } = "Kokoro";
    public string SelectedVoiceId { get; set; } = "af_heart";
    public string SelectedVoiceDisplayName { get; set; } = "AF Heart";

    /// <summary>
    /// Global pitch multiplier applied to all standard TTS output (not phrases).
    /// 1.0 = normal, &lt;1.0 = lower pitch, &gt;1.0 = higher pitch.
    /// Clamped to [0.5, 2.0] before use.
    /// </summary>
    public float GlobalPitch { get; set; } = 1.0f;
}
