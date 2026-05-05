
---
id: FEAT-042
type: feature
status: planned
priority: high
phase: PHASE-04
sprint: SPRINT-11
owner: ""
depends_on: [FEAT-038, FEAT-039, FEAT-040, FEAT-041]
---

# FEAT-042: ThemeSettingsViewModel + Theme Tab UI

## Description
The ViewModel and full Settings → Theme tab UI. Covers color editing (sections A–C from design), the preset dropdown toolbar, and the live preview panel. Appearance tab stays untouched.

## Acceptance Criteria
- [ ] `ThemeSettingsViewModel` exists in `TtsCommunicationTool.UI/ViewModels/ThemeSettingsViewModel.cs`
- [ ] Each of the 17 color properties is a bindable string property; setter calls `IThemeService.Apply(WorkingCopy)` immediately (live preview)
- [ ] `IsDirty` is true when `WorkingCopy` differs from the last-saved theme
- [ ] `IsEditingBuiltIn` is true when the active working theme has `IsBuiltIn = true`
- [ ] **Save command**: if `IsEditingBuiltIn` → shows "Save as New Theme" name input dialog → calls `SaveAsUserTheme` → switches active selection to new theme; else overwrites existing user theme file
- [ ] **Save As command**: always prompts for a name → saves new user theme
- [ ] **Duplicate command**: `SaveAsUserTheme(CurrentTheme, "{name} Copy")`
- [ ] **Reset command**: reverts `WorkingCopy` to the active saved theme (discards unsaved edits)
- [ ] **Delete command**: available only for user (non-built-in) themes; calls `IThemeService.DeleteUserTheme`
- [ ] Preset dropdown lists all themes from `IThemeService.LoadAll()`; selecting one applies it live
- [ ] "Pick" button opens `System.Windows.Forms.ColorDialog`; selected color updates the corresponding hex property
- [ ] Each color row shows: label, hex TextBox, colored swatch Border, Pick button, Reset-to-default button
- [ ] Color sections: Core Colors, Text Colors, Status Colors (matching design sections A–C)
- [ ] Live Preview panel shows representative controls (accent button, secondary button, text input, text at all 4 levels, status chips, overlay snippet) — single panel, no sub-tabs
- [ ] "Built-in — Saving will create a new theme" banner visible when `IsEditingBuiltIn = true`
- [ ] Theme tab is a new `TabItem Header="Theme"` in `SettingsWindow.xaml`; Appearance tab unchanged

## Files Touched
| File | Change |
|------|--------|
| `TtsCommunicationTool.UI/ViewModels/ThemeSettingsViewModel.cs` | New file |
| `TtsCommunicationTool.UI/Views/SettingsWindow.xaml` | Add Theme TabItem |
| `TtsCommunicationTool.UI/Views/SettingsWindow.xaml.cs` | Wire ThemeSettingsViewModel |
| `TtsCommunicationTool.UI/Controls/ThemeLivePreview.xaml` | New UserControl (live preview panel) |

## Implementation Notes
- `System.Windows.Forms.ColorDialog` requires `using System.Windows.Forms;` and a reference to `System.Windows.Forms` assembly — already available in WPF projects targeting net10.0-windows
- Hex TextBox accepts 6-char input with `#` prefix; validate on commit (LostFocus/Enter), revert to last valid on invalid input
- Live preview UserControl reads from `Application.Current.Resources` (DynamicResource) — no special wiring needed once FEAT-040 is done
- "Save as New Theme" dialog: a simple WPF `Window` or `InputDialog` helper (inline prompt)

## Testing
- [ ] Changing a color in the tab live-updates matching controls throughout the app
- [ ] Clicking Pick opens the Windows color picker; cancelling makes no change
- [ ] Saving a built-in theme prompts for a new name and does not modify the built-in
- [ ] Selecting a different preset from the dropdown applies it immediately
- [ ] Reset discards unsaved edits and reverts to saved state
- [ ] Appearance tab still functions normally

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
