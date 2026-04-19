---
id: FEAT-010
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-02
owner: ""
depends_on: [FEAT-007, FEAT-009]
---

# FEAT-010: Dual Audio Routing and Playback

## Description

Implement `IAudioRouterService` / `NAudioRouterService` to play TTS-generated audio simultaneously to both the monitor output device and the secondary output device. Normalize audio format before playback. Support stopping both outputs together.

## Acceptance Criteria

- [ ] `IAudioRouterService` interface defined with `PlayToBothAsync`, `PlayToMonitorAsync`, `PlayToSecondaryAsync`, `Stop` methods
- [ ] Same audio payload plays to both selected devices near-simultaneously
- [ ] Audio format normalized to consistent PCM format before playback
- [ ] Stop method halts both output streams and releases resources
- [ ] Playback failure on one device does not crash the other
- [ ] Missing device triggers graceful failure with user notification

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/IAudioRouterService.cs` | New |
| `src/TtsCommunicationTool.Core/Models/PlaybackRequest.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Audio/NAudioRouterService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Audio/AudioFormatNormalizer.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Audio/PlaybackSession.cs` | New |

## Implementation Notes

- Use two `WasapiOut` or `WaveOutEvent` instances, one per device
- Feed same `IWaveProvider` (or cloned buffer) to each
- Start both as close together as practical
- Resample if devices expect different sample rates

## Testing

- [ ] Audio plays on monitor device
- [ ] Audio plays on secondary device
- [ ] Both play simultaneously
- [ ] Stop halts both devices

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
