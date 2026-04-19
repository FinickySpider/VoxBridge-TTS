using NAudio.CoreAudioApi;
using NAudio.Wave;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Audio;

public sealed class DualOutputAudioRouter : IAudioRouterService
{
    private readonly List<WasapiOut> _activePlayers = new();
    private readonly object _lock = new();
    private bool _disposed;

    public bool IsPlaying
    {
        get
        {
            lock (_lock)
                return _activePlayers.Any(p => p.PlaybackState == PlaybackState.Playing);
        }
    }

    public event EventHandler? PlaybackFinished;

    public async Task PlayAsync(PlaybackRequest request, string? monitorDeviceId, string? secondaryDeviceId, CancellationToken ct = default)
    {
        StopAll();

        var waveFormat = new WaveFormat(request.SampleRate, request.BitsPerSample, request.Channels);
        var tasks = new List<Task>();

        if (!string.IsNullOrEmpty(monitorDeviceId))
        {
            tasks.Add(PlayOnDeviceAsync(request.AudioData, waveFormat, monitorDeviceId, ct));
        }

        if (!string.IsNullOrEmpty(secondaryDeviceId))
        {
            tasks.Add(PlayOnDeviceAsync(request.AudioData, waveFormat, secondaryDeviceId, ct));
        }

        if (tasks.Count == 0)
        {
            // Play on default device
            tasks.Add(PlayOnDeviceAsync(request.AudioData, waveFormat, null, ct));
        }

        await Task.WhenAll(tasks);
        PlaybackFinished?.Invoke(this, EventArgs.Empty);
    }

    public void StopAll()
    {
        lock (_lock)
        {
            foreach (var player in _activePlayers)
            {
                try
                {
                    player.Stop();
                    player.Dispose();
                }
                catch
                {
                    // Best-effort cleanup
                }
            }
            _activePlayers.Clear();
        }
    }

    private async Task PlayOnDeviceAsync(byte[] audioData, WaveFormat format, string? deviceId, CancellationToken ct)
    {
        var tcs = new TaskCompletionSource();
        WasapiOut? player = null;

        try
        {
            if (deviceId is not null)
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDevice(deviceId);
                player = new WasapiOut(device, AudioClientShareMode.Shared, true, 100);
            }
            else
            {
                player = new WasapiOut(AudioClientShareMode.Shared, 100);
            }

            var stream = new RawSourceWaveStream(new MemoryStream(audioData), format);
            player.Init(stream);

            lock (_lock)
                _activePlayers.Add(player);

            player.PlaybackStopped += (_, _) => tcs.TrySetResult();

            ct.Register(() =>
            {
                player.Stop();
                tcs.TrySetCanceled();
            });

            player.Play();
            await tcs.Task;
        }
        catch (OperationCanceledException)
        {
            // Expected on cancellation
        }
        finally
        {
            if (player is not null)
            {
                lock (_lock)
                    _activePlayers.Remove(player);
                player.Dispose();
            }
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        StopAll();
    }
}
