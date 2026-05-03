using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
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

        // Show current assembly version in the title bar so the taskbar always reflects it.
        var ver = System.Reflection.Assembly.GetEntryAssembly()
            ?.GetName().Version;
        if (ver is not null)
            Title = $"Settings — VoxBridge v{ver.Major}.{ver.Minor}.{ver.Build}";
        // Save applies settings and re-takes the snapshot but does NOT close the window.
        // _committed prevents the Closing handler from triggering a rollback after a save.
        vm.Saved += (_, _) => _committed = true;
        vm.Phrases.ImportCompleted += OnImportCompleted;

        // Show a Yes/No confirmation before a phrase is deleted.
        vm.Phrases.ConfirmDelete = name =>
            MessageBox.Show(
                $"Delete the phrase \u201c{name}\u201d?\n\nThis cannot be undone.",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question,
                MessageBoxResult.No) == MessageBoxResult.Yes;

        // PasswordBox bridge: PasswordBox cannot bind to a VM property, so code-behind
        // pushes the value in and also listens for the VM's request to clear the box.
        vm.Voice.RequestPasswordBoxClear += (_, _) =>
        {
            if (ApiKeyPasswordBox is not null)
                ApiKeyPasswordBox.Clear();
        };

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

            // If closed without an explicit Save or Cancel (X button, Alt+F4, etc.)
            // and there are unsaved changes, prompt the user first.
            if (!_committed && (vm.IsDirty || vm.Phrases.HasSessionChanges))
            {
                var result = System.Windows.MessageBox.Show(
                    "You have unsaved changes. Discard them and close?",
                    "Unsaved Changes",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);
                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
                // User confirmed discard — roll back phrase changes then close
                _committed = true;
                _ = vm.Phrases.RollbackAsync();
            }

            // Always persist the current window size (fire-and-forget, non-blocking)
            _ = vm.SaveWindowDimensionsAsync();
        };
    }

    // ── ElevenLabs API key PasswordBox bridge ────────────────────────────────

    /// <summary>
    /// PasswordBox cannot data-bind its Password property, so we push the value
    /// into the ViewModel here instead.  The VM raises <see cref="VoiceSettingsViewModel.RequestPasswordBoxClear"/>
    /// when it needs the box cleared (Cancel / Save), handled in the constructor.
    /// </summary>
    private void ApiKeyPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
            vm.Voice.SetPendingApiKey(((PasswordBox)sender).Password);
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

    // ----- Override hotkey (overlay-only Stop+Send) -----

    private void OverrideHotkeyBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = true;
        var key = e.Key == Key.System ? e.SystemKey : e.Key;

        if (key == Key.Escape)
        {
            if (DataContext is SettingsViewModel vm) vm.Hotkeys.ClearOverrideHotkey();
            CancelCapture();
            Keyboard.ClearFocus();
            return;
        }

        var binding = CaptureHotkey(e);
        if (binding is null) return;

        if (DataContext is SettingsViewModel svm)
        {
            svm.Hotkeys.SetOverrideHotkey(binding);
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
            Ctrl  = mods.HasFlag(ModifierKeys.Control),
            Alt   = mods.HasFlag(ModifierKeys.Alt),
            Shift = mods.HasFlag(ModifierKeys.Shift),
            Win   = false, // Win key not supported — unreliable with RegisterHotKey
            Key   = key.ToString()
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Replacements DataGrid — drag-to-reorder
    // ─────────────────────────────────────────────────────────────────────────

    private TextReplacementRuleViewModel? _draggedRule;
    private Point _dragStartPoint;
    private DropLineAdorner? _dropAdorner;

    private void ReplacementsGrid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _dragStartPoint = e.GetPosition(null);
        var row = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
        _draggedRule = row?.DataContext as TextReplacementRuleViewModel;

        // Deselect when clicking an empty area (no row hit)
        if (row is null)
        {
            ReplacementsGrid.UnselectAll();
            ReplacementsGrid.CurrentCell = new DataGridCellInfo();
        }
    }

    private void ReplacementsGrid_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _draggedRule is null) return;

        var pos  = e.GetPosition(null);
        var diff = pos - _dragStartPoint;

        // Only start drag after the standard drag distance threshold
        if (Math.Abs(diff.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(diff.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        var data = new DataObject(typeof(TextReplacementRuleViewModel), _draggedRule);
        DragDrop.DoDragDrop((DataGrid)sender, data, DragDropEffects.Move);
    }

    private void ReplacementsGrid_CurrentCellChanged(object sender, EventArgs e)
    {
        // Single-click to begin editing on template columns (text fields).
        // Checkbox columns are excluded so ticking them stays a single click.
        if (ReplacementsGrid.CurrentCell.Column is DataGridTemplateColumn)
            ReplacementsGrid.BeginEdit();
    }

    private void ReplacementsGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
        // Auto-focus and select-all in the editing TextBox so typing replaces rather than appends.
        Dispatcher.BeginInvoke(() =>
        {
            var tb = FindVisualChild<TextBox>(e.EditingElement);
            if (tb is null) return;
            tb.Focus();
            tb.SelectAll();
        }, System.Windows.Threading.DispatcherPriority.Input);
    }

    private void ReplacementsGrid_DragOver(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(typeof(TextReplacementRuleViewModel)))
        {
            e.Effects = DragDropEffects.None;
            return;
        }
        e.Effects = DragDropEffects.Move;
        e.Handled = true;

        var grid = (DataGrid)sender;
        var layer = AdornerLayer.GetAdornerLayer(grid);
        if (layer is null) return;

        // Find target row to position the line
        var targetRow = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject);
        double yPos;
        if (targetRow is not null)
        {
            var pos = targetRow.TranslatePoint(new Point(0, targetRow.ActualHeight / 2), grid);
            var dropPos = e.GetPosition(grid);
            // Line above or below target row depending on which half the cursor is in
            yPos = dropPos.Y < pos.Y + targetRow.ActualHeight / 2
                ? targetRow.TranslatePoint(new Point(0, 0), grid).Y
                : targetRow.TranslatePoint(new Point(0, targetRow.ActualHeight), grid).Y;
        }
        else
        {
            yPos = e.GetPosition(grid).Y;
        }

        if (_dropAdorner is null)
        {
            _dropAdorner = new DropLineAdorner(grid);
            layer.Add(_dropAdorner);
        }
        _dropAdorner.UpdatePosition(yPos);
    }

    private void ReplacementsGrid_DragLeave(object sender, DragEventArgs e)
    {
        RemoveDropAdorner((UIElement)sender);
    }

    private void ReplacementsGrid_Drop(object sender, DragEventArgs e)
    {
        RemoveDropAdorner((UIElement)sender);

        if (!e.Data.GetDataPresent(typeof(TextReplacementRuleViewModel))) return;

        var dragged = (TextReplacementRuleViewModel)e.Data.GetData(typeof(TextReplacementRuleViewModel));
        var target  = FindVisualParent<DataGridRow>(e.OriginalSource as DependencyObject)?.DataContext
                      as TextReplacementRuleViewModel;

        if (target is not null && !ReferenceEquals(dragged, target) &&
            DataContext is SettingsViewModel svm)
        {
            var rules  = svm.TextReplacements.Rules;
            var srcIdx = rules.IndexOf(dragged);
            var tgtIdx = rules.IndexOf(target);
            if (srcIdx >= 0 && tgtIdx >= 0)
                rules.Move(srcIdx, tgtIdx);
        }

        _draggedRule = null;
    }

    private void RemoveDropAdorner(UIElement element)
    {
        if (_dropAdorner is null) return;
        var layer = AdornerLayer.GetAdornerLayer(element);
        layer?.Remove(_dropAdorner);
        _dropAdorner = null;
    }

    private static T? FindVisualParent<T>(DependencyObject? child) where T : DependencyObject
    {
        while (child is not null)
        {
            if (child is T match) return match;
            child = VisualTreeHelper.GetParent(child);
        }
        return null;
    }

    private static T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
    {
        if (parent is null) return null;
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            var child = VisualTreeHelper.GetChild(parent, i);
            if (child is T match) return match;
            var result = FindVisualChild<T>(child);
            if (result is not null) return result;
        }
        return null;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Phrases list — column-header sort
    // ─────────────────────────────────────────────────────────────────────────

    private string _phrasesSortColumn = string.Empty;
    private bool _phrasesSortAscending = true;

    private void PhraseSort_Click(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header) return;
        if (header.Role == GridViewColumnHeaderRole.Padding) return;
        if (PhraseListView.View is not GridView gv) return;

        // Column layout after swap: 0=📌Pin(skip), 1=★IsFavorite, 2=Name, 3=Category, 4=Hotkey(skip)
        var idx = gv.Columns.IndexOf(header.Column);
        var column = idx switch
        {
            1 => "IsFavorite",
            2 => "Name",
            3 => "Category",
            _ => string.Empty
        };
        if (string.IsNullOrEmpty(column)) return;

        if (_phrasesSortColumn == column)
            _phrasesSortAscending = !_phrasesSortAscending;
        else
        {
            _phrasesSortColumn = column;
            _phrasesSortAscending = true;
        }

        ApplyPhraseSort();
    }

    /// <summary>
    /// Applies the current sort to the phrases view, always with IsPinned Descending as
    /// the primary key so pinned items are locked to the top regardless of secondary sort.
    /// </summary>
    private void ApplyPhraseSort()
    {
        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(PhraseListView.ItemsSource);
        if (view is null) return;
        view.SortDescriptions.Clear();
        // Primary: pinned items always float to top.
        view.SortDescriptions.Add(new System.ComponentModel.SortDescription(
            "IsPinned", System.ComponentModel.ListSortDirection.Descending));
        // Secondary: user-chosen column (or default to Name ascending).
        if (!string.IsNullOrEmpty(_phrasesSortColumn))
        {
            try
            {
                view.SortDescriptions.Add(new System.ComponentModel.SortDescription(
                    _phrasesSortColumn,
                    _phrasesSortAscending
                        ? System.ComponentModel.ListSortDirection.Ascending
                        : System.ComponentModel.ListSortDirection.Descending));
            }
            catch
            {
                view.SortDescriptions.Clear();
            }
        }
    }

    private void UpdatePhraseSortIndicators()
    {
        // Sort indicators are shown by the column header text (↕ suffix).
    }

    private void PhraseListView_Loaded(object sender, RoutedEventArgs e)
    {
        // Set the initial sort: pinned first, then by Name ascending.
        _phrasesSortColumn = "Name";
        _phrasesSortAscending = true;
        ApplyPhraseSort();
    }

    private void PhraseListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListView lv) return;
        // Walk up from the clicked element; if no ListViewItem is found, clear selection.
        var hit = VisualTreeHelper.HitTest(lv, e.GetPosition(lv))?.VisualHit as DependencyObject;
        while (hit is not null && hit is not ListViewItem)
            hit = VisualTreeHelper.GetParent(hit);
        if (hit is null)
            lv.SelectedItem = null;
    }

    private void PhraseListView_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is not ListView lv) return;
        if (lv.View is not GridView gv) return;
        // Auto-fill the Name column (index 2) with the remaining horizontal space.
        // Fixed columns: ★(28) + 📌(28) + Category(120) + Hotkey(130) + scrollbar(18) + border(4)
        const double fixedTotal = 28 + 28 + 120 + 130 + 18 + 4;
        var available = lv.ActualWidth - fixedTotal;
        if (available > 80 && gv.Columns.Count > 2)
            gv.Columns[2].Width = Math.Max(80, available);
    }
}

/// <summary>
/// Draws a bright horizontal line at a given Y position over a DataGrid to indicate
/// where a dragged row will be dropped.
/// </summary>
internal sealed class DropLineAdorner : Adorner
{
    private double _yPosition;
    private static readonly Pen _pen = new(new SolidColorBrush(Color.FromRgb(0x89, 0xB4, 0xFA)), 2)
    {
        DashStyle = DashStyles.Solid
    };

    public DropLineAdorner(UIElement adornedElement) : base(adornedElement) { }

    public void UpdatePosition(double y)
    {
        _yPosition = y;
        InvalidateVisual();
    }

    protected override void OnRender(DrawingContext dc)
    {
        var width = AdornedElement is FrameworkElement fe ? fe.ActualWidth : 200;
        dc.DrawLine(_pen, new Point(0, _yPosition), new Point(width, _yPosition));
        // Draw small triangle at the start for visual clarity
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            ctx.BeginFigure(new Point(0, _yPosition - 5), true, true);
            ctx.LineTo(new Point(8, _yPosition), true, false);
            ctx.LineTo(new Point(0, _yPosition + 5), true, false);
        }
        dc.DrawGeometry(_pen.Brush, null, geo);
    }
}
