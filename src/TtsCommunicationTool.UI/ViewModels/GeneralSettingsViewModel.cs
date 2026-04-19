using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class GeneralSettingsViewModel : ViewModelBase
{
    private bool _startWithWindows;
    private bool _showNotifications;
    private bool _showSplashScreen;

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

    public bool ShowSplashScreen
    {
        get => _showSplashScreen;
        set => SetField(ref _showSplashScreen, value);
    }

    public void LoadFrom(GeneralSettings s)
    {
        StartWithWindows = s.StartWithWindows;
        ShowNotifications = s.ShowNotifications;
        ShowSplashScreen = s.ShowSplashScreen;
    }

    public void ApplyTo(GeneralSettings s)
    {
        s.StartWithWindows = StartWithWindows;
        s.ShowNotifications = ShowNotifications;
        s.ShowSplashScreen = ShowSplashScreen;
    }
}
