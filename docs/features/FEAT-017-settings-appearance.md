---
id: FEAT-017
type: feature
status: planned
priority: low
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-005, FEAT-013]
---

# FEAT-017: Settings Window — Appearance Section

## Description

Implement the Appearance section of the settings window. Allow the user to configure overlay width, height, and font size. Changes apply to the overlay on next open (or live if feasible).

## Acceptance Criteria

- [ ] Appearance section displayed in settings window
- [ ] Overlay width configurable via numeric input or slider
- [ ] Overlay height configurable via numeric input or slider
- [ ] Font size configurable via numeric input or slider
- [ ] Changes persisted in config
- [ ] Overlay reflects new dimensions and font size

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/ViewModels/AppearanceSettingsViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/Views/SettingsWindow.xaml` | Add Appearance section |

## Testing

- [ ] Width/height changes reflected in overlay
- [ ] Font size change reflected in overlay text input
- [ ] Settings persist across restart

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
