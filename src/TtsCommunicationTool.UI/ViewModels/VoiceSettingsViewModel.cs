using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Tts;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

/// <summary>A model option shown in the ElevenLabs Model dropdown.</summary>
public sealed record ElevenLabsModelOption(string Id, string DisplayName);

public sealed class VoiceSettingsViewModel : ViewModelBase
{
    private readonly ITtsService _tts;
    private readonly KokoroTtsService _kokoro;
    private readonly ElevenLabsTtsService _elevenLabs;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;

    // ── Kokoro ───────────────────────────────────────────────────────────────
    private string _selectedVoiceId = string.Empty;
    private string _savedKokoroVoiceId = string.Empty;  // remembered when switching to ElevenLabs
    private string _engineName = "Kokoro";

    // ── Global pitch ─────────────────────────────────────────────────────────
    private float _globalPitch = 1.0f;
    /// <summary>
    /// Global pitch multiplier for standard TTS (not phrases).
    /// Changes apply LIVE immediately after being set — before saving settings.
    /// Clamped to [0.5, 2.0] on set.
    /// </summary>
    public float GlobalPitch
    {
        get => _globalPitch;
        set
        {
            var clamped = Math.Clamp(value, 0.5f, 2.0f);
            if (SetField(ref _globalPitch, clamped))
            {
                // Apply live to in-memory config so TTS uses new pitch immediately.
                _config.CurrentConfig.VoiceSettings.GlobalPitch = clamped;
                OnPropertyChanged(nameof(GlobalPitchPercent));
            }
        }
    }

    /// <summary>Pitch displayed as a percentage string, e.g. "100%".</summary>
    public string GlobalPitchPercent => $"{(int)(_globalPitch * 100)}%";

    // ── Engine selection ─────────────────────────────────────────────────────
    private VoiceEngine _engine = VoiceEngine.Kokoro;
    public VoiceEngine Engine
    {
        get => _engine;
        set
        {
            if (_engine == value) return;
            // Save Kokoro voice before switching away
            if (_engine == VoiceEngine.Kokoro && !string.IsNullOrEmpty(_selectedVoiceId))
                _savedKokoroVoiceId = _selectedVoiceId;

            SetField(ref _engine, value);
            OnPropertyChanged(nameof(IsKokoro));
            OnPropertyChanged(nameof(IsElevenLabs));
            LoadVoices();

            // Auto-fetch voices + subscription when switching to ElevenLabs
            if (_engine == VoiceEngine.ElevenLabs && !string.IsNullOrWhiteSpace(_elevenLabsApiKey))
                _ = FetchElevenLabsVoicesAsync();

            // Restore Kokoro voice when switching back
            if (_engine == VoiceEngine.Kokoro && !string.IsNullOrEmpty(_savedKokoroVoiceId))
                SelectedVoiceId = _savedKokoroVoiceId;
        }
    }
    public bool IsKokoro
    {
        get => _engine == VoiceEngine.Kokoro;
        set { if (value) Engine = VoiceEngine.Kokoro; }
    }
    public bool IsElevenLabs
    {
        get => _engine == VoiceEngine.ElevenLabs;
        set { if (value) Engine = VoiceEngine.ElevenLabs; }
    }

    // ── Kokoro props ─────────────────────────────────────────────────────────
    public ObservableCollection<VoiceInfo> AvailableVoices { get; } = new();

    public string SelectedVoiceId
    {
        get => _selectedVoiceId;
        set => SetField(ref _selectedVoiceId, value);
    }

    public string EngineName
    {
        get => _engineName;
        set => SetField(ref _engineName, value);
    }

    // ── ElevenLabs model options (instance property so WPF binding works) ───
    private static readonly IReadOnlyList<ElevenLabsModelOption> _elevenLabsModelOptions = new[]
    {
        new ElevenLabsModelOption("eleven_flash_v2_5",      "Flash v2.5 — Fast · $0.05/1K chars"),
        new ElevenLabsModelOption("eleven_turbo_v2_5",      "Turbo v2.5 — Fast · $0.05/1K chars"),
        new ElevenLabsModelOption("eleven_v3",              "v3 Multilingual — Quality · $0.10/1K chars"),
        new ElevenLabsModelOption("eleven_multilingual_v2", "Multilingual v2 — Quality · $0.10/1K chars"),
    };

    /// <summary>Hard-coded ElevenLabs model list bound to the Model dropdown.</summary>
    public IReadOnlyList<ElevenLabsModelOption> ElevenLabsModelOptions => _elevenLabsModelOptions;

    // ── ElevenLabs subscription display ──────────────────────────────────────
    private int _subCharUsed;
    private int _subCharLimit;

    /// <summary>Formatted subscription string, e.g. "5,000 / 100,000 characters remaining".</summary>
    public string ElevenLabsCreditsDisplay
    {
        get
        {
            if (_subCharLimit <= 0) return string.Empty;
            var remaining = Math.Max(0, _subCharLimit - _subCharUsed);
            return $"{remaining:N0} / {_subCharLimit:N0} characters remaining";
        }
    }

