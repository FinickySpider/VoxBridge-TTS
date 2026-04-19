using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TtsCommunicationTool.Core.State;

public sealed class PlaybackState : INotifyPropertyChanged
{
    private bool _isPlaying;
    private string? _currentText;

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
