---
id: FEAT-050
type: feature
status: complete
priority: high
phase: PHASE-07
sprint: SPRINT-15
owner: ""
depends_on: [FEAT-048]
---

# FEAT-050: x86 SAPI5 Bridge

## Description
Provide SAPI5 voice discovery and synthesis through a self-contained x86 helper process so 32-bit-only voices, including the installed NeoSpeech/Pau voice, can be used by the main application.

## Acceptance Criteria
- [x] The bridge is built for win-x86 and does not require a separately installed x86 .NET runtime.
- [x] The bridge enumerates enabled SAPI5 voices and returns stable IDs and display names.
- [x] The bridge synthesizes selected voices to WAV and returns a structured response.
- [x] The main provider invokes the bridge, decodes WAV to PCM, and uses the existing audio router.
- [x] Cancellation terminates the helper process tree.
- [x] The Release build completes with zero errors and zero warnings.

## Files Touched
| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Sapi5Bridge/` | Self-contained x86 SAPI5 helper |
| `src/TtsCommunicationTool.Infrastructure/Tts/Sapi5TtsService.cs` | Bridge process client and WAV decoder |
| `src/TtsCommunicationTool.App/TtsCommunicationTool.App.csproj` | Build-time bridge publish/copy target |
| `TtsCommunicationTool.slnx` | Bridge project registration |

## Implementation Notes
- Communication uses one JSON request and one JSON response over redirected standard streams.
- Audio is base64-encoded only across the process boundary; playback remains centralized in the main process.
- The main application remains unchanged in architecture and the bridge is published as a single-file x86 executable.

## Testing
- [x] x86 bridge enumerated the 32-bit SAPI registry view.
- [x] `VW Paul` was synthesized successfully through the bridge.
- [x] Release build completed with zero errors and zero warnings.

## Done When
- [x] Acceptance criteria met
- [x] Verified manually
