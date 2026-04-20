namespace TtsCommunicationTool.Core.State;

/// <summary>
/// In-memory ring buffer of the last N messages sent via the overlay or phrase hotkeys.
/// Not persisted to disk — cleared on restart.
/// Thread-safe for concurrent reads and writes.
/// </summary>
public sealed class RecentMessagesState
{
    private const int MaxItems = 20;
    private readonly LinkedList<string> _items = new();
    private readonly object _lock = new();

    /// <summary>
    /// Add a message to the front of the recent list.
    /// Duplicate consecutive entries (same text) are collapsed.
    /// Oldest entry beyond MaxItems is dropped.
    /// </summary>
    public void Add(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        text = text.Trim();

        lock (_lock)
        {
            // Avoid duplicate consecutive entries
            if (_items.First?.Value == text) return;

            _items.AddFirst(text);
            while (_items.Count > MaxItems)
                _items.RemoveLast();
        }
    }

    /// <summary>Returns a snapshot of all recent messages, newest first.</summary>
    public IReadOnlyList<string> GetAll()
    {
        lock (_lock)
            return _items.ToList();
    }
}
