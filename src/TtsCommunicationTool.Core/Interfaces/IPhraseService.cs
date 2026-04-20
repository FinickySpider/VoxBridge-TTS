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
    /// Duplicate names get a " (imported)" suffix.
    /// Returns the number of phrases added, or an error message.
    /// </summary>
    OperationResult ImportFromJson(string json, out int addedCount);
}
