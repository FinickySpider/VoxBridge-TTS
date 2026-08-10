namespace TtsCommunicationTool.Core.Models;

public sealed class TtsRequest
{
    public required string Text { get; init; }
    public string VoiceId { get; init; } = string.Empty;
    /// <summary>Optional correlation ID for structured diagnostic logging. Propagated through the TTS pipeline.</summary>
    public string? RequestId { get; init; }

    /// <summary>
    /// Pitch multiplier for this synthesis. 1.0 = normal. Clamped to [0.5, 2.0].
    /// Phrases always pass 1.0 (pitch-neutral); only standard TTS uses the global pitch setting.
    /// </summary>
    public float Pitch { get; init; } = 1.0f;

    /// <summary>
    /// Playback speed multiplier. 1.0 = normal. Phrases leave this at the neutral default.
    /// </summary>
    public float Speed { get; init; } = 1.0f;

    /// <summary>
    /// When non-null, overrides the active TTS engine configured in VoiceSettings.
    /// Used by the phrase cache service and preview synthesis to honour per-phrase engine overrides.
    /// </summary>
    public VoiceEngine? EngineOverride { get; init; }
}
