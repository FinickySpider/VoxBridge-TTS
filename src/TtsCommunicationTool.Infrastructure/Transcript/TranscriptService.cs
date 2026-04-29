using System.Globalization;
using TtsCommunicationTool.Core.Interfaces;

namespace TtsCommunicationTool.Infrastructure.Transcript;

/// <summary>
/// Appends lines to %AppData%\TtsCommunicationTool\transcript.txt.
/// Creates the file and directory on first write.
/// </summary>
public sealed class TranscriptService : ITranscriptService
{
    private readonly IConfigService _config;
    private readonly ILoggingService _log;

    private static readonly string TranscriptPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool",
        "transcript.txt");

    public TranscriptService(IConfigService config, ILoggingService log)
    {
        _config = config;
        _log = log;
    }

    public async Task LogAsync(string text)
    {
        if (!_config.CurrentConfig.GeneralSettings.EnableTranscriptLogging)
            return;

        if (string.IsNullOrWhiteSpace(text))
            return;

        try
        {
            var dir = Path.GetDirectoryName(TranscriptPath)!;
            Directory.CreateDirectory(dir);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var line = $"[{timestamp}] {text.Trim()}{Environment.NewLine}";

            await File.AppendAllTextAsync(TranscriptPath, line);
        }
        catch (Exception ex)
        {
            _log.Error("Transcript write failed", ex);
        }
    }
}
