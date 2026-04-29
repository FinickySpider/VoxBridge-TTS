---
id: SPRINT-08
type: sprint
phase: PHASE-03
status: complete
---

# SPRINT-08: ElevenLabs Premium Voice

## Goal

Add an optional ElevenLabs cloud TTS path selectable in Voice settings, routed through the
same audio pipeline as Kokoro.

## Planned Work

| ID | Title | Status |
|----|-------|--------|
| FEAT-033 | Optional ElevenLabs voice path | complete |

## Notes

- New `ElevenLabsSettings` added to `AppConfig`: `ApiKey`, `SelectedVoiceId`, `ModelId`.
- `IElevenLabsTtsService` implements `ITtsService`; registered alongside Kokoro.
- A `TtsRouter` or factory selects the implementation based on `VoiceSettings.Engine`.
- Voice settings tab gains: engine selector (Kokoro / ElevenLabs), API key field, voice list refresh.
- All audio still routed through existing `IAudioRouterService` — no pipeline changes.
- API key stored in config (local only, no encryption for MVP).

## Definition of Done

- ElevenLabs API key input in settings
- Voice list populated from ElevenLabs API when key is valid
- Switching engine and saving routes subsequent sends to correct service
- Kokoro path unaffected by ElevenLabs being configured
- Graceful error if API key invalid or network unavailable
- Build clean, 0 errors
