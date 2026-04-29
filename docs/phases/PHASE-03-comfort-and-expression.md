---
id: PHASE-03
type: phase
status: complete
owner: ""
---

# PHASE-03: Comfort & Expression

## Goal

Add visual polish, expanded phrase management, transcript logging, and an optional premium
voice path to make the app more comfortable and expressive for daily use without touching
the core send flow.

## In Scope (adjusted from original)

- Visual polish and subtle animations (FEAT-028)
- Better send/success feedback (FEAT-029)
- Phrase categories, folders, search, favorites, pinning (FEAT-030)
- Repeat last / resend (FEAT-031)
- Transcript logging (FEAT-032)
- Optional premium voice path — ElevenLabs (FEAT-033)

## Moved to PHASE-04 (user directive, 2026-04-29)

- Theme presets (dark variants) — deferred
- Speed and pitch controls — deferred

## Already Complete Before Phase 3

- Voice preset switching — complete in PHASE-02 (FEAT-016)
- Cached audio for quick phrases — complete in PHASE-01/02 (phrase cache service)

## Out of Scope

- Message queueing
- VRChat OSC integration
- Voice cloning
- Cloud sync or mobile companion

## Sprints

- [SPRINT-06](../sprints/SPRINT-06.md) — active (feedback + repeat + transcript + polish)
- [SPRINT-07](../sprints/SPRINT-07.md) — planned (phrase organization)
- [SPRINT-08](../sprints/SPRINT-08.md) — planned (ElevenLabs)

## Completion Criteria

- [ ] Overlay fade-in animation on open
- [ ] "✓ Sent" green flash after successful send
- [ ] Resend-last available via overlay button + optional hotkey
- [ ] Transcript log file written for every spoken message
- [ ] Transcript viewable from settings or tray
- [ ] Phrase categories functional with filter
- [ ] Phrase search filter working
- [ ] Phrase favorites and pinning functional
- [ ] ElevenLabs API key + voice selection in settings
- [ ] Voice toggle Kokoro ↔ ElevenLabs saves and routes correctly
