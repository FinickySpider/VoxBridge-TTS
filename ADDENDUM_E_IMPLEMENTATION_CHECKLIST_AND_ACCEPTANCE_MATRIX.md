# ADDENDUM E
## Implementation Checklist and Acceptance Matrix

This addendum converts the design into a practical execution checklist.

## Phase 1 Checklist

### Foundation
- [ ] Create solution and projects
- [ ] Add logging service
- [ ] Add config models
- [ ] Add JSON config load/save
- [ ] Add default config creation
- [ ] Add tray icon and menu

### Overlay
- [ ] Create overlay window
- [ ] Make overlay topmost and borderless
- [ ] Auto-focus text input on open
- [ ] Support Esc close
- [ ] Support outside-click close
- [ ] Prevent duplicate overlay windows

### Hotkeys
- [ ] Register overlay hotkey
- [ ] Register stop hotkey
- [ ] Surface hotkey registration failures
- [ ] Save/load hotkey bindings

### TTS
- [ ] Integrate Kokoro runtime
- [ ] Bundle at least one voice
- [ ] Validate text input
- [ ] Generate playable audio output
- [ ] Return useful error messages on failure

### Audio
- [ ] Enumerate output devices
- [ ] Select monitor device
- [ ] Select secondary device
- [ ] Play to monitor only
- [ ] Play to secondary only
- [ ] Play to both
- [ ] Stop both outputs
- [ ] Handle missing device failure gracefully

### Settings
- [ ] Build settings window
- [ ] Add General section
- [ ] Add Hotkeys section
- [ ] Add Audio section
- [ ] Add Voice section
- [ ] Add Phrases section
- [ ] Add Appearance section
- [ ] Persist changes

### Phrases
- [ ] Add phrase
- [ ] Edit phrase
- [ ] Delete phrase
- [ ] Play phrase
- [ ] Optionally assign phrase hotkeys

### Feedback and Stability
- [ ] Show sending state
- [ ] Show error state
- [ ] Preserve text on send failure
- [ ] Log unhandled exceptions
- [ ] Recover from invalid config

## Acceptance Matrix

| Requirement | Acceptance Condition |
|---|---|
| Tray app | App starts and remains available from system tray |
| Overlay hotkey | Pressing configured hotkey opens overlay |
| Input focus | Caret is active in text box immediately on open |
| Enter send | Pressing Enter starts TTS generation |
| Empty validation | Blank input does not send and shows no crash |
| Kokoro generation | Valid text produces playable speech |
| Dual output | Speech plays on both selected outputs |
| Stop hotkey | Playback stops quickly on stop hotkey press |
| Settings persistence | Changed settings remain after restart |
| Phrase CRUD | User can add, edit, delete, and play phrases |
| Error handling | Missing device or TTS failure shows clear feedback |
| First-run setup | User can test monitor, secondary, and both outputs |

## Manual Smoke Test Script

1. Launch app.
2. Verify tray icon appears.
3. Open settings.
4. Select monitor and secondary outputs.
5. Run monitor test.
6. Run secondary test.
7. Run both test.
8. Press overlay hotkey.
9. Type "Hello".
10. Press Enter.
11. Confirm local playback and virtual cable playback.
12. Trigger stop hotkey during a longer message.
13. Create a phrase.
14. Play phrase.
15. Restart app.
16. Confirm settings and phrases persist.

## Definition of Ready for User Trial
The app is ready for real-world use testing when:
- no startup crashes remain
- overlay open/send/close loop is stable
- dual audio routing works on target machine
- stop hotkey works consistently
- config persists
- obvious failure cases are user-visible
