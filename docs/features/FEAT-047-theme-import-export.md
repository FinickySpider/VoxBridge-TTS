
---
id: FEAT-047
type: feature
status: planned
priority: low
phase: PHASE-05
sprint: SPRINT-13
owner: ""
depends_on: [FEAT-041, FEAT-042]
---

# FEAT-047: Theme Import / Export

## Description
Allow users to export any theme as a `.ttstheme` JSON file and import one back. Supports sharing themes between machines or with other users.

## Acceptance Criteria
- [ ] "Export Theme..." button in the Theme tab opens a `SaveFileDialog` filtered to `*.ttstheme`
- [ ] Exported file is valid `System.Text.Json`-serialized `ThemeSettings` with `IsBuiltIn` omitted
- [ ] "Import Theme..." button opens an `OpenFileDialog` filtered to `*.ttstheme`
- [ ] Imported file is validated: all 17 required color fields present and parseable as hex colors
- [ ] On successful import, the theme is saved to the user themes folder and added to the preset dropdown
- [ ] On validation failure, a user-visible error message is shown; no partial state is applied
- [ ] `IThemeService.ExportAsync(ThemeSettings, string filePath)` and `ImportAsync(string filePath)` implement the file I/O
- [ ] Export/Import available for all themes including built-ins (exporting a built-in produces a user-editable copy)

## Files Touched
| File | Change |
|------|--------|
| `TtsCommunicationTool.Infrastructure/Services/ThemeService.cs` | Add ExportAsync, ImportAsync |
| `TtsCommunicationTool.Core/Interfaces/IThemeService.cs` | Add ExportAsync, ImportAsync signatures |
| `UI/ViewModels/ThemeSettingsViewModel.cs` | Add ExportCommand, ImportCommand |
| `UI/Views/SettingsWindow.xaml` | Add Section G (Import/Export buttons) to Theme tab |

## Testing
- [ ] Export writes a readable JSON file with the correct theme name and all 17 color fields
- [ ] Import of the same file restores the exact same color values
- [ ] Import of a malformed file shows an error message and does not change the active theme
- [ ] Exported built-in theme loads as a user theme (IsBuiltIn = false after import)

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
