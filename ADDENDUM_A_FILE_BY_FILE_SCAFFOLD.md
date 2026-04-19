# ADDENDUM A
## File-by-File Scaffold

This addendum defines the recommended file and class structure for the MVP implementation.

## Solution Layout

```txt
TtsCommunicationTool/
├─ TtsCommunicationTool.sln
├─ src/
│  ├─ TtsCommunicationTool.App/
│  │  ├─ App.xaml
│  │  ├─ App.xaml.cs
│  │  ├─ Bootstrap/
│  │  │  ├─ ServiceRegistration.cs
│  │  │  └─ AppStartup.cs
│  │  ├─ Tray/
│  │  │  ├─ TrayIconManager.cs
│  │  │  └─ TrayMenuBuilder.cs
│  │  └─ Resources/
│  │     ├─ Colors.xaml
│  │     ├─ Typography.xaml
│  │     └─ Controls.xaml
│  ├─ TtsCommunicationTool.UI/
│  │  ├─ Views/
│  │  │  ├─ OverlayWindow.xaml
│  │  │  ├─ OverlayWindow.xaml.cs
│  │  │  ├─ SettingsWindow.xaml
│  │  │  ├─ SettingsWindow.xaml.cs
│  │  │  ├─ PhraseEditorDialog.xaml
│  │  │  └─ PhraseEditorDialog.xaml.cs
│  │  ├─ ViewModels/
│  │  │  ├─ BaseViewModel.cs
│  │  │  ├─ OverlayViewModel.cs
│  │  │  ├─ SettingsViewModel.cs
│  │  │  ├─ GeneralSettingsViewModel.cs
│  │  │  ├─ HotkeySettingsViewModel.cs
│  │  │  ├─ AudioSettingsViewModel.cs
│  │  │  ├─ VoiceSettingsViewModel.cs
│  │  │  ├─ AppearanceSettingsViewModel.cs
│  │  │  ├─ PhraseListViewModel.cs
│  │  │  └─ PhraseEditorViewModel.cs
│  │  ├─ Commands/
│  │  │  ├─ RelayCommand.cs
│  │  │  └─ AsyncRelayCommand.cs
│  │  ├─ Behaviors/
│  │  │  └─ WindowFocusBehavior.cs
│  │  └─ Converters/
│  │     └─ BoolToVisibilityConverter.cs
│  ├─ TtsCommunicationTool.Core/
│  │  ├─ Models/
│  │  │  ├─ AppConfig.cs
│  │  │  ├─ GeneralSettings.cs
│  │  │  ├─ HotkeySettings.cs
│  │  │  ├─ AudioSettings.cs
│  │  │  ├─ VoiceSettings.cs
│  │  │  ├─ OverlaySettings.cs
│  │  │  ├─ PhraseItem.cs
│  │  │  ├─ HotkeyBinding.cs
│  │  │  ├─ AudioDeviceInfo.cs
│  │  │  ├─ VoiceInfo.cs
│  │  │  ├─ TtsRequest.cs
│  │  │  ├─ TtsResult.cs
│  │  │  ├─ PlaybackRequest.cs
│  │  │  └─ OperationResult.cs
│  │  ├─ Interfaces/
│  │  │  ├─ IConfigService.cs
│  │  │  ├─ IHotkeyService.cs
│  │  │  ├─ IAudioDeviceService.cs
│  │  │  ├─ IAudioRouterService.cs
│  │  │  ├─ ITtsService.cs
│  │  │  ├─ IPhraseService.cs
│  │  │  ├─ INotificationService.cs
│  │  │  ├─ ILoggingService.cs
│  │  │  └─ IOverlayCoordinator.cs
│  │  ├─ Validation/
│  │  │  ├─ TextValidation.cs
│  │  │  ├─ PhraseValidation.cs
│  │  │  └─ HotkeyValidation.cs
│  │  └─ State/
│  │     ├─ AppRuntimeState.cs
│  │     ├─ OverlayState.cs
│  │     └─ PlaybackState.cs
│  ├─ TtsCommunicationTool.Infrastructure/
│  │  ├─ Config/
│  │  │  └─ JsonConfigService.cs
│  │  ├─ Hotkeys/
│  │  │  ├─ WindowsHotkeyService.cs
│  │  │  └─ NativeMethods.cs
│  │  ├─ Audio/
│  │  │  ├─ NAudioDeviceService.cs
│  │  │  ├─ NAudioRouterService.cs
│  │  │  ├─ AudioFormatNormalizer.cs
│  │  │  └─ PlaybackSession.cs
│  │  ├─ Tts/
│  │  │  ├─ KokoroTtsService.cs
│  │  │  ├─ KokoroProcessRunner.cs
│  │  │  └─ KokoroVoiceCatalog.cs
│  │  ├─ Notifications/
│  │  │  └─ NotificationService.cs
│  │  ├─ Logging/
│  │  │  └─ FileLoggingService.cs
│  │  └─ Overlay/
│  │     └─ OverlayCoordinator.cs
│  └─ TtsCommunicationTool.Tests/
│     ├─ Unit/
│     │  ├─ ConfigServiceTests.cs
│     │  ├─ PhraseValidationTests.cs
│     │  ├─ HotkeyValidationTests.cs
│     │  └─ OverlayViewModelTests.cs
│     └─ Integration/
│        ├─ AudioRoutingTests.cs
│        ├─ KokoroIntegrationTests.cs
│        └─ HotkeyRegistrationTests.cs
└─ docs/
   ├─ TTS_COMMUNICATION_TOOL_COMPREHENSIVE_DESIGN_DOCUMENT.md
   └─ addenda/
      ├─ ADDENDUM_A_FILE_BY_FILE_SCAFFOLD.md
      ├─ ADDENDUM_B_JSON_SCHEMA_AND_SAMPLE_CONFIG.md
      ├─ ADDENDUM_C_UI_LAYOUT_AND_WINDOW_SPEC.md
      ├─ ADDENDUM_D_COMMAND_EVENT_FLOW_AND_STATE_MAP.md
      └─ ADDENDUM_E_IMPLEMENTATION_CHECKLIST_AND_ACCEPTANCE_MATRIX.md
```

## Class Responsibility Summary

- `AppStartup` initializes config, logging, services, tray, and hotkeys.
- `TrayIconManager` owns the tray icon lifecycle and menu commands.
- `OverlayCoordinator` ensures a single overlay instance and handles show, focus, and close behavior.
- `OverlayViewModel` owns the current input text, send command, cancel command, state labels, and validation.
- `SettingsViewModel` composes tab-specific settings view models and coordinates save and reload.
- `WindowsHotkeyService` wraps native global hotkey registration and raises app events.
- `NAudioDeviceService` enumerates playback endpoints and validates saved device IDs.
- `NAudioRouterService` plays the same audio payload to both selected devices and stops both together.
- `KokoroTtsService` validates text, calls the Kokoro runtime, and returns audio suitable for playback.
- `JsonConfigService` loads and saves the JSON config under the per-user app data folder.
- `NotificationService` shows user-facing toasts, warnings, and recoverable errors.
- `FileLoggingService` writes internal diagnostics to disk.

## Single-Project Alternative

If the build needs to move faster, a single WPF project is acceptable, but keep the same logical folders:
- `Models`
- `Services`
- `Infrastructure`
- `ViewModels`
- `Views`
- `Config`
- `Audio`
- `Tts`
- `Hotkeys`
- `Logging`

The architecture matters more than the exact number of projects.
