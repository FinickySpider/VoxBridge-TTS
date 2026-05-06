using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Themes;

/// <summary>
/// Loads, applies, and persists colour themes.
/// Built-in themes are defined in code (ThemeDefaults); user themes live in
/// %AppData%\TtsCommunicationTool\themes\ as *.ttstheme JSON files.
/// </summary>
public sealed class ThemeService : IThemeService
{
    private readonly IConfigService _config;
    private readonly ILoggingService _log;
    private string _activeThemeName = "Default Dark";

    private static readonly string UserThemesFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool",
        "themes");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    // Keys in Application.Resources that correspond to ThemeSettings properties.
    // Order matches the Apply() reflection loop below.
    private static readonly (string Property, string ResourceKey)[] ColourMap =
    {
        (nameof(ThemeSettings.WindowBackground),   "WindowBackgroundBrush"),
        (nameof(ThemeSettings.PanelBackground),    "PanelBackgroundBrush"),
        (nameof(ThemeSettings.DeepPanelBackground),"DeepPanelBackgroundBrush"),
        (nameof(ThemeSettings.HistoryBackground),  "HistoryBackgroundBrush"),
        (nameof(ThemeSettings.Surface0),           "Surface0Brush"),
        (nameof(ThemeSettings.BorderColor),        "BorderBrush"),
        (nameof(ThemeSettings.Surface2),           "Surface2Brush"),
        (nameof(ThemeSettings.Accent),             "AccentBrush"),
        (nameof(ThemeSettings.AccentHover),        "AccentHoverBrush"),
        (nameof(ThemeSettings.PrimaryText),        "PrimaryTextBrush"),
        (nameof(ThemeSettings.SecondaryText),      "SecondaryTextBrush"),
        (nameof(ThemeSettings.MutedText),          "MutedTextBrush"),
        (nameof(ThemeSettings.MutedIcon),          "MutedIconBrush"),
        (nameof(ThemeSettings.InfoColor),          "InfoBrush"),
        (nameof(ThemeSettings.ErrorColor),         "ErrorBrush"),
        (nameof(ThemeSettings.ErrorHover),         "ErrorHoverBrush"),
        (nameof(ThemeSettings.Warning),            "WarningBrush"),
        (nameof(ThemeSettings.Success),            "SuccessBrush"),
    };

    public string ActiveThemeName => _activeThemeName;

    /// <inheritdoc/>
    public event EventHandler? ThemeResetToDefault;

    public ThemeService(IConfigService config, ILoggingService log)
    {
        _config = config;
        _log = log;
    }

    public IReadOnlyList<ThemeSettings> LoadAll()
    {
        var themes = new List<ThemeSettings>();

        // Built-in themes first (immutable)
        var defaultDark = ThemeDefaults.CreateDefault();
        themes.Add(defaultDark);

        // User themes from disk
        if (Directory.Exists(UserThemesFolder))
        {
            foreach (var file in Directory.GetFiles(UserThemesFolder, "*.ttstheme"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var t = JsonSerializer.Deserialize<ThemeSettings>(json, JsonOptions);
                    if (t is not null)
                    {
                        t.IsBuiltIn = false;
                        themes.Add(t);
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn($"ThemeService: failed to load theme '{file}': {ex.Message}");
                }
            }
        }

        return themes;
    }

    public void Apply(ThemeSettings theme)
    {
        // Must run on the UI thread — DynamicResource propagation requires it.
        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke(() => Apply(theme));
            return;
        }

        var res = Application.Current.Resources;
        var themeType = typeof(ThemeSettings);

        foreach (var (propName, resourceKey) in ColourMap)
        {
            var hex = (string?)themeType.GetProperty(propName)?.GetValue(theme);
            if (hex is null) continue;

            try
            {
                var color = (Color)ColorConverter.ConvertFromString(hex);

                // IMPORTANT: Brushes defined in XAML ResourceDictionaries are frozen by WPF,
                // so mutating existing.Color would throw InvalidOperationException and be swallowed.
                // Replacing the dictionary entry causes all {DynamicResource} bindings to re-resolve.
                res[resourceKey] = new SolidColorBrush(color);

                // Also store a raw Color resource (e.g. "WindowBackgroundColor") for
                // the overlay background which needs Opacity+Color separated.
                var colorKey = resourceKey.Replace("Brush", "Color");
                res[colorKey] = color;
            }
            catch (Exception ex)
            {
                _log.Warn($"ThemeService.Apply: invalid colour '{hex}' for '{propName}': {ex.Message}");
            }
        }

        _activeThemeName = theme.Name;
    }

    public void ResetToDefault()
    {
        var def = ThemeDefaults.CreateDefault();
        Apply(def);

        // Persist the choice
        var cfg = _config.CurrentConfig;
        cfg.ActiveThemeName = def.Name;
        _ = _config.SaveAsync(cfg);

        _log.Info("Theme reset to Default Dark.");
        ThemeResetToDefault?.Invoke(this, EventArgs.Empty);
    }

    public async Task SaveAsUserThemeAsync(ThemeSettings theme, string name)
    {
        Directory.CreateDirectory(UserThemesFolder);
        var copy = theme.Clone();
        copy.Name = name;
        copy.IsBuiltIn = false;

        var filePath = GetUserThemePath(name);
        var json = JsonSerializer.Serialize(copy, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        _log.Info($"Theme saved: '{name}' → {filePath}");
    }

    public async Task DeleteUserThemeAsync(string name)
    {
        // Never delete built-in themes
        if (string.Equals(name, "Default Dark", StringComparison.OrdinalIgnoreCase))
            return;

        var path = GetUserThemePath(name);
        if (File.Exists(path))
        {
            await Task.Run(() => File.Delete(path));
            _log.Info($"Theme deleted: '{name}'");
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetUserThemePath(string name)
    {
        // Sanitise name to a safe filename
        var safe = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(UserThemesFolder, safe + ".ttstheme");
    }
}
