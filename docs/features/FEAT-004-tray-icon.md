---
id: FEAT-004
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-01
owner: ""
depends_on: [FEAT-001, FEAT-002]
---

# FEAT-004: System Tray Icon and Menu

## Description

Implement the system tray icon and context menu using `TrayIconManager` and `TrayMenuBuilder`. The app starts minimized to tray. Tray menu provides access to Open Settings, Open Phrase Manager, and Exit. Close/minimize behavior respects config settings (`closeToTray`, `minimizeToTray`).

## Acceptance Criteria

- [ ] Tray icon appears on app launch
- [ ] Right-click tray icon shows context menu
- [ ] Menu includes: Open Settings, Open Phrase Manager, Exit
- [ ] Exit menu item shuts down the application
- [ ] App minimizes to tray when configured
- [ ] App closes to tray when configured
- [ ] Tray icon removed on application shutdown

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.App/Tray/TrayIconManager.cs` | New |
| `src/TtsCommunicationTool.App/Tray/TrayMenuBuilder.cs` | New |
| `src/TtsCommunicationTool.App/App.xaml.cs` | Bootstrap tray on startup |

## Implementation Notes

- Use `System.Windows.Forms.NotifyIcon` or a WPF-compatible tray library
- Wire menu commands to service actions (settings window open, etc.)

## Testing

- [ ] Tray icon visible after launch
- [ ] Menu items functional
- [ ] Exit shuts down cleanly

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
