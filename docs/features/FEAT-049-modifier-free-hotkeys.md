---
id: FEAT-049
type: feature
status: complete
priority: medium
phase: PHASE-06
sprint: SPRINT-14
owner: ""
depends_on: [FEAT-006, FEAT-024]
---

# FEAT-049: Modifier-Free Global Hotkeys

## Description
Allow global hotkeys to consist of a single non-modifier key, such as F13, while retaining reserved-key and conflict validation.

## Acceptance Criteria
- [x] Hotkey validation accepts a non-modifier key without Ctrl, Alt, Shift, or Win
- [x] Settings capture UI documents that modifiers are optional
- [x] F13–F24 can be registered through the Windows global hotkey service
- [x] Reserved modifier-only keys and Win modifiers remain rejected
- [x] Build succeeds with 0 errors and 0 warnings

## Files Touched
- Core hotkey validation
- Global hotkey registration
- Settings hotkey guidance
