using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.Utilities;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

/// <summary>
/// Backing ViewModel for the Theme settings tab.
/// Changes are applied live to the app via <see cref="IThemeService.Apply"/> as the user
/// edits; the parent <see cref="SettingsViewModel"/> IsDirty flag is driven by
/// <see cref="INotifyPropertyChanged"/> events bubbling up from this VM.
/// </summary>
public sealed class ThemeSettingsViewModel : ViewModelBase
{
    private readonly IThemeService _themeService;
    private readonly ILoggingService _log;
    private readonly IColorPickerService _colorPicker;

    // Working copy — mutated live; _savedSnapshot is what was last committed to disk;
    // _originalSnapshot is what was active when Settings opened (used by Cancel).
    private ThemeSettings _workingCopy = ThemeDefaults.CreateDefault();
    private ThemeSettings _savedSnapshot = ThemeDefaults.CreateDefault();
    private ThemeSettings _originalSnapshot = ThemeDefaults.CreateDefault();
    private bool _isDirty;
    private ThemeSettings? _selectedPreset;

    // ── Pending-delete buffer ─────────────────────────────────────────────────
    // Names queued for deletion. Written to disk only when CommitAsync / SaveAsync runs.
    // Cleared (themes restored in list) when RevertChangesAsync is called.
    private readonly HashSet<string> _pendingDeletes = new(StringComparer.OrdinalIgnoreCase);

    // ── Pending-new buffer ───────────────────────────────────────────────────────
    // Names of themes created this session via Save As / Duplicate that have already
    // been written to disk but should be deleted if the user hits Cancel.
    // Cleared (without deleting) when CommitAsync / SaveAsync succeeds.
    private readonly HashSet<string> _pendingNews = new(StringComparer.OrdinalIgnoreCase);

    // True only when the user has actually edited a colour value this session.
    // Selecting a preset alone does NOT set this. Only set by SetColor().
    // Used to decide whether a built-in preset must be forked on Save.
    private bool _hasColourEdits;

    // ── Dirty / state ────────────────────────────────────────────────────────
    public bool IsDirty
    {
        get => _isDirty;
        private set => SetField(ref _isDirty, value);
    }

    /// <summary>True when the currently selected (saved) theme is a built-in seed.</summary>
    public bool IsEditingBuiltIn => _savedSnapshot.IsBuiltIn;

    // ── Theme name (always editable — becomes the name when saving) ──────────
    public string ThemeName
    {
        get => _workingCopy.Name;
        set
        {
            if (_workingCopy.Name == value) return;
            _workingCopy.Name = value;
            OnPropertyChanged();
            MarkDirty();
        }
    }

    // ── Preset list ──────────────────────────────────────────────────────────
    public ObservableCollection<ThemeSettings> AvailableThemes { get; } = new();

