using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class GeneralSettingsViewModel : ViewModelBase
{
    private bool _closeToTray;
    private bool _minimizeToTray;
    private bool _startWithWindows;
    private bool _showNotifications;

    public bool CloseToTray
    {
        get => _closeToTray;
        set => SetField(ref _closeToTray, value);
    }

    public bool MinimizeToTray
    {
        get => _minimizeToTray;
        set => SetField(ref _minimizeToTray, value);
    }

    public bool StartWithWindows
    {
        get => _startWithWindows;
        set => SetField(ref _startWithWindows, value);
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set => SetField(ref _showNotifications, value);
    }

    public void LoadFrom(GeneralSettings s)
    {
        CloseToTray = s.CloseToTray;
        MinimizeToTray = s.MinimizeToTray;
        StartWithWindows = s.StartWithWindows;
        ShowNotifications = s.ShowNotifications;
    }

    public void ApplyTo(GeneralSettings s)
    {
        s.CloseToTray = CloseToTray;
        s.MinimizeToTray = MinimizeToTray;
        s.StartWithWindows = StartWithWindows;
        s.ShowNotifications = ShowNotifications;
    }
}
