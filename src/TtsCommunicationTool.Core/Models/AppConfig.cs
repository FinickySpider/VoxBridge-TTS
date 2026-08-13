namespace TtsCommunicationTool.Core.Models;

public sealed class AppConfig
{
    public int ConfigVersion { get; set; } = 1;
    public GeneralSettings GeneralSettings { get; set; } = new();
    public HotkeySettings HotkeySettings { get; set; } = new();
    public AudioSettings AudioSettings { get; set; } = new();
    public VoiceSettings VoiceSettings { get; set; } = new();
    public OverlaySettings OverlaySettings { get; set; } = new();
    public List<PhraseItem> Phrases { get; set; } = new();
    public TextReplacementSettings TextReplacements { get; set; } = new();
    public ElevenLabsSettings ElevenLabs { get; set; } = new();
    public Sapi5Settings Sapi5 { get; set; } = new();
    public DiagnosticLoggingSettings DiagnosticLogging { get; set; } = new();

    /// <summary>Name of the last-applied theme.  Resolved by ThemeService at startup.</summary>
    public string ActiveThemeName { get; set; } = "Default Dark";
}
