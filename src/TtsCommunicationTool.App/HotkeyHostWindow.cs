using System.Windows;
using System.Windows.Interop;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.State;
using TtsCommunicationTool.Infrastructure.Hotkeys;

namespace TtsCommunicationTool.App;

/// <summary>
/// Invisible window that hosts the message pump for global hotkey messages.
/// </summary>
public sealed class HotkeyHostWindow : Window, IHotkeyHost
{
    private const int WM_HOTKEY = 0x0312;

    private readonly IConfigService _config;
    private readonly IOverlayCoordinator _overlay;
    private readonly IAudioRouterService _audioRouter;
    private readonly IPhraseService _phraseService;
    private readonly IPhraseCacheService _phraseCache;
    private readonly ILoggingService _log;
    private readonly PlaybackState _playbackState;
    private readonly RecentMessagesState _recentMessages;
    private GlobalHotkeyService? _hotkeyService;

    public HotkeyHostWindow(
        IConfigService config,
        IOverlayCoordinator overlay,
        IAudioRouterService audioRouter,
        IPhraseService phraseService,
        IPhraseCacheService phraseCache,
        ILoggingService log,
        PlaybackState playbackState,
        RecentMessagesState recentMessages)
    {
        _config = config;
        _overlay = overlay;
        _audioRouter = audioRouter;
        _phraseService = phraseService;
        _phraseCache = phraseCache;
        _log = log;
        _playbackState = playbackState;
        _recentMessages = recentMessages;

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

        // Register phrase hotkeys
        RegisterPhraseHotkeys();
    }

    /// <summary>
    /// Unregisters all hotkeys and re-registers everything from current config.
    /// Must be called on the UI thread (Dispatcher).
    /// </summary>
    public void RefreshAllHotkeys()
    {
        if (_hotkeyService is null) return;

        Dispatcher.Invoke(() =>
        {
            _log.Info("Refreshing all hotkey registrations...");
            _hotkeyService.UnregisterAll();
            RegisterConfiguredHotkeys();
        });
    }

    /// <summary>
    /// Registers all phrase hotkeys. Call after settings save to refresh.
    /// </summary>
    public void RegisterPhraseHotkeys()
    {
        if (_hotkeyService is null) return;

        // Unregister existing phrase hotkeys
        foreach (var phrase in _phraseService.GetAll())
        {
            _hotkeyService.Unregister($"phrase:{phrase.Id}");
        }

        // Register current phrase hotkeys
        foreach (var phrase in _phraseService.GetAll())
        {
            if (phrase.Hotkey is null) continue;

            var result = _hotkeyService.Register($"phrase:{phrase.Id}", phrase.Hotkey);
            if (!result.Success)
                _log.Warn($"Failed to register hotkey for phrase '{phrase.Name}': {result.ErrorMessage}");
            else
                _log.Info($"Registered hotkey for phrase '{phrase.Name}': {phrase.Hotkey}");
        }
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
                _playbackState.Reset();
                _log.Info("Playback stopped by stop hotkey.");
                break;
            default:
                if (id.StartsWith("phrase:"))
                {
                    var phraseId = id["phrase:".Length..];
                    _ = PlayPhraseAsync(phraseId);
                }
                break;
        }
    }

    private async Task PlayPhraseAsync(string phraseId)
    {
        // Block if ANY audio (typed text or another phrase) is currently playing or generating
        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            _log.Debug($"Phrase {phraseId} blocked — audio already playing.");
            return;
        }

        try
        {
            // Mark playing immediately to prevent overlapping playback
            _playbackState.IsPlaying = true;
            _playbackState.CurrentText = $"[Phrase: {phraseId}]";

            // Track the phrase text in recent messages
            var phraseForRecent = _phraseService.GetById(phraseId);
            if (phraseForRecent is not null)
                _recentMessages.Add(phraseForRecent.Text);

            // Try cached audio first
            var cached = _phraseCache.GetCachedAudio(phraseId);
            if (cached is not null)
            {
                var cfg = _config.CurrentConfig;
                await _audioRouter.PlayAsync(cached,
                    cfg.AudioSettings.MonitorOutputDeviceId,
                    cfg.AudioSettings.SecondaryOutputDeviceId,
                    cfg.AudioSettings.MonitorVolume,
                    cfg.AudioSettings.SecondaryVolume);
                return;
            }

            // Fallback: phrase exists but not cached yet, generate on-the-fly
            var phrase = _phraseService.GetById(phraseId);
            if (phrase is null)
            {
                _log.Warn($"Phrase {phraseId} not found for hotkey playback.");
                return;
            }

            _log.Info($"Phrase '{phrase.Name}' not cached, generating on-the-fly...");
            await _phraseCache.GenerateCacheAsync(phrase);
            var audio = _phraseCache.GetCachedAudio(phraseId);
            if (audio is not null)
            {
                var cfg = _config.CurrentConfig;
                await _audioRouter.PlayAsync(audio,
                    cfg.AudioSettings.MonitorOutputDeviceId,
                    cfg.AudioSettings.SecondaryOutputDeviceId,
                    cfg.AudioSettings.MonitorVolume,
                    cfg.AudioSettings.SecondaryVolume);
            }
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to play phrase {phraseId}", ex);
        }
        finally
        {
            _playbackState.Reset();
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
