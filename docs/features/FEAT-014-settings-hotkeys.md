---
id: FEAT-014
type: feature
status: complete
priority: medium
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-006, FEAT-013]
---

# FEAT-014: Settings Window — Hotkeys Section

## Description

Implement the Hotkeys section of the settings window. Allow the user to configure the overlay hotkey and stop hotkey via hotkey capture inputs. Show inline validation feedback for conflicts. Optionally display phrase hotkey assignments.

## Acceptance Criteria

- [ ] Hotkeys section displayed in settings window
- [ ] Overlay hotkey configurable via capture input
- [ ] Stop hotkey configurable via capture input
- [ ] Hotkey conflict shows inline warning
- [ ] Changed hotkeys saved and re-registered on save
- [ ] Old hotkey kept if new registration fails

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/ViewModels/HotkeySettingsViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/Views/SettingsWindow.xaml` | Add Hotkeys section |

## Testing

- [ ] Can change overlay hotkey and it works immediately
- [ ] Conflict detected and reported

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
