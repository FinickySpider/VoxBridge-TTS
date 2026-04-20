---
id: FEAT-022
type: feature
status: complete
priority: high
phase: PHASE-02
sprint: SPRINT-04
owner: ""
depends_on: [FEAT-005, FEAT-017]
---

# FEAT-022: Overlay Position Persistence

## Description
Allow the overlay window to be dragged to any screen position. The position is persisted in config and restored on next open. Defaults to center-screen on first run.

## Acceptance Criteria
- [ ] Overlay window is draggable by clicking and dragging anywhere on the window body
- [ ] Position (Left, Top) is saved to OverlaySettings in config on window close
- [ ] Persisted position is restored when overlay opens
- [ ] Falls back to CenterScreen if saved position is off-screen or first-run
- [ ] Position resets to center when manually reset via settings

## Files Touched
| File | Change |
|------|--------|
| `src/.../Core/Models/OverlaySettings.cs` | Add `Left`, `Top`, `UseCustomPosition` properties |
| `src/.../App/OverlayCoordinator.cs` | Restore position on Show; save position on Closed |
| `src/.../Views/OverlayWindow.xaml` | Add MouseLeftButtonDown drag handler |
| `src/.../Views/OverlayWindow.xaml.cs` | DragMove() on mouse down on non-button areas |
| `src/.../Views/SettingsWindow.xaml` | Add "Reset overlay position" button in Appearance tab |

## Implementation Notes
- Use `DragMove()` for dragging â€” no custom hit-test needed
- Save position in `Closed` event handler in OverlayCoordinator
- Off-screen guard: if Left < -width or Top < -height or beyond screen bounds, reset to null
- `UseCustomPosition = false` means CenterScreen

## Testing
- [ ] Drag overlay to new position, close, reopen â€” position restored
- [ ] Move to second monitor, reopen on same monitor
- [ ] Reset button in settings resets to center

## Done When
- [ ] Acceptance criteria met
- [ ] Verified manually
