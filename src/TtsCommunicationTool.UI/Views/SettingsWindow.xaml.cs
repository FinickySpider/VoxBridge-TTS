using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.UI.Views;

public partial class SettingsWindow : Window
{
    private bool _importDialogOpen;
    // Set to true after Save or an explicit Cancel so the Closing handler doesn't double-rollback.
    private bool _committed;

    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(SettingsViewModel vm) : this()
    {
        DataContext = vm;
        vm.Saved += (_, _) => { _committed = true; Close(); };
        vm.Phrases.ImportCompleted += OnImportCompleted;

        // Restore persisted window size
        Width = vm.General.SettingsWindowWidth;
        Height = vm.General.SettingsWindowHeight;

        // Persist window size changes immediately (before save)
        // Use this.Width/Height (includes chrome) rather than e.NewSize (client area only)
        bool _windowSizeInitialized = false;
        Loaded += (_, _) => _windowSizeInitialized = true;
        SizeChanged += (_, _) =>
        {
            if (_windowSizeInitialized && DataContext is SettingsViewModel svm && WindowState == WindowState.Normal)
            {
                svm.General.SettingsWindowWidth = this.Width;
                svm.General.SettingsWindowHeight = this.Height;
            }
        };
        // Cancel any active hotkey capture when the user switches tabs
        vm.PropertyChanged += (_, pe) =>
        {
            if (pe.PropertyName == nameof(SettingsViewModel.SelectedTabIndex))
                CancelCapture();
        };
        Closing += (_, e) =>
        {
            // Block close while phrase cache regeneration or import dialog is in progress
            if (vm.IsRegenerating || _importDialogOpen)
            {
                e.Cancel = true;
                return;
            }
            // If the window is being closed without an explicit Save or Cancel (e.g. Alt+F4 / X button),
            // roll back any phrase changes made this session. Fire-and-forget is intentional here:
            // the storage + cache cleanup finishes in the background after the window closes.
            if (!_committed && vm.Phrases.HasSessionChanges)
                _ = vm.Phrases.RollbackAsync();
            // Always persist the current window size (fire-and-forget, non-blocking)
            _ = vm.SaveWindowDimensionsAsync();
        };
    }

    private void OnImportCompleted(object? sender, EventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        var result = vm.Phrases.LastImportResult;
        if (result is null) return;

        _importDialogOpen = true;
        var dialog = new ImportProgressWindow(result, vm.Phrases.CacheImportedPhrasesAsync);
        dialog.Owner = this;
        dialog.Closed += (_, _) =>
        {
            _importDialogOpen = false;
            // Update status message after dialog closes
            vm.Phrases.PhraseStatusMessage =
                $"Imported {result.AddedCount} phrase(s).";
        };
        dialog.ShowDialog();
    }

    private async void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is not SettingsViewModel vm) return;
        if (vm.IsDirty || vm.Phrases.HasSessionChanges)
        {
            var result = System.Windows.MessageBox.Show(
                "You have unsaved changes. Discard them and close?",
                "Unsaved Changes",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
        }
        _committed = true; // prevent Closing from triggering a second rollback
        await vm.CancelAsync();
        Close();
    }

    // ----- Hotkey capture state -----

    private System.Windows.Controls.TextBox? _capturingBox;

    private static readonly System.Windows.Media.SolidColorBrush _captureActiveBrush =
        new(System.Windows.Media.Color.FromRgb(0x3B, 0x3B, 0x55));
    private static readonly System.Windows.Media.SolidColorBrush _captureNormalBrush =
        new(System.Windows.Media.Color.FromRgb(0x2B, 0x2B, 0x3D));

    /// <summary>
    /// Cancels the current capture: restores the box background, clears any pending
    /// validation message, and nulls the tracking reference.
    /// </summary>
    private void CancelCapture()
    {
        if (_capturingBox is null) return;
        _capturingBox.Background = _captureNormalBrush;
        _capturingBox = null;
        if (DataContext is SettingsViewModel vm)
            vm.Hotkeys.ValidationMessage = string.Empty;
    }

    private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox tb) return;
        // Un-highlight previous box if switching between hotkey fields
        if (_capturingBox is not null && _capturingBox != tb)
            _capturingBox.Background = _captureNormalBrush;
        _capturingBox = tb;
        tb.Background = _captureActiveBrush;
    }

    private void HotkeyBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.TextBox tb) return;
        // Defer: GotFocus of the next element fires after LostFocus in WPF, so by
        // the time this lambda runs _capturingBox is already updated to the new box
        // if focus just moved to another hotkey field.
        Dispatcher.BeginInvoke(() =>
        {
            if (_capturingBox == tb)
                CancelCapture();
        });
    }

    // ----- Overlay hotkey -----

    private void OverlayHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm) vm.Hotkeys.ClearOverlayHotkey();
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel svm)
        {
            svm.Hotkeys.SetOverlayHotkey(binding);
            if (string.IsNullOrEmpty(svm.Hotkeys.ValidationMessage))
            {
                CancelCapture();
                Keyboard.ClearFocus();
            }
        }
    }

    // ----- Stop hotkey -----

    private void StopHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm) vm.Hotkeys.ClearStopHotkey();
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel svm)
        {
            svm.Hotkeys.SetStopHotkey(binding);
            if (string.IsNullOrEmpty(svm.Hotkeys.ValidationMessage))
            {
                CancelCapture();
                Keyboard.ClearFocus();
            }
        }
    }

    // ----- Settings hotkey -----

    private void SettingsHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm) vm.Hotkeys.ClearSettingsHotkey();
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel svm)
        {
            svm.Hotkeys.SetSettingsHotkey(binding);
            if (string.IsNullOrEmpty(svm.Hotkeys.ValidationMessage))
            {
                CancelCapture();
                Keyboard.ClearFocus();
            }
        }
    }

    // ----- Resend hotkey -----

    private void ResendHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm) vm.Hotkeys.ClearResendHotkey();
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel svm)
        {
            svm.Hotkeys.SetResendHotkey(binding);
            if (string.IsNullOrEmpty(svm.Hotkeys.ValidationMessage))
            {
                CancelCapture();
                Keyboard.ClearFocus();
            }
        }
    }

    // ----- Phrase hotkey -----

    private void PhraseHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm) vm.Phrases.ClearHotkeyCommand.Execute(null);
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel svm)
        {
            svm.Phrases.SetSelectedPhraseHotkey(binding);
            CancelCapture();
            Keyboard.ClearFocus();
        }
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
