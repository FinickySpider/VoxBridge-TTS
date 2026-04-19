---
id: ADR-0005
type: decision
status: complete
date: 2026-04-18
supersedes: ""
superseded_by: ""
---

# ADR-0005: Windows Native Global Hotkeys via P/Invoke

## Context

The application must register system-wide hotkeys that work while other applications have focus (e.g., Discord, VRChat in borderless fullscreen). Options considered: Windows `RegisterHotKey` via P/Invoke, third-party hotkey libraries, and keyboard hook (`SetWindowsHookEx`).

## Decision

Use **Windows `RegisterHotKey` / `UnregisterHotKey` API via P/Invoke** for global hotkey registration.

## Consequences

### Positive

- Reliable, well-understood Windows API
- Works across all foreground applications
- No third-party dependency
- Clear conflict detection (registration returns failure if hotkey is taken)
- Simple implementation for the small number of hotkeys needed

### Negative

- Requires a message loop (hidden window or WPF dispatcher) to receive `WM_HOTKEY`
- Best-effort only in exclusive fullscreen (documented non-goal)
- Hotkey modification requires unregister + re-register cycle
- P/Invoke requires `NativeMethods` wrapper class

## Links

- Related items:
  - FEAT-006
  - FEAT-011
  - FEAT-014
