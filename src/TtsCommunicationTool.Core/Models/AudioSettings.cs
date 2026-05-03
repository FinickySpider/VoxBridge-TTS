namespace TtsCommunicationTool.Core.Models;

public sealed class AudioSettings
{
    public string MonitorOutputDeviceId { get; set; } = string.Empty;
    public string SecondaryOutputDeviceId { get; set; } = string.Empty;
    public string MonitorOutputDeviceNameCache { get; set; } = string.Empty;
    public string SecondaryOutputDeviceNameCache { get; set; } = string.Empty;
    public float MonitorVolume { get; set; } = 1.0f;
    public float SecondaryVolume { get; set; } = 1.0f;

    /// <summary>Automatically remove a portion of trailing silence from generated TTS audio.</summary>
    public bool TrimTrailingSilence { get; set; } = false;
    /// <summary>What percentage of trailing silence to retain (5–100). 100 = no trimming.</summary>
    public int SilenceRetentionPercent { get; set; } = 100;
}