    /// <summary>Shows cumulative characters sent through this app across all sessions.</summary>
    public string ElevenLabsLifetimeCharacters
    {
        get
        {
            var total = _config.CurrentConfig.ElevenLabs.TotalCharactersUsed;
            return total == 0 ? string.Empty : $"{total:N0} chars sent lifetime";
        }
    }

    // Debounce token for the API key auto-fetch
    private CancellationTokenSource? _apiKeyDebounceCts;

    // ── ElevenLabs props ─────────────────────────────────────────────────────
    private string _elevenLabsApiKey = string.Empty;
    private string _elevenLabsModelId = "eleven_multilingual_v2";
    private string _elevenLabsSelectedVoiceId = string.Empty;
    private string _elevenLabsSelectedVoiceName = string.Empty;
    private string _elevenLabsStatus = string.Empty;

    public string ElevenLabsApiKey
    {
        get => _elevenLabsApiKey;
        set
        {
            if (!SetField(ref _elevenLabsApiKey, value)) return;

            // Debounce: auto-fetch voices + subscription 800 ms after the user stops typing.
            _apiKeyDebounceCts?.Cancel();
            _apiKeyDebounceCts = new CancellationTokenSource();
            var cts = _apiKeyDebounceCts;
            _ = Task.Delay(800, cts.Token).ContinueWith(t =>
            {
                if (t.IsCanceled || string.IsNullOrWhiteSpace(value)) return;
                System.Windows.Application.Current?.Dispatcher.InvokeAsync(() =>
                    _ = FetchElevenLabsVoicesAsync());
            }, TaskScheduler.Default);
        }
    }
    public string ElevenLabsModelId
    {
        get => _elevenLabsModelId;
        set => SetField(ref _elevenLabsModelId, value);
    }
    public string ElevenLabsSelectedVoiceId
    {
        get => _elevenLabsSelectedVoiceId;
        set => SetField(ref _elevenLabsSelectedVoiceId, value);
    }
    public string ElevenLabsSelectedVoiceName
    {
        get => _elevenLabsSelectedVoiceName;
        set => SetField(ref _elevenLabsSelectedVoiceName, value);
    }
    public string ElevenLabsStatus
    {
        get => _elevenLabsStatus;
        set => SetField(ref _elevenLabsStatus, value);
    }
    public ObservableCollection<VoiceInfo> ElevenLabsVoices { get; } = new();

    // ── Commands ─────────────────────────────────────────────────────────────
    public ICommand TestVoiceCommand { get; }
    public ICommand FetchElevenLabsVoicesCommand { get; }

    public VoiceSettingsViewModel(
        ITtsService tts,
        KokoroTtsService kokoro,
        ElevenLabsTtsService elevenLabs,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log)
    {
        _tts = tts;
        _kokoro = kokoro;
        _elevenLabs = elevenLabs;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;

        TestVoiceCommand = new AsyncRelayCommand(TestVoiceAsync);
        FetchElevenLabsVoicesCommand = new AsyncRelayCommand(FetchElevenLabsVoicesAsync);
        LoadVoices();
    }

    private void LoadVoices()
    {
        AvailableVoices.Clear();
        // Always use the concrete Kokoro service for Kokoro voices — TtsRouter routes
        // to the active engine which may not be Kokoro if ElevenLabs is saved config.
        if (_engine == VoiceEngine.ElevenLabs)
        {
            foreach (var v in _elevenLabs.GetAvailableVoices())
                AvailableVoices.Add(v);
        }
        else
        {
            foreach (var v in _kokoro.GetAvailableVoices())
                AvailableVoices.Add(v);
        }
    }

    private async Task FetchElevenLabsVoicesAsync()
    {
        ElevenLabsStatus = "Fetching voices…";
        // Temporarily persist the API key so the service can use it
        _config.CurrentConfig.ElevenLabs.ApiKey = ElevenLabsApiKey;

        var voices = await _elevenLabs.FetchVoicesAsync();
        ElevenLabsVoices.Clear();
        foreach (var v in voices)
            ElevenLabsVoices.Add(v);

        if (voices.Count > 0)
        {
            // Restore selected voice if still valid
            if (ElevenLabsVoices.All(v => v.Id != ElevenLabsSelectedVoiceId) && voices.Count > 0)
            {
                ElevenLabsSelectedVoiceId = voices[0].Id;
                ElevenLabsSelectedVoiceName = voices[0].DisplayName;
            }
            // Also update AvailableVoices if engine is ElevenLabs
            if (_engine == VoiceEngine.ElevenLabs)
                LoadVoices();
        }
        else
        {
            ElevenLabsStatus = "No voices returned — check API key.";
        }

        // Also refresh subscription info
        var (used, limit) = await _elevenLabs.FetchUserSubscriptionAsync();
        _subCharUsed  = used;
        _subCharLimit = limit;
        OnPropertyChanged(nameof(ElevenLabsCreditsDisplay));
        OnPropertyChanged(nameof(ElevenLabsLifetimeCharacters));

        var remaining = limit > 0 ? Math.Max(0, limit - used) : -1;
        ElevenLabsStatus = voices.Count > 0
            ? (remaining >= 0 ? $"Loaded {voices.Count} voices · {remaining:N0} chars remaining" : $"Loaded {voices.Count} voices.")
            : ElevenLabsStatus;
    }

