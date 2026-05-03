namespace TtsCommunicationTool.Core.Models;

public sealed class ElevenLabsSettings
{
    // ── Secure API key storage (DPAPI-encrypted, CurrentUser scope) ──────────
    /// <summary>DPAPI-encrypted API key bytes, Base64-encoded. Null = no key stored.</summary>
    public string? EncryptedApiKey { get; set; }
    /// <summary>Last 4 chars of the original key for masked tail display (e.g. "a1b2"). Safe to store in plain.</summary>
    public string? ApiKeyTail { get; set; }
    /// <summary>ISO-8601 date when the key was last saved, e.g. "2026-05-03".</summary>
    public string? ApiKeyUpdatedDate { get; set; }

    /// <summary>
    /// Legacy plain-text key — present only in configs written before v0.13.
    /// On first load the app migrates this to <see cref="EncryptedApiKey"/> and clears this field.
    /// </summary>
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

