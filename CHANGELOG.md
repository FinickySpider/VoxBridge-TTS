# Changelog

All notable changes to TTS Communication Tool are documented here.

---









## [v0.12.0] -- 2026-05-03

### Features

- **ElevenLabs subscription credits display**: The Voice tab now shows characters remaining and total quota (e.g. `5,000 / 100,000 characters remaining`) fetched live from the ElevenLabs `/v1/user/subscription` endpoint.
- **Cross-session character usage tracking**: Every successful ElevenLabs synthesis increments a persistent `TotalCharactersUsed` counter written to config. Displayed below the credits line as `X chars sent lifetime`.
- **Auto-fetch voices on API key entry**: Typing an ElevenLabs API key triggers a debounced (800 ms) automatic voice + subscription fetch — no need to click "Fetch Voices" manually. Selecting the ElevenLabs engine also triggers an auto-fetch if a key is already stored.
- **Custom voice highlighting**: Cloned and AI-generated ElevenLabs voices are tagged `IsCustomVoice` and shown in **red** in the Voice dropdown. A persistent info panel below the dropdown explains that red voices require a Creator plan or higher.
- **Model dropdown**: The ElevenLabs Model field is now a dropdown instead of a free-text box. Options: `Flash v2.5`, `Turbo v2.5`, `v3 Multilingual`, `Multilingual v2` with pricing info in each entry. Defaults to `eleven_multilingual_v2`.
- **Overlay credit estimate**: While typing in the overlay with ElevenLabs active, a live `≈N` character estimate is displayed in the status bar (e.g. `≈107`) so you can judge quota usage before sending.

### Improvements

- `ElevenLabsSettings` model extended with `SubscriptionCharacterCount`, `SubscriptionCharacterLimit`, and `TotalCharactersUsed` fields (all persisted in config).
- `VoiceInfo` model extended with `IsCustomVoice` bool, set from the `category` field in the ElevenLabs voices API response.
- `ElevenLabsTtsService` now exposes `FetchUserSubscriptionAsync()` and increments `TotalCharactersUsed` after every successful synthesis.

---
## [v0.11.3] -- 2026-05-03

### Bug Fixes

- **ElevenLabs voice routing**: Overlay and phrase playback no longer pass the Kokoro voice ID to ElevenLabs. Added `ResolveVoiceId()` helper in `OverlayViewModel` — returns empty string for ElevenLabs (service uses its own configured voice) and the Kokoro voice ID otherwise. Same fix applied to `PhraseCacheService.GenerateCacheAsync`.
- **Phrase cache with ElevenLabs**: Cache generation now correctly routes synthesis to ElevenLabs when that engine is active, instead of failing with an unknown Kokoro voice ID.
- **Text replacement grid deselect**: Clicking an empty area in the Replacements DataGrid now clears the selection (same behavior as the Phrases ListView).

### Improvements

- **Column reordering disabled**: Phrases ListView columns can no longer be dragged over each other; `GridView.AllowsColumnReorder="False"` prevents accidental column swaps.
- **Window title shows version**: The Settings window title bar and taskbar button now dynamically display the current assembly version (e.g. `Settings — VoxBridge v0.11.3`) instead of a hardcoded stale version string.

---
## [v0.11.2] -- 2026-05-03

### Bug Fixes

- **Pinned phrases always at top**: Every sort operation now inserts `IsPinned Descending` as the primary `SortDescription` before the user-selected column. Pinned items appear first regardless of how the list is further sorted. Initial load also applies this order.
- **ListView selection thick-bar highlight**: Added a full `ControlTemplate` override to `ListViewItem` using `GridViewRowPresenter`. Removes the WPF Aero/theme glass chrome (the horizontal bar at the top) and replaces it with a flat, flat-colour selection.
- **Deselecting a phrase clears edit fields**: `PhraseListViewModel.SelectedPhrase` setter now clears `EditName`, `EditText`, `EditCategory` when set to `null`. Code-behind `PreviewMouseDown` handler deselects the item when clicking blank space in the list.
- **ElevenLabs 404 diagnostic**: Error message now includes the `voice_id` sent in the request and attempts to parse the JSON error detail body from ElevenLabs for a more human-readable message (e.g. "voice_not_found (voice_id=\"…\")").

