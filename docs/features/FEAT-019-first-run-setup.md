---
id: FEAT-019
type: feature
status: complete
priority: medium
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-009, FEAT-010, FEAT-013, FEAT-015]
---

# FEAT-019: First-Run Setup Flow

## Description

Implement a first-run experience that guides the user through initial configuration: select monitor and secondary output devices, test each output, confirm overlay hotkey, and save settings. Triggered automatically when no config file exists.

## Acceptance Criteria

- [ ] First-run detected when config file is missing or `configVersion` is 0
- [ ] Settings window opens automatically on first run
- [ ] Audio section highlighted or presented first
- [ ] User guided to select monitor output device
- [ ] User guided to select secondary output device
- [ ] Test buttons functional during setup
- [ ] Overlay hotkey displayed and confirmable
- [ ] Settings saved at end of setup
- [ ] Subsequent launches skip first-run flow

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.App/Bootstrap/AppStartup.cs` | Detect first-run, trigger setup |
| `src/TtsCommunicationTool.UI/ViewModels/SettingsViewModel.cs` | First-run mode flag |

## Implementation Notes

- Can reuse settings window in a "first-run" mode rather than building a separate wizard
- Defaults pre-filled (Ctrl+Shift+Space, Kokoro voice, 720×140 overlay)

## Testing

- [ ] Delete config file → first-run triggers on launch
- [ ] Complete setup → config saved, next launch skips setup
- [ ] Test buttons work during first-run

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
