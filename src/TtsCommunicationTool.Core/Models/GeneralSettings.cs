namespace TtsCommunicationTool.Core.Models;

public sealed class GeneralSettings
{
    public bool StartWithWindows { get; set; }
    public bool ShowNotifications { get; set; } = true;
    public bool ShowSplashScreen { get; set; } = true;
    public bool EnableTranscriptLogging { get; set; } = false;
    /// <summary>Preserve unsent overlay text as a draft when the overlay closes without sending.</summary>
    public bool KeepOverlayText { get; set; } = false;
    /// <summary>Persisted size of the Settings window.</summary>
    public double SettingsWindowWidth { get; set; } = 680;
    public double SettingsWindowHeight { get; set; } = 540;
}
