using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Tts;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

/// <summary>A model option shown in the ElevenLabs Model dropdown.</summary>
public sealed record ElevenLabsModelOption(string Id, string DisplayName);

/// <summary>Tracks which sub-panel is shown for the ElevenLabs API key entry area.</summary>
public enum ApiKeyEntryState { Empty, Editing, Saved, Clearing }

public sealed class VoiceSettingsViewModel : ViewModelBase
{
    private readonly ITtsService _tts;
    private readonly KokoroTtsService _kokoro;
    private readonly ElevenLabsTtsService _elevenLabs;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly INotificationService _notifications;

    // ── Kokoro ───────────────────────────────────────────────────────────────
    private string _selectedVoiceId = string.Empty;
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

    // ── Global speed ─────────────────────────────────────────────────────────
    private float _globalSpeed = 1.0f;
    /// <summary>Global playback speed for standard TTS (not phrases), applied live.</summary>
    public float GlobalSpeed
    {
        get => _globalSpeed;
        set
        {
            var clamped = Math.Clamp(value, 0.5f, 2.0f);
            if (SetField(ref _globalSpeed, clamped))
            {
                _config.CurrentConfig.VoiceSettings.GlobalSpeed = clamped;
                OnPropertyChanged(nameof(GlobalSpeedPercent));
            }
        }
    }

    public string GlobalSpeedPercent => $"{(int)(_globalSpeed * 100)}%";