### Improvements

- **Column resize (Phrases)**: Added `PART_HeaderGripper` `Thumb` to the `GridViewColumnHeader` `ControlTemplate`. Drag the right edge of any column header to resize, matching Windows File Explorer behaviour.
- **Pin and Star columns swapped**: 📌 Pin is now col 0; ★ Favorite is col 1. Column index mapping in `PhraseSort_Click` updated accordingly.
- **Import / Export moved to bottom bar**: Import and Export buttons are now in the global settings bottom bar, visible only when the Replacements or Phrases tab is active. Removed from inline tab controls.

---
## [v0.11.1] -- 2026-05-03

### Bug Fixes

- **Live overlay preview**: Appearance tab sliders (transparency, font) now take effect immediately on the overlay even when it is opened *after* the change, before hitting Save. `OverlayCoordinator` now stores the last previewed opacity and font family; `ShowOverlay()` uses those values in preference to the saved config.
- **Voice engine test routing**: Switching from ElevenLabs to Kokoro in the Voice tab and clicking "Test Voice" now routes to the correct engine. `VoiceSettingsViewModel.ApplyToConfig()` was missing a write to `VoiceSettings.Engine`; `TtsRouter` therefore still routed test calls to ElevenLabs.

### Improvements

- **Phrases tab**: Replaced the misaligned ListBox + manually-sized header `Grid` with a native WPF `ListView + GridView`. Columns are now pixel-perfect, drag-to-resize, and auto-fill the Name column to available width. Separate ★ (sortable, col 0) and 📌 (col 1) columns.
- **Font dropdown preview**: Increased preview font size from 13 → 17 pt so individual typefaces are clearly legible in the Appearance dropdown.
- **Replacements ELI5**: Added "Settings used in this example" context block showing which rule flags are active in the omw/w example, making the prerequisite assumptions explicit.

---
## [v0.11.0] -- 2026-05-03

### Features

- **Live font preview**: Changing the font in Appearance settings now immediately updates the open overlay without requiring Save. Cancelling reverts the overlay to the saved font, consistent with how opacity preview already worked.
- **Font dropdown self-preview**: Each font name in the Appearance → Font Type dropdown is now rendered in its own typeface so you can visually browse fonts at a glance.
- **Phrases list column headers with sort**: The Phrases list now shows proper aligned column headers for ★/📌, Name & Text, Category, and Hotkey. Clicking Name or Category sorts ascending/descending. Clicking ★/📌 sorts by favorite status.
- **Phrases list dedicated Category column**: Category is now its own fixed-width column in the list instead of being shown inline after the phrase name.
- **Replacements "How it works" reference panel**: Completely rewritten as a two-column scrollable panel. Left column covers ordering rules with color-coded ELI5 examples. Right column is a quick-reference settings key (Enable, On/Off per row, Trigger, Replacement, Case, Word — all with plain-language descriptions and inline code examples).

### Improvements

- **DataGrid filled-row styling**: Rows that already have Trigger/Replacement text now display as plain blending text (no visible box). Only empty/new rows show the input-styled border, making it visually clear which rows are empty vs. configured. The full edit styling still appears when a row is in edit mode.
- **Phrases tab button layout**: Fav and Pin action buttons commented out (code preserved, not deleted) to unclutter the button row. Import and Export moved to their own second row on both the Phrases tab and the Replacements tab.

### Improvements

- (add improvements here)

---
## [v0.10.2] -- 2026-05-03

### Bug Fixes

- **Context menu white background finally gone** — Added a full `ControlTemplate` override on the `ContextMenu` element itself (inside `ContextMenu.Resources`). WPF's default popup chrome renders its own white `SystemDropShadowChrome` background regardless of the `Background` property setter; this replaces the entire visual tree with a dark `#2B2B3D` border box with no chrome.
- **Replacement rows now obviously editable** — Replaced both `DataGridTextColumn` text columns (Trigger, Replacement) with `DataGridTemplateColumn`. Display mode now shows text inside a visible input-styled `Border` (`#252540` background, `#45475A` 1px border, rounded corners, IBeam cursor, tooltip). Users can now see these are text fields before clicking.
- **Single-click to begin editing** — `CurrentCellChanged` handler now calls `BeginEdit()` immediately when a template column cell is selected, removing the confusing double-click requirement. Checkbox columns (On, Case, Word) are unaffected.
- **Edit TextBox auto-focus and select-all** — `PreparingCellForEdit` handler focuses the editing `TextBox` and selects all text so the user can type a replacement immediately without having to click inside the field first.

