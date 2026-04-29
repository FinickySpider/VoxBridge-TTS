namespace TtsCommunicationTool.Core.Models;

public sealed class TextReplacement
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string TriggerText { get; set; } = string.Empty;
    public string ReplacementText { get; set; } = string.Empty;
    public bool IsEnabled { get; set; } = true;
    public bool IsCaseSensitive { get; set; } = false;
    public bool WholeWordOnly { get; set; } = false;
    public int SortOrder { get; set; } = 0;
}
