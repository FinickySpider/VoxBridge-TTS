---
id: FEAT-026
type: feature
status: complete
priority: medium
phase: PHASE-02
sprint: SPRINT-05
owner: ""
depends_on: [FEAT-018]
---

# FEAT-026: Phrase Import/Export

## Description
Allow the user to export their phrase list to a JSON file and import phrases from a JSON file. Useful for backup, sharing, and migrating between machines.

## Acceptance Criteria
- [ ] "Export Phrases" button in Phrases settings tab opens a Save dialog and writes a JSON file
- [ ] "Import Phrases" button opens an Open dialog, reads the JSON file, and merges phrases into the existing list (duplicate names get a suffix)
- [ ] Exported JSON is human-readable and matches PhraseItem schema (minus internal IDs)
- [ ] Import validates the file format and shows a user-friendly error if invalid
- [ ] Export includes: name, text, hotkey (if set), sortOrder
- [ ] After import, phrase list is refreshed in the UI

## Files Touched
| File | Change |
|------|--------|
| `src/.../Core/Interfaces/IPhraseService.cs` | Add `ImportPhrases`, `ExportPhrases` |
| `src/.../Infrastructure/Phrases/PhraseService.cs` | Implement import/export with JSON |
| `src/.../UI/ViewModels/PhraseListViewModel.cs` | Add ImportCommand, ExportCommand |
| `src/.../Views/SettingsWindow.xaml` | Add Import/Export buttons in Phrases tab |

## Implementation Notes
- Use `System.Text.Json` for serialization
- Export format: `{ "version": 1, "phrases": [ { "name": "", "text": "", "hotkey": null } ] }`
- Import: generate new IDs, assign next sortOrder
- Merge strategy: if name already exists, append " (imported)"

## Testing
- [ ] Export 5 phrases â†’ open JSON â†’ verify content
- [ ] Delete 2 phrases â†’ import same file â†’ 2 re-added with "(imported)" suffix
- [ ] Import malformed JSON â†’ friendly error, no crash

## Done When
- [ ] Acceptance criteria met
- [ ] Verified manually
