using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private readonly IPhraseCacheService _phraseCache;
    private readonly IPhraseService _phraseService;
    private readonly INotificationService _notifications;
    private int _selectedTabIndex;
    private bool _isRegenerating;
    private string _regenerationStatus = string.Empty;
    private string _saveError = string.Empty;
    private bool _isDirty;
    private float _savedPitch = 1.0f;          // snapshot of pitch at last load/save
    private float _savedSpeed = 1.0f;          // snapshot of speed at last load/save
    private DateTime _lastSaveToast = DateTime.MinValue;  // debounce tracker

    public GeneralSettingsViewModel General { get; }
    public HotkeySettingsViewModel Hotkeys { get; }
    public AudioSettingsViewModel Audio { get; }
    public VoiceSettingsViewModel Voice { get; }
    public AppearanceSettingsViewModel Appearance { get; }
    public ThemeSettingsViewModel Theme { get; }
    public PhraseListViewModel Phrases { get; }
    public TextReplacementSettingsViewModel TextReplacements { get; }

    public int SelectedTabIndex
    {
        get => _selectedTabIndex;
        set
        {
            if (SetField(ref _selectedTabIndex, value))
            {
                OnPropertyChanged(nameof(ShowImportExport));
                OnPropertyChanged(nameof(ImportCommand));
                OnPropertyChanged(nameof(ExportCommand));
                OnPropertyChanged(nameof(ImportButtonLabel));
                OnPropertyChanged(nameof(ExportButtonLabel));
            }
        }
    }

    /// <summary>True when the active tab is Replacements (5), Phrases (6), or Theme (7) — shows Import/Export in the bottom bar.</summary>
    public bool ShowImportExport => _selectedTabIndex is 5 or 6 or 7;

    public string ImportButtonLabel => _selectedTabIndex == 7 ? "Import Theme…" : "Import…";
    public string ExportButtonLabel => _selectedTabIndex == 7 ? "Export Theme…" : "Export…";

    /// <summary>Delegates to the active tab's ImportCommand (Replacements, Phrases, or Theme).</summary>
    public ICommand ImportCommand => _selectedTabIndex switch
    {
        5 => (ICommand)TextReplacements.ImportCommand,
        7 => Theme.ImportCommand,
        _ => Phrases.ImportCommand,
    };

    /// <summary>Delegates to the active tab's ExportCommand (Replacements, Phrases, or Theme).</summary>
    public ICommand ExportCommand => _selectedTabIndex switch
    {
        5 => (ICommand)TextReplacements.ExportCommand,
        7 => Theme.ExportCommand,
        _ => Phrases.ExportCommand,
    };

    public bool IsRegenerating
    {
        get => _isRegenerating;
        private set => SetField(ref _isRegenerating, value);
    }

    public string RegenerationStatus
    {
        get => _regenerationStatus;
        private set => SetField(ref _regenerationStatus, value);
    }

    /// <summary>Non-empty when save is blocked due to validation errors.</summary>
    public string SaveError
    {
        get => _saveError;
        private set => SetField(ref _saveError, value);
    }

    /// <summary>True when any child VM has unsaved changes since the last load or save.</summary>
    public bool IsDirty
    {
        get => _isDirty;
        private set => SetField(ref _isDirty, value);
    }

    public bool IsFirstRun { get; set; }

    public ICommand SaveCommand { get; }
    public ICommand ResetDefaultsCommand { get; }
    public ICommand ResetOverlayPositionCommand { get; }

    public event EventHandler? Saved;
    /// <summary>Raised after a successful cancel/rollback. The settings window should close.</summary>
    public event EventHandler? Cancelled;

    /// <summary>
    /// Clears the dirty flag without rolling back values.  Call this after the window's
    /// initial binding pass has settled so that WPF ComboBox TwoWay write-backs that occur
    /// during first render don't permanently mark settings as dirty.
    /// </summary>
    public void ResetDirtyState() => IsDirty = false;

    public SettingsViewModel(
        IConfigService config,
        ILoggingService log,
        IPhraseCacheService phraseCache,
        IPhraseService phraseService,
        INotificationService notifications,
        GeneralSettingsViewModel general,
        HotkeySettingsViewModel hotkeys,
        AudioSettingsViewModel audio,
        VoiceSettingsViewModel voice,
        AppearanceSettingsViewModel appearance,
        ThemeSettingsViewModel theme,
        PhraseListViewModel phrases,
        TextReplacementSettingsViewModel textReplacements)
    {
        _config = config;
        _log = log;
        _phraseCache = phraseCache;
        _phraseService = phraseService;
        _notifications = notifications;
        General = general;
        Hotkeys = hotkeys;
        Audio = audio;
        Voice = voice;
        Appearance = appearance;
        Theme = theme;
        Phrases = phrases;
        TextReplacements = textReplacements;

        SaveCommand = new AsyncRelayCommand(SaveAsync);
        ResetDefaultsCommand = new RelayCommand(ResetDefaults);
        ResetOverlayPositionCommand = new AsyncRelayCommand(ResetOverlayPositionAsync);

        LoadFromConfig();

        // Track dirty state across all child VMs
        foreach (var child in new System.ComponentModel.INotifyPropertyChanged[] { General, Hotkeys, Audio, Voice, Appearance, TextReplacements })
            child.PropertyChanged += (_, _) => IsDirty = true;
        // For Theme: only mark dirty on actual colour/name edits, NOT when the VM resets
        // its own IsDirty flag (which would immediately re-dirty the parent after a revert).
        Theme.PropertyChanged += (_, pe) =>
        {
            if (pe.PropertyName is nameof(ThemeSettingsViewModel.IsDirty)
                                 or nameof(ThemeSettingsViewModel.SelectedPreset)
                                 or nameof(ThemeSettingsViewModel.IsEditingBuiltIn))
                return;
            IsDirty = true;
        };
        // Also track phrase session changes so Cancel shows the confirmation dialog
        Phrases.PropertyChanged += (_, pe) =>
        {
            if (pe.PropertyName == nameof(PhraseListViewModel.HasSessionChanges))
                IsDirty = Phrases.HasSessionChanges || IsDirty;
        };
    }

    private void LoadFromConfig()
    {
        var cfg = _config.CurrentConfig;
        _savedPitch = cfg.VoiceSettings.GlobalPitch;  // snapshot for cancel rollback
        _savedSpeed = cfg.VoiceSettings.GlobalSpeed;  // snapshot for cancel rollback
        General.LoadFrom(cfg.GeneralSettings);
        General.LoadDiagnosticSettings(cfg.DiagnosticLogging);
        Hotkeys.LoadFrom(cfg.HotkeySettings);
        Audio.LoadFrom(cfg.AudioSettings);
        Voice.LoadFrom(cfg.VoiceSettings);
        Appearance.LoadFrom(cfg.OverlaySettings);
        Theme.LoadThemes(cfg.ActiveThemeName);
        TextReplacements.LoadFrom(cfg.TextReplacements);
        IsDirty = false;
        _log.LogEvent(DiagnosticLogLevel.Info, "settings", "settings_loaded",
            "Settings loaded from config");
    }

    private async Task SaveAsync()
    {
        // Block save if real-time hotkey conflict warning is active
        if (!string.IsNullOrEmpty(Hotkeys.ValidationMessage))
        {
            SaveError = Hotkeys.ValidationMessage;
            return;
        }

        // Check for duplicate phrase hotkeys
        var conflictError = CheckPhraseHotkeyConflicts();
        if (conflictError is not null)
        {
            SaveError = conflictError;
            return;
        }

        SaveError = string.Empty;
        var cfg = _config.CurrentConfig;
        var previousVoiceId = cfg.VoiceSettings.SelectedVoiceId;

        General.ApplyTo(cfg.GeneralSettings);
        General.ApplyDiagnosticSettings(cfg.DiagnosticLogging);
        Hotkeys.ApplyTo(cfg.HotkeySettings);
        Audio.ApplyTo(cfg.AudioSettings);
        Voice.ApplyTo(cfg.VoiceSettings);
        Appearance.ApplyTo(cfg.OverlaySettings);
        await Theme.CommitAsync(_config);
        TextReplacements.ApplyTo(cfg.TextReplacements);

        await _config.SaveAsync(cfg);
        _log.Info("Settings saved.");
        _log.UpdateSettings(cfg.DiagnosticLogging);
        _log.LogEvent(DiagnosticLogLevel.Info, "settings", "settings_saved",
            "Settings saved successfully");
        IsDirty = false;
        _savedPitch = cfg.VoiceSettings.GlobalPitch;  // update snapshot after save
        _savedSpeed = cfg.VoiceSettings.GlobalSpeed;  // update snapshot after save
        Phrases.Commit(); // finalize phrase session — takes new snapshot and re-registers hotkeys

        // Detect voice change — regenerate all phrase caches with visible progress
        var newVoiceId = cfg.VoiceSettings.SelectedVoiceId;
        if (!string.Equals(previousVoiceId, newVoiceId, StringComparison.OrdinalIgnoreCase))
        {
            var phrases = _phraseService.GetAll();
            if (phrases.Count > 0)
            {
                _log.Info($"Voice changed from '{previousVoiceId}' to '{newVoiceId}'. Regenerating {phrases.Count} phrase caches...");
                IsRegenerating = true;
                try
                {
                    for (int i = 0; i < phrases.Count; i++)
                    {
                        RegenerationStatus = $"Regenerating phrase {i + 1} of {phrases.Count}...";
                        await _phraseCache.GenerateCacheAsync(phrases[i]);
                    }
                }
                finally
                {
                    IsRegenerating = false;
                    RegenerationStatus = string.Empty;
                }
                _log.Info("Phrase cache regeneration complete.");
            }
        }

        Saved?.Invoke(this, EventArgs.Empty);

        // Toast notification with debounce (1.5 s between repeated toasts)
        var now = DateTime.Now;
        if ((now - _lastSaveToast).TotalSeconds > 1.5)
        {
            _lastSaveToast = now;
            _notifications.ShowInfo("Settings saved.");
        }
    }

    /// <summary>
    /// Rolls back phrase changes made during this settings session and fires <see cref="Cancelled"/>.
    /// Call from the Cancel button or when the window is closed without saving.
    /// </summary>
    public async Task CancelAsync()
    {
        // Restore live pitch in memory so the overlay uses the saved value again
        _config.CurrentConfig.VoiceSettings.GlobalPitch = _savedPitch;
        _config.CurrentConfig.VoiceSettings.GlobalSpeed = _savedSpeed;
        await Phrases.RollbackAsync();
        await Theme.RevertChangesAsync();   // re-applies the saved theme; deletes any unsaved new themes
        LoadFromConfig(); // reload other tabs to last-saved config state
        IsDirty = false;
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Silently saves the window dimensions to config without triggering a full settings validation/save.
    /// Called from SettingsWindow.Closing so size is always persisted.
    /// </summary>
    public async Task SaveWindowDimensionsAsync()
    {
        var cfg = _config.CurrentConfig;
        cfg.GeneralSettings.SettingsWindowWidth  = General.SettingsWindowWidth;
        cfg.GeneralSettings.SettingsWindowHeight = General.SettingsWindowHeight;
        cfg.GeneralSettings.ThemePreviewColumnWidth = General.ThemePreviewColumnWidth;
        await _config.SaveAsync(cfg);
    }

    /// <summary>Returns an error string if any two phrases share the same hotkey, else null.</summary>
    private string? CheckPhraseHotkeyConflicts()
    {
        var phrases = _phraseService.GetAll();
        var seen = new Dictionary<TtsCommunicationTool.Core.Models.HotkeyBinding, string>();
        foreach (var p in phrases)
        {
            if (p.Hotkey is null || p.Hotkey.IsEmpty) continue;
            if (seen.TryGetValue(p.Hotkey, out var existingName))
                return $"Phrases '{existingName}' and '{p.Name}' share the same hotkey ({p.Hotkey})."
                    + " Remove the duplicate before saving.";
            seen[p.Hotkey] = p.Name;
        }
        return null;
    }

    private void ResetDefaults()
    {
        var defaults = _config.GetDefaults();
        General.LoadFrom(defaults.GeneralSettings);
        Hotkeys.LoadFrom(defaults.HotkeySettings);
        Audio.LoadFrom(defaults.AudioSettings);
        Voice.LoadFrom(defaults.VoiceSettings);
        Appearance.LoadFrom(defaults.OverlaySettings);
        Theme.LoadThemes("Default Dark");
        TextReplacements.LoadFrom(defaults.TextReplacements);
        _log.Info("Settings reset to defaults.");
    }

    private async Task ResetOverlayPositionAsync()
    {
        var cfg = _config.CurrentConfig;
        cfg.OverlaySettings.Left = null;
        cfg.OverlaySettings.Top = null;
        await _config.SaveAsync(cfg);
        _log.Info("Overlay position reset to centre-screen.");
    }
}
