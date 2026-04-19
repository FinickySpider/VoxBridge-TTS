namespace TtsCommunicationTool.Core.Interfaces;

public interface ILoggingService
{
    void Debug(string message);
    void Info(string message);
    void Warn(string message);
    void Error(string message, Exception? ex = null);
    void Fatal(string message, Exception? ex = null);
}
