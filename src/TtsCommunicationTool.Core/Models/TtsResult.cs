namespace TtsCommunicationTool.Core.Models;

public sealed class TtsResult
{
    public bool Success { get; init; }
    public byte[]? AudioData { get; init; }
    public int SampleRate { get; init; }
    public int Channels { get; init; } = 1;
    public int BitsPerSample { get; init; } = 16;
    public string? ErrorMessage { get; init; }

    public static TtsResult Ok(byte[] audio, int sampleRate, int channels = 1, int bitsPerSample = 16) => new()
    {
        Success = true,
        AudioData = audio,
        SampleRate = sampleRate,
        Channels = channels,
        BitsPerSample = bitsPerSample
    };

    public static TtsResult Fail(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}
