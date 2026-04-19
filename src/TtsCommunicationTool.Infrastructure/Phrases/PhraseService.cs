using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;
using TtsCommunicationTool.Core.Validation;

namespace TtsCommunicationTool.Infrastructure.Phrases;

public sealed class PhraseService : IPhraseService
{
    private readonly IConfigService _configService;
    private readonly ILoggingService _log;

    public PhraseService(IConfigService configService, ILoggingService log)
    {
        _configService = configService;
        _log = log;
    }

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
        return OperationResult.Ok();
    }

    public OperationResult Update(PhraseItem phrase)
    {
        var (valid, error) = PhraseValidation.Validate(phrase);
        if (!valid) return OperationResult.Fail(error!);

        var existing = GetById(phrase.Id);
        if (existing is null) return OperationResult.Fail("Phrase not found.");

        existing.Name = phrase.Name;
        existing.Text = phrase.Text;
        existing.Hotkey = phrase.Hotkey;
        existing.SortOrder = phrase.SortOrder;
        existing.UpdatedUtc = DateTime.UtcNow;

        _ = _configService.SaveAsync(_configService.CurrentConfig);
        _log.Info($"Updated phrase '{phrase.Name}'");
        return OperationResult.Ok();
    }

    public OperationResult Delete(string id)
    {
        var existing = GetById(id);
        if (existing is null) return OperationResult.Fail("Phrase not found.");

        _configService.CurrentConfig.Phrases.Remove(existing);
        _ = _configService.SaveAsync(_configService.CurrentConfig);
        _log.Info($"Deleted phrase '{existing.Name}'");
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