---
## [v0.10.1] -- 2026-05-03

### Bug Fixes

- **ComboBox popup now dark** — Font Type dropdown in Appearance settings now correctly uses the full dark ControlTemplate (`SettingsComboBox` style). The popup background was previously white due to missing template.
- **Context menu separator now dark** — Separator between "Toggle Pin" and "Delete" in the phrase list context menu now renders as a `#45475A` line via a proper `ControlTemplate` override. The previous property-setter approach was ignored by WPF.

### Improvements

- **Live transparency preview** — The transparency slider in Appearance settings now instantly updates the overlay's opacity while dragging. Cancelled changes revert the overlay automatically. Works only while the overlay is open.
- **Favorites toggle more visible when active** — The "★ Favorites" filter button now turns solid blue (`#89B4FA` background, dark text) when checked, replacing the subtle grey that was easy to miss.
- **Replacement rules drag drop indicator** — A bright blue horizontal line with a pointer triangle now shows exactly where a dragged row will land. Uses WPF `AdornerLayer` so there is no layout disruption.
- **Empty replacement rows more distinct** — Rows with no trigger text now use a darker blue-tinted background (`#1A2540`, 80% opacity) that is clearly different from regular rows.
- **DataGrid cell focus highlighted** — Selected cells now show a bottom border accent (`#89B4FA`); actively editing cells get a full blue border, making it obvious which cell is active.
- **Instructions rewritten for clarity** — The "How replacements work" panel was rewritten in plain language with explicit ✓ Correct / ✗ Wrong examples styled in green/red, removing the ambiguous inline-Run formatting that caused words to run together.

---
## [v0.10.0] -- 2026-05-03

### Features

- **Overlay transparency slider** — New slider in Appearance settings (0.20–0.95) controls the background opacity of the overlay window. Default is 0.93 (matching prior behavior). Applies next time the overlay opens.
- **Overlay font type selector** — New dropdown in Appearance settings populated with all installed system fonts. Changes the font used in the overlay text input. Default is Segoe UI.
- **Drag to reorder text replacements** — Rows in the Replacements DataGrid can now be dragged to new positions without needing the ▲/▼ buttons. Standard WPF drag threshold prevents accidental drags during normal clicks.
- **Toast notification redesign** — Toasts are now taller, wider (380px), and visually distinct per type via a colored left-accent bar and matching tinted background: blue (Info), green (Success), orange (Warning), red (Error). Warning/Error types now show a bold type label above the message body. New `Success` toast type added.
- **`ShowSuccess()` notification method** — `INotificationService` gains a `ShowSuccess()` method for confirming successful operations.

### Bug Fixes

- **Context menu dark theme fix** — Right-click menus in the Phrases list now show proper dark styling. WPF's default MenuItem ControlTemplate was ignoring the Background setter; a full ControlTemplate override is now applied in ContextMenu.Resources.

### Improvements

- **Status bar warning hold (1.5s)** — "Blocked send" warning messages (e.g. "Wait until speaking finishes") are now held for 1.5 seconds before the countdown ticker resumes, so the user can actually read them instead of them being overwritten in 50ms.
- **Empty-row highlight in replacements** — Newly added blank rows in the Text Replacements list show a lighter purple-tinted background and italic style to distinguish them from populated rows.
- **Replacement order instructions panel** — A styled info box at the bottom of the Replacements tab explains top-to-bottom rule matching with two worked examples (correct vs. ambiguous ordering).
- **Settings window title version** — Title bar now shows v0.10.0 (was hardcoded v0.9.2).

---
## [v0.9.6] — 2026-05-08

### Features

