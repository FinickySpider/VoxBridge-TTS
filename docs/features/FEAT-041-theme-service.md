
---
id: FEAT-041
type: feature
status: planned
priority: high
phase: PHASE-04
sprint: SPRINT-10
owner: ""
depends_on: [FEAT-038, FEAT-039]
---

# FEAT-041: IThemeService + ThemeService

## Description
The runtime service that loads all available themes (embedded built-ins + user files), applies a theme to the running application by updating `Application.Current.Resources`, and handles persistence of the active theme choice.

## Acceptance Criteria
- [ ] `IThemeService` interface defined in `TtsCommunicationTool.Core/Interfaces/IThemeService.cs`
- [ ] `ThemeService` implementation in `TtsCommunicationTool.Infrastructure/Services/ThemeService.cs`
- [ ] `Apply(ThemeSettings)` updates all brush/value resources in `Application.Current.Resources` immediately; controls update live via DynamicResource
- [ ] `LoadAll()` returns built-in themes (from embedded JSON resources) merged with any `.ttstheme` files found in `%AppData%\TtsCommunicationTool\themes\`
- [ ] `ResetToDefault()` calls `Apply(ThemeDefaults.CreateDefault())` and saves `"Default Dark"` as `ActiveThemeName` in config
- [ ] `SaveAsUserTheme(ThemeSettings, string name)` writes a JSON file to the user themes folder; `IsBuiltIn = false` is never written
- [ ] `DeleteUserTheme(string name)` deletes the corresponding user theme file; silently no-ops if name is a built-in
- [ ] Built-in theme JSON files are embedded as assembly resources and never written to disk
- [ ] Service is registered in the DI container

## Files Touched
| File | Change |
|------|--------|
| `TtsCommunicationTool.Core/Interfaces/IThemeService.cs` | New file |
| `TtsCommunicationTool.Infrastructure/Services/ThemeService.cs` | New file |
| `TtsCommunicationTool.UI/Themes/DefaultDark.json` | New embedded resource (built-in theme JSON) |
| DI registration file (App.xaml.cs or ServiceCollectionExtensions) | Register ThemeService |

## Implementation Notes
- `Apply()` must run on the UI thread — use `Application.Current.Dispatcher` if called from background
- Hex→Color parsing: `ColorConverter.ConvertFromString(hex)` or manual parse
- User themes folder: `Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "TtsCommunicationTool", "themes")`
- At app startup, `ConfigService` reads `ActiveThemeName`; `ThemeService.LoadAll()` finds the matching theme and calls `Apply()`. If not found, falls back to `ResetToDefault()`

## Testing
- [ ] `Apply()` live-updates a running window's colors without restart
- [ ] `LoadAll()` returns at least 1 item (Default Dark) when no user themes exist
- [ ] `SaveAsUserTheme()` creates a file in the user themes folder
- [ ] `DeleteUserTheme()` on a built-in name is a no-op (no error thrown)
- [ ] `ResetToDefault()` restores Default Dark even when called from the tray

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
