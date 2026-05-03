# FEAT-034 — Global Pitch Control

**Phase:** Post-PHASE-03
**Sprint:** [SPRINT-09](../sprints/SPRINT-09.md)
**Status:** complete

---

## Summary

Allow the user to adjust the pitch of all real-time TTS output via a slider in Settings → Voice. Changes preview immediately (before saving). Cancelling settings restores the last saved pitch.

---

## Acceptance Criteria

- [x] `VoiceSettings.GlobalPitch` property (float, default 1.0, range 0.5–2.0) persisted in config
- [x] `TtsRequest.Pitch` property passed through to TTS service implementations
- [x] Pitch applied in `KokoroTtsService` via sample rate multiplier
- [x] Pitch applied in `ElevenLabsTtsService` via sample rate multiplier
- [x] `VoiceSettingsViewModel.GlobalPitch` writes live to config on change (no save required to preview)
- [x] `VoiceSettingsViewModel.GlobalPitchPercent` displays "100%" etc.
- [x] Pitch slider added to Voice tab in Settings (range 0.5–2.0, small step 0.05)
- [x] `SettingsViewModel` snapshots pitch on load; `CancelAsync` restores snapshot
- [x] Resend hotkey passes current pitch
- [x] Phrase playback unaffected (always uses Pitch = 1.0)
- [x] Build succeeds: 0 errors, 0 warnings
