using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TtsCommunicationTool.Core.State;

public sealed class OverlayState : INotifyPropertyChanged
{
    private string _currentText = string.Empty;
    private bool _isSending;

    public string CurrentText
    {
        get => _currentText;
        set => SetField(ref _currentText, value);
    }

    public bool IsSending
    {
        get => _isSending;
        set => SetField(ref _isSending, value);
    }

    public void Clear()
    {
        CurrentText = string.Empty;
        IsSending = false;
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
