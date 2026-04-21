using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface IPhraseService
{
    IReadOnlyList<PhraseItem> GetAll();
    PhraseItem? GetById(string id);
    OperationResult Add(PhraseItem phrase);
    OperationResult Update(PhraseItem phrase);
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
