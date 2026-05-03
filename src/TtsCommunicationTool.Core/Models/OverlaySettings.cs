namespace TtsCommunicationTool.Core.Models;

public sealed class OverlaySettings
{
    public double Width { get; set; } = 720;
    public double Height { get; set; } = 160;
    public double FontSize { get; set; } = 18;

    /// <summary>Background opacity of the overlay window (0.2 = nearly transparent, 0.95 = nearly opaque).</summary>
    public double OverlayOpacity { get; set; } = 0.93;

    /// <summary>Font family name for the overlay text input field. Empty = Segoe UI default.</summary>
    public string OverlayFontFamily { get; set; } = "Segoe UI";

    /// <summary>Last saved X position. Null = use CenterScreen default.</summary>
    public double? Left { get; set; }
    /// <summary>Last saved Y position. Null = use CenterScreen default.</summary>
    public double? Top { get; set; }
}