- **Clickable star icon in phrase list** — The favorite star (★) in each phrase row is now an interactive button. Clicking it directly toggles `IsFavorite` on that phrase without needing to select it first. Unfavorited rows show a dim outline star; favorited rows show a filled gold star. Cursor changes to hand on hover.
- **Clickable pin icon in phrase list** — The pin icon (📌) in each phrase row is now an interactive button. Clicking it directly toggles `IsPinned` on that phrase. Unpinned rows show the pin at low opacity; pinned rows show it at full opacity with a colored tint. Cursor changes to hand on hover.
- **Right-click context menu on phrase list items** — Right-clicking any phrase row now opens a styled dark context menu with three items: **Toggle Favorite**, **Toggle Pin**, and **Delete**. Commands operate on the right-clicked item directly, not on the current selection.

### Improvements

- **`ToggleFavoriteByItemCommand` and `TogglePinnedByItemCommand`** — New parameterized `RelayCommand` properties on `PhraseListViewModel` that accept a `PhraseItem` argument. Shared implementation via `ToggleFavoriteForItem()` and `TogglePinnedForItem()` private helpers. The original `ToggleFavoriteCommand` and `TogglePinnedCommand` (selected-item versions) now delegate to the same helpers, eliminating duplicate logic.

---

## [v0.9.5] — 2026-05-07

### Features

- **Trailing silence trimming** — New option in Audio settings to trim silence from the end of each generated audio clip. A "Silence Retention" slider (5–100%) controls how much trailing silence is preserved. Disabled by default. Includes a yellow ⚠ warning noting that aggressive trimming may clip the last word.
- **Playback countdown timer** — The overlay status bar now shows a live countdown while audio is playing, e.g. `Speaking...  (45.2s)` or `Speaking...  (1m 02.4s)`. Precision is 50ms (20fps) for smooth sub-second display. Works for all audio sources: Send, Resend, phrase hotkeys, and external playback. Can be disabled in General settings. Timer updates are pulled from a global singleton state so the overlay always shows current values regardless of when it opens.

### Bug Fixes

- **Countdown timer now displays correctly for all playback sources** — Resend hotkey and phrase hotkeys now write duration/start-time to the singleton `PlaybackState` before playback, so the overlay can display a live countdown regardless of open/close order. Previously only the Send path reported timing.
- **Countdown timer resumes mid-stream when overlay reopens** — When the overlay opens during playback, it now reads elapsed time from the singleton and shows the remaining time rather than starting from the full duration.
- **Countdown timer upgrades from static to live display** — When the overlay is open and synthesis is still in progress, `StartSpeaking(0)` shows static "Speaking...". Once synthesis completes and duration becomes available, `OnPlaybackStateChanged` watches `PlaybackDurationSeconds` and automatically upgrades to a live countdown without requiring a close/reopen cycle.
- **Fixed "Speaking... (0.0s)" stuck state blocking all sends** — `PlaybackFinished` event is now subscribed to globally in `OverlayCoordinator` (singleton), not just in the transient `OverlayViewModel`. Previously when playback finished while the overlay was closed, `PlaybackState` was never reset, leaving the old `PlaybackDurationSeconds` value in place. Reopening the overlay would see this stale duration and display "Speaking... (0.0s)" forever, blocking new messages. Now `PlaybackState` is always fully reset when audio naturally completes, regardless of overlay state. Phrases and Resend hotkeys now work repeatedly without requiring manual reset or stop-hotkey intervention.
- **Fixed "Speaking... (0.0s)" stuck state blocking all sends** — `PlaybackFinished` event is now subscribed to globally in `OverlayCoordinator` (singleton), not just in the transient `OverlayViewModel`. Previously when playback finished while the overlay was closed, `PlaybackState` was never reset, leaving the old `PlaybackDurationSeconds` value in place. Reopening the overlay would see this stale duration and display "Speaking... (0.0s)" forever, blocking new messages. Now `PlaybackState` is always fully reset when audio naturally completes, regardless of overlay state. Phrases and Resend hotkeys now work repeatedly without requiring manual reset or stop-hotkey intervention.

---

## [v0.9.41] — 2026-05-02

### Bug Fixes

