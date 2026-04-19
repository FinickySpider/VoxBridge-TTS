# TTS Communication Tool

A lightweight Windows-only desktop text-to-speech application for a mute user who needs fast, reliable voice communication in Discord and VRChat.

The app works like a communication prosthetic: press a global hotkey, type a message, press Enter — and the message is spoken to both your local monitoring output and a virtual audio cable used as microphone input by voice applications.

---

## Features

- **Global hotkey overlay** — press Ctrl+Shift+Space to open a dark, always-on-top input overlay from anywhere
- **Instant speech** — press Enter to send; Kokoro offline TTS generates and plays speech in seconds
- **Dual audio routing** — simultaneous output to a monitoring device and VB-Cable (or any virtual audio cable)
- **Stop hotkey** — Ctrl+Shift+Backspace immediately halts playback
- **Quick phrases** — save common responses and trigger them instantly
- **Settings window** — configure hotkeys, audio devices, voice, overlay size, and appearance
- **System tray** — lives unobtrusively in the background until needed
- **Offline/local TTS** — Kokoro runs entirely on-device, no internet or subscriptions required

---

## Requirements

- Windows 10 or 11 (x64)
- [VB-Cable](https://vb-audio.com/Cable/) or another virtual audio cable (installed separately)
- .NET 8 runtime (bundled in release builds)

---

## Getting Started

1. Install [VB-Audio Virtual Cable](https://vb-audio.com/Cable/).
2. Launch **TTS Communication Tool** — it will appear in your system tray.
3. On first run, the Settings window opens automatically:
   - Select your **Monitor Output** device (headphones/speakers).
   - Select your **Secondary Output** device (CABLE Input / VB-Audio).
   - Test each output with the test buttons.
   - Confirm your overlay hotkey (default: Ctrl+Shift+Space).
   - Save settings.
4. In Discord or VRChat, set your microphone input to **CABLE Output (VB-Audio Virtual Cable)**.
5. Press Ctrl+Shift+Space, type, press Enter — done.

---

## Default Hotkeys

| Action | Default |
|--------|---------|
| Open overlay | Ctrl+Shift+Space |
| Stop playback | Ctrl+Shift+Backspace |

Both are configurable in Settings → Hotkeys.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Language | C# / .NET 8 |
| UI | WPF (MVVM) |
| Audio | NAudio (WASAPI) |
| TTS | Kokoro (offline) |
| Config | JSON (`%AppData%\TtsCommunicationTool\config.json`) |
| Hotkeys | Windows `RegisterHotKey` (P/Invoke) |

---

## Project Structure

```
src/
├─ TtsCommunicationTool.App/           # Bootstrap, tray, resources
├─ TtsCommunicationTool.UI/            # Views, ViewModels, commands
├─ TtsCommunicationTool.Core/          # Models, interfaces, validation, state
├─ TtsCommunicationTool.Infrastructure/ # Config, hotkeys, audio, TTS, logging
└─ TtsCommunicationTool.Tests/         # Unit + integration tests
docs/
├─ design/DESIGN.md                    # Authoritative design document
├─ index/MASTER_INDEX.md               # Project tracking index
├─ phases/                             # Phase files
├─ sprints/                            # Sprint files
├─ features/                           # Feature specs
├─ decisions/                          # Architecture decision records (ADRs)
└─ _system/                            # Agent operating rules
```

---

## Documentation

| Document | Purpose |
|----------|---------|
| [docs/design/DESIGN.md](docs/design/DESIGN.md) | Full product and architecture design |
| [docs/index/MASTER_INDEX.md](docs/index/MASTER_INDEX.md) | Active phase, sprint, and work item index |
| [docs/index/ROADMAP.md](docs/index/ROADMAP.md) | Milestone roadmap |
| [docs/decisions/DECISION_LOG.md](docs/decisions/DECISION_LOG.md) | Architecture decision records |

---

## Logs and Config

- Config: `%AppData%\TtsCommunicationTool\config.json`
- Logs: `%AppData%\TtsCommunicationTool\logs\app.log`

If the config file is corrupted, the app backs it up as `config.invalid.backup.json` and resets to defaults.

---

## License

Private project. Not for public distribution.