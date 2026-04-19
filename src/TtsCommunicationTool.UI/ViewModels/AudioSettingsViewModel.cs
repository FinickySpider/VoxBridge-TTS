using System.Collections.ObjectModel;
using System.Windows.Input;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.Commands;

namespace TtsCommunicationTool.UI.ViewModels;

public sealed class AudioSettingsViewModel : ViewModelBase
{
    private readonly IAudioDeviceService _audioDeviceService;
    private readonly IAudioRouterService _audioRouter;
    private readonly ITtsService _tts;
    private readonly ILoggingService _log;

    private string _selectedMonitorDeviceId = string.Empty;
    private string _selectedSecondaryDeviceId = string.Empty;
    private string _deviceWarning = string.Empty;

    public ObservableCollection<AudioDeviceInfo> OutputDevices { get; } = new();

    public string SelectedMonitorDeviceId
    {
        get => _selectedMonitorDeviceId;
        set => SetField(ref _selectedMonitorDeviceId, value);
    }

    public string SelectedSecondaryDeviceId
    {
        get => _selectedSecondaryDeviceId;
        set => SetField(ref _selectedSecondaryDeviceId, value);
    }

    public string DeviceWarning
    {
        get => _deviceWarning;
        set => SetField(ref _deviceWarning, value);
    }

    public ICommand RefreshDevicesCommand { get; }
    public ICommand TestMonitorCommand { get; }
    public ICommand TestSecondaryCommand { get; }
    public ICommand TestBothCommand { get; }

    public AudioSettingsViewModel(
        IAudioDeviceService audioDeviceService,
        IAudioRouterService audioRouter,
        ITtsService tts,
        ILoggingService log)
    {
        _audioDeviceService = audioDeviceService;
        _audioRouter = audioRouter;
        _tts = tts;
        _log = log;

        RefreshDevicesCommand = new RelayCommand(RefreshDevices);
        TestMonitorCommand = new AsyncRelayCommand(TestMonitorAsync);
        TestSecondaryCommand = new AsyncRelayCommand(TestSecondaryAsync);
        TestBothCommand = new AsyncRelayCommand(TestBothAsync);

        RefreshDevices();
    }

    private void RefreshDevices()
    {
        OutputDevices.Clear();
        foreach (var device in _audioDeviceService.GetOutputDevices())
            OutputDevices.Add(device);

        // Validate saved selections
        DeviceWarning = string.Empty;
        if (!string.IsNullOrEmpty(_selectedMonitorDeviceId) &&
            OutputDevices.All(d => d.Id != _selectedMonitorDeviceId))
            DeviceWarning = "Saved monitor device no longer available.";

        if (!string.IsNullOrEmpty(_selectedSecondaryDeviceId) &&
            OutputDevices.All(d => d.Id != _selectedSecondaryDeviceId))
            DeviceWarning += (DeviceWarning.Length > 0 ? " " : "") + "Saved secondary device no longer available.";
    }

    private async Task TestMonitorAsync()
    {
        var audio = await GenerateTestAudio();
        if (audio is null) return;
        await _audioRouter.PlayAsync(audio, _selectedMonitorDeviceId, null);
    }

    private async Task TestSecondaryAsync()
    {
        var audio = await GenerateTestAudio();
        if (audio is null) return;
        await _audioRouter.PlayAsync(audio, null, _selectedSecondaryDeviceId);
    }

    private async Task TestBothAsync()
    {
        var audio = await GenerateTestAudio();
        if (audio is null) return;
        await _audioRouter.PlayAsync(audio, _selectedMonitorDeviceId, _selectedSecondaryDeviceId);
    }

    private async Task<PlaybackRequest?> GenerateTestAudio()
    {
        var result = await _tts.SynthesizeAsync(new TtsRequest { Text = "This is a test." });
        if (!result.Success || result.AudioData is null)
        {
            _log.Warn($"Test audio generation failed: {result.ErrorMessage}");
            return null;
        }
        return new PlaybackRequest
        {
            AudioData = result.AudioData,
            SampleRate = result.SampleRate,
            Channels = result.Channels,
            BitsPerSample = result.BitsPerSample
        };
    }

    public void LoadFrom(AudioSettings s)
    {
        SelectedMonitorDeviceId = s.MonitorOutputDeviceId;
        SelectedSecondaryDeviceId = s.SecondaryOutputDeviceId;
        RefreshDevices();
    }

    public void ApplyTo(AudioSettings s)
    {
        s.MonitorOutputDeviceId = SelectedMonitorDeviceId;
        s.SecondaryOutputDeviceId = SelectedSecondaryDeviceId;

        // Cache friendly names
        var monitor = OutputDevices.FirstOrDefault(d => d.Id == SelectedMonitorDeviceId);
        var secondary = OutputDevices.FirstOrDefault(d => d.Id == SelectedSecondaryDeviceId);
        s.MonitorOutputDeviceNameCache = monitor?.FriendlyName ?? string.Empty;
        s.SecondaryOutputDeviceNameCache = secondary?.FriendlyName ?? string.Empty;
    }
}
