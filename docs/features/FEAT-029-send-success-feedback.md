---
id: FEAT-029
type: feature
phase: PHASE-03
sprint: SPRINT-06
status: in_progress
dependencies: [FEAT-028]
---

# FEAT-029: Better Send/Success Feedback

## Summary

After a successful send (text dispatched to TTS pipeline), briefly display a green "✓ Sent"
status message in the overlay status bar, then fade/clear it. Provides immediate confirmation
without blocking the user.

## Acceptance Criteria

- [ ] After `SendAsync` dispatches successfully, `StatusText` briefly shows "✓ Sent" in green
- [ ] The "✓ Sent" message clears after ~1.5 s (reverts to empty / idle)
- [ ] Color of status changes: idle=subtle grey, generating=blue, speaking=blue, sent=green, error=red
- [ ] `FireAndForgetSend` does not show "✓ Sent" (overlay already closed)

## Implementation Notes

- `OverlayViewModel`: after `_playbackState.IsPlaying = true` in `SendAsync`, set `StatusText = "✓ Sent"` with is-success flag, then use `Task.Delay(1500)` + clear on UI thread
- New `StatusSeverity` enum: `None`, `Info`, `Success`, `Error`
- `OverlayWindow.xaml`: bind status text color to severity via `IValueConverter`
