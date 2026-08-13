namespace TtsCommunicationTool.Core.Models;

/// <summary>Settings persisted independently for the Windows SAPI5 engine.</summary>
public sealed class Sapi5Settings
{
    /// <summary>Stable SAPI voice token ID. Empty means use the system default voice.</summary>
    public string SelectedVoiceId { get; set; } = string.Empty;

    /// <summary>Display/name fallback retained when a voice is temporarily unavailable.</summary>
    public string SelectedVoiceName { get; set; } = string.Empty;

    /// <summary>SAPI5 native speaking rate, from -10 (slowest) to 10 (fastest).</summary>
    public int Rate { get; set; }

    /// <summary>SAPI5 native volume, from 0 to 100.</summary>
    public int Volume { get; set; } = 100;
}
