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
    private readonly ITtsService _tts;
    private readonly ITextReplacementService _textReplacement;
    private readonly INotificationService _notifications;
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
        ITtsService tts,
        ITextReplacementService textReplacement,
        INotificationService notifications,
        PlaybackState playbackState,
        RecentMessagesState recentMessages)
    {
        _config = config;
        _overlay = overlay;
        _audioRouter = audioRouter;
        _phraseService = phraseService;
        _phraseCache = phraseCache;
        _log = log;
        _tts = tts;
        _textReplacement = textReplacement;
        _notifications = notifications;
        _playbackState = playbackState;
        _recentMessages = recentMessages;

        Width = 0;
        Height = 0;
        WindowStyle = WindowStyle.None;
        ShowInTaskbar = false;
        ShowActivated = false;

        Loaded += OnLoaded;
    }

    public event EventHandler? SettingsRequested;

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

        if (!cfg.HotkeySettings.SettingsHotkey.IsEmpty)
        {
            var settingsResult = _hotkeyService.Register("settings", cfg.HotkeySettings.SettingsHotkey);
            if (!settingsResult.Success)
                _log.Warn($"Failed to register settings hotkey: {settingsResult.ErrorMessage}");
            else
                _log.Info($"Registered settings hotkey: {cfg.HotkeySettings.SettingsHotkey}");
        }

        if (!cfg.HotkeySettings.ResendHotkey.IsEmpty)
        {
            var resendResult = _hotkeyService.Register("resend", cfg.HotkeySettings.ResendHotkey);
            if (!resendResult.Success)
                _log.Warn($"Failed to register resend hotkey: {resendResult.ErrorMessage}");
            else
                _log.Info($"Registered resend hotkey: {cfg.HotkeySettings.ResendHotkey}");
        }

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
            case "settings":
                Dispatcher.Invoke(() => SettingsRequested?.Invoke(this, EventArgs.Empty));
                break;
            case "resend":
                Dispatcher.Invoke(() => _overlay.HideOverlay());
                _ = ResendLastAsync();
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

    private async Task ResendLastAsync()
    {
        var text = _recentMessages.GetAll().FirstOrDefault();
        if (string.IsNullOrWhiteSpace(text))
        {
            _log.Debug("Resend hotkey: no recent message to resend.");
            return;
        }

        if (_playbackState.IsPlaying || _audioRouter.IsPlaying)
        {
            _log.Debug("Resend hotkey blocked — audio already playing.");
            return;
        }

        try
        {
            _playbackState.IsPlaying = true;
            _playbackState.CurrentText = text;
            var processed = _textReplacement.Apply(text);

            var result = await _tts.SynthesizeAsync(new TtsRequest
            {
                Text = processed,
                VoiceId = _config.CurrentConfig.VoiceSettings.SelectedVoiceId
            });

            if (!result.Success)
            {
                _log.Error($"Resend TTS failed: {result.ErrorMessage}");
                _playbackState.Reset();
                Dispatcher.Invoke(() => _notifications.ShowError($"Resend failed: {result.ErrorMessage}"));
                return;
            }

            var cfg = _config.CurrentConfig;
            var playback = new PlaybackRequest
            {
                AudioData = result.AudioData!,
                SampleRate = result.SampleRate,
                Channels = result.Channels,
                BitsPerSample = result.BitsPerSample
            };

            await _audioRouter.PlayAsync(playback,
                cfg.AudioSettings.MonitorOutputDeviceId,
                cfg.AudioSettings.SecondaryOutputDeviceId,
                cfg.AudioSettings.MonitorVolume,
                cfg.AudioSettings.SecondaryVolume);
        }
        catch (Exception ex)
        {
            _log.Error("Resend failed", ex);
            _playbackState.Reset();
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
