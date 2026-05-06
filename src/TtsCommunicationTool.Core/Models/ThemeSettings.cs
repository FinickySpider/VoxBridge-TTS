using System.Text.Json.Serialization;

namespace TtsCommunicationTool.Core.Models;

/// <summary>
/// A flat, self-contained theme definition.  All colours are stored as #RRGGBB hex strings.
/// <see cref="IsBuiltIn"/> is set by the loader at runtime and is never serialised to user files.
/// </summary>
public sealed class ThemeSettings
{
    public string Name { get; set; } = "Default Dark";

    [JsonIgnore]
    public bool IsBuiltIn { get; set; }

    // ── Core Colours ─────────────────────────────────────────────────────────
    /// <summary>Main window / root background.  Resource key: WindowBackgroundBrush</summary>
    public string WindowBackground { get; set; } = "#1E1E2E";

    /// <summary>Card / panel background.  Resource key: PanelBackgroundBrush</summary>
    public string PanelBackground { get; set; } = "#2B2B3D";

    /// <summary>Deep panel (e.g. history strip).  Resource key: DeepPanelBackgroundBrush</summary>
    public string DeepPanelBackground { get; set; } = "#181825";

    /// <summary>Overlay history strip background.  Resource key: HistoryBackgroundBrush</summary>
    public string HistoryBackground { get; set; } = "#14141E";

    /// <summary>Surface elevation 0 (subtle rows, inner borders).  Resource key: Surface0Brush</summary>
    public string Surface0 { get; set; } = "#313244";

    /// <summary>Default border / separator colour.  Resource key: BorderBrush</summary>
    public string BorderColor { get; set; } = "#45475A";

    /// <summary>Surface elevation 2 (hover rows, selected rows).  Resource key: Surface2Brush</summary>
    public string Surface2 { get; set; } = "#585B70";

    /// <summary>Accent / highlight colour.  Resource key: AccentBrush</summary>
    public string Accent { get; set; } = "#89B4FA";

    /// <summary>Accent hover / light variant.  Resource key: AccentHoverBrush</summary>
    public string AccentHover { get; set; } = "#B4D0FB";

    // ── Text Colours ──────────────────────────────────────────────────────────
    /// <summary>Primary body text.  Resource key: PrimaryTextBrush</summary>
    public string PrimaryText { get; set; } = "#CDD6F4";

    /// <summary>Secondary / helper text.  Resource key: SecondaryTextBrush</summary>
    public string SecondaryText { get; set; } = "#A6ADC8";

    /// <summary>Muted text / subtext.  Resource key: MutedTextBrush</summary>
    public string MutedText { get; set; } = "#6C7086";

    /// <summary>Muted icon fill.  Resource key: MutedIconBrush</summary>
    public string MutedIcon { get; set; } = "#8B9CC8";

    // ── Status Colours ────────────────────────────────────────────────────────
    /// <summary>Informational / orange accent (e.g. device-config warnings).  Resource key: InfoBrush</summary>
    public string InfoColor { get; set; } = "#FAB387";

    /// <summary>Error / danger.  Resource key: ErrorBrush</summary>
    public string ErrorColor { get; set; } = "#F38BA8";

    /// <summary>Error hover variant.  Resource key: ErrorHoverBrush</summary>
    public string ErrorHover { get; set; } = "#F5A0B5";

    /// <summary>Warning / caution.  Resource key: WarningBrush</summary>
    public string Warning { get; set; } = "#F9E2AF";

    /// <summary>Success / positive.  Resource key: SuccessBrush</summary>
    public string Success { get; set; } = "#A6E3A1";

    /// <summary>Returns a shallow clone so edits don't mutate the source.</summary>
    public ThemeSettings Clone() => (ThemeSettings)MemberwiseClone();
}
