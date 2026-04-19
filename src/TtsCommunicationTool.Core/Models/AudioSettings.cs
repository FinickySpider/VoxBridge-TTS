namespace TtsCommunicationTool.Core.Models;

public sealed class AudioSettings
{
    public string MonitorOutputDeviceId { get; set; } = string.Empty;
    public string SecondaryOutputDeviceId { get; set; } = string.Empty;
    public string MonitorOutputDeviceNameCache { get; set; } = string.Empty;
    public string SecondaryOutputDeviceNameCache { get; set; } = string.Empty;
    public float MonitorVolume { get; set; } = 1.0f;
    public float SecondaryVolume { get; set; } = 1.0f;
}
