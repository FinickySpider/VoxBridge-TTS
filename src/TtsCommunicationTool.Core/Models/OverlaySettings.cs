namespace TtsCommunicationTool.Core.Models;

public sealed class OverlaySettings
{
    public double Width { get; set; } = 720;
    public double Height { get; set; } = 160;
    public double FontSize { get; set; } = 18;

    /// <summary>Last saved X position. Null = use CenterScreen default.</summary>
    public double? Left { get; set; }
    /// <summary>Last saved Y position. Null = use CenterScreen default.</summary>
    public double? Top { get; set; }
}
