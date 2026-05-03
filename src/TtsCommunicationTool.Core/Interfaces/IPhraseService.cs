using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface IPhraseService
{
    IReadOnlyList<PhraseItem> GetAll();
    PhraseItem? GetById(string id);
    OperationResult Add(PhraseItem phrase);
    /// <param name="skipCacheRegen">Pass true when the caller manages its own cache regen
    /// after Update to prevent an internal fire-and-forget regen racing against the caller's.</param>
    OperationResult Update(PhraseItem phrase, bool skipCacheRegen = false);
    OperationResult Delete(string id);
    OperationResult Reorder(string id, int newSortOrder);

    /// <summary>Serialises all phrases to a human-readable JSON string.</summary>
    string ExportToJson();

    /// <summary>
    /// Parses a previously exported JSON string and merges phrases into the list.
    /// Duplicate names get a unique "(imported)" suffix.
    /// Conflicting hotkeys are automatically cleared.
    /// </summary>
    PhraseImportResult ImportFromJson(string json);
}
