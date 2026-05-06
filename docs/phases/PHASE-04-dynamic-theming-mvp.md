
---
id: PHASE-04
type: phase
status: complete
owner: ""
---

# PHASE-04: Dynamic Theming MVP

## Goal
Replace all hardcoded hex color literals across the entire WPF codebase with a dynamic ResourceDictionary system, ship a Theme tab in Settings with live color editing, a preset system with built-in immutable themes and user-created themes, and tray-based emergency theme reset.

## In Scope
- `ThemeSettings` model (17 semantic color roles as hex strings, name, isBuiltIn flag)
- `ThemeDefaults` static class (returns the default Catppuccin Mocha palette)
- Embedded built-in theme JSON files (at minimum: Default Dark a.k.a. "Catppuccin Mocha")
- `Themes/Default.xaml` ResourceDictionary with 20 named brushes + thickness/double values
- `App.xaml` merging `Default.xaml` into application resources
- Full XAML refactor of all windows (SettingsWindow, OverlayWindow, PhraseEditorWindow, MainWindow) — all hardcoded hex → `{DynamicResource ...}`
- `IThemeService` interface (Core) + `ThemeService` implementation (Infrastructure)
  - `Apply(ThemeSettings)` — pushes colors into `Application.Current.Resources` live
  - `LoadAll()` — merges embedded built-in + `%AppData%\themes\` user themes
  - `ResetToDefault()` — applies Default Dark, saves choice to config
  - `SaveAsUserTheme(ThemeSettings, string name)`
  - `DeleteUserTheme(string name)`
- `ThemeSettingsViewModel` with per-color bindable properties, `IsDirty`, `IsEditingBuiltIn`, Save/SaveAs/Duplicate/Reset commands
- Theme tab in SettingsWindow (sections: Colors → Core, Text, Status; Preset bar; Live Preview panel)
- Color picker: native `System.Windows.Forms.ColorDialog` per "Pick" button
- Preset dropdown with Save As / Duplicate / Reset; built-in themes are read-only seeds (saving creates a new user theme)
- Tray icon "Reset Theme to Default" menu item → `IThemeService.ResetToDefault()`
- `AppConfig` gains `ThemeSettings` property (name of active theme, persisted as a string reference)
- Appearance tab kept separate — no merging with Theme tab

## Out of Scope
- Typography controls (font family/size) — deferred to PHASE-05
- Shape & density controls — deferred to PHASE-05
- WCAG contrast checker — deferred to PHASE-05
- Theme import/export files — deferred to PHASE-05

## Sprints
- [SPRINT-10](../sprints/SPRINT-10.md)
- [SPRINT-11](../sprints/SPRINT-11.md)

## Completion Criteria
- [ ] All hardcoded hex literals removed from all XAML files (grep returns 0 results)
- [ ] Default Dark built-in theme loads at startup; app is visually identical to pre-theme state
- [ ] User can change any color in the Theme tab and see the app update live without restart
- [ ] Built-in themes cannot be overwritten; Save on a built-in triggers Save As dialog
- [ ] User themes persist across app restarts
- [ ] Tray "Reset Theme to Default" applies Default Dark even if Settings window is broken
- [ ] Build: 0 errors, 0 warnings
