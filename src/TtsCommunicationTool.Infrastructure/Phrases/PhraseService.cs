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

    public OperationResult Update(PhraseItem phrase)
    {
        var (valid, error) = PhraseValidation.Validate(phrase);
        if (!valid) return OperationResult.Fail(error!);

        var existing = GetById(phrase.Id);
        if (existing is null) return OperationResult.Fail("Phrase not found.");

        var textChanged = existing.Text != phrase.Text;

        existing.Name = phrase.Name;
        existing.Text = phrase.Text;
        existing.Hotkey = phrase.Hotkey;
        existing.SortOrder = phrase.SortOrder;
        existing.UpdatedUtc = DateTime.UtcNow;

        _ = _configService.SaveAsync(_configService.CurrentConfig);
        _log.Info($"Updated phrase '{phrase.Name}'");

        // Regenerate cache if text changed
        if (textChanged && _phraseCache is not null)
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

    public OperationResult ImportFromJson(string json, out int addedCount)
    {
        addedCount = 0;
        PhraseExportFile? dto;
        try
        {
            dto = JsonSerializer.Deserialize<PhraseExportFile>(json);
        }
        catch (JsonException)
        {
            return OperationResult.Fail("Invalid JSON file format.");
        }

        if (dto is null || dto.Phrases is null)
            return OperationResult.Fail("File does not contain valid phrase data.");

        var existingNames = GetAll().Select(p => p.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var nextSort = GetAll().Count > 0 ? GetAll().Max(p => p.SortOrder) + 1 : 0;

        foreach (var item in dto.Phrases)
        {
            if (string.IsNullOrWhiteSpace(item.Name) || string.IsNullOrWhiteSpace(item.Text))
                continue;

            var name = existingNames.Contains(item.Name) ? item.Name + " (imported)" : item.Name;
            existingNames.Add(name);

            var phrase = new PhraseItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Text = item.Text,
                Hotkey = item.Hotkey,
                SortOrder = nextSort++
            };

            var result = Add(phrase);
            if (result.Success) addedCount++;
        }

        _log.Info($"Imported {addedCount} phrases from JSON.");
        return OperationResult.Ok();
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
