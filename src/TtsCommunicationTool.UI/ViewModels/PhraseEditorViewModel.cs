using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Infrastructure.Tts;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public enum PhraseEditorResult { None, Saved, Deleted }

/// <summary>
/// ViewModel for the Phrase Editor modal window.
/// Handles a single phrase create/edit session including per-phrase TTS preview,
/// voice override selection, and cache generation on save.
/// </summary>
public sealed class PhraseEditorViewModel : ViewModelBase
{
    private readonly IPhraseService    _phraseService;
    private readonly IPhraseCacheService _phraseCache;
    private readonly KokoroTtsService    _kokoro;
    private readonly ElevenLabsTtsService _elevenLabs;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService    _config;
    private readonly IHotkeyHost       _hotkeyHost;
    private readonly ILoggingService   _log;

    // ── Session flags ─────────────────────────────────────────────────────────
    private bool   _isNewPhrase;
    private string _phraseId    = string.Empty;
    private bool   _isGenerating;
    private bool   _isPlaying;

    // ── In-memory preview cache ───────────────────────────────────────────────
    // Stores the last synthesized audio for the CURRENT settings (text + engine +
    // voice + pitch). Set on first Play Preview or Regen Cache. Cleared whenever
    // any setting that affects synthesis is changed. This guarantees that Play
    // Preview never calls a TTS engine a second time — ElevenLabs credits are
    // spent only on the first preview (or an explicit Regen Cache).
    private PlaybackRequest? _previewCache;
    private bool             _previewCacheValid;   // true = _previewCache matches current settings

    // ── Phrase core fields ────────────────────────────────────────────────────
    private string _name     = string.Empty;
    private string _text     = string.Empty;
    private string _category = string.Empty;
    private bool   _isFavorite;
    private bool   _isPinned;
    private HotkeyBinding? _hotkey;

    // ── Voice override fields ─────────────────────────────────────────────────
    private VoiceEngine _editorEngine     = VoiceEngine.Kokoro;
    private bool        _useVoiceOverride;
    private string      _overrideVoiceId   = string.Empty;
    private string      _overrideVoiceName = string.Empty;
    private float       _pitch             = 1.0f;

    // ── Status ────────────────────────────────────────────────────────────────
    private string _statusText  = "Ready to preview.";
    private string _cacheStatus = string.Empty;

    // ── Public result ─────────────────────────────────────────────────────────
    /// <summary>Outcome of the editor session. Check this after ShowDialog() returns.</summary>
    public PhraseEditorResult Result { get; private set; } = PhraseEditorResult.None;

    /// <summary>ID of the phrase that was saved or deleted.</summary>
    public string PhraseId => _phraseId;

    /// <summary>True when the editor was opened with no existing phrase (creating a new one).</summary>
    public bool IsNewPhrase => _isNewPhrase;

    /// <summary>True when editing an existing phrase (not new).</summary>
    public bool IsExistingPhrase => !_isNewPhrase;

    /// <summary>Confirm-before-delete callback injected by the view.</summary>
    public Func<string, bool>? ConfirmDelete { get; set; }

    /// <summary>Raised when Save, Cancel, or Delete finishes — the window should close.</summary>
    public event EventHandler? CloseRequested;

    // ── Window geometry (backed by IConfigService; written to disk on next global save) ────────
    public double WindowWidth
    {
        get => Math.Max(480, _config.CurrentConfig.GeneralSettings.PhraseEditorWindowWidth);
        set => _config.CurrentConfig.GeneralSettings.PhraseEditorWindowWidth = value;
    }

    public double WindowHeight
    {
        get => Math.Max(660, _config.CurrentConfig.GeneralSettings.PhraseEditorWindowHeight);
        set => _config.CurrentConfig.GeneralSettings.PhraseEditorWindowHeight = value;
    }

    // ── Voice / category lists ────────────────────────────────────────────────
    public ObservableCollection<VoiceInfo> KokoroVoices      { get; } = new();
    public ObservableCollection<VoiceInfo> ElevenLabsVoices  { get; } = new();
    public ObservableCollection<string>    Categories         { get; } = new();

    // ── Phrase field properties ───────────────────────────────────────────────
    public string Name
    {
        get => _name;
        set { if (SetField(ref _name, value)) OnPropertyChanged(nameof(CanSave)); }
    }

