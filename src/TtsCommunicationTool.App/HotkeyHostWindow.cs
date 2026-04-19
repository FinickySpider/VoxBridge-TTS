using System.Windows;
using System.Windows.Interop;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Hotkeys;

namespace TtsCommunicationTool.App;

/// <summary>
/// Invisible window that hosts the message pump for global hotkey messages.
/// </summary>
public sealed class HotkeyHostWindow : Window
{
    private const int WM_HOTKEY = 0x0312;

    private readonly IConfigService _config;
    private readonly IOverlayCoordinator _overlay;
    private readonly IAudioRouterService _audioRouter;
    private readonly ILoggingService _log;
    private GlobalHotkeyService? _hotkeyService;

    public HotkeyHostWindow(
        IConfigService config,
        IOverlayCoordinator overlay,
        IAudioRouterService audioRouter,
        ILoggingService log)
    {
        _config = config;
        _overlay = overlay;
        _audioRouter = audioRouter;
        _log = log;

        Width = 0;
        Height = 0;
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;
        ShowActivated = false;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(hwnd);
        source?.AddHook(WndProc);

        _hotkeyService = new GlobalHotkeyService(hwnd);
        _hotkeyService.HotkeyPressed += OnHotkeyPressed;

        RegisterConfiguredHotkeys();
    }

    private void RegisterConfiguredHotkeys()
    {
        if (_hotkeyService is null) return;

        var cfg = _config.CurrentConfig;

        var overlayResult = _hotkeyService.Register("overlay", cfg.HotkeySettings.OverlayHotkey);
        if (!overlayResult.Success)
            _log.Warn($"Failed to register overlay hotkey: {overlayResult.ErrorMessage}");
        else
            _log.Info($"Registered overlay hotkey: {cfg.HotkeySettings.OverlayHotkey}");

        var stopResult = _hotkeyService.Register("stop", cfg.HotkeySettings.StopHotkey);
        if (!stopResult.Success)
            _log.Warn($"Failed to register stop hotkey: {stopResult.ErrorMessage}");
        else
            _log.Info($"Registered stop hotkey: {cfg.HotkeySettings.StopHotkey}");
    }

    private void OnHotkeyPressed(object? sender, string id)
    {
        _log.Debug($"Hotkey pressed: {id}");
        switch (id)
        {
            case "overlay":
                Dispatcher.Invoke(() => _overlay.ToggleOverlay());
                break;
            case "stop":
                _audioRouter.StopAll();
                break;
        }
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg == WM_HOTKEY)
        {
            _hotkeyService?.ProcessHotkeyMessage((int)wParam);
            handled = true;
        }
        return nint.Zero;
    }

    protected override void OnClosed(EventArgs e)
    {
        _hotkeyService?.Dispose();
        base.OnClosed(e);
    }
}
