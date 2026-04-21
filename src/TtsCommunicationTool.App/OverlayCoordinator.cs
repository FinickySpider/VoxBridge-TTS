using System.Windows;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.State;
using TtsCommunicationTool.UI.Views;
using TtsCommunicationTool.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TtsCommunicationTool.App;

public sealed class OverlayCoordinator : IOverlayCoordinator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _log;
    private readonly AppRuntimeState _appState;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly PlaybackState _playbackState;
    private OverlayWindow? _overlayWindow;
    private bool _isOpening; // Absolute guard against concurrent opens

    public OverlayCoordinator(
        IServiceProvider serviceProvider,
        ILoggingService log,
        AppRuntimeState appState,
        IAudioRouterService audioRouter,
        IConfigService config,
        PlaybackState playbackState)
    {
        _serviceProvider = serviceProvider;
        _log = log;
        _appState = appState;
        _audioRouter = audioRouter;
        _config = config;
        _playbackState = playbackState;
    }

    public event EventHandler? SettingsRequested;

    public bool IsOverlayVisible => _overlayWindow is not null || _isOpening;

    public void ShowOverlay()
    {
        // ABSOLUTE block: if overlay exists in any state or is being opened, do nothing
        if (_isOpening || _overlayWindow is not null)
        {
            _log.Debug("Overlay open blocked — already visible or being opened.");
            return;
        }

        // Block overlay while audio is playing or TTS is generating
        if (_audioRouter.IsPlaying || _playbackState.IsPlaying)
        {
            _log.Debug("Overlay blocked — audio is currently playing or generating.");
            return;
        }

        _isOpening = true;
        try
        {
            var vm = _serviceProvider.GetRequiredService<OverlayViewModel>();
            _overlayWindow = new OverlayWindow { DataContext = vm };

            // Apply appearance settings from config
            var overlaySettings = _config.CurrentConfig.OverlaySettings;
            _overlayWindow.Width = overlaySettings.Width;
            _overlayWindow.Height = overlaySettings.Height;
            _overlayWindow.SetFontSize(overlaySettings.FontSize);

            // Restore last position if saved and on-screen; otherwise center
            if (overlaySettings.Left.HasValue && overlaySettings.Top.HasValue &&
                IsOnScreen(overlaySettings.Left.Value, overlaySettings.Top.Value, overlaySettings.Width, overlaySettings.Height))
            {
                _overlayWindow.WindowStartupLocation = System.Windows.WindowStartupLocation.Manual;
                _overlayWindow.Left = overlaySettings.Left.Value;
                _overlayWindow.Top = overlaySettings.Top.Value;
            }

            _overlayWindow.Closed += (s, e) =>
            {
                // Save position from the closing window instance
                if (s is TtsCommunicationTool.UI.Views.OverlayWindow closedWindow)
                {
                    var cfg = _config.CurrentConfig;
                    cfg.OverlaySettings.Left = closedWindow.Left;
                    cfg.OverlaySettings.Top = closedWindow.Top;
                    _ = _config.SaveAsync(cfg);
                }
                _overlayWindow = null;
                _appState.IsOverlayVisible = false;
            };
            _overlayWindow.SettingsRequested += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
            _overlayWindow.Show();
            _overlayWindow.FocusInput();
            _appState.IsOverlayVisible = true;
            _log.Debug("Overlay shown.");
        }
        finally
        {
            _isOpening = false;
        }
    }

    public void ShowOverlayWithText(string text)
    {
        ShowOverlay();
        // If overlay opened successfully, pre-fill the input
        if (_overlayWindow?.DataContext is TtsCommunicationTool.UI.ViewModels.OverlayViewModel vm)
            vm.InputText = text;
    }

    public void HideOverlay()
    {
        if (_overlayWindow is null) return;

        // Capture reference and null the field BEFORE calling Close()
        // so that any re-entrant call (from Deactivated, hotkey, etc.)
        // sees _overlayWindow == null and exits immediately.
        var window = _overlayWindow;
        _overlayWindow = null;
        _appState.IsOverlayVisible = false;

        try
        {
            window.Close();
        }
        catch (InvalidOperationException)
        {
            // Window was already closing — safe to ignore.
        }
        _log.Debug("Overlay hidden.");
    }

    public void ToggleOverlay()
    {
        if (IsOverlayVisible)
            HideOverlay();
        else
            ShowOverlay();
    }

    /// <summary>
    /// Returns true if the window position puts at least part of the title-bar
    /// area on one of the current screens (guards against off-screen positions
    /// after display changes).
    /// </summary>
    private static bool IsOnScreen(double left, double top, double width, double height)
    {
        // Check that the centre of the window is on any screen
        var cx = left + width / 2;
        var cy = top + height / 2;
        foreach (var screen in System.Windows.Forms.Screen.AllScreens)
        {
            var b = screen.Bounds;
            if (cx >= b.Left && cx <= b.Right && cy >= b.Top && cy <= b.Bottom)
                return true;
        }
        return false;
    }
}
