
---
id: FEAT-043
type: feature
status: planned
priority: high
phase: PHASE-04
sprint: SPRINT-11
owner: ""
depends_on: [FEAT-041]
---

# FEAT-043: Tray Icon "Reset Theme to Default"

## Description
Add a tray context menu item that calls `IThemeService.ResetToDefault()`. This serves as an emergency recovery path when a user has misconfigured a theme so severely that the Settings window is broken or unusable.

## Acceptance Criteria
- [ ] Tray context menu contains "Reset Theme to Default" item
- [ ] Clicking it calls `IThemeService.ResetToDefault()` synchronously on the UI thread
- [ ] The Default Dark theme is applied immediately to all open windows without restart
- [ ] `ActiveThemeName = "Default Dark"` is persisted to config after reset
- [ ] A tray tooltip or notification briefly confirms the reset occurred (e.g. "Theme reset to Default Dark")
- [ ] Menu item is always visible regardless of current theme or settings state

## Files Touched
| File | Change |
|------|--------|
| Tray icon context menu (App.xaml.cs or TrayIconService) | Add menu item + handler |

## Implementation Notes
- `IThemeService` must be accessible from wherever tray menu handlers are wired (DI-injected into the tray handler class)
- Keep handler to: call `ResetToDefault()`, show brief confirmation

## Testing
- [ ] Menu item appears in tray right-click menu
- [ ] With a garish/broken custom theme applied, clicking reset restores correct appearance immediately
- [ ] Config file shows `ActiveThemeName: "Default Dark"` after reset

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
