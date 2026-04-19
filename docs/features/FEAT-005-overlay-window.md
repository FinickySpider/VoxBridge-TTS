---
id: FEAT-005
type: feature
status: complete
priority: high
phase: PHASE-01
sprint: SPRINT-01
owner: ""
depends_on: [FEAT-001]
---

# FEAT-005: Overlay Window Shell

## Description

Create the overlay input window: borderless, always-on-top, dark themed, centered by default, hidden from taskbar. Contains a text input box, optional send button, close button, and status text. Supports Esc close, click-outside close, X close. Single instance enforced via `IOverlayCoordinator`.

## Acceptance Criteria

- [ ] Overlay window is borderless and always-on-top
- [ ] Overlay is hidden from taskbar
- [ ] Overlay centered on screen at default 720×140 size
- [ ] Text input auto-focused on open
- [ ] Esc closes overlay without sending
- [ ] Clicking outside overlay closes it
- [ ] X button closes overlay
- [ ] Only one overlay instance can exist at a time
- [ ] Status text area shows "Ready" on open
- [ ] Dark theme applied (charcoal background, soft text)

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.UI/Views/OverlayWindow.xaml` | New |
| `src/TtsCommunicationTool.UI/Views/OverlayWindow.xaml.cs` | New |
| `src/TtsCommunicationTool.UI/ViewModels/OverlayViewModel.cs` | New |
| `src/TtsCommunicationTool.Core/Interfaces/IOverlayCoordinator.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Overlay/OverlayCoordinator.cs` | New |

## Implementation Notes

- Use `WindowStyle="None"`, `Topmost="True"`, `ShowInTaskbar="False"`
- Click-outside close can use `Deactivated` event
- Size configurable from `OverlaySettings`

## Testing

- [ ] Overlay opens centered with correct style
- [ ] All close methods work (Esc, click-outside, X)
- [ ] No duplicate overlays

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