    public string Text
    {
        get => _text;
        set
        {
            if (SetField(ref _text, value))
            {
                OnPropertyChanged(nameof(TextLength));
                OnPropertyChanged(nameof(CostEstimate));
                OnPropertyChanged(nameof(CanSave));
                OnPropertyChanged(nameof(CanPreview));
                InvalidatePreviewCache();
            }
        }
    }

    public string Category
    {
        get => _category;
        set => SetField(ref _category, value);
    }

    public bool IsFavorite
    {
        get => _isFavorite;
        set => SetField(ref _isFavorite, value);
    }

    public bool IsPinned
    {
        get => _isPinned;
        set => SetField(ref _isPinned, value);
    }

    public HotkeyBinding? Hotkey
    {
        get => _hotkey;
        set { if (SetField(ref _hotkey, value)) OnPropertyChanged(nameof(HotkeyDisplay)); }
    }

    public string HotkeyDisplay
    {
        get
        {
            if (_hotkey is null) return "(none)";
            var parts = new List<string>();
            if (_hotkey.Ctrl)  parts.Add("Ctrl");
            if (_hotkey.Alt)   parts.Add("Alt");
            if (_hotkey.Shift) parts.Add("Shift");
            if (!string.IsNullOrEmpty(_hotkey.Key)) parts.Add(_hotkey.Key);
            return parts.Count > 0 ? string.Join("+", parts) : "(none)";
        }
    }

    public int TextLength => _text.Length;

    // ── Engine / voice properties ─────────────────────────────────────────────
    public VoiceEngine EditorEngine
    {
        get => _editorEngine;
        set
        {
            if (_editorEngine == value) return;
            SetField(ref _editorEngine, value);
            OnPropertyChanged(nameof(IsKokoro));
            OnPropertyChanged(nameof(IsElevenLabs));
            OnPropertyChanged(nameof(CostEstimate));
            InvalidatePreviewCache();
        }
    }

    public bool IsKokoro
    {
        get => _editorEngine == VoiceEngine.Kokoro;
        set { if (value) EditorEngine = VoiceEngine.Kokoro; }
    }

    public bool IsElevenLabs
    {
        get => _editorEngine == VoiceEngine.ElevenLabs;
        set { if (value) EditorEngine = VoiceEngine.ElevenLabs; }
    }

    public bool UseVoiceOverride
    {
        get => _useVoiceOverride;
        set
        {
            if (SetField(ref _useVoiceOverride, value))
            {
                OnPropertyChanged(nameof(UseDefaultVoice));
                OnPropertyChanged(nameof(IsOverrideVoiceEnabled));
                InvalidatePreviewCache();
            }
        }
    }

    public bool UseDefaultVoice
    {
        get => !_useVoiceOverride;
        set { if (value) UseVoiceOverride = false; }
    }

    public bool IsOverrideVoiceEnabled => _useVoiceOverride;

    public string OverrideVoiceId
    {
        get => _overrideVoiceId;
        set
        {
            if (SetField(ref _overrideVoiceId, value))
            {
                // Sync display name from the active voice list
                var voices = _editorEngine == VoiceEngine.ElevenLabs
                    ? (IEnumerable<VoiceInfo>)ElevenLabsVoices
                    : KokoroVoices;
                var match = voices.FirstOrDefault(v => v.Id == value);
                if (match is not null) OverrideVoiceName = match.DisplayName;
                InvalidatePreviewCache();
            }
        }
    }

    public string OverrideVoiceName
    {
        get => _overrideVoiceName;
        set => SetField(ref _overrideVoiceName, value);
    }

    public float Pitch
    {
        get => _pitch;
        set
        {
            var clamped = Math.Clamp(value, 0.5f, 2.0f);
            if (SetField(ref _pitch, clamped))
            {
                OnPropertyChanged(nameof(PitchDisplay));
                OnPropertyChanged(nameof(PitchPercent));
                InvalidatePreviewCache();
            }
        }
    }

    public string PitchDisplay  => _pitch.ToString("F2");
    public string PitchPercent  => $"{(int)(_pitch * 100)}%";

    // ── Playback state ────────────────────────────────────────────────────────
    public bool IsGenerating
    {
        get => _isGenerating;
        private set { if (SetField(ref _isGenerating, value)) NotifyPlaybackStateChanged(); }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set { if (SetField(ref _isPlaying, value)) NotifyPlaybackStateChanged(); }
    }

