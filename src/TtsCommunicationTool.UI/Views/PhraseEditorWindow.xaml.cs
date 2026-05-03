using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.UI.ViewModels;

namespace TtsCommunicationTool.UI.Views;

public partial class PhraseEditorWindow : Window
{
    private TextBox? _capturingBox;

    private static readonly System.Windows.Media.SolidColorBrush _captureActiveBrush =
        new(System.Windows.Media.Color.FromRgb(0x1C, 0x3A, 0x6E));   // dark blue highlight while recording
    private static readonly System.Windows.Media.SolidColorBrush _captureNormalBrush =
        new(System.Windows.Media.Color.FromRgb(0x1E, 0x1E, 0x2E));   // matches EditorTextBox background

    public PhraseEditorWindow()
    {
        InitializeComponent();
    }

    public PhraseEditorWindow(PhraseEditorViewModel vm) : this()
    {
        DataContext = vm;

        // Inject delete confirmation that shows a WPF MessageBox
        vm.ConfirmDelete = name =>
            MessageBox.Show(
                $"Delete the phrase \u201c{name}\u201d?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.Yes;

        // ViewModel raises this when Save / Cancel / Delete completes
        vm.CloseRequested += (_, _) => Close();
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        if (DataContext is not PhraseEditorViewModel vm) return;

        vm.BeginEditing();

        // Restore persisted size (written to disk on next global settings save).
        Width  = vm.WindowWidth;
        Height = vm.WindowHeight;

        // Track when the window has fully loaded so we don't record pre-layout resize events.
        bool _sizeInit = false;
        Loaded += (_, _) => _sizeInit = true;

        SizeChanged += (_, _) =>
        {
            if (_sizeInit && DataContext is PhraseEditorViewModel svm && WindowState == WindowState.Normal)
            {
                svm.WindowWidth  = this.Width;
                svm.WindowHeight = this.Height;
            }
        };
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        if (DataContext is PhraseEditorViewModel vm)
            vm.EndEditing();
    }

    // ─── Hotkey capture ───────────────────────────────────────────────────────

    private void HotkeyBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        if (_capturingBox is not null && _capturingBox != tb)
            _capturingBox.Background = _captureNormalBrush;
        _capturingBox = tb;
        tb.Background = _captureActiveBrush;
    }

    private void HotkeyBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb) return;
        Dispatcher.BeginInvoke(() =>
        {
            if (_capturingBox == tb) CancelCapture();
        });
    }

    private void HotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is PhraseEditorViewModel vm)
                vm.ClearHotkeyCommand.Execute(null);
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is PhraseEditorViewModel svm)
            svm.SetHotkey(binding);

        CancelCapture();
        Keyboard.ClearFocus();
    }

    private void CancelCapture()
    {
        if (_capturingBox is null) return;
        _capturingBox.Background = _captureNormalBrush;
        _capturingBox = null;
    }

    private static HotkeyBinding? CaptureHotkey(KeyEventArgs e)
    {
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key is Key.LeftCtrl or Key.RightCtrl or Key.LeftAlt or Key.RightAlt
            or Key.LeftShift or Key.RightShift or Key.LWin or Key.RWin)
            return null;

        var mods = Keyboard.Modifiers;
        return new HotkeyBinding
        {
            Ctrl  = mods.HasFlag(ModifierKeys.Control),
            Alt   = mods.HasFlag(ModifierKeys.Alt),
            Shift = mods.HasFlag(ModifierKeys.Shift),
            Win   = false,
            Key   = key.ToString()
        };
    }

    // ─── Category ComboBox ─────────────────────────────────────────────────────

    /// <summary>
    /// Forces the typed category text into the ViewModel when the ComboBox loses focus.
    /// WPF editable ComboBox can silently revert the Text binding when SelectedItem changes
    /// (e.g. after selecting from dropdown then typing a new value). This handler guarantees
    /// the VM always reflects exactly what the user typed.
    /// </summary>
    private void CategoryComboBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is ComboBox cb && DataContext is PhraseEditorViewModel vm)
            vm.Category = cb.Text;
    }
}
