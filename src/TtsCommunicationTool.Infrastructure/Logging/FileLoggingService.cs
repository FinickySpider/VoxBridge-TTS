using Serilog;
using Serilog.Events;
using TtsCommunicationTool.Core.Interfaces;

namespace TtsCommunicationTool.Infrastructure.Logging;

public sealed class FileLoggingService : ILoggingService, IDisposable
{
    private readonly Serilog.Core.Logger _logger;

    public FileLoggingService()
    {
        var logDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TtsCommunicationTool", "logs");

        _logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                Path.Combine(logDir, "app.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .CreateLogger();
    }

    public void Debug(string message) => _logger.Debug(message);
    public void Info(string message) => _logger.Information(message);
    public void Warn(string message) => _logger.Warning(message);
    public void Error(string message, Exception? ex = null) => _logger.Error(ex, message);
    public void Fatal(string message, Exception? ex = null) => _logger.Fatal(ex, message);

    public void Dispose() => _logger.Dispose();
}
