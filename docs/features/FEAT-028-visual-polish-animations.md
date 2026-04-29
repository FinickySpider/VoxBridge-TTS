---
id: FEAT-028
type: feature
phase: PHASE-03
sprint: SPRINT-06
status: in_progress
dependencies: []
---

# FEAT-028: Visual Polish and Subtle Animations

## Summary

Add a fade-in animation to the overlay on open, and a brief success glow on the send button
and status bar after a message is sent. All animations use WPF Storyboards and respect
`SystemParameters.ClientAreaAnimation`.

## Acceptance Criteria

- [ ] Overlay window fades in from 0 to 1 opacity over ~150 ms when opened
- [ ] No layout jank — opacity animation does not affect window size or position
- [ ] Animation is skipped gracefully if system animations are disabled

## Implementation Notes

- `OverlayWindow.xaml.cs`: trigger `BeginAnimation(OpacityProperty, …)` in `OnLoaded` or after `Show()`
- Use `DoubleAnimation` Duration=150ms, From=0, To=1, EasingFunction=`CubicEase` Out
- Check `SystemParameters.ClientAreaAnimation` before applying