    public ThemeSettings? SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            if (value is null || ReferenceEquals(_selectedPreset, value)) return;
            SetField(ref _selectedPreset, value);
            ApplyPreset(value);
        }
    }

    // ── Core colour properties ────────────────────────────────────────────────
    public string WindowBackground   { get => _workingCopy.WindowBackground;   set => SetColor(v => _workingCopy.WindowBackground   = v, value); }
    public string PanelBackground    { get => _workingCopy.PanelBackground;    set => SetColor(v => _workingCopy.PanelBackground    = v, value); }
    public string DeepPanelBackground{ get => _workingCopy.DeepPanelBackground;set => SetColor(v => _workingCopy.DeepPanelBackground= v, value); }
    public string HistoryBackground  { get => _workingCopy.HistoryBackground;  set => SetColor(v => _workingCopy.HistoryBackground  = v, value); }
    public string Surface0           { get => _workingCopy.Surface0;           set => SetColor(v => _workingCopy.Surface0           = v, value); }
    public string BorderColor        { get => _workingCopy.BorderColor;        set => SetColor(v => _workingCopy.BorderColor        = v, value); }
    public string Surface2           { get => _workingCopy.Surface2;           set => SetColor(v => _workingCopy.Surface2           = v, value); }
    public string Accent             { get => _workingCopy.Accent;             set => SetColor(v => _workingCopy.Accent             = v, value); }
    public string AccentHover        { get => _workingCopy.AccentHover;        set => SetColor(v => _workingCopy.AccentHover        = v, value); }

    // ── Text colour properties ────────────────────────────────────────────────
    public string PrimaryText   { get => _workingCopy.PrimaryText;   set => SetColor(v => _workingCopy.PrimaryText   = v, value); }
    public string SecondaryText { get => _workingCopy.SecondaryText; set => SetColor(v => _workingCopy.SecondaryText = v, value); }
    public string MutedText     { get => _workingCopy.MutedText;     set => SetColor(v => _workingCopy.MutedText     = v, value); }
    public string MutedIcon     { get => _workingCopy.MutedIcon;     set => SetColor(v => _workingCopy.MutedIcon     = v, value); }

    // ── Status colour properties ──────────────────────────────────────────────
    public string InfoColor   { get => _workingCopy.InfoColor;   set => SetColor(v => _workingCopy.InfoColor   = v, value); }
    public string ErrorColor  { get => _workingCopy.ErrorColor;  set => SetColor(v => _workingCopy.ErrorColor  = v, value); }
    public string ErrorHover  { get => _workingCopy.ErrorHover;  set => SetColor(v => _workingCopy.ErrorHover  = v, value); }
    public string Warning     { get => _workingCopy.Warning;     set => SetColor(v => _workingCopy.Warning     = v, value); }
    public string Success     { get => _workingCopy.Success;     set => SetColor(v => _workingCopy.Success     = v, value); }

    // ── Typography properties ─────────────────────────────────────────────────
    public IReadOnlyList<string> InstalledFonts { get; } =
        Fonts.SystemFontFamilies.Select(f => f.Source)
             .OrderBy(n => n, StringComparer.OrdinalIgnoreCase).ToList();

    public string UiFontFamily
    {
        get => _workingCopy.UiFontFamily;
        set => SetTypography(() => { _workingCopy.UiFontFamily = value; });
    }
    public double BaseFontSize
    {
        get => _workingCopy.BaseFontSize;
        set => SetTypography(() => { _workingCopy.BaseFontSize = value; });
    }
    public string OverlayFontFamily
    {
        get => _workingCopy.OverlayFontFamily;
        set => SetTypography(() => { _workingCopy.OverlayFontFamily = value; });
    }
    public double OverlayFontSize
    {
        get => _workingCopy.OverlayFontSize;
        set => SetTypography(() => { _workingCopy.OverlayFontSize = value; });
    }

    // ── Shape & density properties ────────────────────────────────────────────
    public double ThemeCornerRadius
    {
        get => _workingCopy.CornerRadius;
        set => SetShape(() => { _workingCopy.CornerRadius = value; });
    }
    public double ThemeBorderThickness
    {
        get => _workingCopy.BorderThickness;
        set => SetShape(() => { _workingCopy.BorderThickness = value; });
    }
    public double ThemeControlHeight
    {
        get => _workingCopy.ControlHeight;
        set => SetShape(() => { _workingCopy.ControlHeight = value; });
    }
    public SpacingDensity SpacingDensity
    {
        get => _workingCopy.SpacingDensity;
        set => SetShape(() => { _workingCopy.SpacingDensity = value; });
    }
    public IReadOnlyList<SpacingDensity> SpacingDensityValues { get; } =
        (SpacingDensity[])Enum.GetValues(typeof(SpacingDensity));

    // ── Contrast computed properties (WCAG 2.1) ───────────────────────────────
    public double ContrastPrimaryOnWindow        => ContrastCalculator.GetRatio(_workingCopy.PrimaryText,   _workingCopy.WindowBackground);
    public string ContrastPrimaryOnWindowStatus  => ContrastCalculator.GetStatus(ContrastPrimaryOnWindow);
    public double ContrastPrimaryOnPanel         => ContrastCalculator.GetRatio(_workingCopy.PrimaryText,   _workingCopy.PanelBackground);
    public string ContrastPrimaryOnPanelStatus   => ContrastCalculator.GetStatus(ContrastPrimaryOnPanel);
    public double ContrastMutedOnWindow          => ContrastCalculator.GetRatio(_workingCopy.MutedText,     _workingCopy.WindowBackground);
    public string ContrastMutedOnWindowStatus    => ContrastCalculator.GetStatus(ContrastMutedOnWindow);
    public double ContrastAccentOnWindow         => ContrastCalculator.GetRatio(_workingCopy.Accent,        _workingCopy.WindowBackground);
    public string ContrastAccentOnWindowStatus   => ContrastCalculator.GetStatus(ContrastAccentOnWindow);

    // ── Commands ─────────────────────────────────────────────────────────────
    public ICommand SaveCommand           { get; }
    public ICommand SaveAsCommand         { get; }
    public ICommand DuplicateCommand      { get; }
    public ICommand ResetEditsCommand     { get; }
    public ICommand DeleteCommand         { get; }
    /// <summary>Parameter: property name string (e.g. "Accent").</summary>
    public ICommand PickColorCommand      { get; }
    /// <summary>Parameter: property name string — resets that single colour to its default value.</summary>
    public ICommand ResetColorCommand     { get; }
    /// <summary>Exports the current working copy to a .ttstheme file chosen by the user.</summary>
    public ICommand ExportCommand         { get; }
    /// <summary>Imports a .ttstheme file and applies it as a new working copy.</summary>
    public ICommand ImportCommand         { get; }

    // ── Constructor ──────────────────────────────────────────────────────────
    public ThemeSettingsViewModel(IThemeService themeService, ILoggingService log, IColorPickerService colorPicker)
    {
        _themeService  = themeService;
        _log           = log;
        _colorPicker   = colorPicker;

        SaveCommand       = new AsyncRelayCommand(SaveAsync,       () => IsDirty);
        SaveAsCommand     = new AsyncRelayCommand(SaveAsPromptAsync);
        DuplicateCommand  = new AsyncRelayCommand(DuplicateAsync);
        ResetEditsCommand = new RelayCommand(RevertToSaved,        () => IsDirty);
        DeleteCommand     = new RelayCommand(QueueDelete,          () => !IsEditingBuiltIn);
        PickColorCommand  = new RelayCommand(obj => PickColor(obj as string));
        ResetColorCommand = new RelayCommand(obj => ResetColorToDefault(obj as string));
        ExportCommand     = new AsyncRelayCommand(ExecuteExportAsync);
        ImportCommand     = new AsyncRelayCommand(ExecuteImportAsync);
    }

    // ── Load / revert ────────────────────────────────────────────────────────

    /// <summary>Called by SettingsViewModel when the settings window opens.</summary>
    public void LoadThemes(string activeThemeName)
    {
        _pendingDeletes.Clear();
        _pendingNews.Clear();
        _hasColourEdits = false;
        AvailableThemes.Clear();
        foreach (var t in _themeService.LoadAll())
            AvailableThemes.Add(t);

        var active = AvailableThemes.FirstOrDefault(t =>
            string.Equals(t.Name, activeThemeName, StringComparison.OrdinalIgnoreCase))
            ?? AvailableThemes.First();

        _savedSnapshot = active.Clone();
        _savedSnapshot.IsBuiltIn = active.IsBuiltIn;
        _originalSnapshot = active.Clone();
        _originalSnapshot.IsBuiltIn = active.IsBuiltIn;
        _workingCopy   = active.Clone();
        _themeService.Apply(_workingCopy);   // ensure live brushes match what we loaded
        _selectedPreset = active;
        OnPropertyChanged(nameof(SelectedPreset));
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        IsDirty = false;
    }

    /// <summary>
    /// Called by SettingsViewModel.CancelAsync() — reverts the live app colours to the
    /// last-saved theme state without persisting anything.
    /// </summary>
    public async Task RevertChangesAsync()
    {
        // Delete all themes created this session via Save As / Duplicate — user hit Cancel
        // so they never intended to keep them.
        foreach (var name in _pendingNews)
            await _themeService.DeleteUserThemeAsync(name);
        _pendingNews.Clear();

        // Discard all pending deletes — reload the full list from disk so queued-deleted
        // themes reappear in the dropdown.
        _pendingDeletes.Clear();
        AvailableThemes.Clear();
        foreach (var t in _themeService.LoadAll())
            AvailableThemes.Add(t);

        // Restore to whatever was active when Settings was opened — not just the last Save.
        _workingCopy    = _originalSnapshot.Clone();
        _savedSnapshot  = _originalSnapshot.Clone();
        _themeService.Apply(_workingCopy);
        // Restore the preset combobox to the original selection
        _selectedPreset = AvailableThemes.FirstOrDefault(t =>
            string.Equals(t.Name, _originalSnapshot.Name, StringComparison.OrdinalIgnoreCase));
        OnPropertyChanged(nameof(SelectedPreset));
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        _hasColourEdits = false;
        _hasColourEdits = false;
        IsDirty = false;
    }

    /// <summary>
    /// Called by SettingsViewModel.SaveAsync() — persists the active theme choice and name.
    /// If editing a built-in, always saves as a NEW user theme.
    /// </summary>
    public async Task CommitAsync(IConfigService config)
    {
        // Always persist the currently-active theme name to config — even if no colour
        // edits are dirty.  The inner Save button clears IsDirty, but the outer OK/Save
        // still needs to record which theme is selected.
        config.CurrentConfig.ActiveThemeName = _workingCopy.Name;

        if (!IsDirty)
        {
            // Nothing else to flush or save — name is already written above.
            return;
        }

        // Flush pending deletes first
        await FlushPendingDeletesAsync();

        if (IsEditingBuiltIn && _hasColourEdits)
        {
            // User actually edited colours on a built-in — fork to a new user theme.
            var forkName = GenerateUniqueName(_workingCopy.Name);
            _workingCopy.Name = forkName;
            OnPropertyChanged(nameof(ThemeName));
            await _themeService.SaveAsUserThemeAsync(_workingCopy, forkName);
            _pendingNews.Clear();
            _savedSnapshot = _workingCopy.Clone();
            _savedSnapshot.IsBuiltIn = false;
            _originalSnapshot = _savedSnapshot.Clone();
            RefreshPresetList();
            _hasColourEdits = false;
            IsDirty = false;
            OnPropertyChanged(nameof(IsEditingBuiltIn));
            _log.Info($"Built-in theme forked as new user theme '{forkName}'.");
        }
        else if (!IsEditingBuiltIn)
        {
            // Rename: if the user changed the theme name, delete the old file first.
            if (!string.Equals(_savedSnapshot.Name, _workingCopy.Name, StringComparison.OrdinalIgnoreCase))
            {
                await _themeService.DeleteUserThemeAsync(_savedSnapshot.Name);
                _pendingNews.Remove(_savedSnapshot.Name);
                _log.Info($"Theme renamed '{_savedSnapshot.Name}' → '{_workingCopy.Name}'.");
            }
            await _themeService.SaveAsUserThemeAsync(_workingCopy, _workingCopy.Name);
            _pendingNews.Clear();
            _savedSnapshot = _workingCopy.Clone();
            _originalSnapshot = _savedSnapshot.Clone();
            RefreshPresetList();
            _hasColourEdits = false;
            IsDirty = false;
            _log.Info($"User theme '{_workingCopy.Name}' saved.");
        }
        else
        {
            // IsEditingBuiltIn && !_hasColourEdits: user just selected a built-in preset
            // without changing any colours.  ActiveThemeName is already written above;
            // no file I/O needed — just reset state.
            _hasColourEdits = false;
            IsDirty = false;
        }
    }

    // ── Private command implementations ──────────────────────────────────────

    private async Task SaveAsync()
    {
        // Flush pending deletes regardless of which branch runs below
        await FlushPendingDeletesAsync();

        if (IsEditingBuiltIn && _hasColourEdits)
        {
            // User actually edited colours on a built-in — fork to a new user theme.
            var forkName = GenerateUniqueName(_workingCopy.Name);
            _workingCopy.Name = forkName;
            OnPropertyChanged(nameof(ThemeName));
            await _themeService.SaveAsUserThemeAsync(_workingCopy, forkName);
            _savedSnapshot = _workingCopy.Clone();
            _savedSnapshot.IsBuiltIn = false;
            _originalSnapshot = _savedSnapshot.Clone();
            _pendingNews.Clear();
            _hasColourEdits = false;
            RefreshPresetList();
            IsDirty = false;
            OnPropertyChanged(nameof(IsEditingBuiltIn));
            _log.Info($"New theme '{forkName}' created from built-in.");
        }
        else if (!IsEditingBuiltIn)
        {
            // Rename: if the user changed the theme name, delete the old file first.
            if (!string.Equals(_savedSnapshot.Name, _workingCopy.Name, StringComparison.OrdinalIgnoreCase))
            {
                await _themeService.DeleteUserThemeAsync(_savedSnapshot.Name);
                _pendingNews.Remove(_savedSnapshot.Name);
                _log.Info($"Theme renamed '{_savedSnapshot.Name}' → '{_workingCopy.Name}'.");
            }
            await _themeService.SaveAsUserThemeAsync(_workingCopy, _workingCopy.Name);
            _savedSnapshot = _workingCopy.Clone();
            _originalSnapshot = _savedSnapshot.Clone();
            _pendingNews.Clear();
            _hasColourEdits = false;
            RefreshPresetList();
            IsDirty = false;
            _log.Info($"Theme '{_workingCopy.Name}' saved.");
        }
        else
        {
            // IsEditingBuiltIn && !_hasColourEdits: user just selected a built-in preset.
            // No file I/O needed — just reset state.
            _hasColourEdits = false;
            IsDirty = false;
        }
    }

    private async Task SaveAsPromptAsync()
    {
        var defaultName = GenerateUniqueName(_workingCopy.Name);
        var name = PromptName("Save Theme As", defaultName);
        if (name is null) return;
        // If the chosen name collides with an existing theme, make it unique.
        var safeName = string.Equals(name, _workingCopy.Name, StringComparison.OrdinalIgnoreCase)
            ? GenerateUniqueName(name)
            : name;
        _workingCopy.Name = safeName;
        OnPropertyChanged(nameof(ThemeName));
        await _themeService.SaveAsUserThemeAsync(_workingCopy, safeName);
        // Track this new file so RevertChangesAsync can delete it if the user hits Cancel.
        _pendingNews.Add(safeName);
        _savedSnapshot = _workingCopy.Clone();
        _savedSnapshot.IsBuiltIn = false;
        _originalSnapshot = _savedSnapshot.Clone();
        RefreshPresetList();
        IsDirty = false;
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        _log.Info($"Theme saved as '{safeName}' (pending session commit).");
    }

    private async Task DuplicateAsync()
    {
        // GenerateUniqueName handles "Copy", "Copy (2)", "Copy (3)" … so repeated
        // duplicates never collide with an existing theme.
        var name = GenerateUniqueName(_workingCopy.Name);
        var dupe = _workingCopy.Clone();
        dupe.Name = name;
        dupe.IsBuiltIn = false;
        await _themeService.SaveAsUserThemeAsync(dupe, name);
        // Track so Cancel cleans it up.
        _pendingNews.Add(name);
        RefreshPresetList();
        _log.Info($"Theme duplicated as '{name}' (pending session commit).");
    }

    private void RevertToSaved()
    {
        _workingCopy = _savedSnapshot.Clone();
        _themeService.Apply(_workingCopy);
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        _hasColourEdits = false;
        IsDirty = false;
    }

    /// <summary>
    /// Queues the current user theme for deletion without touching disk.
    /// The delete is executed when the user clicks Save (inner or outer).
    /// Cancel clears the queue and restores the full list from disk.
    /// </summary>
    private void QueueDelete()
    {
        if (IsEditingBuiltIn) return;
        var nameToDelete = _savedSnapshot.Name;
        if (_pendingDeletes.Contains(nameToDelete)) return;

        _pendingDeletes.Add(nameToDelete);
        _log.Info($"Theme '{nameToDelete}' queued for deletion (will be removed on Save).");

        // Remove from the visible list immediately so it disappears from the dropdown
        var toRemove = AvailableThemes.FirstOrDefault(t =>
            string.Equals(t.Name, nameToDelete, StringComparison.OrdinalIgnoreCase));
        if (toRemove is not null) AvailableThemes.Remove(toRemove);

        // Switch to the first remaining theme (prefer Default Dark)
        var fallback = AvailableThemes.FirstOrDefault(t =>
                           string.Equals(t.Name, "Default Dark", StringComparison.OrdinalIgnoreCase))
                       ?? AvailableThemes.FirstOrDefault();

        if (fallback is not null)
        {
            _selectedPreset = fallback;
            ApplyPreset(fallback);
            OnPropertyChanged(nameof(SelectedPreset));
        }

        MarkDirty();
    }

    private void ApplyPreset(ThemeSettings preset)
    {
        // _savedSnapshot tracks the baseline for the in-tab Reset button (undo colour edits).
        // _originalSnapshot stays fixed at what was active when Settings opened (used by Cancel).
        _savedSnapshot = preset.Clone();
        _savedSnapshot.IsBuiltIn = preset.IsBuiltIn;
        _workingCopy = preset.Clone();
        _themeService.Apply(_workingCopy);
        RaiseAllColourProperties();
        OnPropertyChanged(nameof(ThemeName));
        OnPropertyChanged(nameof(IsEditingBuiltIn));
        // Switching to a different preset IS a change — mark dirty so Cancel knows to revert,
        // and so the parent Save button is enabled.
        // Selecting a preset does NOT count as a colour edit — only actual colour mutations do.
        _hasColourEdits = false;
        IsDirty = !string.Equals(preset.Name, _originalSnapshot.Name, StringComparison.OrdinalIgnoreCase);
    }

    private void PickColor(string? propertyName)
    {
        if (propertyName is null) return;
        var currentHex = GetColorProperty(propertyName);
        if (currentHex is null) return;

        var hex = _colorPicker.PickColor(currentHex);
        if (hex is null) return;
        SetColorByName(propertyName, hex);
    }

    private void ResetColorToDefault(string? propertyName)
    {
        if (propertyName is null) return;
        var defaults = ThemeDefaults.CreateDefault();
        var defaultHex = GetColorPropertyFrom(defaults, propertyName);
        if (defaultHex is null) return;
        SetColorByName(propertyName, defaultHex);
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private void SetColor(Action<string> setter, string value, [CallerMemberName] string? propName = null)
    {
        if (!IsValidHex(value)) return;
        setter(value);
        _themeService.Apply(_workingCopy);
        if (propName is not null) OnPropertyChanged(propName);
        RaiseContrastProperties();
        _hasColourEdits = true;
        MarkDirty();
    }

    private void SetTypography(Action setter, [CallerMemberName] string? propName = null)
    {
        setter();
        _themeService.Apply(_workingCopy);
        if (propName is not null) OnPropertyChanged(propName);
        _hasColourEdits = true;
        MarkDirty();
    }

    private void SetShape(Action setter, [CallerMemberName] string? propName = null)
    {
        setter();
        _themeService.Apply(_workingCopy);
        if (propName is not null) OnPropertyChanged(propName);
        _hasColourEdits = true;
        MarkDirty();
    }

    private void MarkDirty()
    {
        IsDirty = true;
    }

    /// <summary>Executes all queued deletes against disk and clears the queue.</summary>
    private async Task FlushPendingDeletesAsync()
    {
        foreach (var name in _pendingDeletes)
        {
            await _themeService.DeleteUserThemeAsync(name);
            _log.Info($"Theme '{name}' deleted from disk.");
        }
        _pendingDeletes.Clear();
    }

    private void RaiseAllColourProperties()
    {
        foreach (var name in ColourPropertyNames)
            OnPropertyChanged(name);
        RaiseContrastProperties();
        RaiseTypographyProperties();
        RaiseShapeProperties();
    }

    private void RaiseContrastProperties()
    {
        OnPropertyChanged(nameof(ContrastPrimaryOnWindow));
        OnPropertyChanged(nameof(ContrastPrimaryOnWindowStatus));
        OnPropertyChanged(nameof(ContrastPrimaryOnPanel));
        OnPropertyChanged(nameof(ContrastPrimaryOnPanelStatus));
        OnPropertyChanged(nameof(ContrastMutedOnWindow));
        OnPropertyChanged(nameof(ContrastMutedOnWindowStatus));
        OnPropertyChanged(nameof(ContrastAccentOnWindow));
        OnPropertyChanged(nameof(ContrastAccentOnWindowStatus));
    }

    private void RaiseTypographyProperties()
    {
        OnPropertyChanged(nameof(UiFontFamily));
        OnPropertyChanged(nameof(BaseFontSize));
        OnPropertyChanged(nameof(OverlayFontFamily));
        OnPropertyChanged(nameof(OverlayFontSize));
    }

    private void RaiseShapeProperties()
    {
        OnPropertyChanged(nameof(ThemeCornerRadius));
        OnPropertyChanged(nameof(ThemeBorderThickness));
        OnPropertyChanged(nameof(ThemeControlHeight));
        OnPropertyChanged(nameof(SpacingDensity));
    }

    private static readonly string[] ColourPropertyNames =
    {
        nameof(WindowBackground), nameof(PanelBackground), nameof(DeepPanelBackground),
        nameof(HistoryBackground), nameof(Surface0), nameof(BorderColor), nameof(Surface2),
        nameof(Accent), nameof(AccentHover),
        nameof(PrimaryText), nameof(SecondaryText), nameof(MutedText), nameof(MutedIcon),
        nameof(InfoColor), nameof(ErrorColor), nameof(ErrorHover), nameof(Warning), nameof(Success),
    };

    private string? GetColorProperty(string propertyName) =>
        GetColorPropertyFrom(_workingCopy, propertyName);

    private static string? GetColorPropertyFrom(ThemeSettings t, string name) => name switch
    {
        nameof(WindowBackground)    => t.WindowBackground,
        nameof(PanelBackground)     => t.PanelBackground,
        nameof(DeepPanelBackground) => t.DeepPanelBackground,
        nameof(HistoryBackground)   => t.HistoryBackground,
        nameof(Surface0)            => t.Surface0,
        nameof(BorderColor)         => t.BorderColor,
        nameof(Surface2)            => t.Surface2,
        nameof(Accent)              => t.Accent,
        nameof(AccentHover)         => t.AccentHover,
        nameof(PrimaryText)         => t.PrimaryText,
        nameof(SecondaryText)       => t.SecondaryText,
        nameof(MutedText)           => t.MutedText,
        nameof(MutedIcon)           => t.MutedIcon,
        nameof(InfoColor)           => t.InfoColor,
        nameof(ErrorColor)          => t.ErrorColor,
        nameof(ErrorHover)          => t.ErrorHover,
        nameof(Warning)             => t.Warning,
        nameof(Success)             => t.Success,
        _ => null
    };

    private void SetColorByName(string name, string hex)
    {
        switch (name)
        {
            case nameof(WindowBackground):    WindowBackground    = hex; break;
            case nameof(PanelBackground):     PanelBackground     = hex; break;
            case nameof(DeepPanelBackground): DeepPanelBackground = hex; break;
            case nameof(HistoryBackground):   HistoryBackground   = hex; break;
            case nameof(Surface0):            Surface0            = hex; break;
            case nameof(BorderColor):         BorderColor         = hex; break;
            case nameof(Surface2):            Surface2            = hex; break;
            case nameof(Accent):              Accent              = hex; break;
            case nameof(AccentHover):         AccentHover         = hex; break;
            case nameof(PrimaryText):         PrimaryText         = hex; break;
            case nameof(SecondaryText):       SecondaryText       = hex; break;
            case nameof(MutedText):           MutedText           = hex; break;
            case nameof(MutedIcon):           MutedIcon           = hex; break;
            case nameof(InfoColor):           InfoColor           = hex; break;
            case nameof(ErrorColor):          ErrorColor          = hex; break;
            case nameof(ErrorHover):          ErrorHover          = hex; break;
            case nameof(Warning):             Warning             = hex; break;
            case nameof(Success):             Success             = hex; break;
        }
    }

    private void RefreshPresetList()
    {
        AvailableThemes.Clear();
        foreach (var t in _themeService.LoadAll())
            AvailableThemes.Add(t);
        _selectedPreset = AvailableThemes.FirstOrDefault(t =>
            string.Equals(t.Name, _workingCopy.Name, StringComparison.OrdinalIgnoreCase))
            ?? AvailableThemes.First();
        OnPropertyChanged(nameof(SelectedPreset));
    }

    /// <summary>
    /// Returns a name based on <paramref name="baseName"/> that does not already exist in
    /// <see cref="AvailableThemes"/>.  Appends " Copy", " Copy (2)", " Copy (3)", … until unique.
    /// </summary>
    private string GenerateUniqueName(string baseName)
    {
        // Strip any trailing built-in marker so we get clean fork names
        var root = baseName.Trim();
        var candidate = root + " Copy";
        int n = 2;
        while (AvailableThemes.Any(t =>
            string.Equals(t.Name, candidate, StringComparison.OrdinalIgnoreCase)))
        {
            candidate = $"{root} Copy ({n++})";
        }
        return candidate;
    }

    private static bool IsValidHex(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return false;
        try { ColorConverter.ConvertFromString(hex); return true; }
        catch { return false; }
    }

    // ── Import / Export command implementations ──────────────────────────────

    private async Task ExecuteExportAsync()
    {
        var dlg = new SaveFileDialog
        {
            Title      = "Export Theme",
            Filter     = "TTS Theme|*.ttstheme",
            FileName   = _workingCopy.Name,
            DefaultExt = ".ttstheme",
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            await _themeService.ExportAsync(_workingCopy, dlg.FileName);
            _log.Info($"Theme exported to '{dlg.FileName}'.");
        }
        catch (Exception ex)
        {
            _log.Error($"Theme export failed: {ex.Message}");
            MessageBox.Show($"Export failed: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async Task ExecuteImportAsync()
    {
        var dlg = new OpenFileDialog
        {
            Title  = "Import Theme",
            Filter = "TTS Theme|*.ttstheme",
        };
        if (dlg.ShowDialog() != true) return;
        try
        {
            var imported = await _themeService.ImportAsync(dlg.FileName);
            // Ensure a unique name before saving
            if (AvailableThemes.Any(t => string.Equals(t.Name, imported.Name, StringComparison.OrdinalIgnoreCase)))
                imported.Name = GenerateUniqueName(imported.Name);
            await _themeService.SaveAsUserThemeAsync(imported, imported.Name);
            _pendingNews.Add(imported.Name);
            _workingCopy = imported.Clone();
            _savedSnapshot = imported.Clone();
            _themeService.Apply(_workingCopy);
            RefreshPresetList();
            RaiseAllColourProperties();
            OnPropertyChanged(nameof(ThemeName));
            OnPropertyChanged(nameof(IsEditingBuiltIn));
            _hasColourEdits = true;
            IsDirty = true;
            _log.Info($"Theme imported from '{dlg.FileName}' as '{imported.Name}'.");
        }
        catch (Exception ex)
        {
            _log.Error($"Theme import failed: {ex.Message}");
            MessageBox.Show($"Import failed: {ex.Message}", "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ── Name prompt (minimal WPF input window) ────────────────────────────────
    private static string? PromptName(string title, string defaultValue)
    {
        string? result = null;
        var bg = Application.Current.Resources["WindowBackgroundBrush"] as SolidColorBrush
                 ?? new SolidColorBrush(Color.FromRgb(0x1E, 0x1E, 0x2E));
        var fg = Application.Current.Resources["PrimaryTextBrush"] as SolidColorBrush
                 ?? new SolidColorBrush(Color.FromRgb(0xCD, 0xD6, 0xF4));
        var border = Application.Current.Resources["BorderBrush"] as SolidColorBrush
                     ?? new SolidColorBrush(Color.FromRgb(0x45, 0x47, 0x5A));

        var tb = new System.Windows.Controls.TextBox
        {
            Text = defaultValue,
            Margin = new Thickness(12, 12, 12, 8),
            Padding = new Thickness(8, 6, 8, 6),
            Background = Application.Current.Resources["PanelBackgroundBrush"] as SolidColorBrush ?? bg,
            Foreground = fg,
            BorderBrush = border,
            BorderThickness = new Thickness(1),
            FontSize = 13,
            CaretBrush = Application.Current.Resources["AccentBrush"] as SolidColorBrush ?? fg,
        };

        var ok = new System.Windows.Controls.Button
        {
            Content = "OK",
            Width = 80, Height = 30,
            Margin = new Thickness(0, 0, 8, 0),
            Background = Application.Current.Resources["AccentBrush"] as SolidColorBrush ?? fg,
            Foreground = bg,
            BorderThickness = new Thickness(0),
        };
        var cancel = new System.Windows.Controls.Button
        {
            Content = "Cancel",
            Width = 80, Height = 30,
            Background = Application.Current.Resources["Surface2Brush"] as SolidColorBrush ?? border,
            Foreground = fg,
            BorderThickness = new Thickness(0),
        };

        var btns = new System.Windows.Controls.StackPanel
        {
            Orientation = System.Windows.Controls.Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(12, 0, 12, 12),
        };
        btns.Children.Add(ok);
        btns.Children.Add(cancel);

        var panel = new System.Windows.Controls.StackPanel();
        panel.Children.Add(tb);
        panel.Children.Add(btns);

        var win = new Window
        {
            Title = title,
            Width = 360, Height = 130,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            Background = bg,
            Content = panel,
            Owner = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive),
        };

        ok.Click     += (_, _) => { result = tb.Text.Trim(); win.Close(); };
        cancel.Click += (_, _) => win.Close();
        win.Loaded   += (_, _) => { tb.SelectAll(); tb.Focus(); };
        tb.KeyDown   += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)  { result = tb.Text.Trim(); win.Close(); }
            if (e.Key == System.Windows.Input.Key.Escape) { win.Close(); }
        };

        win.ShowDialog();
        return string.IsNullOrWhiteSpace(result) ? null : result;
    }
}
