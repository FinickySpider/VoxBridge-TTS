using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace TtsCommunicationTool.UI.Views;

public partial class ToastWindow : Window
{
    private readonly DispatcherTimer _timer;

    public enum ToastLevel { Info, Success, Warning, Error }

    public ToastWindow(string message, ToastLevel level = ToastLevel.Info, int durationMs = 4000)
    {
        InitializeComponent();

        MessageBlock.Text = message;

        // Per-level: icon, icon foreground, accent bar color, content background, optional title
        switch (level)
        {
            case ToastLevel.Info:
                IconBlock.Text = "\u2139";
                IconBlock.Foreground    = new SolidColorBrush(Color.FromRgb(0x89, 0xB4, 0xFA));
                AccentBar.Background    = new SolidColorBrush(Color.FromRgb(0x89, 0xB4, 0xFA));
                ContentPanel.Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x1C, 0x1E, 0x3A));
                break;

            case ToastLevel.Success:
                IconBlock.Text = "\u2714";
                IconBlock.Foreground    = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
                AccentBar.Background    = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
                ContentPanel.Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x10, 0x25, 0x18));
                TitleBlock.Text = "Success";
                TitleBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xA6, 0xE3, 0xA1));
                TitleBlock.Visibility = Visibility.Visible;
                break;

            case ToastLevel.Warning:
                IconBlock.Text = "\u26A0";
                IconBlock.Foreground    = new SolidColorBrush(Color.FromRgb(0xFA, 0xB3, 0x87));
                AccentBar.Background    = new SolidColorBrush(Color.FromRgb(0xFA, 0xB3, 0x87));
                ContentPanel.Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x2E, 0x1E, 0x0F));
                TitleBlock.Text = "Warning";
                TitleBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xB3, 0x87));
                TitleBlock.Visibility = Visibility.Visible;
                break;

            case ToastLevel.Error:
                IconBlock.Text = "\u2715";
                IconBlock.Foreground    = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
                AccentBar.Background    = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
                ContentPanel.Background = new SolidColorBrush(Color.FromArgb(0xF0, 0x2A, 0x0E, 0x17));
                TitleBlock.Text = "Error";
                TitleBlock.Foreground = new SolidColorBrush(Color.FromRgb(0xF3, 0x8B, 0xA8));
                TitleBlock.Visibility = Visibility.Visible;
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
