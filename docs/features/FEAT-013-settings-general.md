---
id: FEAT-013
type: feature
status: planned
priority: medium
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-002, FEAT-004]
---

# FEAT-013: Settings Window — General Section

## Description

Create the settings window shell with tab/section navigation and implement the General section. General section includes: Close to tray toggle, Minimize to tray toggle, optional Start with Windows toggle, and a button to run the audio setup test flow.

## Acceptance Criteria

- [ ] Settings window opens from tray menu
- [ ] Settings window has tabbed or sectioned navigation for all MVP sections
- [ ] General section displays: Close to tray, Minimize to tray, Start with Windows checkboxes
- [ ] Checkbox changes are persisted when saved
- [ ] Audio setup test button navigates to audio section or triggers test flow
- [ ] Dark theme applied consistently

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/Views/SettingsWindow.xaml` | New |
| `src/TtsCommunicationTool.UI/Views/SettingsWindow.xaml.cs` | New |
| `src/TtsCommunicationTool.UI/ViewModels/SettingsViewModel.cs` | New |
| `src/TtsCommunicationTool.UI/ViewModels/GeneralSettingsViewModel.cs` | New |

## Testing

- [ ] Settings window opens and shows General section
- [ ] Toggle changes persist after restart

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
