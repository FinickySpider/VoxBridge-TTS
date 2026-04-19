namespace TtsCommunicationTool.Core.Models;

public sealed class GeneralSettings
{
    public bool MinimizeToTray { get; set; } = true;
    public bool CloseToTray { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool ShowNotifications { get; set; } = true;
}
