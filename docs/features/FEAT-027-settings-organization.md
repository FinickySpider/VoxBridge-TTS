---
id: FEAT-027
type: feature
status: complete
priority: low
phase: PHASE-02
sprint: SPRINT-05
owner: ""
depends_on: [FEAT-013, FEAT-014, FEAT-015, FEAT-016, FEAT-017, FEAT-018]
---

# FEAT-027: Settings Organization Improvements

## Description
Improve the usability and clarity of the settings window based on real-world use: better grouping, clearer labels, helpful tooltips, remove dead options, and ensure all sections feel complete and consistent.

## Acceptance Criteria
- [ ] All settings sections have consistent heading styles and spacing
- [ ] Removed CloseToTray/MinimizeToTray checkboxes (already hardcoded behaviour) are gone
- [ ] Tooltips added to non-obvious controls (hotkey fields, device dropdowns, volume sliders)
- [ ] Volume sliders show numeric percentage alongside slider
- [ ] Phrase tab has Import/Export buttons (wired to FEAT-026)
- [ ] No orphaned or non-functional UI elements remain
- [ ] Settings window resizes gracefully if content grows

## Files Touched
| File | Change |
|------|--------|
| `src/.../Views/SettingsWindow.xaml` | Cleanup, tooltips, labels, consistency pass |

## Implementation Notes
- Pure UI polish â€” no new settings keys
- Tooltips: `<ToolTip>` on key controls
- Ensure volume sliders added in FEAT-023 are styled consistently

## Testing
- [ ] Open settings â€” all tabs look clean and consistent
- [ ] Hover over hotkey boxes â€” tooltips appear
- [ ] No cut-off text or misaligned controls at default window size

## Done When
- [ ] Acceptance criteria met
- [ ] Verified manually
