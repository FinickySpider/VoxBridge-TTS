using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.UI.Views;

public partial class OverlayWindow : Window
{
    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    private bool _isClosing;

    /// <summary>Raised when the user clicks the gear button to open settings. The overlay will close.</summary>
    public event EventHandler? SettingsRequested;

    public OverlayWindow()
    {
        InitializeComponent();
        Activated += OverlayWindow_Activated;
        DataContextChanged += OverlayWindow_DataContextChanged;
    }

    private void OverlayWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is OverlayViewModel oldVm)
            oldVm.ShakeRequested -= TriggerShake;
        if (e.NewValue is OverlayViewModel newVm)
            newVm.ShakeRequested += TriggerShake;
    }

    private void OverlayWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (!SystemParameters.ClientAreaAnimation)
            return;

        Opacity = 0;
        BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1,
            new Duration(TimeSpan.FromMilliseconds(150)))
        {
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        });
    }

    /// <summary>
    /// Plays a short shake animation to indicate a blocked action.
    /// Animates window Left position to give a subtle horizontal jitter.
    /// Safe to call from any thread via Dispatcher.
    /// </summary>
    public void TriggerShake()
    {
        Dispatcher.BeginInvoke(() =>
        {
            var origin = Left;
            var sb = new Storyboard();
            var offsets = new double[] { 10, -10, 7, -7, 4, -4, 0 };
            var step = TimeSpan.FromMilliseconds(40);
            for (int i = 0; i < offsets.Length; i++)
            {
                var da = new DoubleAnimation
                {
                    To = origin + offsets[i],
                    Duration = step,
                    BeginTime = step * i
                };
                Storyboard.SetTarget(da, this);
                Storyboard.SetTargetProperty(da, new PropertyPath(Window.LeftProperty));
                sb.Children.Add(da);
            }
            sb.Begin();
        });
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
        if (DataContext is OverlayViewModel vm)
        {
            if (vm.FireAndForgetSend())
                SafeClose();
            // If blocked, FireAndForgetSend already triggered shake + status — don't close
        }
    }

    private void ResendButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is OverlayViewModel vm)
        {
            var last = vm.LastMessage;
            if (last is null) return;
            vm.InputText = last;
            if (vm.FireAndForgetSend())
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
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        switch (key)
        {
            case Key.Return when e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Shift):
                // Shift+Enter: let the TextBox insert a newline — don't mark handled
                break;

            case Key.Return:
                e.Handled = true;
                if (DataContext is OverlayViewModel vm)
                {
                    // Check override hotkey (e.g. Ctrl+Enter) — stops current audio then sends
                    if (vm.MatchesOverrideHotkey(key, e.KeyboardDevice.Modifiers))
                    {
                        if (vm.ForceStopAndSend())
                            SafeClose();
                        // If ForceStopAndSend returns false (no text), shake was already triggered
                    }
                    else if (!e.KeyboardDevice.Modifiers.HasFlag(ModifierKeys.Control))
                    {
                        // Plain Enter: send if allowed; shake if blocked
                        if (vm.FireAndForgetSend())
                            SafeClose();
                    }
                    // Ctrl+Enter not matching override: swallow without action
                }
                break;

            case Key.Escape:
                SafeClose();
                e.Handled = true;
                break;
        }
    }

    private void GearButton_Click(object sender, RoutedEventArgs e)
    {
        // Close the overlay first so it releases foreground ownership,
        // then open settings on the next dispatcher cycle — this prevents
        // the overlay's close from handing focus back to the background app
        // and minimising/hiding the settings window that just opened.
        SafeClose();
        Dispatcher.BeginInvoke(
            () => SettingsRequested?.Invoke(this, EventArgs.Empty),
            System.Windows.Threading.DispatcherPriority.Background);
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

