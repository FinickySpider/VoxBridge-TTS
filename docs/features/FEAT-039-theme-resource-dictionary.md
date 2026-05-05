
---
id: FEAT-039
type: feature
status: planned
priority: high
phase: PHASE-04
sprint: SPRINT-10
owner: ""
depends_on: [FEAT-038]
---

# FEAT-039: Themes/Default.xaml ResourceDictionary + App.xaml Wiring

## Description
Create the named WPF ResourceDictionary that owns all theme brush and value resources. Wire it into `App.xaml` as a merged dictionary so every window in the app can reference resources via `{DynamicResource ...}`.

## Acceptance Criteria
- [ ] `TtsCommunicationTool.UI/Themes/Default.xaml` exists as a `ResourceDictionary`
- [ ] Contains one `SolidColorBrush` entry per color role using the keys below
- [ ] Contains `CornerRadius` (double), `BorderThickness` (double), `ControlHeight` (double) as resource values
- [ ] `Default.xaml` is merged in `App.xaml` under `Application.Resources > MergedDictionaries`
- [ ] App launches with no resource lookup warnings and appearance is visually identical to pre-change

### Required resource keys
`AccentBrush`, `WindowBackgroundBrush`, `PanelBackgroundBrush`, `InputBackgroundBrush`, `BorderBrush`, `PrimaryTextBrush`, `SecondaryTextBrush`, `MutedTextBrush`, `DisabledTextBrush`, `WarningBrush`, `ErrorBrush`, `SuccessBrush`, `InfoBrush`, `DeepPanelBackgroundBrush`, `HistoryBackgroundBrush`, `Surface0Brush`, `Surface2Brush`

## Files Touched
| File | Change |
|------|--------|
| `TtsCommunicationTool.UI/Themes/Default.xaml` | New file |
| `TtsCommunicationTool.UI/App.xaml` | Add MergedDictionary entry |

## Implementation Notes
- Default values in `Default.xaml` must exactly match `ThemeDefaults.CreateDefault()` hex values
- Use `x:Key` names consistent with the `ThemeSettings` property → brush key mapping convention: `{PropertyName}` → `{PropertyName}Brush` (e.g. `AccentColor` → `AccentBrush`)

## Testing
- [ ] App launches; no yellow squiggles or runtime resource-not-found warnings
- [ ] Visual appearance matches pre-change screenshots

## Done When
- [ ] Acceptance criteria met
- [ ] Build: 0 errors, 0 warnings
