# Features

## Core Features

### Global Hotkey Overlay
Press a configurable global hotkey (default: Ctrl+Shift+Space) from anywhere to open a dark, always-on-top input overlay. The overlay auto-focuses the text input so you can start typing immediately.

### Instant Speech
Press Enter to send. The Kokoro offline TTS engine generates speech audio and plays it to both your monitoring output and the virtual cable simultaneously.

### Dual Audio Routing
Audio is played simultaneously to two outputs:
- **Monitor Output** — Your headphones or speakers (you hear the speech)
- **Secondary Output** — A virtual audio cable (voice apps hear the speech)

### Stop Playback
Press Ctrl+Shift+Backspace to immediately halt any speech that's playing on both outputs.

### System Tray
The app lives in the system tray. Right-click the tray icon for:
- **Open Settings** — Configure all options
- **Open Phrase Manager** — Manage saved phrases
- **Reset Theme to Default** — Revert to the Default Dark theme
- **Exit** — Close the app

### Offline TTS
Kokoro runs entirely on-device. No internet connection or API subscriptions are required for the default voice.

## Advanced Features

### ElevenLabs Integration (Optional)
Connect your ElevenLabs account for premium AI voices. The API key is encrypted with DPAPI and stored securely.

### Quick Phrases
Save frequently-used messages as phrases. Trigger them instantly from the Phrase Manager or assign hotkeys to individual phrases.

### Phrase Categories & Favorites
Organize phrases into categories, mark favorites, and pin important phrases to the top of the list.

### Phrase Editor
A dedicated window for creating and editing phrases with per-phrase voice overrides (engine, voice, pitch) and TTS preview.

### Text Replacements
Define automatic text substitution rules that run before TTS synthesis. Supports case-sensitive matching, whole-word matching, and priority ordering.

### Theme System
Customize the entire visual appearance with built-in themes (Default Dark, Default Light, Midnight Purple, Nord) or create your own. Full colour editor with WCAG 2.1 AA contrast checking.

### Per-Output Volume
Set independent volume levels for the monitor output and secondary output (0–100%).

### Global Pitch Control
Adjust the pitch of all TTS output (0.5x–2.0x). Changes apply live without saving.

### Trailing Silence Trimming
Automatically remove trailing silence from generated audio to reduce delays between messages.

### Transcript Logging
Optionally log all spoken text to a timestamped transcript file at `%AppData%\TtsCommunicationTool\transcript.txt`.

### Diagnostic Logging
Structured JSONL session logs for debugging. Configurable verbosity levels (INFO, DEBUG, TRACE) with optional request ID correlation.

### Recent Messages
The last 20 sent messages are tracked in memory (not persisted). Duplicate consecutive entries are collapsed.

### Repeat Last / Resend
Resend the last spoken message via a configurable hotkey or the overlay's resend button.

### Overlay Position Persistence
The overlay remembers its last position on screen across sessions.

### Playback Timer
Shows a live countdown of remaining playback time in the overlay while speech is playing.

### Draft Preservation
Optionally preserve unsent overlay text when the overlay closes without sending.

### Character Limit
Optionally enforce a maximum character limit (default: 500) on overlay input.

### Hotkey Conflict Detection
The settings UI warns you if you try to assign the same hotkey to multiple actions.

### Phrase Import/Export
Import and export phrases as JSON files for backup or sharing.

### Theme Import/Export
Import and export themes as `.ttstheme` JSON files.

### Splash Screen
An optional splash screen shown on startup (can be disabled in settings).

### First-Run Setup Wizard
The Settings window opens automatically on first launch, guiding you through device selection and testing.
