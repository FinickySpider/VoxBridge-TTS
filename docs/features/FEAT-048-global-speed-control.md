---
id: FEAT-048
type: feature
status: complete
priority: medium
phase: PHASE-06
sprint: SPRINT-14
owner: ""
depends_on: [FEAT-034]
---

# FEAT-048: Global Standard TTS Speed Control

## Description
Add a Voice settings slider for the playback speed of standard floating-overlay TTS. Phrase playback remains individually configured and is not affected.

## Acceptance Criteria
- [x] `VoiceSettings.GlobalSpeed` is persisted with a default of 1.0 and range 0.5–2.0
- [x] Standard overlay, resend, and voice-test requests pass the configured speed
- [x] Kokoro applies speed to generated PCM playback without changing phrase requests
- [x] Voice settings provide a live speed slider and percentage display
- [x] Cancel restores the previously saved speed
- [x] Build succeeds with 0 errors and 0 warnings

## Files Touched
- Core TTS and voice models
- Kokoro/ElevenLabs output services
- Overlay, app hotkey, and Voice settings view models
- Settings Voice tab
