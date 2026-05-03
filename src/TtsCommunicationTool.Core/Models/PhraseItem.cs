namespace TtsCommunicationTool.Core.Models;

public sealed class PhraseItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public HotkeyBinding? Hotkey { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    // FEAT-030: organization
    public string? Category { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPinned { get; set; }

    // ── Per-phrase voice override (Phrase Editor) ─────────────────────────
    /// <summary>When non-null, overrides the global TTS engine for this phrase.</summary>
    public VoiceEngine? OverrideEngine { get; set; }
    /// <summary>When true, use <see cref="OverrideVoiceId"/> instead of the engine's current default voice.</summary>
    public bool UseVoiceOverride { get; set; }
    /// <summary>Specific voice ID to use for this phrase, when <see cref="UseVoiceOverride"/> is true.</summary>
    public string? OverrideVoiceId { get; set; }
    /// <summary>Display name of the override voice, for UI labels.</summary>
    public string? OverrideVoiceName { get; set; }
    /// <summary>Per-phrase pitch multiplier. Null = use global pitch (1.0 fallback for phrases).</summary>
    public float? OverridePitch { get; set; }
}
