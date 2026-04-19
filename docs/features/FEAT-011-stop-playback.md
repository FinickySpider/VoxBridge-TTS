---
id: FEAT-011
type: feature
status: complete
priority: high
phase: PHASE-01
sprint: SPRINT-02
owner: ""
depends_on: [FEAT-006, FEAT-010]
---

# FEAT-011: Stop Playback Hotkey

## Description

Wire the stop hotkey (default: Ctrl+Shift+Backspace) to immediately halt active audio playback on both output devices. No effect when idle. Resets app state from PlayingSpeech to Idle.

## Acceptance Criteria

- [ ] Stop hotkey registered globally via `IHotkeyService`
- [ ] Pressing stop hotkey during playback stops both outputs immediately
- [ ] Pressing stop hotkey when idle has no effect (no error)
- [ ] App state transitions from PlayingSpeech to Idle after stop
- [ ] Playback resources released on stop

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Infrastructure/Hotkeys/WindowsHotkeyService.cs` | Wire stop event |
| `src/TtsCommunicationTool.Core/State/PlaybackState.cs` | New |

## Implementation Notes

- Stop hotkey callback calls `IAudioRouterService.Stop()`
- Must be safe to call stop when nothing is playing

## Testing

- [ ] Stop during playback halts audio
- [ ] Stop when idle does nothing
- [ ] No crash on rapid repeated stop presses

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
