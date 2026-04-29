# Changelog

All notable changes to TTS Communication Tool are documented here.

---

## [v0.9.1] — 2026-04-30

### Bug Fixes

- **Kokoro voice lost when switching to ElevenLabs** — Selecting ElevenLabs in the Voice settings tab no longer clears the saved Kokoro voice. The previously selected Kokoro voice is now correctly preserved and restored when saving settings regardless of which engine tab is active.
- **Settings window size not persisting** — The settings window no longer slowly shrinks each session. The `SizeChanged` handler now reads `this.Width`/`this.Height` (which include the window chrome) instead of the client-area size, and a `Loaded` gate prevents spurious writes during initial layout.
- **Favorites toggle unreadable when active** — The "⭐ Favorites" toggle button in the Phrases settings tab now uses a custom `ControlTemplate` that prevents the Windows system theme from painting a blue highlight over it. The checked state now correctly displays an amber (`#F9E2AF`) label on a muted overlay background.
- **Case-insensitive phrase categories** — Category names are now normalised to Title Case on save (`"fun"` → `"Fun"`). The category dropdown deduplicates case-insensitively, so `"Fun"` and `"fun"` will never appear as separate entries in the same list.

### New Features

- **Configurable message-length limit** — A new "Message Length" section in the General settings tab lets you enable or disable the character limit on the overlay input field and set a custom maximum (default: 500). When the limit is disabled the counter shows only the current character count instead of `0/500`.

---

## [v0.9.0] — 2026-04-29

### New Features

#### Overlay & Sending
- **Keep unsent text** — New option in General settings to preserve text typed in the overlay when it closes without sending. The draft is restored the next time the overlay opens.
- **Error toasts for TTS failures** — When TTS generation fails (e.g. invalid ElevenLabs API key, network error), a notification popup is shown rather than silently failing.

#### Hotkeys
- **Resend global hotkey** — A new configurable hotkey in the Hotkeys settings tab re-speaks the last sent message. Works for both typed text and phrase playbacks. Automatically closes the overlay if it is open, and routes through the full text-replacement pipeline before synthesizing.
- **Settings hotkey** — Optional global hotkey to open the Settings window from anywhere.

#### Voice Engine
- **ElevenLabs support** — Full ElevenLabs TTS integration: API key entry, voice list fetch, voice selection, and model ID override. Switchable per-session. Settings persisted separately from Kokoro settings.
- **Per-engine voice memory** — When switching from ElevenLabs back to Kokoro, the previously selected Kokoro voice is automatically restored rather than defaulting to the default voice.
- **Voice list bug fix** — Kokoro voice list now always populates correctly even when ElevenLabs is the saved active engine. Previously the list appeared empty until settings were closed and reopened.

#### Phrases
- **Phrase categories** — Phrases can be assigned a category. The phrases panel shows the category in a distinct purple (`#CBA6F7`) label alongside each phrase name.
- **Category filter dropdown** — Filter the phrase list by category via a dropdown above the list. Properly styled to match the dark theme.
- **Favorites & pinning** — Mark phrases as favorites (gold star) or pinned (pushpin). Filter by favorites only using the "⭐ Favorites" toggle. Pinned phrases sort to the top.
- **Phrase search** — Live text filter across phrase name and text.
- **Edit / Update existing phrases** — Select any phrase, edit its name, text, or category in the input fields, then click Update. Cache is invalidated and regenerated automatically when the text changes.
- **Phrase import / export** — Import and export phrase libraries as JSON. Export includes all fields: name, text, category, favorites, pin state, and hotkey binding.

#### Text Replacements
- **Export / Import text replacement rules** — Export all rules to a `.json` file and re-import them on any machine. Button row added to Replacements tab with status feedback.

#### Settings Window
- **Unsaved changes warning** — Clicking Cancel when there are unsaved changes now shows a confirmation dialog before closing.
- **Remember window size** — The Settings window restores its last resized dimensions on next open. Dimensions are saved silently on close and do not trigger the unsaved changes warning.
- **Version number in title bar** — Settings window title now shows `v0.9.0`.

