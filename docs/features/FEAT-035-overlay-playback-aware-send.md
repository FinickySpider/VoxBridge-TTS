# FEAT-035 — Overlay Playback-Aware Send

**Phase:** Post-PHASE-03
**Sprint:** [SPRINT-09](../sprints/SPRINT-09.md)
**Status:** complete
**Dependencies:** FEAT-034

---

## Summary

The overlay was previously blocked from opening while audio was playing. This feature removes that block and instead manages the interaction at the Send level: the overlay opens freely, but the Send action is disabled while audio plays. A Stop+Send override shortcut allows the user to pre-empt current audio.

---

## Acceptance Criteria

- [x] `OverlayCoordinator.ShowOverlay()` no longer returns early when audio is playing
- [x] `OverlayViewModel.CanSubmit = CanSend && !IsPlaying` (both audio sources checked)
- [x] Send button `IsEnabled` binds to `CanSubmit`
- [x] Overlay status shows "Wait until speaking finishes…" (Warning severity, orange) when audio is playing at overlay open or when send is blocked
- [x] Pressing Enter while `CanSubmit == false` triggers shake animation + Warning status
- [x] `ShakeRequested` event on OverlayViewModel; code-behind subscribes and animates `Window.Left`
- [x] `FireAndForgetSend()` returns `bool` (false if blocked); callers only close on `true`
- [x] `ForceStopAndSend()` stops audio then sends; exposed on OverlayViewModel
- [x] Override hotkey (default Ctrl+Enter) stored in `HotkeySettings.OverrideHotkey`, checked in-overlay only (not Win32 global)
- [x] `MatchesOverrideHotkey(key, modifiers)` helper on OverlayViewModel reads config
- [x] Override hotkey configurable in Settings → Hotkeys tab ("Overlay Shortcuts" section)
- [x] `OverrideHotkeyBox_PreviewKeyDown` handler added in SettingsWindow.xaml.cs
- [x] `StatusSeverity.Warning` added with orange color (#FAB387)
- [x] `_notifications.ShowError()` called on TTS failure
- [x] `_audioRouter.PlaybackFinished` triggers `NotifyCanSubmitChanged()` via dispatcher
- [x] Build succeeds: 0 errors, 0 warnings
