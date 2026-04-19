---
id: FEAT-007
type: feature
status: complete
priority: high
phase: PHASE-01
sprint: SPRINT-02
owner: ""
depends_on: [FEAT-001, FEAT-003]
---

# FEAT-007: Kokoro TTS Integration

## Description

Integrate the Kokoro offline TTS engine. Implement `ITtsService` / `KokoroTtsService` that accepts text input, selects a voice, generates audio output, and returns a normalized audio stream or byte array usable by the audio router. Bundle at least one default voice.

## Acceptance Criteria

- [ ] `ITtsService` interface defined with `GenerateAsync(TtsRequest)` returning `TtsResult`
- [ ] `KokoroTtsService` calls Kokoro runtime to generate speech
- [ ] At least one default voice bundled and selectable
- [ ] Voice enumeration returns available voices
- [ ] Audio output is in a format consumable by NAudio (PCM WAV or compatible)
- [ ] Engine unavailable or generation failure returns clear error in `TtsResult`
- [ ] Generation completes in acceptable time for short/medium messages

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/ITtsService.cs` | New |
| `src/TtsCommunicationTool.Core/Models/TtsRequest.cs` | New |
| `src/TtsCommunicationTool.Core/Models/TtsResult.cs` | New |
| `src/TtsCommunicationTool.Core/Models/VoiceInfo.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Tts/KokoroTtsService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Tts/KokoroProcessRunner.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Tts/KokoroVoiceCatalog.cs` | New |

## Implementation Notes

- Kokoro integration method depends on runtime (Python subprocess, ONNX, or native binding)
- Early spike recommended to validate latency and packaging (see OQ-01)
- Normalize output to consistent sample rate for audio router

## Testing

- [ ] Valid text produces playable audio bytes
- [ ] Empty/invalid text returns error result
- [ ] Voice list returns at least one entry

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
