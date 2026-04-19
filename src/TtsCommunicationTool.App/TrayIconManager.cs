using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TtsCommunicationTool.Core.Interfaces;

namespace TtsCommunicationTool.App;

public sealed class TrayIconManager : IDisposable
{
    private readonly IOverlayCoordinator _overlay;
    private readonly ILoggingService _log;
    private NotifyIcon? _notifyIcon;

    /// <summary>Raised when the user clicks "Settings" in the tray menu.</summary>
    public event EventHandler? SettingsRequested;

    public TrayIconManager(IOverlayCoordinator overlay, ILoggingService log)
    {
        _overlay = overlay;
        _log = log;
    }

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "TTS Communication Tool",
            Icon = SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = CreateContextMenu()
        };

        _notifyIcon.DoubleClick += (_, _) => _overlay.ToggleOverlay();
        _log.Info("Tray icon created.");
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();

        var showOverlay = new ToolStripMenuItem("Show Overlay");
        showOverlay.Click += (_, _) => _overlay.ShowOverlay();
        menu.Items.Add(showOverlay);

        var settings = new ToolStripMenuItem("Settings");
        settings.Click += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        menu.Items.Add(settings);

        menu.Items.Add(new ToolStripSeparator());

        var exit = new ToolStripMenuItem("Exit");
        exit.Click += (_, _) =>
        {
            _log.Info("Exit requested from tray.");
            System.Windows.Application.Current.Shutdown();
        };
        menu.Items.Add(exit);

        return menu;
    }

    public void Dispose()
    {
        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }
    }
}