    public bool CanPreview => !_isGenerating && !string.IsNullOrWhiteSpace(_text);
    public bool CanStop    => _isPlaying || _isGenerating;
    public bool CanSave    => !string.IsNullOrWhiteSpace(_name) && !string.IsNullOrWhiteSpace(_text);

    // ── Status / display ──────────────────────────────────────────────────────
    public string StatusText
    {
        get => _statusText;
        private set => SetField(ref _statusText, value);
    }

    public string CacheStatus
    {
        get => _cacheStatus;
        private set => SetField(ref _cacheStatus, value);
    }

    /// <summary>Approximate ElevenLabs credit cost for current text. Empty when Kokoro is active.</summary>
    public string CostEstimate
    {
        get
        {
            if (_editorEngine != VoiceEngine.ElevenLabs || string.IsNullOrEmpty(_text))
                return string.Empty;
            double cost = _text.Length * 0.0001;   // ~$0.0001/char at EL standard rate
            return $"≈${cost:F4}";
        }
    }

    // ── Commands ──────────────────────────────────────────────────────────────
    public ICommand PlayPreviewCommand { get; }
    public ICommand StopCommand        { get; }
    public ICommand RegenVoiceCommand  { get; }
    public ICommand ClearHotkeyCommand { get; }
    public ICommand SaveCommand        { get; }
    public ICommand CancelCommand      { get; }
    public ICommand DeleteCommand      { get; }

    public PhraseEditorViewModel(
        IPhraseService       phraseService,
        IPhraseCacheService  phraseCache,
        KokoroTtsService     kokoro,
        ElevenLabsTtsService elevenLabs,
        IAudioRouterService  audioRouter,
        IConfigService       config,
        IHotkeyHost          hotkeyHost,
        ILoggingService      log)
    {
        _phraseService = phraseService;
        _phraseCache   = phraseCache;
        _kokoro        = kokoro;
        _elevenLabs    = elevenLabs;
        _audioRouter   = audioRouter;
        _config        = config;
        _hotkeyHost    = hotkeyHost;
        _log           = log;

        PlayPreviewCommand = new AsyncRelayCommand(PlayPreviewAsync, () => CanPreview);
        StopCommand        = new RelayCommand(StopPlayback, () => CanStop);
        RegenVoiceCommand  = new AsyncRelayCommand(RegenVoiceAsync,  () => CanPreview);
        ClearHotkeyCommand = new RelayCommand(() => { Hotkey = null; });
        SaveCommand        = new AsyncRelayCommand(SaveAsync,    () => CanSave);
        CancelCommand      = new RelayCommand(Cancel);
        DeleteCommand      = new RelayCommand(DeletePhrase, () => !_isNewPhrase);
    }

    // ── Lifecycle (called from window code-behind) ────────────────────────────

    /// <summary>Suppress global hotkeys while the editor is open.</summary>
    public void BeginEditing()
        => _hotkeyHost.SuppressPhraseAndOverlayHotkeys = true;

    /// <summary>Restore global hotkeys and stop any in-progress audio when the editor closes.</summary>
    public void EndEditing()
    {
        _hotkeyHost.SuppressPhraseAndOverlayHotkeys = false;
        StopPlayback();
    }

    /// <summary>Called from the window code-behind after capturing a key combination.</summary>
    public void SetHotkey(HotkeyBinding binding) => Hotkey = binding;

    // ── Initialization ────────────────────────────────────────────────────────

