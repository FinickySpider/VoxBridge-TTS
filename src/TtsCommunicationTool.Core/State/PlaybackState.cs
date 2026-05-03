using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TtsCommunicationTool.Core.State;

public sealed class PlaybackState : INotifyPropertyChanged
{
    private bool _isPlaying;
    private string? _currentText;
    private double _playbackDurationSeconds;
    private DateTime _playbackStartedUtc;

    /// <summary>Total audio duration in seconds, set when playback begins.</summary>
    public double PlaybackDurationSeconds
    {
        get => _playbackDurationSeconds;
        set => SetField(ref _playbackDurationSeconds, value);
    }

    /// <summary>UTC time when the current audio clip started playing.</summary>
    public DateTime PlaybackStartedUtc
    {
        get => _playbackStartedUtc;
        set => SetField(ref _playbackStartedUtc, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetField(ref _isPlaying, value);
    }

    public string? CurrentText
    {
        get => _currentText;
        set => SetField(ref _currentText, value);
    }

    public void Reset()
    {
        IsPlaying = false;
        CurrentText = null;
        PlaybackDurationSeconds = 0;
        PlaybackStartedUtc = default;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
