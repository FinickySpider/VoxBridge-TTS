# FEAT-036 — Settings Save Toast + Debounce

**Phase:** Post-PHASE-03
**Sprint:** [SPRINT-09](../sprints/SPRINT-09.md)
**Status:** complete

---

## Summary

Show a "Settings saved." info toast after saving from the Settings window. Debounce to prevent duplicate toasts from rapid repeated saves.

---

## Acceptance Criteria

- [x] `SettingsViewModel` receives `INotificationService` via DI
- [x] `SaveAsync()` calls `_notifications.ShowInfo("Settings saved.")` after a successful save
- [x] Debounce: toast is skipped if a toast was shown within the last 1.5 seconds
- [x] `_savedPitch` snapshot updated on save (so Cancel knows the correct restore point after a save)
- [x] Bundled improvements: Settings tab reorder (Replacements before Phrases), improved tooltips across all tabs, audio routing callout note in Audio tab
- [x] Build succeeds: 0 errors, 0 warnings
