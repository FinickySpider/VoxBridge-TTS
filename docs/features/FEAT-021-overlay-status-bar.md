---
id: FEAT-021
type: feature
status: complete
priority: high
phase: PHASE-02
sprint: SPRINT-04
owner: ""
depends_on: [FEAT-005, FEAT-012]
---

# FEAT-021: Overlay Status Bar

## Description
Add a slim, seamless status bar at the bottom of the overlay window (1â€“2 lines, font size 12â€“14) that displays contextual state messages such as "Speaking...", "Generating...", or "Ready". The bar is always present but visually unobtrusive and only draws attention when something notable is happening.

## Acceptance Criteria
- [x] Status bar visible at bottom of overlay, below the text input
- [x] Font size 12â€“14, muted colour, no distracting border
- [x] Displays "Speaking..." when audio is playing
- [x] Displays "Generating..." while TTS synthesis is in progress
- [x] Displays nothing (empty) when idle/ready
- [x] Updates reactively via data binding from OverlayViewModel.StatusText

## Files Touched
| File | Change |
|------|--------|
| `src/.../Views/OverlayWindow.xaml` | Replace old status row with styled status bar border |
| `src/.../ViewModels/OverlayViewModel.cs` | StatusText updates to match new states |

## Implementation Notes
- Status bar is a `Border` with `CornerRadius` on bottom corners only, slightly different background
- Text is left-aligned, single line, truncated with ellipsis if too long
- No character counter in the status bar â€” keep it for the text input row

## Testing
- [x] Status shows "Generating..." on send
- [x] Status shows "Speaking..." during playback
- [x] Status clears when playback ends

## Done When
- [x] Acceptance criteria met
- [x] Verified manually
