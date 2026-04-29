namespace TtsCommunicationTool.Core.Models;

public sealed class PhraseItem
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public HotkeyBinding? Hotkey { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedUtc { get; set; } = DateTime.UtcNow;

    // FEAT-030: organization
    public string? Category { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsPinned { get; set; }
}
