using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class GeneralSettingsViewModel : ViewModelBase
{
    private static readonly string TranscriptPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool",
        "transcript.txt");

    private bool _startWithWindows;
    private bool _showNotifications;
    private bool _showSplashScreen;
    private bool _enableTranscriptLogging;
    private bool _keepOverlayText;

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

    public bool EnableTranscriptLogging
    {
        get => _enableTranscriptLogging;
        set => SetField(ref _enableTranscriptLogging, value);
    }

    public bool KeepOverlayText
    {
        get => _keepOverlayText;
        set => SetField(ref _keepOverlayText, value);
    }

    // Window dimensions are persisted independently — not tracked by IsDirty
    public double SettingsWindowWidth { get; set; } = 680;
    public double SettingsWindowHeight { get; set; } = 540;

    /// <summary>True when the transcript file exists on disk.</summary>
    public bool HasTranscriptFile => File.Exists(TranscriptPath);

    public ICommand OpenTranscriptCommand { get; } = new RelayCommand(() =>
    {
        if (!File.Exists(TranscriptPath)) return;
        Process.Start(new ProcessStartInfo { FileName = TranscriptPath, UseShellExecute = true });
    });

    public void LoadFrom(GeneralSettings s)
    {
        StartWithWindows = s.StartWithWindows;
        ShowNotifications = s.ShowNotifications;
        ShowSplashScreen = s.ShowSplashScreen;
        EnableTranscriptLogging = s.EnableTranscriptLogging;
        KeepOverlayText = s.KeepOverlayText;
        SettingsWindowWidth = s.SettingsWindowWidth > 400 ? s.SettingsWindowWidth : 680;
        SettingsWindowHeight = s.SettingsWindowHeight > 300 ? s.SettingsWindowHeight : 540;
    }

    public void ApplyTo(GeneralSettings s)
    {
        s.StartWithWindows = StartWithWindows;
        s.ShowNotifications = ShowNotifications;
        s.ShowSplashScreen = ShowSplashScreen;
        s.EnableTranscriptLogging = EnableTranscriptLogging;
        s.KeepOverlayText = KeepOverlayText;
        s.SettingsWindowWidth = SettingsWindowWidth;
        s.SettingsWindowHeight = SettingsWindowHeight;
    }
}
