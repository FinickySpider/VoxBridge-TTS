---
id: PHASE-01
type: phase
status: active
owner: ""
---

# PHASE-01: Foundation & Core Loop (MVP)

## Goal

Deliver a daily-usable TTS communication tool with the complete core workflow: hotkey → overlay → type → speak → dual audio output. This phase covers the entire MVP scope including project scaffold, config system, tray shell, overlay input, global hotkeys, Kokoro TTS integration, dual audio routing, phrase system, settings UI, error handling, and first-run setup.

## In Scope

- Solution structure and project scaffold
- Config models, JSON persistence, schema versioning, corrupt-config recovery
- App shell bootstrap and tray icon/menu
- File-based diagnostic logging
- Overlay window (borderless, topmost, single-instance, auto-focus)
- Overlay input flow: Enter to send, Esc to cancel, click-outside to close
- Global hotkey registration (overlay + stop)
- Kokoro TTS integration with at least one bundled voice
- Text validation (empty rejection, max length)
- Dual audio routing via NAudio (monitor + secondary output)
- Stop playback hotkey
- Device enumeration and selection
- Audio test buttons (monitor, secondary, both)
- Settings window with all MVP sections (General, Hotkeys, Audio, Voice, Phrases, Appearance)
- Phrase CRUD and playback
- First-run setup flow
- Dark mode UI
- Error handling and user-facing error messages

## Out of Scope

- Multiple TTS engines
- Advanced phrase management (folders, search, categories)
- Transcript logging or message history
- Theme presets or visual customization beyond dark mode
- Per-output volume control
- Overlay position persistence/adjustment
- Import/export phrases

## Sprints

- [SPRINT-01](../sprints/SPRINT-01.md)
- [SPRINT-02](../sprints/SPRINT-02.md)
- [SPRINT-03](../sprints/SPRINT-03.md)

## Completion Criteria

- [ ] App launches and runs in system tray
- [ ] Overlay opens reliably from configured hotkey
- [ ] Overlay input is automatically focused
- [ ] User can type text and press Enter to speak
- [ ] Empty input is rejected safely
- [ ] Speech is generated using Kokoro
- [ ] Speech plays to both configured outputs simultaneously
- [ ] Stop hotkey interrupts playback
- [ ] User can choose output devices in settings
- [ ] User can test outputs in settings
- [ ] User can create, edit, delete, and play phrases
- [ ] Settings persist across restart
- [ ] App handles missing devices and hotkey conflicts visibly
- [ ] App is usable for real-world trial in Discord/VRChat workflow
