
---
id: PHASE-05
type: phase
status: complete
owner: ""
---

# PHASE-05: Theme Polish & Advanced

## Goal
Extend the dynamic theming system built in PHASE-04 with typography and shape controls, WCAG contrast checking, and theme import/export. This phase is purely additive polish — the app is fully functional without it.

## In Scope
- Typography controls (UI font family, base font size, overlay font family, overlay font size)
- Shape & density controls (corner radius, border thickness, control height, spacing density)
- WCAG 2.1 AA contrast ratio checker as live computed properties on ThemeSettingsViewModel
- Contrast status indicators in the Theme tab UI (Good / Warning per color pair)
- Theme import (OpenFileDialog → JSON) and export (SaveFileDialog → JSON)

## Out of Scope
- Any new non-theme features
- Built-in theme additions beyond what shipped in PHASE-04
- Cloud sync of themes

## Sprints
- [SPRINT-12](../sprints/SPRINT-12.md)
- [SPRINT-13](../sprints/SPRINT-13.md)

## Completion Criteria
- [x] UI font and base font size controls in Theme tab, applied via DynamicResource
- [x] Overlay font family and size migrated from Appearance tab into Theme tab
- [x] Corner radius, border thickness, control height, and spacing density controls functional
- [x] WCAG contrast check displays live Good/Warning status for primary + muted text pairs
- [x] Export Theme writes a valid `.ttstheme` JSON file loadable by Import
- [x] Import Theme loads a `.ttstheme` file, validates it, and applies it as a new user theme
- [x] Build: 0 errors, 0 warnings
