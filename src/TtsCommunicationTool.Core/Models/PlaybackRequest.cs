namespace TtsCommunicationTool.Core.Models;

public sealed class PlaybackRequest
{
    public required byte[] AudioData { get; init; }
    public int SampleRate { get; init; }
    public int Channels { get; init; } = 1;
    public int BitsPerSample { get; init; } = 16;
}
