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

        // ── Typography ────────────────────────────────────────────────────────
        try
        {
            res["UiFontFamilyResource"]      = new FontFamily(theme.UiFontFamily);
            res["BaseFontSizeResource"]      = theme.BaseFontSize;
            res["OverlayFontFamilyResource"] = new FontFamily(theme.OverlayFontFamily);
            res["OverlayFontSizeResource"]   = theme.OverlayFontSize;
        }
        catch (Exception ex)
        {
            _log.Warn($"ThemeService.Apply: font resource error: {ex.Message}");
        }

        // ── Shape & Density ───────────────────────────────────────────────────
        try
        {
            res["ControlCornerRadiusResource"]   = new CornerRadius(theme.CornerRadius);
            res["ControlBorderThicknessResource"] = new Thickness(theme.BorderThickness);
            res["ControlHeightResource"]          = theme.ControlHeight;

            // Spacing density → three Thickness resources
            var (cp, isp, sec) = theme.SpacingDensity switch
            {
                SpacingDensity.Compact     => (new Thickness(4, 2, 4, 2), new Thickness(0, 2, 0, 2), new Thickness(0, 4, 0, 4)),
                SpacingDensity.Spacious    => (new Thickness(10, 6, 10, 6), new Thickness(0, 6, 0, 6), new Thickness(0, 12, 0, 12)),
                _  /* Comfortable */       => (new Thickness(6, 4, 6, 4), new Thickness(0, 4, 0, 4), new Thickness(0, 8, 0, 8)),
            };
            res["ControlPaddingResource"] = cp;
            res["ItemSpacingResource"]    = isp;
            res["SectionSpacingResource"] = sec;
        }
        catch (Exception ex)
        {
            _log.Warn($"ThemeService.Apply: shape/density resource error: {ex.Message}");
        }
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

    public async Task ExportAsync(ThemeSettings theme, string filePath)
    {
        var copy = theme.Clone();
        copy.IsBuiltIn = false;
        var json = JsonSerializer.Serialize(copy, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
        _log.Info($"Theme exported: '{theme.Name}' → {filePath}");
    }

    public async Task<ThemeSettings> ImportAsync(string filePath)
    {
        var json = await File.ReadAllTextAsync(filePath);
        ThemeSettings? theme;
        try
        {
            theme = JsonSerializer.Deserialize<ThemeSettings>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Could not parse theme file: {ex.Message}", ex);
        }

        if (theme is null)
            throw new InvalidOperationException("Theme file was empty or could not be read.");

        // Validate all 17 required colour fields
        ValidateHex(theme.WindowBackground,    nameof(theme.WindowBackground));
        ValidateHex(theme.PanelBackground,     nameof(theme.PanelBackground));
        ValidateHex(theme.DeepPanelBackground, nameof(theme.DeepPanelBackground));
        ValidateHex(theme.HistoryBackground,   nameof(theme.HistoryBackground));
        ValidateHex(theme.Surface0,            nameof(theme.Surface0));
        ValidateHex(theme.BorderColor,         nameof(theme.BorderColor));
        ValidateHex(theme.Surface2,            nameof(theme.Surface2));
        ValidateHex(theme.Accent,              nameof(theme.Accent));
        ValidateHex(theme.AccentHover,         nameof(theme.AccentHover));
        ValidateHex(theme.PrimaryText,         nameof(theme.PrimaryText));
        ValidateHex(theme.SecondaryText,       nameof(theme.SecondaryText));
        ValidateHex(theme.MutedText,           nameof(theme.MutedText));
        ValidateHex(theme.MutedIcon,           nameof(theme.MutedIcon));
        ValidateHex(theme.InfoColor,           nameof(theme.InfoColor));
        ValidateHex(theme.ErrorColor,          nameof(theme.ErrorColor));
        ValidateHex(theme.ErrorHover,          nameof(theme.ErrorHover));
        ValidateHex(theme.Warning,             nameof(theme.Warning));
        ValidateHex(theme.Success,             nameof(theme.Success));

        theme.IsBuiltIn = false;

        // Apply sensible defaults for new fields that may be absent in older files
        if (string.IsNullOrWhiteSpace(theme.UiFontFamily))      theme.UiFontFamily      = "Segoe UI";
        if (string.IsNullOrWhiteSpace(theme.OverlayFontFamily)) theme.OverlayFontFamily = "Segoe UI";
        if (theme.BaseFontSize   <= 0) theme.BaseFontSize   = 13;
        if (theme.OverlayFontSize <= 0) theme.OverlayFontSize = 18;
        if (theme.CornerRadius    < 0) theme.CornerRadius    = 6;
        if (theme.BorderThickness < 0) theme.BorderThickness = 1;
        if (theme.ControlHeight  <= 0) theme.ControlHeight   = 28;

        _log.Info($"Theme imported: '{theme.Name}' from {filePath}");
        return theme;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string GetUserThemePath(string name)
    {
        // Sanitise name to a safe filename
        var safe = string.Concat(name.Select(c => Path.GetInvalidFileNameChars().Contains(c) ? '_' : c));
        return Path.Combine(UserThemesFolder, safe + ".ttstheme");
    }

    private static void ValidateHex(string value, string fieldName)
    {
        try { System.Windows.Media.ColorConverter.ConvertFromString(value); }
        catch { throw new InvalidOperationException($"Invalid colour value '{value}' in field '{fieldName}'."); }
    }
}
