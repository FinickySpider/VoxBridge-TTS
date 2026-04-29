namespace TtsCommunicationTool.Core.Models;

public sealed class GeneralSettings
{
    public bool StartWithWindows { get; set; }
    public bool ShowNotifications { get; set; } = true;
    public bool ShowSplashScreen { get; set; } = true;
    public bool EnableTranscriptLogging { get; set; } = false;
}
