namespace TtsCommunicationTool.Core.Utilities;

/// <summary>
/// Trims trailing silence from raw 16-bit PCM audio data using amplitude thresholding.
/// </summary>
public static class SilenceTrimmer
{
    /// <summary>
    /// Amplitude threshold below which a sample is considered silent.
    /// ~0.5 % of max signed 16-bit amplitude (32767).
    /// </summary>
    private const short SilenceThreshold = 164;

    /// <summary>
    /// Trims trailing silence from a 16-bit PCM byte array.
    /// Non-16-bit formats are returned unchanged.
    /// </summary>
    /// <param name="pcmData">Raw PCM byte array.</param>
    /// <param name="sampleRate">Samples per second (e.g., 24000).</param>
    /// <param name="channels">Number of channels (1 = mono, 2 = stereo).</param>
    /// <param name="bitsPerSample">Must be 16 for trimming to apply.</param>
    /// <param name="retentionFraction">
    /// Fraction of trailing silence to keep: 1.0 = keep all (no trim), 0.0 = remove all silence.
    /// Clamped to [0.0, 1.0].
    /// </param>
    /// <returns>Trimmed audio, or the original array if no silence is detected or format is unsupported.</returns>
    public static byte[] Trim(byte[] pcmData, int sampleRate, int channels, int bitsPerSample, float retentionFraction)
    {
        retentionFraction = Math.Clamp(retentionFraction, 0f, 1f);

        // Only support 16-bit PCM; return as-is for anything else
        if (bitsPerSample != 16 || pcmData.Length < 4)
            return pcmData;

        int bytesPerSample = 2; // 16-bit = 2 bytes
        int bytesPerFrame  = bytesPerSample * channels;
        int totalFrames    = pcmData.Length / bytesPerFrame;

        // Scan backwards from the end, looking for the last frame with a non-silent sample
        int lastSpeechFrame = 0;
        for (int i = totalFrames - 1; i >= 0; i--)
        {
            bool frameSilent = true;
            for (int ch = 0; ch < channels; ch++)
            {
                int offset = i * bytesPerFrame + ch * bytesPerSample;
                short sample = BitConverter.ToInt16(pcmData, offset);
                if (Math.Abs(sample) > SilenceThreshold)
                {
                    frameSilent = false;
                    break;
                }
            }
            if (!frameSilent)
            {
                lastSpeechFrame = i;
                break;
            }
        }

        int speechEndByte = (lastSpeechFrame + 1) * bytesPerFrame;
        int silenceBytes  = pcmData.Length - speechEndByte;

        if (silenceBytes <= 0)
            return pcmData; // No detectable silence — return as-is

        // Compute how many silence bytes to keep, aligned to a frame boundary
        int keptSilenceBytes = (int)(silenceBytes * retentionFraction);
        keptSilenceBytes = (keptSilenceBytes / bytesPerFrame) * bytesPerFrame;

        int trimmedLength = speechEndByte + keptSilenceBytes;
        if (trimmedLength >= pcmData.Length)
            return pcmData; // Nothing to trim

        var trimmed = new byte[trimmedLength];
        Buffer.BlockCopy(pcmData, 0, trimmed, 0, trimmedLength);
        return trimmed;
    }
}
