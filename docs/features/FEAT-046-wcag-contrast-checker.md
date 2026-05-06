
---
id: FEAT-046
type: feature
status: complete
priority: low
phase: PHASE-05
sprint: SPRINT-13
owner: ""
depends_on: [FEAT-042]
---

# FEAT-046: WCAG 2.1 AA Contrast Checker

## Description
Display live WCAG 2.1 AA contrast ratio status for key color pairs in the Theme tab. Pure local math — no external library. Helps users avoid creating unreadable themes.

## Acceptance Criteria
- [ ] Contrast ratio computed via the WCAG relative luminance formula: `(L1 + 0.05) / (L2 + 0.05)` where L1 > L2
- [ ] Checked pairs (minimum): Primary Text on Window Background, Primary Text on Panel Background, Muted Text on Window Background, Overlay text on Overlay Background (if overlay bg is a theme color)
- [ ] Each pair shows: label, ratio value (e.g. "7.3:1"), status chip ("Good" in green / "Warning" in amber / "Fail" in red)
- [ ] AA thresholds: ≥ 4.5 = Good, 3.0–4.49 = Warning, < 3.0 = Fail
- [ ] Status updates live as the user edits colors
- [ ] Contrast checker lives in Theme tab Section F
- [ ] No external NuGet packages required

## Files Touched
| File | Change |
|------|--------|
| `TtsCommunicationTool.Core/Utilities/ContrastCalculator.cs` | New file — pure math |
| `UI/ViewModels/ThemeSettingsViewModel.cs` | Add computed contrast ratio properties |
| `UI/Views/SettingsWindow.xaml` | Add Section F to Theme tab |

## Implementation Notes
- Relative luminance: linearize each RGB channel (`c/255 <= 0.04045 ? c/12.92 : ((c+0.055)/1.055)^2.4`), then `L = 0.2126R + 0.7152G + 0.0722B`
- `ContrastCalculator` is a pure static class — no WPF dependencies, fully unit-testable

## Testing
- [ ] `ContrastCalculator.GetRatio("#CDD6F4", "#1E1E2E")` returns a value ≥ 4.5
- [ ] `ContrastCalculator.GetRatio("#6C7086", "#1E1E2E")` returns a lower ratio (expected ~3.x)
- [ ] UI chips update when either color in a pair is edited

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