    /// <summary>
    /// Configures the editor for a new phrase (phrase = null) or an existing one.
    /// Call before showing the window.
    /// </summary>
    public void Initialize(PhraseItem? phrase, IEnumerable<string>? existingCategories = null)
    {
        _isNewPhrase = phrase is null;

        // Populate voice lists from the singleton services (already have cached voices)
        KokoroVoices.Clear();
        foreach (var v in _kokoro.GetAvailableVoices())
            KokoroVoices.Add(v);

        ElevenLabsVoices.Clear();
        foreach (var v in _elevenLabs.GetAvailableVoices())
            ElevenLabsVoices.Add(v);

        Categories.Clear();
        if (existingCategories is not null)
            foreach (var c in existingCategories.Where(c => !string.IsNullOrEmpty(c)))
                Categories.Add(c);

        if (phrase is null)
        {
            _phraseId = Guid.NewGuid().ToString("N");
            Name             = string.Empty;
            Text             = string.Empty;
            Category         = string.Empty;
            IsFavorite       = false;
            IsPinned         = false;
            Hotkey           = null;
            _editorEngine    = _config.CurrentConfig.VoiceSettings.Engine;
            _useVoiceOverride  = false;
            _overrideVoiceId   = string.Empty;
            _overrideVoiceName = string.Empty;
            _pitch             = 1.0f;
            _previewCache      = null;
            _previewCacheValid = false;
        }
        else
        {
            _phraseId = phrase.Id;
            Name             = phrase.Name;
            Text             = phrase.Text;
            Category         = phrase.Category ?? string.Empty;
            IsFavorite       = phrase.IsFavorite;
            IsPinned         = phrase.IsPinned;
            Hotkey           = phrase.Hotkey;
            _editorEngine    = phrase.OverrideEngine ?? _config.CurrentConfig.VoiceSettings.Engine;
            _useVoiceOverride  = phrase.UseVoiceOverride;
            _overrideVoiceId   = phrase.OverrideVoiceId   ?? string.Empty;
            _overrideVoiceName = phrase.OverrideVoiceName ?? string.Empty;
            _pitch             = phrase.OverridePitch ?? 1.0f;

            // Pre-load the on-disk cache into memory so the first Play Preview
            // is instant and costs zero credits — no synthesis needed.
            _previewCache      = _phraseCache.GetCachedAudio(_phraseId);
            _previewCacheValid = _previewCache is not null;
        }

        // Notify all derived properties after bulk set
        OnPropertyChanged(nameof(EditorEngine));
        OnPropertyChanged(nameof(IsKokoro));
        OnPropertyChanged(nameof(IsElevenLabs));
        OnPropertyChanged(nameof(UseVoiceOverride));
        OnPropertyChanged(nameof(UseDefaultVoice));
        OnPropertyChanged(nameof(IsOverrideVoiceEnabled));
        OnPropertyChanged(nameof(OverrideVoiceId));
        OnPropertyChanged(nameof(OverrideVoiceName));
        OnPropertyChanged(nameof(Pitch));
        OnPropertyChanged(nameof(PitchDisplay));
        OnPropertyChanged(nameof(PitchPercent));
        OnPropertyChanged(nameof(HotkeyDisplay));
        OnPropertyChanged(nameof(IsNewPhrase));
        OnPropertyChanged(nameof(IsExistingPhrase));
        OnPropertyChanged(nameof(TextLength));
        OnPropertyChanged(nameof(CostEstimate));

        RefreshCacheStatus();
        StatusText = "Ready to preview.";
        NotifyPlaybackStateChanged();
    }

    // ── Preview / regen ───────────────────────────────────────────────────────

    private async Task PlayPreviewAsync()
    {
        if (string.IsNullOrWhiteSpace(_text)) return;

        // Stop any in-progress audio first
        _audioRouter.StopAll();
        IsGenerating = true;
        IsPlaying    = false;

        var cfg = _config.CurrentConfig;

        // ── Check cache first ─────────────────────────────────────────────────
        // Using cached audio avoids re-synthesizing on every click, which is
        // critical for ElevenLabs (each synthesis consumes credits). Cache is
        // written on the first preview and reused until explicitly regenerated
        // via "Regen Cache" or until the phrase is saved with changed settings.
        var cached = _phraseCache.GetCachedAudio(_phraseId);
        if (cached is not null)
        {
            IsGenerating = false;
            IsPlaying    = true;
            StatusText   = "Playing preview (cached)…";
            try
            {
                await _audioRouter.PlayAsync(
                    cached,
                    cfg.AudioSettings.MonitorOutputDeviceId,
                    null,
                    cfg.AudioSettings.MonitorVolume,
                    0);
                StatusText = "Ready to preview.";
            }
            catch (Exception ex)
            {
                StatusText = $"Playback error: {ex.Message}";
                _log.Error("PhraseEditor cached playback exception", ex);
            }
            finally
            {
                IsGenerating = false;
                IsPlaying    = false;
            }
            return;
        }

        // ── No cache — synthesize, write cache, then play ─────────────────────
        StatusText = _editorEngine == VoiceEngine.ElevenLabs
            ? "Synthesizing via ElevenLabs (credits will be used)…"
            : "Generating preview…";

        try
        {
            var request = BuildPreviewRequest();
            var svc     = GetActiveService();

            var result = await svc.SynthesizeAsync(request);
            if (!result.Success || result.AudioData is null)
            {
                StatusText = $"Preview failed: {result.ErrorMessage}";
                _log.Warn($"PhraseEditor preview failed: {result.ErrorMessage}");
                return;
            }

            // Write to cache so all subsequent previews are free.
            WriteWavCache(_phraseId, result);
            RefreshCacheStatus();

            IsGenerating = false;
            IsPlaying    = true;
            StatusText   = "Playing preview…";

            var playback = new PlaybackRequest
            {
                AudioData     = result.AudioData,
                SampleRate    = result.SampleRate,
                Channels      = result.Channels,
                BitsPerSample = result.BitsPerSample
            };

            // Preview → monitor (headphones) only; no secondary output.
            await _audioRouter.PlayAsync(
                playback,
                cfg.AudioSettings.MonitorOutputDeviceId,
                null,
                cfg.AudioSettings.MonitorVolume,
                0);

            StatusText = "Ready to preview.";
        }
        catch (Exception ex)
        {
            StatusText = $"Preview error: {ex.Message}";
            _log.Error("PhraseEditor preview exception", ex);
        }
        finally
        {
            IsGenerating = false;
            IsPlaying    = false;
        }
    }

