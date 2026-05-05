
---
id: FEAT-045
type: feature
status: planned
priority: medium
phase: PHASE-05
sprint: SPRINT-12
owner: ""
depends_on: [FEAT-038, FEAT-039, FEAT-040, FEAT-041, FEAT-042]
---

# FEAT-045: Shape & Density Controls in Theme Tab

## Description
Expose corner radius, border thickness, control height, and spacing density as theme-controlled values in the Theme tab. All controls across the app use `DynamicResource` for these values.

## Acceptance Criteria
- [ ] `ThemeSettings` gains: `CornerRadius` (double), `BorderThickness` (double), `ControlHeight` (double), `SpacingDensity` (enum: Comfortable / Compact / Spacious)
- [ ] `Default.xaml` already has `CornerRadius`, `BorderThickness`, `ControlHeight` entries (added in FEAT-039); this feature wires them into the UI
- [ ] Spacing density maps to three sets of `Padding`/`Margin` resource values; switching density updates controls live
- [ ] Theme tab Section E shows: Corner Radius slider+number, Border Thickness spinner, Control Height spinner, Spacing Density dropdown
- [ ] All `Border` CornerRadius values in all XAML files reference `{DynamicResource CornerRadius}` (or a `CornerRadius` converter resource)
- [ ] `ThemeDefaults.CreateDefault()` returns values matching current hardcoded defaults
- [ ] Controls update live when sliders/spinners change

## Files Touched
| File | Change |
|------|--------|
| `Core/Models/ThemeSettings.cs` | Add shape/density properties |
| `Core/Models/ThemeDefaults.cs` | Add shape/density defaults |
| `UI/Themes/Default.xaml` | Add spacing density resource sets |
| `UI/ViewModels/ThemeSettingsViewModel.cs` | Add shape/density properties |
| `UI/Views/SettingsWindow.xaml` | Add Section E to Theme tab |
| All XAML files with hardcoded CornerRadius/Padding/Margin | DynamicResource refactor |

## Implementation Notes
- WPF `CornerRadius` struct: use a `CornerRadiusConverter` or define a `CornerRadius` resource as a `CornerRadius` value type
- Spacing density: define three separate resource merges (Comfortable.xaml, Compact.xaml, Spacious.xaml) swapped via `ThemeService`

## Testing
- [ ] Corner radius slider visibly rounds/sharpens all bordered controls live
- [ ] Spacing density dropdown changes padding visibly
- [ ] Values persist across restart

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
