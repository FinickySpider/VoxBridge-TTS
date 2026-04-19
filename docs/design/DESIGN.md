
# Design Document — TTS Communication Tool

## Summary

A lightweight Windows desktop text-to-speech application for a mute user who needs fast, reliable voice communication in Discord and VRChat. The app functions as a communication prosthetic: the user presses a global hotkey, an overlay appears, they type a message, press Enter, and the message is spoken to both a local monitoring output and a virtual audio cable used as microphone input by voice applications. The app lives unobtrusively in the system tray, uses Kokoro as an offline/local TTS engine, and prioritises speed, reliability, and emotional comfort over feature count.

## Goals

- Provide instant hotkey-driven overlay access for TTS input
- Deliver speech output simultaneously to a monitoring device and a virtual cable
- Achieve a faster, less clunky workflow than existing TTS alternatives
- Operate entirely offline after installation using Kokoro TTS
- Support a small saved phrase library for common responses
- Offer a dark, calm, keyboard-first UI that is comfortable for daily use
- Fail visibly — never silently — with user-friendly error messages

## Non-Goals

- Multiple TTS engines or ElevenLabs integration (post-MVP)
- Advanced phrase folders, tags, search, or transcript logging
- Message queueing or autocomplete/prediction
- VRChat OSC integration or voice cloning
- Mobile support, cloud sync, or CSS theme engine
- Cutesy visual customization beyond dark mode
- Support for non-Windows platforms

## Users

**Primary user:** A mute individual who relies on TTS for daily social communication in voice-enabled applications (Discord, VRChat).

**Environment:** Windows 10/11 desktop, keyboard-first interaction, VB-Cable or equivalent virtual audio cable installed separately, app running in background while other apps are in focus.

**User priorities (ordered):**

1. Speed of access
2. Speed of speaking
3. Reliability
4. Low stress / low cognitive load
5. Acceptable voice quality
6. Some voice choice
7. Dark and comfortable UI

## Core User Flows

### Flow A — Speak via Overlay

1. User presses global hotkey (default: Ctrl+Shift+Space).
2. A small, dark, always-on-top overlay appears centered on screen.
3. Text input is auto-focused.
4. User types a message.
5. User presses Enter.
6. Message text is validated (non-empty, within length limit).
7. TTS engine generates speech audio.
8. Audio is played simultaneously to the monitoring output and the virtual cable output.
9. Overlay clears and closes on successful send.
10. User and voice-app listeners hear the speech.

### Flow B — Cancel / Stop

1. While overlay is open: press Esc, click outside, or click X to close without speaking.
2. While speech is playing: press stop hotkey (default: Ctrl+Shift+Backspace) to immediately halt playback on both outputs.

### Flow C — Use a Quick Phrase

1. User opens phrase manager or presses a phrase hotkey.
2. Saved phrase text is sent through the same TTS → audio pipeline.
3. Speech plays to both outputs identically to free-text input.

### Flow D — First-Run Setup

1. App launches for the first time.
2. Settings window opens automatically.
3. User selects monitor output device.
4. User selects secondary output device (virtual cable).
5. User tests monitor, secondary, and both outputs.
6. User confirms overlay hotkey.
7. Settings are saved.

## Requirements

### Functional

- **FR-01** Global hotkey opens an always-on-top, borderless overlay input window
- **FR-02** Overlay auto-focuses text input; Enter sends; Esc/click-outside/X closes
- **FR-03** Re-triggering overlay hotkey while open refocuses rather than duplicating
- **FR-04** Empty or whitespace-only input is rejected without sending
- **FR-05** Speech generated via Kokoro offline TTS from validated text
- **FR-06** Audio played simultaneously to monitor output device and secondary output device
- **FR-07** Stop hotkey interrupts active playback on both outputs immediately
- **FR-08** Settings window accessible from system tray with sections: General, Hotkeys, Audio, Voice, Phrases, Appearance
- **FR-09** All settings persist in a local JSON config file across restarts
- **FR-10** Phrase CRUD: add, edit, delete, trigger playback; optional phrase hotkeys
- **FR-11** Audio test buttons in settings: Test Monitor, Test Secondary, Test Both
- **FR-12** System tray icon with menu: Open Settings, Open Phrase Manager, Exit
- **FR-13** On error (hotkey conflict, missing device, TTS failure, playback failure), show clear user-facing message and preserve input text
- **FR-14** Config schema versioned; corrupt config backed up and reset to defaults with user notification
- **FR-15** First-run flow guides device selection, output testing, and hotkey confirmation

### Non-Functional

- **NFR-01** Overlay perceived open latency under 200 ms
- **NFR-02** Minimal idle CPU and memory footprint (tray utility class)
- **NFR-03** Survives long background runtime without degradation
- **NFR-04** Dark mode only, clean, soft, readable, keyboard-first
- **NFR-05** No automatic speech unless explicitly initiated by user action
- **NFR-06** Clear service boundaries; testable modules; MVVM architecture

### Constraints

- Windows-only (x64, Windows 10/11)
- C# / .NET / WPF / NAudio
- Kokoro local TTS as sole MVP engine
- User must install VB-Cable (or equivalent) separately
- Overlay in exclusive fullscreen is best-effort only

### Out of Scope

- Multiple TTS engines, ElevenLabs, voice cloning
- Phrase folders/categories/search/transcript logging
- Message queueing, autocomplete, prediction
- VRChat OSC integration
- Themes, CSS customization, cutesy styling
- Cloud sync, mobile companion
- Multi-line input (Shift+Enter newlines deferred)

## Data Model

### AppConfig (JSON, versioned)

