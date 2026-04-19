---
id: FEAT-009
type: feature
status: complete
priority: high
phase: PHASE-01
sprint: SPRINT-02
owner: ""
depends_on: [FEAT-001]
---

# FEAT-009: Audio Device Enumeration and Selection

## Description

Implement `IAudioDeviceService` / `NAudioDeviceService` to enumerate available audio output devices using NAudio. Display friendly device names. Persist selected device IDs in config. Warn if a saved device is no longer available.

## Acceptance Criteria

- [ ] `IAudioDeviceService` interface defined with `GetOutputDevices()` and `ValidateDevice(id)` methods
- [ ] `NAudioDeviceService` enumerates Windows audio output endpoints via NAudio
- [ ] Device list includes friendly display names
- [ ] Selected monitor and secondary device IDs persisted in config
- [ ] Cached display names stored alongside IDs for debugging
- [ ] Missing saved device triggers a warning (no crash)

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/IAudioDeviceService.cs` | New |
| `src/TtsCommunicationTool.Core/Models/AudioDeviceInfo.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Audio/NAudioDeviceService.cs` | New |

## Implementation Notes

- Use NAudio's `MMDeviceEnumerator` for WASAPI device listing
- Store both device ID and friendly name in audio settings

## Testing

- [ ] Device list returns at least one device on a machine with audio
- [ ] Missing device ID returns validation warning

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
