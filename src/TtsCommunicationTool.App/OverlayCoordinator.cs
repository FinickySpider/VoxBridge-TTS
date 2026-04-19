using System.Windows;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.State;
using TtsCommunicationTool.UI.Views;
using TtsCommunicationTool.UI.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TtsCommunicationTool.App;

public sealed class OverlayCoordinator : IOverlayCoordinator
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _log;
    private readonly AppRuntimeState _appState;
    private OverlayWindow? _overlayWindow;

    public OverlayCoordinator(IServiceProvider serviceProvider, ILoggingService log, AppRuntimeState appState)
    {
        _serviceProvider = serviceProvider;
        _log = log;
        _appState = appState;
    }

    public bool IsOverlayVisible => _overlayWindow?.IsVisible == true;

    public void ShowOverlay()
    {
        if (_overlayWindow is { IsVisible: true })
        {
            _overlayWindow.Activate();
            return;
        }

        var vm = _serviceProvider.GetRequiredService<OverlayViewModel>();
        _overlayWindow = new OverlayWindow { DataContext = vm };
        _overlayWindow.Closed += (_, _) =>
        {
            _overlayWindow = null;
            _appState.IsOverlayVisible = false;
        };
        _overlayWindow.Show();
        _overlayWindow.FocusInput();
        _appState.IsOverlayVisible = true;
        _log.Debug("Overlay shown.");
    }

    public void HideOverlay()
    {
        if (_overlayWindow is null) return;
        _overlayWindow.Close();
        _overlayWindow = null;
        _appState.IsOverlayVisible = false;
        _log.Debug("Overlay hidden.");
    }

    public void ToggleOverlay()
    {
        if (IsOverlayVisible)
            HideOverlay();
        else
            ShowOverlay();
    }
}