| Field | Type | Notes |
|-------|------|-------|
| `configVersion` | int | Schema migration key |
| `generalSettings` | GeneralSettings | Tray/startup behavior |
| `hotkeySettings` | HotkeySettings | Overlay + stop hotkeys |
| `audioSettings` | AudioSettings | Device IDs + cached names |
| `voiceSettings` | VoiceSettings | Engine + voice selection |
| `overlaySettings` | OverlaySettings | Width, height, font size |
| `phrases` | PhraseItem[] | Flat list of saved phrases |

### Key Sub-Models

- **GeneralSettings**: `minimizeToTray`, `closeToTray`, `startWithWindows`, `showNotifications`
- **HotkeySettings**: `overlayHotkey` (HotkeyBinding), `stopHotkey` (HotkeyBinding)
- **AudioSettings**: `monitorOutputDeviceId`, `secondaryOutputDeviceId`, cached display names
- **VoiceSettings**: `engineName` ("Kokoro"), `selectedVoiceId`, `selectedVoiceDisplayName`
- **OverlaySettings**: `width` (720), `height` (140), `fontSize` (18)
- **PhraseItem**: `id`, `name`, `text`, `hotkey?`, `sortOrder`, `createdUtc`, `updatedUtc`
- **HotkeyBinding**: `ctrl`, `alt`, `shift`, `win`, `key`

### Storage

- Location: `%AppData%\TtsCommunicationTool\config.json`
- Logs: `%AppData%\TtsCommunicationTool\logs\app.log`
- Corrupt config recovery: backup → reset defaults → notify user

## APIs

### Internal Service Interfaces

| Interface | Responsibility |
|-----------|---------------|
| `IConfigService` | Load/save/default config, schema versioning |
| `IHotkeyService` | Register/unregister global hotkeys, emit events |
| `IAudioDeviceService` | Enumerate output devices, validate saved IDs |
| `IAudioRouterService` | Play to both outputs, stop both, test playback |
| `ITtsService` | Generate audio from text, enumerate voices |
| `IPhraseService` | CRUD phrases, validation |
| `INotificationService` | User-facing toasts, warnings, errors |
| `IOverlayCoordinator` | Single-instance overlay show/focus/close |
| `ILoggingService` | File-based diagnostic logging |

### External Dependencies

- **Kokoro TTS**: Local offline engine, bundled voice model(s), no network calls
- **NAudio**: Audio device enumeration, dual-output playback, format conversion
- **Windows API**: Global hotkey registration via native interop

## Architecture

### Layered MVVM

```
┌─────────────────────────────┐
│  App Shell (Bootstrap/Tray) │
├─────────────────────────────┤
│  UI Layer (WPF Views + VMs) │
├─────────────────────────────┤
│  Core (Models/Interfaces)   │
├─────────────────────────────┤
│  Infrastructure (Services)  │
└─────────────────────────────┘
```

### Solution Structure

```
TtsCommunicationTool/
├─ src/
│  ├─ TtsCommunicationTool.App/        # Bootstrap, tray, resources
│  ├─ TtsCommunicationTool.UI/         # Views, ViewModels, commands
│  ├─ TtsCommunicationTool.Core/       # Models, interfaces, validation, state
│  ├─ TtsCommunicationTool.Infrastructure/  # Config, hotkeys, audio, TTS, logging
│  └─ TtsCommunicationTool.Tests/      # Unit + integration tests
└─ docs/
```

### Application State Machine

- **Starting** → **Idle** → **OverlayOpen** → **GeneratingSpeech** → **PlayingSpeech** → **Idle**
- **Any** → **RecoverableError** → **Idle**
- **Any** → **ShuttingDown**

### Audio Pipeline

1. TTS engine generates audio
2. Audio normalized to consistent PCM format
3. Same buffer fanned to two NAudio output streams (monitor + secondary)
4. Both streams started near-simultaneously
5. Stop command halts both streams and releases resources

## Security and Privacy

- All data is local only. No network calls, no telemetry, no cloud.
- Config file contains no sensitive data (device IDs and hotkeys only).
- No voice data leaves the machine.
- No automatic speech — all speech requires explicit user action.
- Logging captures diagnostics only, not user message content.

## Rollout Plan

### Phase 1 — Foundation & Core Loop (MVP)

Establish project scaffold, config system, tray shell, overlay input, global hotkeys, Kokoro TTS integration, dual audio routing, phrase system, settings UI, error handling, and first-run setup. Delivers a daily-usable communication tool.

### Phase 2 — Usability Hardening

Overlay position adjustment, font/UI size controls, hotkey conflict detection improvements, per-output volume, recent phrases/messages, import/export phrases, better settings organization, improved onboarding.

### Phase 3 — Comfort & Expression

Theme presets, visual polish, phrase categories/folders/search/favorites, voice preset switching, speed/pitch controls, optional premium voice path, cached phrase audio.

### Phase 4 — Advanced & Deferred

Message queue system, VRChat OSC integration, multiple TTS engines, voice cloning, cloud sync, mobile companion, phrase prediction/autocomplete.

## Open Questions

- **OQ-01** Kokoro integration packaging: what is the exact bundling strategy for the model and runtime? Needs early spike.
- **OQ-02** Optimal PCM format (16-bit vs 32-bit float) for the internal audio pipeline — depends on Kokoro output format and NAudio performance.
- **OQ-03** Should the overlay position be persisted and adjustable in MVP, or deferred to Phase 2?
- **OQ-04** Exact maximum character limit for TTS input — 500 vs 1000 characters. Depends on Kokoro latency testing.
- **OQ-05** Single-project vs multi-project solution structure — multi-project is architecturally cleaner but single-project may be faster to bootstrap.
