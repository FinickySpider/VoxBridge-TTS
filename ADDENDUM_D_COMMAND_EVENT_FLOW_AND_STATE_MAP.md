# ADDENDUM D
## Command, Event Flow, and State Map

## 1. Core Commands

### Overlay Commands
- `OpenOverlayCommand`
- `CloseOverlayCommand`
- `SendTextCommand`
- `CancelOverlayCommand`

### Settings Commands
- `OpenSettingsCommand`
- `SaveSettingsCommand`
- `ReloadSettingsCommand`

### Phrase Commands
- `AddPhraseCommand`
- `EditPhraseCommand`
- `DeletePhraseCommand`
- `PlayPhraseCommand`

### Audio Commands
- `TestMonitorOutputCommand`
- `TestSecondaryOutputCommand`
- `TestBothOutputsCommand`
- `StopPlaybackCommand`

## 2. High-Level Event Flow

### App Startup
1. `App.xaml.cs` starts.
2. Logging initialized.
3. Config loaded.
4. Services registered.
5. Tray icon created.
6. Hotkeys registered.
7. Device and voice validation executed.
8. App enters idle state.

### Overlay Hotkey Pressed
1. Native hotkey event raised.
2. `IHotkeyService` emits overlay event.
3. `IOverlayCoordinator` checks for existing overlay instance.
4. If not open, create and show overlay.
5. Focus textbox.
6. Set overlay state to `Ready`.

### Send Text
1. User presses Enter or Send.
2. `OverlayViewModel.SendTextCommand` executes.
3. Validate text.
4. Set overlay state to `Sending`.
5. Call `ITtsService.GenerateAsync(request)`.
6. Receive audio payload.
7. Call `IAudioRouterService.PlayToBothAsync(payload)`.
8. Close overlay after playback starts successfully.
9. App state becomes `PlayingSpeech`.
10. On completion, return to `Idle`.

### Send Failure
1. Validation, TTS, or playback fails.
2. Error is logged.
3. User-facing notification shown.
4. Overlay remains open.
5. Original text remains.
6. Overlay state becomes `Error`.

### Stop Playback
1. User presses stop hotkey.
2. `IHotkeyService` emits stop event.
3. `IAudioRouterService.Stop()` executes.
4. Playback session disposed.
5. App state returns to `Idle`.

### Phrase Playback
1. User clicks phrase or phrase hotkey fires.
2. Phrase text loaded.
3. Same pipeline as manual text send.
4. Playback executes.
5. Errors handled identically.

## 3. State Map

### App Runtime State
- `Starting`
- `Idle`
- `OverlayOpen`
- `GeneratingSpeech`
- `PlayingSpeech`
- `RecoverableError`
- `ShuttingDown`

### Overlay State
- `Hidden`
- `Ready`
- `Sending`
- `Error`

### Playback State
- `Idle`
- `Starting`
- `Playing`
- `Stopping`
- `Faulted`

## 4. State Transition Rules

### Overlay
- `Hidden -> Ready` when overlay opens
- `Ready -> Sending` when send starts
- `Sending -> Hidden` on successful handoff to audio playback
- `Sending -> Error` on failure
- `Error -> Ready` when user edits text or retries
- `Ready -> Hidden` on cancel/escape/outside click

### Playback
- `Idle -> Starting` when router receives payload
- `Starting -> Playing` when outputs begin
- `Playing -> Idle` on completion
- `Playing -> Stopping` when stop hotkey used
- `Stopping -> Idle` when both outputs are halted
- `Any -> Faulted` on playback exception

## 5. Concurrency Rules

- Only one overlay instance is allowed.
- Only one active playback session is allowed in MVP.
- Duplicate send actions while `Sending` must be ignored or disabled.
- Hotkey callbacks must marshal safely to the UI thread when needed.

## 6. Recommended Sequence Diagram Summary

```txt
Hotkey -> HotkeyService -> OverlayCoordinator -> OverlayWindow
User -> OverlayViewModel -> TtsService -> AudioRouter -> Device1
                                                -> Device2
Stop Hotkey -> HotkeyService -> AudioRouter.Stop()
```
