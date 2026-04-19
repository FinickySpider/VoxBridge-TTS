using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface IAudioDeviceService
{
    IReadOnlyList<AudioDeviceInfo> GetOutputDevices();
    event EventHandler? DevicesChanged;
}