- **Stop + Send no longer freezes the UI** — `WasapiOut.Stop()` blocks the calling thread (it joins the NAudio playback thread internally). Previously `ForceStopAndSend` called `StopAll()` on the UI dispatcher, freezing the window. It now runs `StopAll()` inside `Task.Run`, keeping the UI thread free.
- **Stop + Send no longer breaks subsequent sends** — `DualOutputAudioRouter` now uses a per-session `CancellationTokenSource`. When `StopAll()` cancels it, in-flight `PlayOnDeviceAsync` tasks throw `OperationCanceledException`, causing `PlayAsync` to skip `PlaybackFinished.Invoke()`. Previously the forced-stop triggered `PlaybackFinished`, which called `_playbackState.Reset()` *after* `ForceStopAndSend` had already set `IsPlaying = true`, permanently locking `CanSubmit = false` and breaking all further audio.
- **`CanSubmit` now always recovers after a `PlayAsync` exception** — Added `_playbackState.Reset()` to the catch block in `FireAndForgetSend`'s background task so a device crash or driver error can never leave the Send button permanently disabled.
- **Test Voice button now uses the current pitch slider value** — Previously the Test Voice playback always used the default pitch (1.0×) regardless of where the slider was set.

---

## [v0.9.4] — 2026-05-03

### New Features

#### Global Pitch Control
- **Pitch slider (Voice tab)** — New slider in the Voice settings tab adjusts the pitch of all real-time TTS output. Range 0.5× (lower/slower) to 2.0× (higher/faster), default 1.0×. Displays as a percentage (e.g. "100%").
- **Live preview before save** — Moving the slider updates the pitch immediately, so you can use "Test Voice" to hear the change without saving first. Cancelling settings reverts the pitch to the last saved value.
- **Resend hotkey respects pitch** — The global Resend Last hotkey now passes the current pitch value when re-speaking the last message.
- **Phrase audio unaffected** — Cached phrase audio plays back verbatim; pitch multiplier is not applied to phrase playback.

#### Overlay Improvements
- **Overlay opens while audio is playing** — The overlay can now be opened even when TTS or phrase audio is actively playing. Previously the overlay open was blocked.
- **Send disabled during playback** — The Send button is disabled while audio is playing (`CanSubmit` property). A status warning ("Wait until speaking finishes to send next message") is shown in the overlay.
- **Stop + Send overlay shortcut** — New in-overlay shortcut (default: Ctrl+Enter) immediately stops any playing audio and sends the current overlay text. Only active while the overlay is open; does not require global hotkey registration.
- **Shake animation on blocked send** — If Enter is pressed while `CanSubmit` is false, the overlay window shakes horizontally with a short animation to indicate the action is blocked.
- **Improved status messages** — Overlay status bar now shows contextual messages: "Wait until speaking finishes…" (orange, Warning), "Generating…" (Info), "Speaking…" (Info), "Error — see log." (Error).
- **Error toast on TTS failure** — A notification toast is now shown when TTS generation fails, in addition to the overlay status message.

#### Settings UX
- **Save toast notification** — A "Settings saved." info toast appears after saving from the Settings window. Debounced to 1.5 s to prevent rapid duplicates.
- **Pitch cancel restore** — Clicking Cancel in Settings reverts the global pitch to the last saved value (undoing any live preview edits).

#### Hotkeys Tab — Overlay Override Shortcut
- **Stop + Send configurable** — The new overlay override shortcut (Ctrl+Enter by default) can be reconfigured in the Hotkeys tab under "Overlay Shortcuts".
- Stored in config as `HotkeySettings.OverrideHotkey`.

### Improvements

#### Settings Tooltips
- Added or improved ToolTip attributes across all settings tabs: Hotkeys, Audio, Voice (engine selector, API key, model, voice dropdowns, pitch slider), Appearance (width, height, font size).
- Volume sliders now have descriptive tooltips.

#### Settings Tab Reorder
- **Replacements tab** now appears before the Phrases tab in settings (alphabetically: Replacements → Phrases). Existing tab order: General → Hotkeys → Audio → Voice → Appearance → Replacements → Phrases.

#### Audio Routing Note
- Added an informational callout in the Audio settings tab clarifying that only TTS speech and phrase audio are routed to the secondary (voice app) device. Non-voice UI sounds stay on the monitor device only.

