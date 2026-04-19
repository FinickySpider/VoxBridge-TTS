namespace TtsCommunicationTool.Core.Models;

public sealed class AudioDeviceInfo
{
    public string Id { get; set; } = string.Empty;
    public string FriendlyName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
