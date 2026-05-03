using System.Windows.Media;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class AppearanceSettingsViewModel : ViewModelBase
{
    private double _overlayWidth;
    private double _overlayHeight;
    private double _fontSize;
    private double _overlayOpacity = 0.93;
    private string _overlayFontFamily = "Segoe UI";

    public double OverlayWidth
    {
        get => _overlayWidth;
        set => SetField(ref _overlayWidth, value);
    }

    public double OverlayHeight
    {
        get => _overlayHeight;
        set => SetField(ref _overlayHeight, value);
    }

    public double FontSize
    {
        get => _fontSize;
        set => SetField(ref _fontSize, value);
    }

    /// <summary>
    /// Optional callback invoked every time <see cref="OverlayOpacity"/> changes so the
    /// overlay window can preview the new opacity live without waiting for a Save.
    /// Wire this up in <c>App.xaml.cs</c> before showing the settings window.
    /// </summary>
    public Action<double>? LiveOpacityPreview { get; set; }

    /// <summary>
    /// Optional callback invoked every time <see cref="OverlayFontFamily"/> changes so the
    /// overlay window can preview the new font live without waiting for a Save.
    /// Wire this up in <c>App.xaml.cs</c> before showing the settings window.
    /// </summary>
    public Action<string>? LiveFontPreview { get; set; }

    /// <summary>Background opacity of the overlay (clamped 0.2 – 0.95).</summary>
    public double OverlayOpacity
    {
        get => _overlayOpacity;
        set
        {
            if (SetField(ref _overlayOpacity, Math.Clamp(value, 0.20, 0.95)))
                LiveOpacityPreview?.Invoke(_overlayOpacity);
        }
    }

    /// <summary>Font family name for the overlay text input.</summary>
    public string OverlayFontFamily
    {
        get => _overlayFontFamily;
        set
        {
            if (SetField(ref _overlayFontFamily, value))
                LiveFontPreview?.Invoke(_overlayFontFamily);
        }
    }

    /// <summary>All installed font families sorted alphabetically (for the settings dropdown).</summary>
    public IReadOnlyList<string> InstalledFonts { get; } =
        Fonts.SystemFontFamilies
             .Select(f => f.Source)
             .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
             .ToList();

    public void LoadFrom(OverlaySettings s)
    {
        OverlayWidth = s.Width;
        OverlayHeight = s.Height;
        FontSize = s.FontSize;
        OverlayOpacity = s.OverlayOpacity;
        OverlayFontFamily = s.OverlayFontFamily;
    }

    public void ApplyTo(OverlaySettings s)
    {
        s.Width = OverlayWidth;
        s.Height = OverlayHeight;
        s.FontSize = FontSize;
        s.OverlayOpacity = Math.Clamp(OverlayOpacity, 0.20, 0.95);
        s.OverlayFontFamily = OverlayFontFamily;
    }
}
