namespace TtsCommunicationTool.Core.Models;

/// <summary>
/// Controls the overall padding and margin density of the UI.
/// Maps to three sets of WPF Thickness resources applied by <c>ThemeService.Apply()</c>.
/// </summary>
public enum SpacingDensity
{
    /// <summary>Tighter padding — maximises visible content.</summary>
    Compact = 0,
    /// <summary>Balanced default — comfortable for daily use.</summary>
    Comfortable = 1,
    /// <summary>Extra whitespace — easier to target for low-precision input.</summary>
    Spacious = 2,
}
