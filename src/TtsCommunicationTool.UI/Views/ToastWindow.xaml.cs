using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace TtsCommunicationTool.UI.Views;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;

    public enum ToastLevel { Info, Warning, Error }

    public ToastWindow(string message, ToastLevel level = ToastLevel.Info, int durationMs = 4000)
    {
        InitializeComponent();

        MessageBlock.Text = message;

        switch (level)
        {
            case ToastLevel.Info:
                IconBlock.Text = "ℹ";
                IconBlock.Foreground = new SolidColorBrush(Color.FromRgb(0x89, 0xB4, 0xFA));
                break;
            case ToastLevel.Warning:
                IconBlock.Text = "⚠";
                IconBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xB3, 0x87));
                break;
            case ToastLevel.Error:
                IconBlock.Text = "✕";
                IconBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
                break;
        }

        Loaded += (_, _) => PositionBottomRight();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Close();
        };
        _timer.Start();

        MouseLeftButtonDown += (_, _) =>
        {
            _timer.Stop();
            Close();
        };
    }

    private void PositionBottomRight()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - ActualWidth - 8;
        Top = workArea.Bottom - ActualHeight - 8;
    }
}
