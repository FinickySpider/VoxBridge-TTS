---
id: FEAT-025
type: feature
status: complete
priority: medium
phase: PHASE-02
sprint: SPRINT-05
owner: ""
depends_on: [FEAT-012, FEAT-018]
---

# FEAT-025: Recent Phrases/Messages

## Description
Keep a rolling history of the last N messages sent via the overlay (typed free-text and phrase hotkeys). Display them in a "Recent" section accessible from the overlay or tray menu. Clicking a recent entry pre-fills the overlay input or replays the phrase.

## Acceptance Criteria
- [ ] Last 20 sent messages are stored in memory (not persisted to disk)
- [ ] Recent list is accessible via a tray menu item "Recent Messages"
- [ ] Clicking a recent entry opens the overlay with that text pre-filled
- [ ] Phrase hotkey playbacks are also tracked in the recent list
- [ ] Recent list is cleared on app restart (in-memory only for MVP)
- [ ] List is displayed newest-first

## Files Touched
| File | Change |
|------|--------|
| `src/.../Core/State/RecentMessagesState.cs` | New â€” in-memory ring buffer, max 20 |
| `src/.../App/HotkeyHostWindow.cs` | Record phrase plays to RecentMessagesState |
| `src/.../UI/ViewModels/OverlayViewModel.cs` | Record typed sends to RecentMessagesState |
| `src/.../App/TrayIconManager.cs` | Add "Recent Messages" submenu |
| `src/.../UI/Views/RecentMessagesWindow.xaml` (optional) | Or inline in tray submenu |

## Implementation Notes
- Ring buffer: `Queue<string>` capped at 20
- Tray submenu items are dynamic â€” rebuild on each open
- Pre-fill: open overlay with text already populated

## Testing
- [ ] Send 5 messages, open recent â€” all 5 shown newest first
- [ ] Click recent entry â€” overlay opens with text pre-filled
- [ ] More than 20 â€” oldest drops off

## Done When
- [ ] Acceptance criteria met
- [ ] Verified manually
