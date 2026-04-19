using NAudio.CoreAudioApi;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Audio;

public sealed class WasapiAudioDeviceService : IAudioDeviceService
{
    public event EventHandler? DevicesChanged;

    public IReadOnlyList<AudioDeviceInfo> GetOutputDevices()
    {
        var enumerator = new MMDeviceEnumerator();
        var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);

        return devices.Select(d => new AudioDeviceInfo
        {
            Id = d.ID,
            FriendlyName = d.FriendlyName,
            IsActive = true
        }).ToList().AsReadOnly();
    }

    public void RaiseDevicesChanged()
    {
        DevicesChanged?.Invoke(this, EventArgs.Empty);
    }
}
