using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.State;

namespace TtsCommunicationTool.App;

public sealed class TrayIconManager : IDisposable
{
    private readonly IOverlayCoordinator _overlay;
    private readonly ILoggingService _log;
    private readonly RecentMessagesState _recentMessages;
    private NotifyIcon? _notifyIcon;
    private ToolStripMenuItem? _recentMenuItem;

    /// <summary>Raised when the user clicks "Settings" in the tray menu.</summary>
    public event EventHandler? SettingsRequested;

    public TrayIconManager(IOverlayCoordinator overlay, ILoggingService log, RecentMessagesState recentMessages)
    {
        _overlay = overlay;
        _log = log;
        _recentMessages = recentMessages;
    }

    public void Initialize()
    {
        _notifyIcon = new NotifyIcon
        {
            Text = "TTS Swirlotl",
            Icon = LoadAppIcon(),
            Visible = true,
            ContextMenuStrip = CreateContextMenu()
        };

        _notifyIcon.DoubleClick += (_, _) => SettingsRequested?.Invoke(this, EventArgs.Empty);
        _log.Info("Tray icon created.");
    }

    private static Icon LoadAppIcon()
    {
        try
        {
            var uri = new Uri("pack://application:,,,/icon.ico", UriKind.Absolute);
            var stream = System.Windows.Application.GetResourceStream(uri)?.Stream;
            if (stream is not null)
                return new Icon(stream);
        }
        catch { /* fall through to default */ }
        return SystemIcons.Application;
    }

    private ContextMenuStrip CreateContextMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Opening += OnMenuOpening;

        var showOverlay = new ToolStripMenuItem("Show Overlay");
        showOverlay.Click += (_, _) => _overlay.ShowOverlay();
        menu.Items.Add(showOverlay);

        // Recent Messages submenu — rebuilt dynamically on each open
        _recentMenuItem = new ToolStripMenuItem("Recent Messages");
        menu.Items.Add(_recentMenuItem);

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

    private void OnMenuOpening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_recentMenuItem is null) return;

        _recentMenuItem.DropDownItems.Clear();
        var recents = _recentMessages.GetAll();

        if (recents.Count == 0)
        {
            var empty = new ToolStripMenuItem("(none)") { Enabled = false };
            _recentMenuItem.DropDownItems.Add(empty);
        }
        else
        {
            foreach (var text in recents)
            {
                var display = text.Length > 50 ? text[..47] + "…" : text;
                var item = new ToolStripMenuItem(display);
                var captured = text; // capture for closure
                item.Click += (_, _) => _overlay.ShowOverlayWithText(captured);
                _recentMenuItem.DropDownItems.Add(item);
            }
        }
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