    // ── Engine selection ─────────────────────────────────────────────────────
    private VoiceEngine _engine = VoiceEngine.Kokoro;
    public VoiceEngine Engine
    {
        get => _engine;
        set
        {
            if (_engine == value) return;
            SetField(ref _engine, value);
            OnPropertyChanged(nameof(IsKokoro));
            OnPropertyChanged(nameof(IsElevenLabs));

            // Auto-fetch voices + subscription when switching to ElevenLabs
            if (_engine == VoiceEngine.ElevenLabs && _elevenLabs.HasApiKey())
                _ = FetchElevenLabsVoicesAsync();
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
    /// <summary>
    /// Permanent Kokoro voice list — populated once at load, never cleared on engine switch.
    /// Bound exclusively to the Kokoro voice ComboBox so switching to ElevenLabs never corrupts it.
    /// </summary>
    public ObservableCollection<VoiceInfo> KokoroVoices { get; } = new();

    // Kept for any callers that still reference AvailableVoices.
    public ObservableCollection<VoiceInfo> AvailableVoices => KokoroVoices;

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

    // ── ElevenLabs API key state machine ─────────────────────────────────────
    private ApiKeyEntryState _apiKeyState = ApiKeyEntryState.Empty;
    private string _pendingApiKey     = string.Empty;  // typed in PasswordBox, not yet saved
    private bool   _apiKeyPriorSaved  = false;         // used by Cancel to restore correct prior state

    public ApiKeyEntryState ApiKeyState
    {
        get => _apiKeyState;
        private set
        {
            if (SetField(ref _apiKeyState, value))
            {
                OnPropertyChanged(nameof(IsApiKeySaved));
                OnPropertyChanged(nameof(IsApiKeyEditing));
                OnPropertyChanged(nameof(IsApiKeyClearing));
                OnPropertyChanged(nameof(IsApiKeyEmpty));
                OnPropertyChanged(nameof(IsApiKeyInputShown));
                OnPropertyChanged(nameof(CanSaveApiKey));
                OnPropertyChanged(nameof(IsTestingKeyUnsaved));
                OnPropertyChanged(nameof(ApiKeyMaskedDisplay));
                RefreshApiKeyCommands();
            }
        }
    }

    public bool IsApiKeySaved      => _apiKeyState == ApiKeyEntryState.Saved;
    public bool IsApiKeyEditing    => _apiKeyState == ApiKeyEntryState.Editing;
    public bool IsApiKeyClearing   => _apiKeyState == ApiKeyEntryState.Clearing;
    public bool IsApiKeyEmpty      => _apiKeyState == ApiKeyEntryState.Empty;
    /// <summary>True when the PasswordBox input panel should be visible (Empty or Editing states).</summary>
    public bool IsApiKeyInputShown => _apiKeyState == ApiKeyEntryState.Empty || _apiKeyState == ApiKeyEntryState.Editing;

    /// <summary>True when a pending key has been typed and can be saved.</summary>
    public bool CanSaveApiKey => IsApiKeyInputShown && !string.IsNullOrWhiteSpace(_pendingApiKey);
    /// <summary>True when the key being tested is not the stored encrypted one (button label changes).</summary>
    public bool IsTestingKeyUnsaved => _apiKeyState != ApiKeyEntryState.Saved;
    /// <summary>"sk_...a1b2   Updated 2026-05-03" when a key is saved; empty otherwise.</summary>
    public string ApiKeyMaskedDisplay => _elevenLabs.GetApiKeyMaskedDisplay();

    /// <summary>Called from PasswordBox.PasswordChanged in code-behind (PasswordBox cannot data-bind).</summary>
    public void SetPendingApiKey(string key)
    {
        _pendingApiKey = key;
        OnPropertyChanged(nameof(CanSaveApiKey));
    }

    /// <summary>Raised when the PasswordBox should be cleared (Cancel / after Save).</summary>
    public event EventHandler? RequestPasswordBoxClear;

    // ── ElevenLabs other props ────────────────────────────────────────────────
    private string _elevenLabsModelId = "eleven_multilingual_v2";
    private string _elevenLabsSelectedVoiceId = string.Empty;
    private string _elevenLabsSelectedVoiceName = string.Empty;
    private string _elevenLabsStatus = string.Empty;
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
    public ICommand TestVoiceCommand              { get; }
    public ICommand FetchElevenLabsVoicesCommand  { get; }
    public ICommand SaveApiKeyCommand             { get; }
    public ICommand TestApiKeyCommand             { get; }
    public ICommand StartEditApiKeyCommand        { get; }
    public ICommand ClearApiKeyCommand            { get; }
    public ICommand ConfirmClearApiKeyCommand     { get; }
    public ICommand CancelApiKeyCommand           { get; }

    public VoiceSettingsViewModel(
        ITtsService tts,
        KokoroTtsService kokoro,
        ElevenLabsTtsService elevenLabs,
        IAudioRouterService audioRouter,
        IConfigService config,
        INotificationService notifications,
        ILoggingService log)
    {
        _tts           = tts;
        _kokoro        = kokoro;
        _elevenLabs    = elevenLabs;
        _audioRouter   = audioRouter;
        _config        = config;
        _notifications = notifications;
        _log           = log;

        TestVoiceCommand             = new AsyncRelayCommand(TestVoiceAsync);
        FetchElevenLabsVoicesCommand = new AsyncRelayCommand(FetchElevenLabsVoicesAsync);
        SaveApiKeyCommand            = new AsyncRelayCommand(SaveApiKeyAsync);
        TestApiKeyCommand            = new AsyncRelayCommand(TestApiKeyExecuteAsync);
        StartEditApiKeyCommand       = new RelayCommand(StartEditApiKey);
        ClearApiKeyCommand           = new RelayCommand(BeginClearApiKey);
        ConfirmClearApiKeyCommand    = new RelayCommand(ConfirmClearApiKey);
        CancelApiKeyCommand          = new RelayCommand(CancelApiKey);

        PopulateKokoroVoices();
    }

    /// <summary>
    /// Fills <see cref="KokoroVoices"/> from the Kokoro service.
    /// Safe to call multiple times; clears and repopulates.
    /// Never touches ElevenLabsVoices or engine state.
    /// </summary>
    private void PopulateKokoroVoices()
    {
        KokoroVoices.Clear();
        foreach (var v in _kokoro.GetAvailableVoices())
            KokoroVoices.Add(v);
    }

    private async Task FetchElevenLabsVoicesAsync()
    {
        ElevenLabsStatus = "Fetching voices…";

        var voices = await _elevenLabs.FetchVoicesAsync();

        // Snapshot before clearing: the ElevenLabs ComboBox has a TwoWay SelectedValue binding.
        // When the collection is cleared the ComboBox can't match the current ID against an empty
        // list and writes null/empty back through the binding — destroying the selection before
        // new items are added. Capture the ID now and restore it after repopulation.
        var savedVoiceId = ElevenLabsSelectedVoiceId;

        ElevenLabsVoices.Clear();
        foreach (var v in voices)
            ElevenLabsVoices.Add(v);

        // Restore — TwoWay binding may have cleared it during the Clear() call above.
        if (!string.IsNullOrEmpty(savedVoiceId))
            ElevenLabsSelectedVoiceId = savedVoiceId;

        if (voices.Count > 0)
        {
            if (ElevenLabsVoices.All(v => v.Id != ElevenLabsSelectedVoiceId))
            {
                // Saved voice is no longer in the account's voice list — fall back to first available.
                ElevenLabsSelectedVoiceId = voices[0].Id;
                ElevenLabsSelectedVoiceName = voices[0].DisplayName;
            }
            else
            {
                // Saved voice is valid — sync the display name in case it changed on EL side.
                var matched = voices.FirstOrDefault(v => v.Id == ElevenLabsSelectedVoiceId);
                if (matched is not null)
                    ElevenLabsSelectedVoiceName = matched.DisplayName;
            }
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
            Pitch = Math.Clamp(_globalPitch, 0.5f, 2.0f),
            Speed = Math.Clamp(_globalSpeed, 0.5f, 2.0f)
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
        // Always populate Kokoro voices from the singleton service (always available offline).
        PopulateKokoroVoices();

        // Restore Kokoro voice selection directly from config — no in-memory tracking needed.
        SelectedVoiceId = s.SelectedVoiceId;
        EngineName = s.EngineName;
        _engine = s.Engine;   // set backing field directly to avoid triggering Engine setter side-effects
        OnPropertyChanged(nameof(Engine));
        OnPropertyChanged(nameof(IsKokoro));
        OnPropertyChanged(nameof(IsElevenLabs));

        // Load pitch — do NOT write back to config to avoid dirty the snapshot
        _globalPitch = Math.Clamp(s.GlobalPitch, 0.5f, 2.0f);
        OnPropertyChanged(nameof(GlobalPitch));
        OnPropertyChanged(nameof(GlobalPitchPercent));

        _globalSpeed = Math.Clamp(s.GlobalSpeed, 0.5f, 2.0f);
        OnPropertyChanged(nameof(GlobalSpeed));
        OnPropertyChanged(nameof(GlobalSpeedPercent));

        var el = _config.CurrentConfig.ElevenLabs;

        // One-time migration: if a plain-text key was saved before v0.13, encrypt it now.
        if (_elevenLabs.MigrateLegacyApiKey())
            _notifications.ShowInfo("API key re-encrypted for secure storage.");

        // Initialise API key display state
        _apiKeyPriorSaved = _elevenLabs.HasApiKey();
        ApiKeyState = _apiKeyPriorSaved ? ApiKeyEntryState.Saved : ApiKeyEntryState.Empty;

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

        // If ElevenLabs is the active engine and we have an API key, trigger a background
        // fetch so the voice list is populated even on first open after a fresh process start.
        if (_engine == VoiceEngine.ElevenLabs && _elevenLabs.HasApiKey() && ElevenLabsVoices.Count == 0)
            _ = FetchElevenLabsVoicesAsync();
    }

    public void ApplyTo(VoiceSettings s)
    {
        s.Engine = _engine;
        s.EngineName = _engine.ToString();
        s.GlobalPitch = _globalPitch;
        s.GlobalSpeed = _globalSpeed;
        // SelectedVoiceId always holds the Kokoro voice regardless of which engine is active,
        // because KokoroVoices is a separate collection that is never cleared on engine switch.
        var kokoroId = SelectedVoiceId.Length > 0 ? SelectedVoiceId : "af_heart";
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
        el.ModelId = ElevenLabsModelId;
        el.SelectedVoiceId = ElevenLabsSelectedVoiceId;
        el.SelectedVoiceName = ElevenLabsSelectedVoiceName;
    }

    // ── API key command handlers ──────────────────────────────────────────────

    private async Task SaveApiKeyAsync()
    {
        if (string.IsNullOrWhiteSpace(_pendingApiKey)) return;

        _elevenLabs.SaveApiKey(_pendingApiKey);
        _pendingApiKey = string.Empty;
        RequestPasswordBoxClear?.Invoke(this, EventArgs.Empty);

        _apiKeyPriorSaved = true;
        ApiKeyState = ApiKeyEntryState.Saved;
        _notifications.ShowSuccess("API key stored securely.");

        // Auto-fetch voices now that the key is saved.
        await FetchElevenLabsVoicesAsync();
    }

    private async Task TestApiKeyExecuteAsync()
    {
        if (_apiKeyState == ApiKeyEntryState.Saved)
        {
            ElevenLabsStatus = "Testing saved key\u2026";
            var (ok, message) = await _elevenLabs.TestSavedApiKeyAsync();
            ElevenLabsStatus = message;
            if (ok) _notifications.ShowSuccess(message);
            else    _notifications.ShowError(message);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_pendingApiKey))
            {
                ElevenLabsStatus = "Enter an API key first.";
                return;
            }
            ElevenLabsStatus = "Testing unsaved key\u2026";
            var (ok, message) = await _elevenLabs.TestApiKeyAsync(_pendingApiKey);
            ElevenLabsStatus = message;
            if (ok) _notifications.ShowSuccess(message);
            else    _notifications.ShowError(message);
        }
    }

    private void StartEditApiKey()
    {
        _apiKeyPriorSaved = _apiKeyState == ApiKeyEntryState.Saved;
        _pendingApiKey = string.Empty;
        RequestPasswordBoxClear?.Invoke(this, EventArgs.Empty);
        ApiKeyState = ApiKeyEntryState.Editing;
    }

    private void BeginClearApiKey()
    {
        ApiKeyState = ApiKeyEntryState.Clearing;
    }

    private void ConfirmClearApiKey()
    {
        _elevenLabs.ClearApiKey();
        _apiKeyPriorSaved = false;
        _pendingApiKey    = string.Empty;
        ApiKeyState = ApiKeyEntryState.Empty;
        ElevenLabsVoices.Clear();
        ElevenLabsStatus  = string.Empty;
        _subCharUsed  = 0;
        _subCharLimit = 0;
        OnPropertyChanged(nameof(ElevenLabsCreditsDisplay));
        _notifications.ShowInfo("API key removed.");
    }

    private void CancelApiKey()
    {
        _pendingApiKey = string.Empty;
        RequestPasswordBoxClear?.Invoke(this, EventArgs.Empty);
        ApiKeyState = _apiKeyPriorSaved ? ApiKeyEntryState.Saved : ApiKeyEntryState.Empty;
    }

    /// <summary>Forces all API key command CanExecute re-evaluations.</summary>
    private void RefreshApiKeyCommands()
    {
        System.Windows.Input.CommandManager.InvalidateRequerySuggested();
    }
}

