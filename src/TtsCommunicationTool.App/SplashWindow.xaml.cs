using System.Windows;
using System.Windows.Threading;

namespace TtsCommunicationTool.App;

public partial class SplashWindow : Window
{
    private readonly DispatcherTimer _timer;
    private bool _closed;

    public SplashWindow()
    {
        InitializeComponent();
        // Maximum display time — 3 seconds fallback
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(3)
        };
        _timer.Tick += (_, _) => CloseNow();
    }

    /// <summary>
    /// Show the splash and start the max-duration timer.
    /// </summary>
    public void ShowSplash()
    {
        Show();
        _timer.Start();
    }

    /// <summary>
    /// Close the splash immediately (called when app finishes loading,
    /// or after the 3-second timeout — whichever comes first).
    /// </summary>
    public void CloseNow()
    {
        if (_closed) return;
        _closed = true;
        _timer.Stop();
        Close();
    }
}
