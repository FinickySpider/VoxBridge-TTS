using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface IHotkeyService : IDisposable
{
    event EventHandler<string>? HotkeyPressed;
    OperationResult Register(string id, HotkeyBinding binding);
    void Unregister(string id);
    void UnregisterAll();
}