#### Status Severity
- Added `Warning` severity level with an orange color (`#FAB387`) to the overlay status bar. Previously only `Success`, `Info`, and `Error` existed.

---

## [v0.9.3] — 2026-05-02

### New Features

#### Structured Diagnostic Logging
- **JSONL session logging** — Optional structured diagnostic logging to disk as JSONL (one file per app session, timestamped filename: `session_YYYY-MM-DD_HH-mm-ss_{id}.jsonl`). Files stored at `%AppData%\TtsCommunicationTool\logs\`.
- **Always-on crash logging** — Unhandled exceptions and fatal errors always logged to `crash.log` regardless of settings. Ensures critical failures are never silent.
- **Privacy-safe by default** — Text content logged as SHA-256 hash (16-char hex prefix) instead of raw text. Raw text only logged when explicitly enabled in settings with a privacy warning.
- **Diagnostic settings** — New "Diagnostics / Debug Logging" section at bottom of General settings tab:
  - **Enable diagnostic logging** — Master toggle for session JSONL logs (default: off)
  - **Verbose logging** — Include DEBUG-level events (default: off)
  - **Trace logging** — Include TRACE-level events, extremely chatty (default: off)
  - **Include request correlation IDs** — Attach request IDs to TTS pipeline events for tracing (default: off)
  - **Log raw spoken text (unsafe)** — Store full spoken text in logs; shows strong privacy warning (default: off)
- **Log folder access** — "Open Logs Folder" button opens the logs directory in File Explorer
- **Cleanup utility** — "Delete Old Logs" button with confirmation dialog; preserves current session log while deleting older session files

#### Instrumentation
Structured log events added to:
- **App lifecycle**: `app_startup`, `app_shutdown`, `app_unhandled_exception`
- **TTS pipeline**: `synthesis_requested`, `synthesis_started`, `synthesis_completed`, `synthesis_failed` (Kokoro + ElevenLabs)
- **API calls**: `api_request_started`, `api_request_completed`, `api_request_failed`, `api_rate_limited` (ElevenLabs)
- **Audio playback**: `playback_started`, `playback_stopped`, `playback_failed`
- **Phrase cache**: `cache_hit`, `cache_miss`, `cache_write`
- **Import/Export**: `import_started`, `import_completed`, `import_failed`, `export_started`, `export_completed`, `export_failed`
- **UI interactions**: `overlay_submit_blocked` (blocked send, with reason)
- **Settings**: `settings_loaded`, `settings_saved`

#### Log Schema
Each JSONL entry includes:
- `ts` — RFC 3339 UTC timestamp
- `level` — `TRACE`, `DEBUG`, `INFO`, `WARN`, `ERROR`, `FATAL` (uppercase)
- `category` — `app`, `settings`, `tts`, `audio`, `cache`, `api`, `file`, `import`, `export`, `ui`, `hotkey`
- `event` — snake_case event name
- `msg` — human-readable message
- `session_id` — unique session identifier (16-char hex)
- Optional metadata fields: `request_id`, `voice_id`, `engine`, `provider`, `duration_ms`, `text_length`, `text_hash`, `cache_key`, `error`, `stack`, `source`, `path`, etc.

---

## [v0.9.2] — 2026-05-02

### Renamed
- **App renamed to "The Traveling Star Swirlotl" (TTS Swirlotl)** — System tray tooltip, settings window title, and already-running dialog all updated to the new name.

### Changes
- **Save no longer closes the settings window** — Clicking Save applies all settings (including phrase commit and voice-change regeneration) but leaves the window open. Use the **Cancel** button or the **X** button to close. Both paths still prompt if there are unsaved changes.

### Bug Fixes
- **Win key blocked as hotkey modifier** — The Windows key is no longer accepted as a hotkey modifier. Validation rejects any binding that includes Win, and the capture logic never stores it.
- **OEM key numbering fixed** — Hotkey names for OEM keys (e.g., backtick, braces, semicolon) now correctly resolve in the virtual-key lookup table. WPF sometimes produces numbered aliases (e.g., `Oem3` for backtick) instead of descriptive names. Added support for all numbered OEM variants in `KeyToVk`, and improved the display normalizer to show user-friendly names (e.g., `` ` / ~ `` instead of `Oem3`).

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
