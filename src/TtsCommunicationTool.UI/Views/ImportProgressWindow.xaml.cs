using System.Windows;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.UI.Views;

public partial class ImportProgressWindow : Window
{
    private readonly Func<IProgress<(int current, int total)>, CancellationToken, Task> _cachingWork;
    private readonly int _totalToCache;

    public ImportProgressWindow(
        PhraseImportResult result,
        Func<IProgress<(int current, int total)>, CancellationToken, Task> cachingWork)
    {
        InitializeComponent();

        _cachingWork = cachingWork;
        _totalToCache = result.AddedPhraseIds.Count;

        // Populate stats
        AddedText.Text = result.AddedCount == 1
            ? "1 phrase imported"
            : $"{result.AddedCount} phrases imported";

        if (result.NamesChangedCount > 0)
        {
            NamesText.Text = result.NamesChangedCount == 1
                ? "1 name changed (duplicate)"
                : $"{result.NamesChangedCount} names changed (duplicates)";
            NamesText.Visibility = Visibility.Visible;
        }

        if (result.HotkeysCleared > 0)
        {
            HotkeysText.Text = result.HotkeysCleared == 1
                ? "1 hotkey cleared due to conflict"
                : $"{result.HotkeysCleared} hotkeys cleared due to conflicts";
            HotkeysText.Visibility = Visibility.Visible;
        }

        if (_totalToCache > 0)
        {
            CacheSeparator.Visibility = Visibility.Visible;
            CachingHeaderText.Text = "Caching audio for imported phrases...";
            CachingHeaderText.Visibility = Visibility.Visible;
            CacheProgressBar.Maximum = _totalToCache;
            ProgressGrid.Visibility = Visibility.Visible;
        }

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_totalToCache == 0)
        {
            OkButton.IsEnabled = true;
            return;
        }

        var progress = new Progress<(int current, int total)>(t =>
        {
            CacheProgressBar.Value = t.current;
            CacheStatusText.Text = $"{t.current}/{t.total}";
        });

        try
        {
            await _cachingWork(progress, default);
            CachingHeaderText.Text = "Caching complete.";
            CacheStatusText.Text = string.Empty;
        }
        catch (Exception)
        {
            CachingHeaderText.Text = "Caching failed — phrases will generate on first use.";
            CacheStatusText.Text = string.Empty;
        }

        OkButton.IsEnabled = true;
        OkButton.Focus();
    }

    private void OkButton_Click(object sender, RoutedEventArgs e) => Close();
}
