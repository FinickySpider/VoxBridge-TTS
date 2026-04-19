using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TtsCommunicationTool.Core.State;

public sealed class AppRuntimeState : INotifyPropertyChanged
{
    private bool _isTtsReady;
    private bool _isPlaying;
    private bool _isOverlayVisible;
    private string _statusMessage = "Ready";

    public bool IsTtsReady
    {
        get => _isTtsReady;
        set => SetField(ref _isTtsReady, value);
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        set => SetField(ref _isPlaying, value);
    }

    public bool IsOverlayVisible
    {
        get => _isOverlayVisible;
        set => SetField(ref _isOverlayVisible, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetField(ref _statusMessage, value);
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
