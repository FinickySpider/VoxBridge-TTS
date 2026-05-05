
---
id: FEAT-040
type: feature
status: planned
priority: high
phase: PHASE-04
sprint: SPRINT-10
owner: ""
depends_on: [FEAT-039]
---

# FEAT-040: Full XAML DynamicResource Refactor

## Description
Replace every hardcoded hex color literal in every XAML file with a `{DynamicResource ...}` reference to the appropriate named brush defined in `Default.xaml`. This makes the entire app respond to theme changes at runtime.

## Acceptance Criteria
- [ ] Grep for `#[0-9A-Fa-f]{6}` across all `.xaml` files returns 0 matches (no hardcoded color hex)
- [ ] All affected XAML files compile without errors
- [ ] App launches and all windows render correctly with the default theme applied
- [ ] No `StaticResource` used for theme colors — all color resources use `{DynamicResource ...}`

### Files in scope
- `TtsCommunicationTool.UI/Views/SettingsWindow.xaml`
- `TtsCommunicationTool.UI/Views/OverlayWindow.xaml`
- `TtsCommunicationTool.UI/Views/PhraseEditorWindow.xaml`
- Any other `.xaml` files discovered to contain hardcoded hex

## Files Touched
| File | Change |
|------|--------|
| `Views/SettingsWindow.xaml` | All hex → `{DynamicResource ...}` |
| `Views/OverlayWindow.xaml` | All hex → `{DynamicResource ...}` |
| `Views/PhraseEditorWindow.xaml` | All hex → `{DynamicResource ...}` |
| Other `.xaml` files (if any) | All hex → `{DynamicResource ...}` |

## Implementation Notes
- Work file by file; verify build passes after each file
- Some hex values appear in `ControlTemplate` triggers where `TargetName` scoping applies — ensure these still resolve correctly with DynamicResource
- A few semantic-intent colors (e.g. ElevenLabs red voice highlight `#F38BA8`) map to `ErrorBrush`; document any non-obvious mappings

## Testing
- [ ] `grep -rP "#[0-9A-Fa-f]{6}" src/**/*.xaml` returns no results
- [ ] All windows open without visual regressions
- [ ] Phrase Editor ElevenLabs red voice text still renders red

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
