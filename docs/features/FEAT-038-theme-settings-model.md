
---
id: FEAT-038
type: feature
status: planned
priority: high
phase: PHASE-04
sprint: SPRINT-10
owner: ""
depends_on: []
---

# FEAT-038: ThemeSettings Model + ThemeDefaults

## Description
Define the core data model for a theme and a static defaults class that encodes the current hardcoded Catppuccin Mocha palette. This is the foundation every other PHASE-04 feature depends on.

## Acceptance Criteria
- [ ] `ThemeSettings` POCO exists in `TtsCommunicationTool.Core/Models/ThemeSettings.cs`
- [ ] `ThemeSettings` has 17 semantic color properties as `string` (hex), a `Name` string, and a `bool IsBuiltIn`
- [ ] Color roles: `AccentColor`, `WindowBackground`, `PanelBackground`, `InputBackground`, `BorderColor`, `PrimaryText`, `SecondaryText`, `MutedText`, `DisabledText`, `WarningColor`, `ErrorColor`, `SuccessColor`, `InfoColor`, `DeepPanelBackground`, `HistoryBackground`, `Surface0`, `Surface2`
- [ ] `ThemeDefaults` static class exists in `TtsCommunicationTool.Core/Models/ThemeDefaults.cs`
- [ ] `ThemeDefaults.CreateDefault()` returns a fully populated `ThemeSettings` with Catppuccin Mocha hex values and `Name = "Default Dark"`, `IsBuiltIn = true`
- [ ] `AppConfig` gains an `ActiveThemeName` string property (defaults to `"Default Dark"`)
- [ ] All new types serialize cleanly with `System.Text.Json`

## Files Touched
| File | Change |
|------|--------|
| `TtsCommunicationTool.Core/Models/ThemeSettings.cs` | New file |
| `TtsCommunicationTool.Core/Models/ThemeDefaults.cs` | New file |
| `TtsCommunicationTool.Core/Models/AppConfig.cs` | Add `ActiveThemeName` property |

## Implementation Notes
- Color property names map 1:1 to ResourceDictionary keys (e.g. `AccentColor` → resource key `AccentBrush`)
- Hex values stored as `#RRGGBB` strings; parsing to `Color` done in `ThemeService`
- `IsBuiltIn` is never serialized to user theme files — it is set by the loader at runtime

## Testing
- [ ] `ThemeDefaults.CreateDefault()` returns non-null with all 17 color properties non-empty
- [ ] `AppConfig` round-trips through JSON with `ActiveThemeName` preserved

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
