using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.UI.Views;

public partial class OverlayWindow : Window
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private bool _isClosing;

    public OverlayWindow()
    {
        InitializeComponent();
        Activated += OverlayWindow_Activated;
    }

    /// <summary>
    /// Force the overlay to the foreground and give keyboard focus to the input box.
    /// Uses Win32 SetForegroundWindow to guarantee focus even when the app lost
    /// foreground rights (e.g. after alt-tab closed a previous overlay).
    /// </summary>
    public void FocusInput()
    {
        Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, () =>
        {
            // Win32 force-foreground — needed after the app loses foreground via alt-tab
            var hwnd = new WindowInteropHelper(this).Handle;
            if (hwnd != IntPtr.Zero)
                SetForegroundWindow(hwnd);

            Activate();
            InputBox.Focus();
            Keyboard.Focus(InputBox);
            InputBox.CaretIndex = InputBox.Text?.Length ?? 0;
        });
    }

    /// <summary>
    /// Apply font size from appearance settings.
    /// </summary>
    public void SetFontSize(double fontSize)
    {
        if (fontSize > 0)
            InputBox.FontSize = fontSize;
    }

    private void OverlayWindow_Activated(object? sender, EventArgs e)
    {
        // Always grab keyboard focus when the overlay gets activated
        FocusInput();
    }

    private void OverlayWindow_Deactivated(object? sender, EventArgs e)
    {
        // ALWAYS close overlay on any focus loss (alt-tab, click away, etc.)
        // This ensures the next hotkey open always starts with a fresh, focused window.
        SafeClose();
    }

    private void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is OverlayViewModel vm && vm.CanSend)
        {
            vm.FireAndForgetSend();
            SafeClose();
        }
    }

    /// <summary>
    /// PreviewKeyDown intercepts keys BEFORE the TextBox handles them.
    /// This is required because AcceptsReturn="True" causes the TextBox to
    /// swallow Enter in its own KeyDown, so Window.KeyDown never sees it.
    /// </summary>
    private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter when !e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift):
                if (DataContext is OverlayViewModel vm && vm.CanSend)
                {
                    vm.FireAndForgetSend();
                    SafeClose();
                }
                e.Handled = true;
                break;

            case Key.Enter:
                // Shift+Enter: let the TextBox insert a newline (don't mark handled)
                break;

            case Key.Escape:
                SafeClose();
                e.Handled = true;
                break;
        }
    }

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // Allow dragging from anywhere except the input textbox so typing isn't interrupted
        if (e.OriginalSource is not System.Windows.Controls.TextBox)
            DragMove();
    }

    /// <summary>
    /// Close the window only once — prevents the crash from Deactivated firing
    /// during an already-in-progress Close (e.g., from Send or Escape).
    /// </summary>
    private void SafeClose()
    {
        if (_isClosing) return;
        _isClosing = true;
        Close();
    }

    /// <summary>
    /// Set _isClosing as early as possible so that Deactivated (which fires
    /// mid-close) cannot start a second Close() call.
    /// </summary>
    protected override void OnClosing(CancelEventArgs e)
    {
        _isClosing = true;
        base.OnClosing(e);
    }
}
