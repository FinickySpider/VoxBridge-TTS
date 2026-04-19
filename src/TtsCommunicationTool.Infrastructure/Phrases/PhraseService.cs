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
}