    private void StopPlayback()
    {
        _audioRouter.StopAll();
        IsPlaying    = false;
        IsGenerating = false;
        StatusText   = "Ready to preview.";
    }

    private async Task RegenVoiceAsync()
    {
        if (string.IsNullOrWhiteSpace(_text)) return;

        IsGenerating = true;
        StatusText   = "Generating audio cache…";

        try
        {
            var request = BuildPreviewRequest();
            var svc     = GetActiveService();
            var result  = await svc.SynthesizeAsync(request);

            if (!result.Success || result.AudioData is null)
            {
                StatusText = $"Regen failed: {result.ErrorMessage}";
                return;
            }

            WriteWavCache(_phraseId, result);
            RefreshCacheStatus();
            StatusText  = "Cache updated.";
            _log.Info($"PhraseEditor: voice cache regenerated for phrase '{_name}'.");
        }
        catch (Exception ex)
        {
            StatusText = $"Regen error: {ex.Message}";
            _log.Error("PhraseEditor regen exception", ex);
        }
        finally
        {
            IsGenerating = false;
        }
    }

    // ── Save / Cancel / Delete ────────────────────────────────────────────────

    private async Task SaveAsync()
    {
        if (!CanSave) return;

        // Stop any audio before committing
        _audioRouter.StopAll();

        var phrase = BuildPhraseItem();

        if (_isNewPhrase)
        {
            var addResult = _phraseService.Add(phrase);
            if (!addResult.Success)
            {
                StatusText = $"Save failed: {addResult.ErrorMessage}";
                return;
            }
        }
        else
        {
            // skipCacheRegen=true: we manage cache ourselves below to avoid a race.
            _phraseService.Update(phrase, skipCacheRegen: true);
            // Invalidate stale cache before regenerating
            _phraseCache.DeleteCache(phrase.Id);
        }

        StatusText = "Generating audio cache…";
        await _phraseCache.GenerateCacheAsync(phrase);

        Result = PhraseEditorResult.Saved;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Cancel()
    {
        StopPlayback();
        Result = PhraseEditorResult.None;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void DeletePhrase()
    {
        if (_isNewPhrase) return;
        if (ConfirmDelete is not null && !ConfirmDelete(_name)) return;

        _audioRouter.StopAll();
        _phraseCache.DeleteCache(_phraseId);
        _phraseService.Delete(_phraseId);
        Result = PhraseEditorResult.Deleted;
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private ITtsService GetActiveService() =>
        _editorEngine == VoiceEngine.ElevenLabs ? _elevenLabs : _kokoro;

    private TtsRequest BuildPreviewRequest()
    {
        var voiceId = string.Empty;
        if (_editorEngine == VoiceEngine.Kokoro)
        {
            voiceId = (_useVoiceOverride && !string.IsNullOrEmpty(_overrideVoiceId))
                ? _overrideVoiceId
                : _config.CurrentConfig.VoiceSettings.SelectedVoiceId;
        }
        else
        {
            // ElevenLabs: pass override if set; service uses its own configured default otherwise.
            voiceId = (_useVoiceOverride && !string.IsNullOrEmpty(_overrideVoiceId))
                ? _overrideVoiceId
                : string.Empty;
        }
        return new TtsRequest
        {
            Text    = _text,
            VoiceId = voiceId,
            Pitch   = Math.Clamp(_pitch, 0.5f, 2.0f)
        };
    }

    private PhraseItem BuildPhraseItem()
    {
        var existing   = _isNewPhrase ? null : _phraseService.GetById(_phraseId);
        var globalEngine = _config.CurrentConfig.VoiceSettings.Engine;

        return new PhraseItem
        {
            Id         = _phraseId,
            Name       = _name.Trim(),
            Text       = _text.Trim(),
            Category   = NormalizeCategory(_category),
            IsFavorite = _isFavorite,
            IsPinned   = _isPinned,
            Hotkey     = _hotkey,
            SortOrder  = existing?.SortOrder ?? _phraseService.GetAll().Count,
            CreatedUtc = existing?.CreatedUtc ?? DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            // Only store engine override when it differs from global config
            OverrideEngine     = _editorEngine != globalEngine ? _editorEngine : null,
            UseVoiceOverride   = _useVoiceOverride,
            OverrideVoiceId    = _useVoiceOverride && !string.IsNullOrEmpty(_overrideVoiceId)
                                     ? _overrideVoiceId : null,
            OverrideVoiceName  = _useVoiceOverride && !string.IsNullOrEmpty(_overrideVoiceName)
                                     ? _overrideVoiceName : null,
            // Only store pitch override when it's meaningfully different from neutral
            OverridePitch      = Math.Abs(_pitch - 1.0f) > 0.005f ? _pitch : null
        };
    }

    /// <summary>
    /// True when the in-memory preview cache is valid for the current settings.
    /// Bound by XAML to colour the cache status indicator green vs amber.
    /// </summary>
    public bool HasEditorCache => _previewCacheValid;

    /// <summary>
    /// Clears the in-memory preview cache and updates the cache status indicator.
    /// Called whenever any setting that affects synthesis is changed.
    /// </summary>
    private void InvalidatePreviewCache()
    {
        _previewCache      = null;
        _previewCacheValid = false;
        RefreshCacheStatus();
    }

    private void RefreshCacheStatus()
    {
        OnPropertyChanged(nameof(HasEditorCache));
        if (_isNewPhrase)
        {
            CacheStatus = _previewCacheValid
                ? "\u2713 Cached \u2014 Play Preview will use stored audio (no re-synthesis)."
                : "New phrase \u2014 Play Preview will synthesize and cache on first run.";
            return;
        }
        CacheStatus = _previewCacheValid
            ? "\u2713 Cached \u2014 Play Preview will use stored audio (no re-synthesis)."
            : "\u2717 No cache for current settings \u2014 Play Preview will synthesize once, then cache.";
    }

    /// <summary>Persists the current window dimensions to the config file.</summary>
    public void SaveWindowSize() => _ = _config.SaveAsync(_config.CurrentConfig);

    private void NotifyPlaybackStateChanged()
    {
        OnPropertyChanged(nameof(CanPreview));
        OnPropertyChanged(nameof(CanStop));
        CommandManager.InvalidateRequerySuggested();
    }

    /// <summary>Writes PCM audio data as a WAV file to the phrase cache directory.</summary>
    private static void WriteWavCache(string phraseId, TtsResult result)
    {
        if (result.AudioData is null) return;
        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TtsCommunicationTool", "phrase_cache");
        Directory.CreateDirectory(cacheDir);
        var path = Path.Combine(cacheDir, $"{phraseId}.wav");

        using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
        using var bw = new BinaryWriter(fs);
        var data      = result.AudioData;
        int byteRate  = result.SampleRate * result.Channels * (result.BitsPerSample / 8);
        int blockAlign = result.Channels * (result.BitsPerSample / 8);

        bw.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
        bw.Write(36 + data.Length);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
        bw.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
        bw.Write(16);
        bw.Write((short)1);                     // PCM
        bw.Write((short)result.Channels);
        bw.Write(result.SampleRate);
        bw.Write(byteRate);
        bw.Write((short)blockAlign);
        bw.Write((short)result.BitsPerSample);
        bw.Write(System.Text.Encoding.ASCII.GetBytes("data"));
        bw.Write(data.Length);
        bw.Write(data);
    }

    private static string? NormalizeCategory(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.Trim().ToLower());
    }
}
