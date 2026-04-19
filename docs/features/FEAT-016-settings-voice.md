---
id: FEAT-016
type: feature
status: complete
priority: medium
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-007, FEAT-013]
---

# FEAT-016: Settings Window — Voice Section

## Description

Implement the Voice section of the settings window. Show a dropdown of available voices from the TTS engine, display the engine name (Kokoro), and provide an optional Test Voice button that generates and plays a sample phrase.

## Acceptance Criteria

- [ ] Voice section displayed in settings window
- [ ] Voice dropdown populated from TTS service voice enumeration
- [ ] Engine name label shows "Kokoro"
- [ ] Selected voice persisted in config
- [ ] Test Voice button generates and plays a sample phrase
- [ ] Missing engine or voice shows clear status message

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/ViewModels/VoiceSettingsViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/Views/SettingsWindow.xaml` | Add Voice section |

## Testing

- [ ] Voice list populated
- [ ] Voice selection persisted
- [ ] Test button plays audio

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
