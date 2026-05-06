namespace TtsCommunicationTool.Core.Models;

/// <summary>
/// Returns the built-in "Default Dark" (Catppuccin Mocha) theme.
/// This is the canonical fallback used by <c>ThemeService.ResetToDefault()</c>
/// and as the base for the embedded DefaultDark resource.
/// </summary>
public static class ThemeDefaults
{
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
        // Typography
        UiFontFamily      = "Segoe UI",
        BaseFontSize      = 13,
        OverlayFontFamily = "Segoe UI",
        OverlayFontSize   = 18,
        // Shape & Density
        CornerRadius      = 6,
        BorderThickness   = 1,
        ControlHeight     = 28,
        SpacingDensity    = SpacingDensity.Comfortable,
        // Overlay overrides (empty = inherit from main colours)
        OverlayBackgroundHex = "#1E1E2E",
        OverlayBorderHex     = "#45475A",
        OverlayOpacity       = 0.93,
        OverlayCornerRadius  = 12,
    };
}
