using System.Text.Json;
using System.Text.Json.Serialization;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.Validation;

namespace TtsCommunicationTool.Infrastructure.Phrases;

public sealed class PhraseService : IPhraseService
{
    private readonly IConfigService _configService;
    private readonly ILoggingService _log;
    private IPhraseCacheService? _phraseCache;

    public PhraseService(IConfigService configService, ILoggingService log)
    {
        _configService = configService;
        _log = log;
    }

    /// <summary>
    /// Late-bind the cache service to avoid circular DI (cache depends on TTS which may not be ready).
    /// Called from App.xaml.cs after TTS initialization.
    /// </summary>
    public void SetCacheService(IPhraseCacheService phraseCache) => _phraseCache = phraseCache;

    public IReadOnlyList<PhraseItem> GetAll()
        => _configService.CurrentConfig.Phrases.OrderBy(p => p.SortOrder).ToList().AsReadOnly();

    public PhraseItem? GetById(string id)
        => _configService.CurrentConfig.Phrases.FirstOrDefault(p => p.Id == id);

    public OperationResult Add(PhraseItem phrase)
    {
        var (valid, error) = PhraseValidation.Validate(phrase);
        if (!valid) return OperationResult.Fail(error!);

        phrase.CreatedUtc = DateTime.UtcNow;
        phrase.UpdatedUtc = DateTime.UtcNow;
        if (string.IsNullOrEmpty(phrase.Id)) phrase.Id = Guid.NewGuid().ToString();

        _configService.CurrentConfig.Phrases.Add(phrase);
        _ = _configService.SaveAsync(_configService.CurrentConfig);
        _log.Info($"Added phrase '{phrase.Name}'");

        // Auto-generate cached audio
        if (_phraseCache is not null)
            _ = _phraseCache.GenerateCacheAsync(phrase);

        return OperationResult.Ok();
    }

    public OperationResult Update(PhraseItem phrase, bool skipCacheRegen = false)
    {
        var (valid, error) = PhraseValidation.Validate(phrase);
        if (!valid) return OperationResult.Fail(error!);

        var existing = GetById(phrase.Id);
        if (existing is null) return OperationResult.Fail("Phrase not found.");

        var textChanged = existing.Text != phrase.Text;

        // Copy ALL phrase fields — missing fields caused silent data loss on every phrase-editor save.
        existing.Name              = phrase.Name;
        existing.Text              = phrase.Text;
        existing.Category          = phrase.Category;
        existing.IsFavorite        = phrase.IsFavorite;
        existing.IsPinned          = phrase.IsPinned;
        existing.Hotkey            = phrase.Hotkey;
        existing.SortOrder         = phrase.SortOrder;
        existing.OverrideEngine    = phrase.OverrideEngine;
        existing.UseVoiceOverride  = phrase.UseVoiceOverride;
        existing.OverrideVoiceId   = phrase.OverrideVoiceId;
        existing.OverrideVoiceName = phrase.OverrideVoiceName;
        existing.OverridePitch     = phrase.OverridePitch;
        existing.UpdatedUtc        = DateTime.UtcNow;

        _ = _configService.SaveAsync(_configService.CurrentConfig);
        _log.Info($"Updated phrase '{phrase.Name}'");

        // Regenerate cache only when text changed AND the caller hasn't taken ownership of regen.
        // Pass skipCacheRegen=true from PhraseEditorViewModel to prevent a race with its own regen.
        if (textChanged && !skipCacheRegen && _phraseCache is not null)
            _ = _phraseCache.GenerateCacheAsync(existing);

        return OperationResult.Ok();
    }

    public OperationResult Delete(string id)
    {
        var existing = GetById(id);
        if (existing is null) return OperationResult.Fail("Phrase not found.");

        _configService.CurrentConfig.Phrases.Remove(existing);
        _ = _configService.SaveAsync(_configService.CurrentConfig);
        _log.Info($"Deleted phrase '{existing.Name}'");

        // Delete cached audio
        _phraseCache?.DeleteCache(id);

        return OperationResult.Ok();
    }

