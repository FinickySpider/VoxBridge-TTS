---
id: FEAT-015
type: feature
status: complete
priority: high
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-009, FEAT-010, FEAT-013]
---

# FEAT-015: Settings Window — Audio Section with Test Buttons

## Description

Implement the Audio section of the settings window. Provide dropdowns for monitor and secondary output device selection. Include Test Monitor, Test Secondary, and Test Both buttons that play a test phrase through the respective audio paths. Show device status warnings.

## Acceptance Criteria

- [ ] Audio section displayed in settings window
- [ ] Monitor output device dropdown populated from device enumeration
- [ ] Secondary output device dropdown populated from device enumeration
- [ ] Test Monitor button plays test audio to monitor device only
- [ ] Test Secondary button plays test audio to secondary device only
- [ ] Test Both button plays test audio to both devices
- [ ] Device selection persisted in config
- [ ] Missing device shows warning status line

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/ViewModels/AudioSettingsViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/Views/SettingsWindow.xaml` | Add Audio section |

## Testing

- [ ] Devices listed in dropdowns
- [ ] Test buttons produce audible output
- [ ] Missing device shows warning

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
