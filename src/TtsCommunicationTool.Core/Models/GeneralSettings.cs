namespace TtsCommunicationTool.Core.Models;

public sealed class GeneralSettings
{
    public bool StartWithWindows { get; set; }
    public bool ShowNotifications { get; set; } = true;
    public bool ShowSplashScreen { get; set; } = true;
    public bool EnableTranscriptLogging { get; set; } = false;
    /// <summary>Preserve unsent overlay text as a draft when the overlay closes without sending.</summary>
    public bool KeepOverlayText { get; set; } = false;
    /// <summary>When true, limit overlay input to MaxOverlayInputLength characters.</summary>
    public bool EnableCharacterLimit { get; set; } = true;
    /// <summary>Maximum overlay input length when EnableCharacterLimit is true.</summary>
    public int MaxOverlayInputLength { get; set; } = 500;
    /// <summary>Persisted size of the Settings window.</summary>
    public double SettingsWindowWidth { get; set; } = 680;
    public double SettingsWindowHeight { get; set; } = 540;

    /// <summary>Persisted width of the Theme preview column in the Settings Theme tab.</summary>
    public double ThemePreviewColumnWidth { get; set; } = 300;

    /// <summary>Persisted size of the Phrase Editor window.</summary>
    public double PhraseEditorWindowWidth  { get; set; } = 560;
    public double PhraseEditorWindowHeight { get; set; } = 760;

    /// <summary>Show a live countdown of remaining playback time in the overlay while speaking.</summary>
    public bool ShowPlaybackTimer { get; set; } = true;
}
