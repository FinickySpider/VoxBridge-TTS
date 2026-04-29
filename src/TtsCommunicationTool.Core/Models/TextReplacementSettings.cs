namespace TtsCommunicationTool.Core.Models;

public sealed class TextReplacementSettings
{
    public bool IsEnabled { get; set; } = true;
    public List<TextReplacement> Rules { get; set; } = [];
}
