---
id: FEAT-018
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-002, FEAT-007, FEAT-010]
---

# FEAT-018: Phrase CRUD and Playback

## Description

Implement the phrase management system: `IPhraseService` for CRUD operations, phrase list UI in settings (or dedicated panel), and phrase playback through the same TTS → audio pipeline as free text. Optionally support phrase hotkey assignment.

## Acceptance Criteria

- [ ] `IPhraseService` interface defined with Add, Edit, Delete, GetAll methods
- [ ] User can add a new phrase (name + text)
- [ ] User can edit an existing phrase
- [ ] User can delete a phrase
- [ ] User can trigger playback of a phrase from the UI
- [ ] Phrase playback uses the same TTS + audio routing path as overlay send
- [ ] Phrases persisted in config across restarts
- [ ] Optional: phrase hotkey assignment and trigger
- [ ] Phrase list accessible from settings and/or tray menu

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/IPhraseService.cs` | New |
| `src/TtsCommunicationTool.UI/ViewModels/PhraseListViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/ViewModels/PhraseEditorViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/Views/PhraseEditorDialog.xaml` | New |
| `src/TtsCommunicationTool.UI/Views/PhraseEditorDialog.xaml.cs` | New |
| `src/TtsCommunicationTool.Core/Validation/PhraseValidation.cs` | New |

## Implementation Notes

- Phrase list can be a section within settings or a separate panel/dialog
- Playback command reuses `ITtsService.GenerateAsync` + `IAudioRouterService.PlayToBothAsync`
- Keep phrase list flat (no folders/categories in MVP)

## Testing

- [ ] Add phrase → appears in list
- [ ] Edit phrase → changes reflected
- [ ] Delete phrase → removed from list
- [ ] Play phrase → audio plays on both outputs
- [ ] Phrases persist after restart

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