    public OperationResult Reorder(string id, int newSortOrder)
    {
        var existing = GetById(id);
        if (existing is null) return OperationResult.Fail("Phrase not found.");

        existing.SortOrder = newSortOrder;
        existing.UpdatedUtc = DateTime.UtcNow;
        _ = _configService.SaveAsync(_configService.CurrentConfig);
        return OperationResult.Ok();
    }

    public string ExportToJson()
    {
        var phrases = GetAll();
        var dto = new PhraseExportFile
        {
            Version = 1,
            Phrases = phrases.Select(p => new PhraseExportItem
            {
                Name = p.Name,
                Text = p.Text,
                Hotkey = p.Hotkey,
                SortOrder = p.SortOrder
            }).ToList()
        };
        return JsonSerializer.Serialize(dto, new JsonSerializerOptions { WriteIndented = true });
    }

    public PhraseImportResult ImportFromJson(string json)
    {
        PhraseExportFile? dto;
        try
        {
            dto = JsonSerializer.Deserialize<PhraseExportFile>(json);
        }
        catch (JsonException)
        {
            return PhraseImportResult.Fail("Invalid JSON file format.");
        }

        if (dto is null || dto.Phrases is null)
            return PhraseImportResult.Fail("File does not contain valid phrase data.");

        var existing = GetAll();
        var existingNames = existing
            .Select(p => p.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Track hotkeys used by existing AND already-imported-in-this-batch phrases
        var usedHotkeys = existing
            .Where(p => p.Hotkey is not null && !p.Hotkey.IsEmpty)
            .Select(p => p.Hotkey!)
            .ToHashSet();

        var nextSort = existing.Count > 0 ? existing.Max(p => p.SortOrder) + 1 : 0;
        var addedIds = new List<string>();
        int namesChanged = 0;
        int hotkeysCleared = 0;

        foreach (var item in dto.Phrases)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Text))
                continue;

            // Unique name: "Name", "Name (imported)", "Name (imported 2)", ...
            var candidate = item.Name;
            if (existingNames.Contains(candidate))
            {
                namesChanged++;
                candidate = item.Name + " (imported)";
                int n = 2;
                while (existingNames.Contains(candidate))
                    candidate = item.Name + $" (imported {n++})";
            }
            existingNames.Add(candidate);

            // Clear hotkey if it conflicts with any existing or already-imported phrase
            HotkeyBinding? hotkey = item.Hotkey;
            if (hotkey is not null && !hotkey.IsEmpty)
            {
                if (usedHotkeys.Contains(hotkey))
                {
                    hotkey = null;
                    hotkeysCleared++;
                }
                else
                {
                    usedHotkeys.Add(hotkey);
                }
            }

            var phrase = new PhraseItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = candidate,
                Text = item.Text,
                Hotkey = hotkey,
                SortOrder = nextSort++
            };

            var result = Add(phrase);
            if (result.Success)
                addedIds.Add(phrase.Id);
        }

        _log.Info($"Imported {addedIds.Count} phrases ({namesChanged} names changed, {hotkeysCleared} hotkeys cleared).");
        return new PhraseImportResult
        {
            Success = true,
            AddedCount = addedIds.Count,
            NamesChangedCount = namesChanged,
            HotkeysCleared = hotkeysCleared,
            AddedPhraseIds = addedIds
        };
    }

    // --- DTOs for import/export ---

    private sealed class PhraseExportFile
    {
        [JsonPropertyName("version")]
        public int Version { get; set; }

        [JsonPropertyName("phrases")]
        public List<PhraseExportItem>? Phrases { get; set; }
    }

    private sealed class PhraseExportItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("hotkey")]
        public HotkeyBinding? Hotkey { get; set; }

        [JsonPropertyName("sortOrder")]
        public int SortOrder { get; set; }
    }
}
