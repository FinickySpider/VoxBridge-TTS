
---
id: FEAT-044
type: feature
status: complete
priority: medium
phase: PHASE-05
sprint: SPRINT-12
owner: ""
depends_on: [FEAT-038, FEAT-039, FEAT-040, FEAT-041, FEAT-042]
---

# FEAT-044: Typography Controls in Theme Tab

## Description
Add font family and font size controls to the Theme tab. UI font controls the Settings and all non-overlay windows; overlay font controls the overlay window. Overlay font family and size are migrated out of the Appearance tab into Theme.

## Acceptance Criteria
- [ ] `ThemeSettings` gains: `UiFontFamily`, `BaseFontSize` (double), `OverlayFontFamily`, `OverlayFontSize` (double)
- [ ] `Default.xaml` gains `UiFontFamilyResource`, `BaseFontSizeResource`, `OverlayFontFamilyResource`, `OverlayFontSizeResource` entries
- [ ] All non-overlay windows bind their root `FontFamily` and `FontSize` to the appropriate `DynamicResource`
- [ ] Overlay window binds `FontFamily` and `FontSize` to the overlay-specific resources
- [ ] Theme tab Section D shows: UI Font dropdown (InstalledFonts list), Base Font Size spinner, Overlay Font dropdown, Overlay Font Size spinner
- [ ] Changing any typography control updates live (no restart required)
- [ ] Overlay font family and size **removed** from Appearance tab (migrated here); `AppearanceSettingsViewModel` no longer owns them
- [ ] `ThemeSettings` serializes/deserializes typography fields correctly
- [ ] `ThemeDefaults.CreateDefault()` returns `UiFontFamily = "Segoe UI"`, `BaseFontSize = 13`, `OverlayFontFamily = "Segoe UI"`, `OverlayFontSize = 18` (or current actual defaults)

## Files Touched
| File | Change |
|------|--------|
| `Core/Models/ThemeSettings.cs` | Add typography properties |
| `Core/Models/ThemeDefaults.cs` | Add typography defaults |
| `UI/Themes/Default.xaml` | Add typography resource entries |
| `UI/ViewModels/ThemeSettingsViewModel.cs` | Add typography bindable properties |
| `UI/ViewModels/AppearanceSettingsViewModel.cs` | Remove OverlayFontFamily + FontSize |
| `UI/Views/SettingsWindow.xaml` | Add Section D to Theme tab; remove from Appearance tab |
| `UI/Views/OverlayWindow.xaml` | Bind font to DynamicResource |
| Other windows (if hardcoded fonts exist) | Bind to DynamicResource |

## Testing
- [ ] Changing UI font live-updates Settings window text
- [ ] Changing overlay font live-updates the overlay window text
- [ ] Appearance tab no longer shows font controls
- [ ] Fonts persist across app restart

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
