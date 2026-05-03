using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class PhraseListViewModel : ViewModelBase
{
    private readonly IPhraseService _phraseService;
    private readonly IPhraseCacheService _phraseCache;
    private readonly ITtsService _tts;
    private readonly IAudioRouterService _audioRouter;
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly IHotkeyHost _hotkeyHost;
    private PhraseItem? _selectedPhrase;
    private string _filterText = string.Empty;
    private string _selectedCategory = "All";
    private bool _showFavoritesOnly;

    // ── Session snapshot / rollback support ──────────────────────────────────
    // A deep-copy taken when the settings window opens (TakeSnapshot).
    // If the user cancels, RollbackAsync restores this state.
    private List<PhraseItem> _snapshot = new();

    // IDs of phrases added this session (need cache deletion on rollback).
    private readonly HashSet<string> _sessionAddedIds = new();

    // IDs of phrases whose cache was created or regenerated this session
    // (snapshot versions need a cache rebuild on rollback if they still exist).
    private readonly HashSet<string> _sessionCacheModifiedIds = new();

    // True as soon as any phrase mutation occurs this session.
    private bool _sessionDirty;

    /// <summary>True when phrase changes have been made during the current settings session that have not yet been committed or rolled back.</summary>
    public bool HasSessionChanges => _sessionDirty;

    private void MarkDirty()
    {
        if (_sessionDirty) return;
        _sessionDirty = true;
        OnPropertyChanged(nameof(HasSessionChanges));
    }

    public ObservableCollection<PhraseItem> Phrases { get; } = new();
    public ObservableCollection<PhraseItem> FilteredPhrases { get; } = new();
    public ObservableCollection<string> Categories { get; } = new();

    public PhraseItem? SelectedPhrase
    {
        get => _selectedPhrase;
        set
        {
            if (SetField(ref _selectedPhrase, value))
            {
                OnPropertyChanged(nameof(SelectedPhraseHotkeyDisplay));
                // Populate edit fields when a phrase is selected; clear them when deselected
                if (value is not null)
                {
                    EditName = value.Name;
                    EditText = value.Text;
                    EditCategory = value.Category ?? string.Empty;
                }
                else
                {
                    EditName = string.Empty;
                    EditText = string.Empty;
                    EditCategory = string.Empty;
                }
            }
        }
    }

    public string SelectedPhraseHotkeyDisplay
    {
        get
        {
            if (SelectedPhrase?.Hotkey is not { } hk)
                return "(none)";

            var parts = new List<string>();
            if (hk.Ctrl) parts.Add("Ctrl");
            if (hk.Alt) parts.Add("Alt");
            if (hk.Shift) parts.Add("Shift");
            if (hk.Win) parts.Add("Win");
            if (!string.IsNullOrEmpty(hk.Key)) parts.Add(hk.Key);
            return parts.Count > 0 ? string.Join("+", parts) : "(none)";
        }
    }

    public ICommand AddCommand { get; }
    public ICommand UpdateCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand PlayCommand { get; }

    /// <summary>
    /// Optional confirmation callback injected by the view.
    /// Called with the phrase name before deletion. Return false to cancel.
    /// </summary>
    public Func<string, bool>? ConfirmDelete { get; set; }

    /// <summary>Raised when the view should open the Phrase Editor. Null item = new phrase.</summary>
    public event EventHandler<PhraseItem?>? EditorRequested;

    /// <summary>Opens editor for a new phrase (bound to the New Phrase button).</summary>
    public ICommand OpenEditorForNewCommand { get; }

    /// <summary>Opens editor for the currently selected phrase (bound to Edit button / double-click / Enter).</summary>
    public ICommand OpenEditorForSelectedCommand { get; }
    public ICommand RefreshCommand { get; }
    public ICommand ClearHotkeyCommand { get; }
    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }
    public ICommand ToggleFavoriteCommand { get; }
    public ICommand TogglePinnedCommand { get; }
    // Inline list-item variants — accept a PhraseItem parameter directly so the
    // star/pin buttons in the DataTemplate don't require changing SelectedPhrase first.
    public ICommand ToggleFavoriteByItemCommand { get; }
    public ICommand TogglePinnedByItemCommand { get; }

    // For inline editing
    private string _editName = string.Empty;
    private string _editText = string.Empty;
    private string _editCategory = string.Empty;
    private string _phraseStatusMessage = string.Empty;

    public string PhraseStatusMessage
    {
        get => _phraseStatusMessage;
        set => SetField(ref _phraseStatusMessage, value);
    }

    public string EditName
    {
        get => _editName;
        set => SetField(ref _editName, value);
    }

    public string EditText
    {
        get => _editText;
        set => SetField(ref _editText, value);
    }

    public string EditCategory
    {
        get => _editCategory;
        set => SetField(ref _editCategory, value);
    }

    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetField(ref _filterText, value))
                ApplyFilter();
        }
    }

    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (SetField(ref _selectedCategory, value))
                ApplyFilter();
        }
    }

    public bool ShowFavoritesOnly
    {
        get => _showFavoritesOnly;
        set
        {
            if (SetField(ref _showFavoritesOnly, value))
                ApplyFilter();
        }
    }

    /// <summary>Result of the most recent successful import. Used by the import progress dialog.</summary>
    public PhraseImportResult? LastImportResult { get; private set; }

    /// <summary>Raised after a successful import so the view can show the progress dialog.</summary>
    public event EventHandler? ImportCompleted;

    public PhraseListViewModel(
        IPhraseService phraseService,
        IPhraseCacheService phraseCache,
        ITtsService tts,
        IAudioRouterService audioRouter,
        IConfigService config,
        ILoggingService log,
        IHotkeyHost hotkeyHost)
    {
        _phraseService = phraseService;
        _phraseCache = phraseCache;
        _tts = tts;
        _audioRouter = audioRouter;
        _config = config;
        _log = log;
        _hotkeyHost = hotkeyHost;

        AddCommand = new RelayCommand(AddPhrase);
        UpdateCommand = new AsyncRelayCommand(UpdateSelectedPhraseAsync, () => SelectedPhrase is not null);
        DeleteCommand = new RelayCommand(DeleteSelected, () => SelectedPhrase is not null);
        PlayCommand = new AsyncRelayCommand(PlaySelectedAsync, () => SelectedPhrase is not null);
        RefreshCommand = new RelayCommand(Refresh);
        ClearHotkeyCommand = new RelayCommand(ClearSelectedHotkey, () => SelectedPhrase is not null);
        ImportCommand = new RelayCommand(ImportPhrases);
        ExportCommand = new RelayCommand(ExportPhrases, () => Phrases.Count > 0);
        ToggleFavoriteCommand = new RelayCommand(ToggleFavorite, () => SelectedPhrase is not null);
        TogglePinnedCommand = new RelayCommand(TogglePinned, () => SelectedPhrase is not null);
        ToggleFavoriteByItemCommand = new RelayCommand(param =>
        {
            if (param is PhraseItem item) ToggleFavoriteForItem(item);
        });
        TogglePinnedByItemCommand = new RelayCommand(param =>
        {
            if (param is PhraseItem item) TogglePinnedForItem(item);
        });
        OpenEditorForNewCommand = new RelayCommand(() => EditorRequested?.Invoke(this, null));
        OpenEditorForSelectedCommand = new RelayCommand(
            () => EditorRequested?.Invoke(this, SelectedPhrase),
            () => SelectedPhrase is not null);

        Refresh();
        TakeSnapshot(); // baseline for cancel/rollback
    }

    public void Refresh()
    {
        Phrases.Clear();
        foreach (var p in _phraseService.GetAll())
            Phrases.Add(p);
        RebuildCategories();
        ApplyFilter();
    }

    private void RebuildCategories()
    {
        var prev = SelectedCategory;
        Categories.Clear();
        Categories.Add("All");
        foreach (var cat in _phraseService.GetAll()
            .Select(p => p.Category)
            .Where(c => !string.IsNullOrEmpty(c))
            .GroupBy(c => c!.ToLowerInvariant())
            .Select(g => g.First()!)
            .OrderBy(c => c))
        {
            Categories.Add(cat);
        }
        // Restore selection if still valid (case-insensitive), else default to All
        var match = Categories.FirstOrDefault(c => string.Equals(c, prev, StringComparison.OrdinalIgnoreCase));
        SelectedCategory = match ?? "All";
    }

    private void ApplyFilter()
    {
        var query = Phrases.AsEnumerable();

        if (_showFavoritesOnly)
            query = query.Where(p => p.IsFavorite);

        if (!string.IsNullOrEmpty(_selectedCategory) && _selectedCategory != "All")
            query = query.Where(p => string.Equals(p.Category, _selectedCategory, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(_filterText))
        {
            var ft = _filterText.Trim().ToLowerInvariant();
            query = query.Where(p =>
                p.Name.ToLowerInvariant().Contains(ft) ||
                p.Text.ToLowerInvariant().Contains(ft));
        }

        // Pinned first, then by SortOrder
        var results = query.OrderByDescending(p => p.IsPinned).ThenBy(p => p.SortOrder).ToList();

        FilteredPhrases.Clear();
        foreach (var p in results)
            FilteredPhrases.Add(p);
    }

    /// <summary>
    /// Called by SettingsWindow after the Phrase Editor closes with a Save or Delete result.
    /// Marks the session dirty and refreshes the list.
    /// </summary>
    public void OnEditorCommit()
    {
        MarkDirty();
        Refresh();
    }

    /// <summary>
    /// Records a phrase added via the Phrase Editor so it can be cleaned up on rollback.
    /// </summary>
    public void TrackSessionAdd(string phraseId)
    {
        _sessionAddedIds.Add(phraseId);
        _sessionCacheModifiedIds.Add(phraseId);
        MarkDirty();
    }

    /// <summary>
    /// Records that the cache for an existing phrase was regenerated via the Phrase Editor.
    /// </summary>
    public void TrackSessionCacheModify(string phraseId)
    {
        _sessionCacheModifiedIds.Add(phraseId);
        MarkDirty();
    }

    private void AddPhrase()
    {
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditText))
            return;

        var phrase = new PhraseItem
        {
            Name = EditName.Trim(),
            Text = EditText.Trim(),
            Category = NormalizeCategory(EditCategory),
            SortOrder = Phrases.Count
        };

        var result = _phraseService.Add(phrase);
        if (result.Success)
        {
            // Track this ID so cancel can delete its cache and remove it from storage
            _sessionAddedIds.Add(phrase.Id);
            _sessionCacheModifiedIds.Add(phrase.Id);
            MarkDirty();
            EditName = string.Empty;
            EditText = string.Empty;
            EditCategory = string.Empty;
            Refresh();
        }
    }

    private void DeleteSelected()
    {
        if (SelectedPhrase is null) return;
        // Ask the view to confirm before deleting.
        if (ConfirmDelete is not null && !ConfirmDelete(SelectedPhrase.Name))
            return;
        var id = SelectedPhrase.Id;
        // If this phrase was in the snapshot, track it so rollback can regenerate its cache
        if (_snapshot.Any(p => p.Id == id))
            _sessionCacheModifiedIds.Add(id);
        // If it was added this session and is now deleted, remove from both tracking sets
        _sessionAddedIds.Remove(id);
        MarkDirty();
        var result = _phraseService.Delete(id);
        if (result.Success)
            Refresh();
    }

    private async Task UpdateSelectedPhraseAsync()
    {
        if (SelectedPhrase is null) return;
        if (string.IsNullOrWhiteSpace(EditName) || string.IsNullOrWhiteSpace(EditText))
        {
            PhraseStatusMessage = "Name and text are required.";
            return;
        }

        var textChanged = !string.Equals(SelectedPhrase.Text.Trim(), EditText.Trim(), StringComparison.Ordinal);

        SelectedPhrase.Name = EditName.Trim();
        SelectedPhrase.Text = EditText.Trim();
        SelectedPhrase.Category = NormalizeCategory(EditCategory);
        SelectedPhrase.UpdatedUtc = DateTime.UtcNow;

        _phraseService.Update(SelectedPhrase);

        // If text changed, invalidate the cache so it's regenerated on next use
        if (textChanged)
        {
            // Track so rollback can restore the original cache
            _sessionCacheModifiedIds.Add(SelectedPhrase.Id);
            _phraseCache.DeleteCache(SelectedPhrase.Id);
            PhraseStatusMessage = "Phrase updated. Cache will regenerate on next use.";
            await _phraseCache.GenerateCacheAsync(SelectedPhrase);
        }
        else
        {
            PhraseStatusMessage = "Phrase updated.";
        }
        MarkDirty();

        Refresh();
    }

    public void SetSelectedPhraseHotkey(HotkeyBinding binding)
    {
        if (SelectedPhrase is null) return;
        SelectedPhrase.Hotkey = binding;
        SelectedPhrase.UpdatedUtc = DateTime.UtcNow;
        _phraseService.Update(SelectedPhrase);
        MarkDirty();
        _hotkeyHost.RegisterPhraseHotkeys();
        OnPropertyChanged(nameof(SelectedPhraseHotkeyDisplay));
        Refresh();
    }

    private void ClearSelectedHotkey()
    {
        if (SelectedPhrase is null) return;
        SelectedPhrase.Hotkey = null;
        SelectedPhrase.UpdatedUtc = DateTime.UtcNow;
        _phraseService.Update(SelectedPhrase);
        MarkDirty();
        _hotkeyHost.RegisterPhraseHotkeys();
        OnPropertyChanged(nameof(SelectedPhraseHotkeyDisplay));
        Refresh();
    }

    private void ToggleFavorite()
    {
        if (SelectedPhrase is null) return;
        ToggleFavoriteForItem(SelectedPhrase);
    }

    private void ToggleFavoriteForItem(PhraseItem item)
    {
        item.IsFavorite = !item.IsFavorite;
        item.UpdatedUtc = DateTime.UtcNow;
        _phraseService.Update(item);
        MarkDirty();
        Refresh();
    }

    /// <summary>
    /// Normalizes a category string to Title Case and trims whitespace.
    /// Returns null for empty/whitespace input so uncategorised phrases stay uncategorised.
    /// </summary>
    private static string? NormalizeCategory(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;
        var trimmed = input.Trim();
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(trimmed.ToLower());
    }

    private void TogglePinned()
    {
        if (SelectedPhrase is null) return;
        TogglePinnedForItem(SelectedPhrase);
    }

    private void TogglePinnedForItem(PhraseItem item)
    {
        item.IsPinned = !item.IsPinned;
        item.UpdatedUtc = DateTime.UtcNow;
        _phraseService.Update(item);
        MarkDirty();
        Refresh();
    }

    // ── Snapshot / commit / rollback ─────────────────────────────────────────

    /// <summary>
    /// Captures the current phrase list as a deep-copy baseline.
    /// Called when the settings window opens and again after each successful commit.
    /// </summary>
    public void TakeSnapshot()
    {
        _snapshot = _phraseService.GetAll().Select(DeepClone).ToList();
        _sessionAddedIds.Clear();
        _sessionCacheModifiedIds.Clear();
        _sessionDirty = false;
        OnPropertyChanged(nameof(HasSessionChanges));
    }

    /// <summary>
    /// Called when the user clicks Save. Refreshes hotkey registrations and
    /// takes a new snapshot so a subsequent cancel won't undo the saved state.
    /// </summary>
    public void Commit()
    {
        _hotkeyHost.RegisterPhraseHotkeys();
        TakeSnapshot();
        _log.Info("Phrase session committed.");
    }

    /// <summary>
    /// Called when the user clicks Cancel or closes the settings window without saving.
    /// Restores the phrase list to the snapshot taken when the window opened,
    /// deletes cache files for phrases that were added this session,
    /// and regenerates caches for phrases that existed but had their cache changed.
    /// </summary>
    public async Task RollbackAsync()
    {
        _log.Info($"Rolling back phrase session: {_sessionAddedIds.Count} added, {_sessionCacheModifiedIds.Count} cache-modified.");

        // Restore the config's phrase list to the snapshot
        _config.CurrentConfig.Phrases.Clear();
        foreach (var p in _snapshot)
            _config.CurrentConfig.Phrases.Add(DeepClone(p));
        await _config.SaveAsync(_config.CurrentConfig);

        // Delete cache files for phrases that were added this session (they no longer exist)
        foreach (var id in _sessionAddedIds)
            _phraseCache.DeleteCache(id);

        // Regenerate caches for snapshot phrases whose cache was modified during the session
        foreach (var id in _sessionCacheModifiedIds.Except(_sessionAddedIds))
        {
            var phrase = _config.CurrentConfig.Phrases.FirstOrDefault(p => p.Id == id);
            if (phrase is not null)
                await _phraseCache.GenerateCacheAsync(phrase);
        }

        _hotkeyHost.RegisterPhraseHotkeys();
        TakeSnapshot();
        Refresh();
    }

    /// <summary>Creates a deep copy of a <see cref="PhraseItem"/>, including its optional hotkey binding and voice overrides.</summary>
    private static PhraseItem DeepClone(PhraseItem p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Text = p.Text,
        Hotkey = p.Hotkey is null ? null : new HotkeyBinding
        {
            Ctrl = p.Hotkey.Ctrl,
            Alt = p.Hotkey.Alt,
            Shift = p.Hotkey.Shift,
            Win = p.Hotkey.Win,
            Key = p.Hotkey.Key
        },
        SortOrder = p.SortOrder,
        CreatedUtc = p.CreatedUtc,
        UpdatedUtc = p.UpdatedUtc,
        Category = p.Category,
        IsFavorite = p.IsFavorite,
        IsPinned = p.IsPinned,
        OverrideEngine = p.OverrideEngine,
        UseVoiceOverride = p.UseVoiceOverride,
        OverrideVoiceId = p.OverrideVoiceId,
        OverrideVoiceName = p.OverrideVoiceName,
        OverridePitch = p.OverridePitch,
    };

    private async Task PlaySelectedAsync()
    {
        if (SelectedPhrase is null) return;

        var cfg = _config.CurrentConfig;

        // Try cached audio first
        var cached = _phraseCache.GetCachedAudio(SelectedPhrase.Id);
        if (cached is not null)
        {
            await _audioRouter.PlayAsync(cached,
                cfg.AudioSettings.MonitorOutputDeviceId,
                cfg.AudioSettings.SecondaryOutputDeviceId,
                cfg.AudioSettings.MonitorVolume,
                cfg.AudioSettings.SecondaryVolume);
            return;
        }

        // Fallback: generate on-the-fly and cache it
        var ttsResult = await _tts.SynthesizeAsync(new TtsRequest
        {
            Text = SelectedPhrase.Text,
            VoiceId = cfg.VoiceSettings.SelectedVoiceId
        });

        if (!ttsResult.Success || ttsResult.AudioData is null)
        {
            _log.Warn($"Phrase playback failed: {ttsResult.ErrorMessage}");
            return;
        }

        // Cache for next time
        _ = _phraseCache.GenerateCacheAsync(SelectedPhrase);

        var playback = new PlaybackRequest
        {
            AudioData = ttsResult.AudioData,
            SampleRate = ttsResult.SampleRate,
            Channels = ttsResult.Channels,
            BitsPerSample = ttsResult.BitsPerSample
        };

        await _audioRouter.PlayAsync(playback,
            cfg.AudioSettings.MonitorOutputDeviceId,
            cfg.AudioSettings.SecondaryOutputDeviceId,
            cfg.AudioSettings.MonitorVolume,
            cfg.AudioSettings.SecondaryVolume);
    }

    private void ExportPhrases()
    {
        var dlg = new SaveFileDialog
        {
            Title = "Export Phrases",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json",
            FileName = "tts-phrases"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            _log.LogEvent(DiagnosticLogLevel.Info, "export", "export_started",
                "Phrase export started",
                new { path = Path.GetFileName(dlg.FileName), count = Phrases.Count });
            var json = _phraseService.ExportToJson();
            File.WriteAllText(dlg.FileName, json, System.Text.Encoding.UTF8);
            PhraseStatusMessage = $"Exported {Phrases.Count} phrase(s) to {Path.GetFileName(dlg.FileName)}.";
            _log.Info($"Phrases exported to '{dlg.FileName}'.");
            _log.LogEvent(DiagnosticLogLevel.Info, "export", "export_completed",
                "Phrase export completed",
                new { path = Path.GetFileName(dlg.FileName), count = Phrases.Count });
        }
        catch (Exception ex)
        {
            PhraseStatusMessage = "Export failed — see log.";
            _log.Error("Phrase export failed", ex);
            _log.LogEvent(DiagnosticLogLevel.Error, "export", "export_failed",
                "Phrase export failed",
                new { path = Path.GetFileName(dlg.FileName), error = ex.Message });
        }
    }

    private void ImportPhrases()
    {
        var dlg = new OpenFileDialog
        {
            Title = "Import Phrases",
            Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*",
            DefaultExt = "json"
        };

        if (dlg.ShowDialog() != true) return;

        try
        {
            _log.LogEvent(DiagnosticLogLevel.Info, "import", "import_started",
                "Phrase import started",
                new { path = Path.GetFileName(dlg.FileName) });
            var json = File.ReadAllText(dlg.FileName, System.Text.Encoding.UTF8);
            var result = _phraseService.ImportFromJson(json);
            if (!result.Success)
            {
                PhraseStatusMessage = $"Import failed: {result.ErrorMessage}";
                _log.Warn($"Phrase import failed: {result.ErrorMessage}");
                _log.LogEvent(DiagnosticLogLevel.Warn, "import", "import_failed",
                    "Phrase import failed",
                    new { path = Path.GetFileName(dlg.FileName), error = result.ErrorMessage });
                return;
            }

            LastImportResult = result;
            // Track all imported IDs so cancel can remove them and their caches
            foreach (var id in result.AddedPhraseIds)
            {
                _sessionAddedIds.Add(id);
                _sessionCacheModifiedIds.Add(id);
            }
            MarkDirty();
            Refresh();
            ImportCompleted?.Invoke(this, EventArgs.Empty);
            _log.Info($"Imported {result.AddedCount} phrases from '{dlg.FileName}'.");
            _log.LogEvent(DiagnosticLogLevel.Info, "import", "import_completed",
                "Phrase import completed",
                new { path = Path.GetFileName(dlg.FileName), count = result.AddedCount });
        }
        catch (Exception ex)
        {
            PhraseStatusMessage = "Import failed — see log.";
            _log.Error("Phrase import failed", ex);
            _log.LogEvent(DiagnosticLogLevel.Error, "import", "import_failed",
                "Phrase import failed with exception",
                new { path = Path.GetFileName(dlg.FileName), error = ex.Message });
        }
    }

    /// <summary>
    /// Caches audio for all phrases in the last import result.
    /// Call from the import progress dialog.
    /// </summary>
    public async Task CacheImportedPhrasesAsync(
        IProgress<(int current, int total)> progress,
        CancellationToken ct = default)
    {
        if (LastImportResult is null) return;
        var ids = LastImportResult.AddedPhraseIds;
        for (int i = 0; i < ids.Count; i++)
        {
            ct.ThrowIfCancellationRequested();
            var phrase = _phraseService.GetById(ids[i]);
            if (phrase is not null)
                await _phraseCache.GenerateCacheAsync(phrase);
            progress.Report((i + 1, ids.Count));
        }
    }
}
