---
id: FEAT-033
type: feature
phase: PHASE-03
sprint: SPRINT-08
status: complete
dependencies: [FEAT-016]
---

# FEAT-033: Optional ElevenLabs Voice Path

## Summary

Add an optional ElevenLabs cloud TTS path that users can enable in Voice settings. When
active, speech generation is routed to ElevenLabs instead of Kokoro. The audio is passed
through the existing dual-output pipeline unchanged.

## Acceptance Criteria

- [x] `ElevenLabsSettings` added to `AppConfig`: `ApiKey`, `SelectedVoiceId`, `ModelId`
- [x] `VoiceSettings` gains `Engine` enum: `Kokoro` | `ElevenLabs`
- [x] `TtsRouter : ITtsService` registered; routes based on `Engine`
- [x] Voice settings tab: engine selector (RadioButtons), API key field, "Fetch Voices" button, voice list refresh from API
- [x] Voice list populated from ElevenLabs `/v1/voices` when API key is set
- [x] Switching engine + saving correctly routes next send to chosen service
- [x] Graceful error message if API key invalid or network unavailable
- [x] Kokoro path entirely unaffected when ElevenLabs is not selected

## Implementation Notes

- `ElevenLabsSettings` model in Core; `AppConfig` references it
- `ElevenLabsTtsService : ITtsService` in Infrastructure using `HttpClient`
- `ITtsServiceFactory` or simple conditional in `OverlayViewModel` selects implementation
- Voice settings tab: new UI section (hidden when Engine=Kokoro)
- Model: default `eleven_multilingual_v2`
- API base: `https://api.elevenlabs.io/v1/`
