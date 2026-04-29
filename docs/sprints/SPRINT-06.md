---
id: SPRINT-06
type: sprint
phase: PHASE-03
status: complete
---

# SPRINT-06: Polish, Feedback, Repeat, Transcript

## Goal

Ship the "feel-good" layer: smooth animations on the overlay, clear success feedback, a
one-key resend of the last message, and a persistent transcript log of everything spoken.

## Planned Work

| ID | Title | Status |
|----|-------|--------|
| FEAT-028 | Visual polish and subtle animations | in_progress |
| FEAT-029 | Better send/success feedback | in_progress |
| FEAT-031 | Repeat last / resend | planned |
| FEAT-032 | Transcript logging | planned |

## Notes

- FEAT-028 and FEAT-029 are implemented together (animations live in same XAML storyboards as feedback).
- FEAT-031 leverages existing `RecentMessagesState`; the overlay adds a "↩ Resend" button.
- FEAT-032 writes to `%AppData%\TtsCommunicationTool\transcript.txt`; enabled by toggle in General settings.

## Definition of Done

- Build compiles with 0 errors
- Overlay visibly fades in on open
- Status shows green "✓ Sent" flash after send, then clears
- "↩ Resend" button re-queues last spoken text
- Every spoken text appended to transcript file (when enabled)
- Settings expose transcript enable toggle + "Open transcript" button
