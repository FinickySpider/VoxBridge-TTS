# FEAT-037: Phrase Editor Window + Per-Phrase Voice Override

**Phase:** PHASE-04 (Advanced)  
**Sprint:** (out-of-sprint, delivered v0.14.0)  
**Status:** complete  
**Dependencies:** FEAT-018 (Phrase system), FEAT-033 (ElevenLabs voice), FEAT-034 (Global pitch)

---

## Summary

A full-screen modal `PhraseEditorWindow` for creating and editing a single phrase in one focused place, with per-phrase TTS engine/voice/pitch override and integrated audio preview.

---

## Acceptance Criteria

- [x] "✦ New Phrase" button in Settings → Phrases opens editor with blank fields
- [x] "✏ Edit" button / double-click / Enter / context-menu "Edit" opens editor for selected phrase
- [x] Editor has Name, Category (autocomplete ComboBox), IsFavorite, IsPinned fields
- [x] Editor has multi-line text area with character count
- [x] Per-phrase engine radio (Kokoro / ElevenLabs), voice radio (Default / Override), voice dropdown, pitch slider
- [x] Play Preview → plays monitor output only (no secondary)
- [x] Stop button halts playback
- [x] Regen Cache → synthesise with current settings and write phrase cache
- [x] Hotkey capture field (same UX as Settings hotkey boxes)
- [x] Save → adds or updates phrase in `IPhraseService`, regenerates cache, tracks session change
- [x] Cancel → no change, closes
- [x] Delete → confirm dialog, deletes phrase + cache, closes (only shown for existing phrases)
- [x] Global phrase/overlay hotkeys suppressed while editor is open (`IHotkeyHost.SuppressPhraseAndOverlayHotkeys`)
- [x] Session rollback correctly undoes new-phrase adds and cache modifications from the editor
- [x] Inline add panel in Settings → Phrases replaced with action button bar

---

## Key Files Changed

| File | Change |
|------|--------|
| `Core/Models/PhraseItem.cs` | +5 voice override fields |
| `Core/Models/TtsRequest.cs` | +`EngineOverride` field |
| `Core/Interfaces/IHotkeyHost.cs` | +`SuppressPhraseAndOverlayHotkeys` property |
| `Infrastructure/Tts/TtsRouter.cs` | +`Resolve(request)` engine override routing |
| `Infrastructure/Phrases/PhraseCacheService.cs` | Per-phrase voice override in cache generation |
| `App/HotkeyHostWindow.cs` | Suppression logic |
| `UI/ViewModels/PhraseEditorViewModel.cs` | New — full editor VM |
| `UI/ViewModels/PhraseListViewModel.cs` | +`EditorRequested`, `TrackSessionAdd/CacheModify`, `OnEditorCommit` |
| `UI/Views/PhraseEditorWindow.xaml` | New — editor UI |
| `UI/Views/PhraseEditorWindow.xaml.cs` | New — code-behind |
| `UI/Views/SettingsWindow.xaml` | Updated Phrases tab; Edit in context menu; double-click/Enter |
| `UI/Views/SettingsWindow.xaml.cs` | `IServiceProvider`, `OpenPhraseEditor`, double-click/KeyDown handlers |
| `App/ServiceRegistration.cs` | Register `PhraseEditorViewModel` as Transient |
| `App/App.xaml.cs` | Pass `_serviceProvider` to `SettingsWindow` |
