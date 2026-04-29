---
id: FEAT-031
type: feature
phase: PHASE-03
sprint: SPRINT-06
status: planned
dependencies: [FEAT-025]
---

# FEAT-031: Repeat Last / Resend

## Summary

Add a "↩ Resend" button to the overlay that re-queues and speaks the most recently spoken
message without re-typing it.

## Acceptance Criteria

- [ ] Overlay shows a "↩ Resend" button (or keyboard shortcut hint) when `RecentMessagesState` has at least one entry
- [ ] Pressing "↩ Resend" populates `InputText` with the last message and immediately sends
- [ ] Button is hidden/disabled when no recent messages exist
- [ ] Resend goes through the same validation + text replacement + TTS pipeline as a normal send

## Implementation Notes

- `OverlayViewModel`: add `ResendLastCommand`; checks `_recentMessages.Messages.FirstOrDefault()`
- Command: sets `InputText = lastMessage` then calls `FireAndForgetSend()` or `SendAsync()`
- `OverlayWindow.xaml`: small "↩" button or text link below/beside the input field, visible only when `HasRecentMessage`
