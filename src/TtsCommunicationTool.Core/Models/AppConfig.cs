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
}
