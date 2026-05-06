namespace TtsCommunicationTool.Core.Models;

/// <summary>
/// Factory methods for all built-in themes.
/// Built-ins are always sorted before user themes in the preset list.
/// </summary>
public static class ThemeDefaults
{
    /// <summary>All built-in themes in display order.</summary>
    public static IReadOnlyList<ThemeSettings> All() =>
    [
        CreateDefault(),
        CreateLight(),
        CreateMidnightPurple(),
        CreateNord(),
    ];

    // ── Default Dark (Catppuccin Mocha) ──────────────────────────────────────
    public static ThemeSettings CreateDefault() => new()
    {
        Name              = "Default Dark",
        IsBuiltIn         = true,
        WindowBackground  = "#1E1E2E",
        PanelBackground   = "#2B2B3D",
        DeepPanelBackground = "#181825",
        HistoryBackground = "#14141E",
        Surface0          = "#313244",
        BorderColor       = "#45475A",
        Surface2          = "#585B70",
        Accent            = "#89B4FA",
        AccentHover       = "#B4D0FB",
        PrimaryText       = "#CDD6F4",
        SecondaryText     = "#A6ADC8",
        MutedText         = "#6C7086",
        MutedIcon         = "#8B9CC8",
        ErrorColor        = "#F38BA8",
        ErrorHover        = "#F5A0B5",
        Warning           = "#F9E2AF",
        InfoColor         = "#FAB387",
        Success           = "#A6E3A1",
        UiFontFamily      = "Segoe UI",
        BaseFontSize      = 13,
        OverlayFontFamily = "Segoe UI",
        OverlayFontSize   = 18,
        CornerRadius      = 6,
        BorderThickness   = 1,
        ControlHeight     = 28,
        SpacingDensity    = SpacingDensity.Comfortable,
        OverlayBackgroundHex = "#1E1E2E",
        OverlayBorderHex     = "#45475A",
        OverlayOpacity       = 0.93,
        OverlayCornerRadius  = 12,
    };

    // ── Default Light (Catppuccin Latte) ─────────────────────────────────────
    public static ThemeSettings CreateLight() => new()
    {
        Name              = "Default Light",
        IsBuiltIn         = true,
        WindowBackground  = "#EFF1F5",
        PanelBackground   = "#E6E9EF",
        DeepPanelBackground = "#DCE0E8",
        HistoryBackground = "#CCD0DA",
        Surface0          = "#CCD0DA",
        BorderColor       = "#BCC0CC",
        Surface2          = "#ACB0BE",
        Accent            = "#1E66F5",
        AccentHover       = "#4C7EF7",
        PrimaryText       = "#4C4F69",
        SecondaryText     = "#5C5F77",
        MutedText         = "#9CA0B0",
        MutedIcon         = "#8C8FA1",
        ErrorColor        = "#D20F39",
        ErrorHover        = "#D9395F",
        Warning           = "#DF8E1D",
        InfoColor         = "#FE640B",
        Success           = "#40A02B",
        UiFontFamily      = "Segoe UI",
        BaseFontSize      = 13,
        OverlayFontFamily = "Segoe UI",
        OverlayFontSize   = 18,
        CornerRadius      = 6,
        BorderThickness   = 1,
        ControlHeight     = 28,
        SpacingDensity    = SpacingDensity.Comfortable,
        OverlayBackgroundHex = "#EFF1F5",
        OverlayBorderHex     = "#BCC0CC",
        OverlayOpacity       = 0.95,
        OverlayCornerRadius  = 12,
    };

    // ── Midnight Purple ───────────────────────────────────────────────────────
    public static ThemeSettings CreateMidnightPurple() => new()
    {
        Name              = "Midnight Purple",
        IsBuiltIn         = true,
        WindowBackground  = "#1A0E2E",
        PanelBackground   = "#221440",
        DeepPanelBackground = "#130A22",
        HistoryBackground = "#0D061A",
        Surface0          = "#2D1B52",
        BorderColor       = "#4A3070",
        Surface2          = "#5C3F8A",
        Accent            = "#B06AFF",
        AccentHover       = "#C890FF",
        PrimaryText       = "#E8DAFF",
        SecondaryText     = "#C0A8E8",
        MutedText         = "#7A5FA0",
        MutedIcon         = "#9070B8",
        ErrorColor        = "#FF6E8A",
        ErrorHover        = "#FF8FA5",
        Warning           = "#FFCA80",
        InfoColor         = "#FF9E64",
        Success           = "#7FD97F",
        UiFontFamily      = "Segoe UI",
        BaseFontSize      = 13,
        OverlayFontFamily = "Segoe UI",
        OverlayFontSize   = 18,
        CornerRadius      = 8,
        BorderThickness   = 1,
        ControlHeight     = 28,
        SpacingDensity    = SpacingDensity.Comfortable,
        OverlayBackgroundHex = "#1A0E2E",
        OverlayBorderHex     = "#4A3070",
        OverlayOpacity       = 0.93,
        OverlayCornerRadius  = 14,
    };

    // ── Nord ─────────────────────────────────────────────────────────────────
    public static ThemeSettings CreateNord() => new()
    {
        Name              = "Nord",
        IsBuiltIn         = true,
        WindowBackground  = "#2E3440",
        PanelBackground   = "#3B4252",
        DeepPanelBackground = "#252A35",
        HistoryBackground = "#1E2430",
        Surface0          = "#434C5E",
        BorderColor       = "#4C566A",
        Surface2          = "#60708A",
        Accent            = "#88C0D0",
        AccentHover       = "#8FBCBB",
        PrimaryText       = "#ECEFF4",
        SecondaryText     = "#D8DEE9",
        MutedText         = "#7B88A4",
        MutedIcon         = "#6B7A94",
        ErrorColor        = "#BF616A",
        ErrorHover        = "#D0737B",
        Warning           = "#EBCB8B",
        InfoColor         = "#D08770",
        Success           = "#A3BE8C",
        UiFontFamily      = "Segoe UI",
        BaseFontSize      = 13,
        OverlayFontFamily = "Segoe UI",
        OverlayFontSize   = 18,
        CornerRadius      = 4,
        BorderThickness   = 1,
        ControlHeight     = 28,
        SpacingDensity    = SpacingDensity.Comfortable,
        OverlayBackgroundHex = "#2E3440",
        OverlayBorderHex     = "#4C566A",
        OverlayOpacity       = 0.93,
        OverlayCornerRadius  = 10,
    };
}
