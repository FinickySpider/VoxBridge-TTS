using NAudio.CoreAudioApi;
using NAudio.Wave;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Audio;

public sealed class DualOutputAudioRouter : IAudioRouterService
{
    private readonly List<WasapiOut> _activePlayers = new();
    private readonly object _lock = new();
    private CancellationTokenSource? _playbackCts;
    private readonly object _ctsLock = new();
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

    public async Task PlayAsync(PlaybackRequest request, string? monitorDeviceId, string? secondaryDeviceId,
        float monitorVolume = 1.0f, float secondaryVolume = 1.0f,
        CancellationToken ct = default)
    {
        // Cancel any in-flight playback BEFORE creating a new CTS, so the old
        // PlayAsync (if still running) throws OperationCanceledException and does
        // NOT fire PlaybackFinished. Then stop/dispose the NAudio players.
        StopAll();

        // Create a fresh CTS for this playback session so StopAll() can cancel it
        // without touching the caller-supplied 'ct'.
        using var cts = ct.CanBeCanceled
            ? CancellationTokenSource.CreateLinkedTokenSource(ct)
            : new CancellationTokenSource();

        lock (_ctsLock) { _playbackCts = cts; }

        var token = cts.Token;
        var waveFormat = new WaveFormat(request.SampleRate, request.BitsPerSample, request.Channels);
        var tasks = new List<Task>();

        if (!string.IsNullOrEmpty(monitorDeviceId))
            tasks.Add(PlayOnDeviceAsync(request.AudioData, waveFormat, monitorDeviceId, Math.Clamp(monitorVolume, 0f, 1f), token));

        if (!string.IsNullOrEmpty(secondaryDeviceId))
            tasks.Add(PlayOnDeviceAsync(request.AudioData, waveFormat, secondaryDeviceId, Math.Clamp(secondaryVolume, 0f, 1f), token));

        if (tasks.Count == 0)
            tasks.Add(PlayOnDeviceAsync(request.AudioData, waveFormat, null, Math.Clamp(monitorVolume, 0f, 1f), token));

        try
        {
            await Task.WhenAll(tasks);

            // Only fire if playback completed naturally — not when StopAll() cancelled it.
            if (!cts.IsCancellationRequested)
                PlaybackFinished?.Invoke(this, EventArgs.Empty);
        }
        catch (OperationCanceledException)
        {
            // Stopped by StopAll() — suppress PlaybackFinished so callers don't
            // misinterpret a forced stop as natural completion.
        }
        finally
        {
            lock (_ctsLock)
            {
                if (_playbackCts == cts)
                    _playbackCts = null;
            }
        }
    }

    public void StopAll()
    {
        // Cancel the current playback CTS first so the in-flight PlayAsync throws
        // OperationCanceledException and skips PlaybackFinished.Invoke.
        CancellationTokenSource? cts;
        lock (_ctsLock)
        {
            cts = _playbackCts;
            _playbackCts = null;
        }
        cts?.Cancel();

        // Force-stop the NAudio players directly (handles the case where PlayAsync
        // hasn't called PlayOnDeviceAsync yet, or where the CTS cancel arrives late).
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

        cts?.Dispose();
    }

    private async Task PlayOnDeviceAsync(byte[] audioData, WaveFormat format, string? deviceId, float volume, CancellationToken ct)
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
            // Use SampleChannel for software volume control (per-stream gain).
            // WasapiOut.Volume changes the system endpoint master volume and is unreliable
            // in shared mode, so we apply gain in the audio pipeline instead.
            var sampleChannel = new NAudio.Wave.SampleProviders.SampleChannel(stream) { Volume = volume };
            player.Init(sampleChannel);

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
                // Guard against double-dispose: StopAll() may have already
                // disposed the player while we were awaiting tcs.Task.
                try { player.Dispose(); } catch { }
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
