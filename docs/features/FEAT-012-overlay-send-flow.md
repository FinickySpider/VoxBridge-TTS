---
id: FEAT-012
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-02
owner: ""
depends_on: [FEAT-005, FEAT-007, FEAT-008, FEAT-010]
---

# FEAT-012: Overlay Send Flow (End-to-End)

## Description

Wire the complete overlay send workflow: user presses Enter → text validated → TTS generates audio → audio routed to both outputs → overlay closes on success. On failure, overlay stays open with error message and user text preserved.

## Acceptance Criteria

- [ ] Enter key in overlay triggers send command
- [ ] Send button (optional) also triggers send command
- [ ] Text validated before generation (empty rejected)
- [ ] Overlay state transitions: Ready → Sending → (close on success) or Error
- [ ] Duplicate sends blocked during Sending state
- [ ] On success: text cleared, overlay closed, audio playing
- [ ] On failure: error message shown, text preserved, overlay stays open
- [ ] Status text updates to reflect current state (Ready, Sending, Error)

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/ViewModels/OverlayViewModel.cs` | Add send command logic |
| `src/TtsCommunicationTool.UI/Views/OverlayWindow.xaml` | Wire Enter key and send button |
| `src/TtsCommunicationTool.Core/State/OverlayState.cs` | New |
| `src/TtsCommunicationTool.Core/State/AppRuntimeState.cs` | New |

## Implementation Notes

- `OverlayViewModel.SendTextCommand` orchestrates: validate → generate → route → close
- Use async/await for non-blocking TTS generation
- Disable send button/enter during Sending to prevent duplicates

## Testing

- [ ] Type "Hello", press Enter → audio plays, overlay closes
- [ ] Type nothing, press Enter → validation error, overlay stays
- [ ] Force TTS failure → error shown, text preserved

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
