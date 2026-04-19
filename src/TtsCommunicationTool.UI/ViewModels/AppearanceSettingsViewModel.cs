using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class AppearanceSettingsViewModel : ViewModelBase
{
    private double _overlayWidth;
    private double _overlayHeight;
    private double _fontSize;

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

    public void LoadFrom(OverlaySettings s)
    {
        OverlayWidth = s.Width;
        OverlayHeight = s.Height;
        FontSize = s.FontSize;
    }

    public void ApplyTo(OverlaySettings s)
    {
        s.Width = OverlayWidth;
        s.Height = OverlayHeight;
        s.FontSize = FontSize;
    }
}
