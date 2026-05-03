namespace TtsCommunicationTool.Core.Models;

public sealed class ElevenLabsSettings
{
    public string ApiKey { get; set; } = string.Empty;
    public string SelectedVoiceId { get; set; } = string.Empty;
    public string SelectedVoiceName { get; set; } = string.Empty;
    public string ModelId { get; set; } = "eleven_multilingual_v2";

    // ── Subscription info (refreshed on API key entry / voice fetch) ──────────
    /// <summary>Characters used so far this billing period (from /v1/user/subscription).</summary>
    public int SubscriptionCharacterCount { get; set; }
    /// <summary>Total character quota for this billing period.</summary>
    public int SubscriptionCharacterLimit { get; set; }

    // ── Cross-session usage tracking ─────────────────────────────────────────
    /// <summary>Cumulative characters synthesised through this app across all sessions.</summary>
    public long TotalCharactersUsed { get; set; }
}
