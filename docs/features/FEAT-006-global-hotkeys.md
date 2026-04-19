---
id: FEAT-006
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-01
owner: ""
depends_on: [FEAT-001, FEAT-002, FEAT-005]
---

# FEAT-006: Global Hotkey Registration

## Description

Implement `IHotkeyService` / `WindowsHotkeyService` for registering and managing global hotkeys using Windows native API (`RegisterHotKey`). Register the overlay hotkey and stop hotkey from config. Surface registration failures clearly. Overlay hotkey triggers `IOverlayCoordinator` to show/focus overlay.

## Acceptance Criteria

- [ ] `IHotkeyService` interface defined with Register, Unregister, and event-raising methods
- [ ] `WindowsHotkeyService` uses Windows API `RegisterHotKey` / `UnregisterHotKey`
- [ ] Overlay hotkey opens or refocuses overlay (no duplicate windows)
- [ ] Stop hotkey stub wired (functional in SPRINT-02)
- [ ] Registration failure shows user-facing error message
- [ ] Hotkey bindings loaded from config
- [ ] Hotkeys unregistered on app shutdown

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/IHotkeyService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Hotkeys/WindowsHotkeyService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Hotkeys/NativeMethods.cs` | New |

## Implementation Notes

- Use P/Invoke for `RegisterHotKey` and `UnregisterHotKey`
- Need a hidden window or message hook to receive `WM_HOTKEY`
- Marshal hotkey callbacks to UI thread

## Testing

- [ ] Overlay hotkey opens overlay
- [ ] Conflicting hotkey shows error
- [ ] Hotkeys cleaned up on exit

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
