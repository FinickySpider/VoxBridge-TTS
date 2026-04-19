using System.Windows;
using System.Windows.Input;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.UI.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel vm) : this()
    {
        DataContext = vm;
        vm.Saved += (_, _) => Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

    // ----- Hotkey capture logic -----

    private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb)
            tb.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x3B, 0x3B, 0x55));
    }

    private void HotkeyBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.TextBox tb)
            tb.Background = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromRgb(0x2B, 0x2B, 0x3D));
    }

    private void OverlayHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel vm)
            vm.Hotkeys.SetOverlayHotkey(binding);
    }

    private void StopHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel vm)
            vm.Hotkeys.SetStopHotkey(binding);
    }

    /// <summary>
    /// Captures the currently pressed modifier + key combination and returns a HotkeyBinding.
    /// Returns null if only modifiers are pressed (no actual key yet).
    /// </summary>
    private static HotkeyBinding? CaptureHotkey(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        // Ignore pure modifier keys — wait for an actual key
        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
            return null;

        var mods = Keyboard.Modifiers;
        return new HotkeyBinding
        {
            Ctrl = mods.HasFlag(ModifierKeys.Control),
            Alt = mods.HasFlag(ModifierKeys.Alt),
            Shift = mods.HasFlag(ModifierKeys.Shift),
            Win = mods.HasFlag(ModifierKeys.Windows),
            Key = key.ToString()
        };
    }
}
