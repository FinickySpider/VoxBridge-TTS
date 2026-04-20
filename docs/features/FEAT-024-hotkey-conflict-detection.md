---
id: FEAT-024
type: feature
status: complete
priority: medium
phase: PHASE-02
sprint: SPRINT-04
owner: ""
depends_on: [FEAT-006, FEAT-014]
---

# FEAT-024: Hotkey Conflict Detection

## Description
When the user sets hotkeys in settings, detect and warn about:
1. Internal conflicts (overlay hotkey == stop hotkey, or two phrases share a hotkey)
2. Registration failure (system-level conflict with another app)

Show inline error messages in the settings UI so the user knows immediately before saving.

## Acceptance Criteria
- [ ] Saving settings with overlay hotkey == stop hotkey shows an inline error and blocks save
- [ ] Saving with two phrases sharing the same hotkey shows an inline error and blocks save
- [ ] When a hotkey fails to register (system conflict), show a persistent warning notification â€” not a crash
- [ ] Inline error messages appear next to the conflicting hotkey fields in the settings UI
- [ ] Conflict errors are cleared when the conflict is resolved

## Files Touched
| File | Change |
|------|--------|
| `src/.../UI/ViewModels/HotkeySettingsViewModel.cs` | Add conflict validation method |
| `src/.../UI/ViewModels/PhraseListViewModel.cs` | Add duplicate hotkey check |
| `src/.../UI/ViewModels/SettingsViewModel.cs` | Call validation before SaveAsync proceeds |
| `src/.../Views/SettingsWindow.xaml` | Bind inline error text blocks |

## Implementation Notes
- Validation runs on SaveCommand CanExecute or at start of SaveAsync before writing config
- Use `HotkeyBindingEqualityComparer` or compare fields directly
- Registration failure already logs â€” surface that to NotificationService as a warning

## Testing
- [ ] Set overlay = stop hotkey â†’ save blocked with message
- [ ] Two phrases same hotkey â†’ save blocked with message
- [ ] Set a hotkey already used by another app â†’ warning shown, app does not crash
- [ ] Fix conflict â†’ error clears â†’ save works

## Done When
- [ ] Acceptance criteria met
- [ ] Verified manually
