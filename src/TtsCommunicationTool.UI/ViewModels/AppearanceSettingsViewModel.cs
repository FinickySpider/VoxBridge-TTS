using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class AppearanceSettingsViewModel : ViewModelBase
{
    private double _overlayWidth;
    private double _overlayHeight;
    private double _overlayOpacity = 0.93;

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

    /// <summary>
    /// Optional callback invoked every time <see cref="OverlayOpacity"/> changes so the
    /// overlay window can preview the new opacity live without waiting for a Save.
    /// Wire this up in <c>App.xaml.cs</c> before showing the settings window.
    /// </summary>
    public Action<double>? LiveOpacityPreview { get; set; }

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

    public void LoadFrom(OverlaySettings s)
    {
        OverlayWidth  = s.Width;
        OverlayHeight = s.Height;
        OverlayOpacity = s.OverlayOpacity;
    }

    public void ApplyTo(OverlaySettings s)
    {
        s.Width  = OverlayWidth;
        s.Height = OverlayHeight;
        s.OverlayOpacity = Math.Clamp(OverlayOpacity, 0.20, 0.95);
    }
}
