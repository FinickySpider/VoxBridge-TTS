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
}