    private async Task TestVoiceAsync()
    {
        string voiceId = _engine == VoiceEngine.ElevenLabs ? ElevenLabsSelectedVoiceId : SelectedVoiceId;
        if (string.IsNullOrEmpty(voiceId)) return;

        // Temporarily apply settings so the active service has the right values
        ApplyToConfig();

        var result = await _tts.SynthesizeAsync(new TtsRequest
        {
            Text = "Hello, this is a voice test.",
            VoiceId = voiceId,
            Pitch = Math.Clamp(_globalPitch, 0.5f, 2.0f)
        });

        if (!result.Success || result.AudioData is null)
        {
            _log.Warn($"Voice test failed: {result.ErrorMessage}");
            if (_engine == VoiceEngine.ElevenLabs)
                ElevenLabsStatus = $"Test failed: {result.ErrorMessage}";
            return;
        }

        var playback = new PlaybackRequest
        {
            AudioData = result.AudioData,
            SampleRate = result.SampleRate,
            Channels = result.Channels,
            BitsPerSample = result.BitsPerSample
        };

        var cfg = _config.CurrentConfig;
        await _audioRouter.PlayAsync(playback, cfg.AudioSettings.MonitorOutputDeviceId, null);
        if (_engine == VoiceEngine.ElevenLabs)
            ElevenLabsStatus = "Test playback started.";
    }

    public void LoadFrom(VoiceSettings s)
    {
        _savedKokoroVoiceId = s.SelectedVoiceId;  // remember the persisted Kokoro voice
        LoadVoices();
        SelectedVoiceId = s.SelectedVoiceId;
        EngineName = s.EngineName;
        _engine = s.Engine;   // set backing field directly to avoid double LoadVoices
        OnPropertyChanged(nameof(Engine));
        OnPropertyChanged(nameof(IsKokoro));
        OnPropertyChanged(nameof(IsElevenLabs));

        // Load pitch — do NOT write back to config to avoid dirty the snapshot
        _globalPitch = Math.Clamp(s.GlobalPitch, 0.5f, 2.0f);
        OnPropertyChanged(nameof(GlobalPitch));
        OnPropertyChanged(nameof(GlobalPitchPercent));

        var el = _config.CurrentConfig.ElevenLabs;
        _elevenLabsApiKey = el.ApiKey;   // set backing field to avoid triggering debounce on load
        OnPropertyChanged(nameof(ElevenLabsApiKey));
        ElevenLabsModelId = el.ModelId;
        ElevenLabsSelectedVoiceId = el.SelectedVoiceId;
        ElevenLabsSelectedVoiceName = el.SelectedVoiceName;

        // Restore persisted subscription info
        _subCharUsed  = el.SubscriptionCharacterCount;
        _subCharLimit = el.SubscriptionCharacterLimit;
        OnPropertyChanged(nameof(ElevenLabsCreditsDisplay));
        OnPropertyChanged(nameof(ElevenLabsLifetimeCharacters));

        // Load any previously cached ElevenLabs voices
        ElevenLabsVoices.Clear();
        foreach (var v in _elevenLabs.GetAvailableVoices())
            ElevenLabsVoices.Add(v);

        LoadVoices();
    }

    public void ApplyTo(VoiceSettings s)
    {
        s.Engine = _engine;
        s.EngineName = _engine.ToString();
        s.GlobalPitch = _globalPitch;
        // Always persist the Kokoro voice ID — when ElevenLabs is active the ComboBox
        // binding clears SelectedVoiceId from the VM, so use the saved copy.
        var kokoroId = _engine == VoiceEngine.ElevenLabs
            ? (_savedKokoroVoiceId.Length > 0 ? _savedKokoroVoiceId : "af_heart")
            : (SelectedVoiceId.Length > 0 ? SelectedVoiceId : "af_heart");
        s.SelectedVoiceId = kokoroId;
        s.SelectedVoiceDisplayName = _kokoro.GetAvailableVoices()
            .FirstOrDefault(v => v.Id == kokoroId)?.DisplayName ?? string.Empty;
        ApplyToConfig();
    }

    /// <summary>Persists voice engine selection and ElevenLabs settings into live config without a full save.</summary>
    private void ApplyToConfig()
    {
        // Must write the selected engine first so TtsRouter routes to the correct provider during test playback.
        _config.CurrentConfig.VoiceSettings.Engine = _engine;

        var el = _config.CurrentConfig.ElevenLabs;
        el.ApiKey = ElevenLabsApiKey;
        el.ModelId = ElevenLabsModelId;
        el.SelectedVoiceId = ElevenLabsSelectedVoiceId;
        el.SelectedVoiceName = ElevenLabsSelectedVoiceName;
    }
}