### Bug Fixes
- Fixed Kokoro voice dropdown appearing empty when ElevenLabs was the last saved engine — voice list now uses the Kokoro service directly instead of routing through the engine-aware TtsRouter.
- Fixed resend hotkey passing the Kokoro voice ID to ElevenLabs synthesis — each engine now uses its own configured voice ID when resending.
- Fixed category ComboBox in phrase filter using raw inline properties instead of the shared SettingsComboBox style, causing unreadable text.
- Fixed favorites ToggleButton hover state overriding text foreground to an unreadable color.

### Visual / UX
- Category text in the phrase list uses accent purple (`#CBA6F7`) for visual distinction.
- Phrase list item hover and selection foreground is explicitly set to prevent theme inheritance issues.

---

## [v0.8.x] — Phase 3 Foundation (Sprints 6–8)

### Sprint 8 — ElevenLabs Voice Path (FEAT-033)
- Added ElevenLabs TTS as an optional voice engine alongside Kokoro
- Engine switcher (radio buttons) in Voice settings tab
- API key, model ID, and voice selection fields for ElevenLabs
- "Fetch Voices" button to retrieve voice list from the ElevenLabs API
- Voice test button works for both engines
- Engine and voice preferences persisted separately per engine

### Sprint 7 — Phrase Organization (FEAT-030)
- Phrase categories with free-text input and filter dropdown
- Favorites flag with star indicator and filter toggle
- Pinned flag with pin indicator; pinned phrases sort first
- Live search bar filtering across phrase name and body text
- Phrase import/export updated to include all new fields

### Sprint 6 — Polish, Feedback, Repeat, Transcript (FEAT-028, FEAT-029, FEAT-031, FEAT-032)
- Overlay fade-in animation on open
- Status bar shows "Generating…" → "Speaking…" → "✓ Sent" flow with color coding
- "↩ Resend" button in overlay re-sends the last spoken message
- `RecentMessagesState` tracks last N messages across overlay and phrase hotkeys
- Transcript logging to `%AppData%\TtsCommunicationTool\transcript.txt` (opt-in)
- "Open Transcript" button in General settings
- Visual polish: smooth transitions, consistent spacing, dark-theme refinements

---

## [v0.7.x] — Phase 2: Usability Hardening (Sprints 4–5)

### Sprint 5 — Workflow (FEAT-025, FEAT-026, FEAT-027)
- Recent messages panel in overlay with "↩" one-tap resend buttons
- Phrase import / export (JSON) with progress dialog for cache pre-generation
- Settings tab reordering and organization improvements

### Sprint 4 — Polish (FEAT-021–FEAT-024)
- Overlay status bar with send/error/playing state display
- Overlay window position persistence (saved on close, restored on open)
- Per-output volume sliders for Monitor and Secondary audio devices
- Real-time hotkey conflict detection with inline validation messages

---

## [v0.6.x] — Phase 1: Foundation & Core Loop (Sprints 1–3)

### Sprint 3 — Settings (FEAT-013–FEAT-020)
- Full Settings window with tabs: General, Hotkeys, Audio, Voice, Appearance, Phrases
- General: start with Windows, notifications, splash screen toggles
- Hotkeys: configurable overlay toggle, stop playback, and optional Settings hotkey
- Audio: device selection for Monitor and Secondary outputs; Test playback buttons
- Voice: Kokoro voice selection dropdown with preview
- Appearance: overlay width, height, and font size
- Quick Phrases: add, delete, play, and assign hotkeys to saved phrases
- First-run setup wizard for initial device and voice configuration
- Error handling: all failures surfaced in UI status areas and logged to file

### Sprint 2 — TTS & Audio (FEAT-007–FEAT-012)
- Kokoro TTS integration (local/offline, bundled voice models via KokoroSharp)
- Text validation (max length, whitespace sanitization, character count)
- WASAPI audio device enumeration via NAudio
- Dual-output audio routing: simultaneous Monitor + Secondary playback
- Stop playback hotkey (default: F4)
- Overlay send flow: validate → synthesize → play → auto-clear input

### Sprint 1 — Core Skeleton (FEAT-001–FEAT-006)
- Solution structure: App / UI / Core / Infrastructure / Tests projects
- JSON config persistence at `%AppData%\TtsCommunicationTool\config.json`
- File-based logging via Serilog to `%AppData%\TtsCommunicationTool\logs\app.log`
- System tray icon with Show/Settings/Exit menu
- Overlay window (WPF, always-on-top, dark theme)
- Global hotkey registration via Windows `RegisterHotKey` P/Invoke (default: F2)
