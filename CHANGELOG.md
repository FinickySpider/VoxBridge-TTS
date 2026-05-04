# Changelog

All notable changes to TTS Communication Tool are documented here.

---









## [v0.15.5] -- 2026-05-03

### Features

- (add features here)

### Bug Fixes

- (add bug fixes here)

### Improvements

- (add improvements here)

---
## [v0.15.4] -- 2026-05-03

### Bug Fixes

- **Entire program hangs after closing Phrase Editor**: `SaveWindowSize()` called `.GetAwaiter().GetResult()` directly on the UI thread. `JsonConfigService.SaveAsync` uses `await File.WriteAllTextAsync(...)` whose continuation is scheduled back onto the WPF `SynchronizationContext` (the UI thread). Since the UI thread was blocked waiting for the task, and the task was waiting for the UI thread to resume it — classic deadlock. Fixed: wrapped in `Task.Run(...)` so the async work runs on the thread pool, continuations complete there, and the UI thread is never asked to resume a task it's blocking on.


---
## [v0.15.3] -- 2026-05-03

### Bug Fixes

- **CRITICAL — ElevenLabs credits still charged on every Play Preview (real root cause)**: The `_previewCache`/`_previewCacheValid` fields added in v0.15.2 were never checked in `PlayPreviewAsync`. The method still called `_phraseCache.GetCachedAudio` (disk I/O + WAV parse) as its cache gate. If that returned null for any reason execution fell through to a fresh ElevenLabs call. Fixed: `PlayPreviewAsync` now checks `_previewCacheValid && _previewCache is not null` as the SOLE gate — disk is never touched during playback, there is no fallback path to synthesis.
- **Cache-miss path never populated `_previewCache`**: When synthesis succeeded in the old miss path, a local `playback` variable was created and played but `_previewCache` and `_previewCacheValid` were never set. So the second Play Preview always found `_previewCacheValid = false` and synthesized again. Fixed: after synthesis, audio is stored in `_previewCache` and `_previewCacheValid = true` before playback.
- **`RegenVoiceAsync` never populated `_previewCache`**: After Regen Cache, the in-memory field was empty, so next Play Preview fell through to synthesis instead of using the just-generated audio. Fixed: Regen now sets `_previewCache` and `_previewCacheValid = true` after a successful synthesize.
- **`SaveAsync` always deletes cache and regenerates via TTS on Save**: `_phraseCache.DeleteCache` + `GenerateCacheAsync` was unconditional — every Save called ElevenLabs at full cost and produced different audio than what the user heard in preview. Fixed: if `_previewCacheValid` is true, Save writes the in-memory preview audio directly to the disk cache WAV file (zero TTS calls, exact same audio the user approved). `GenerateCacheAsync` is only called when the user presses Save without having previewed at all.
- **Window size not persisting across sessions**: `SaveWindowSize()` used `_ = _config.SaveAsync(...)` (fire-and-forget). If the async task didn't complete before the window was destroyed the size was silently lost. Fixed: `SaveWindowSize` now blocks synchronously via `.GetAwaiter().GetResult()` — the file is guaranteed written before `OnClosed` returns.

---
## [v0.15.2] -- 2026-05-03

### Bug Fixes

- **CRITICAL — ElevenLabs credits consumed on every Play Preview despite showing "✓ Cached"**: The v0.15.1 fix used `PhraseCacheService.GetCachedAudio` (disk I/O + WAV parsing) as the cache gate. If the WAV parse returned `null` for any reason, execution fell through to synthesis. Replaced with a **private in-memory `PlaybackRequest? _previewCache` field** in the ViewModel. The audio bytes are stored in RAM after the first synthesis; all subsequent Play Preview clicks use this in-memory copy directly — no disk read, no WAV parsing, no TTS call, zero ambiguity. Credits are spent **only** on: first Play Preview with no cache, or Regen Cache.
- **Cache status indicator not updating live**: Previously `RefreshCacheStatus` checked `PhraseCacheService.HasCache` (file exists on disk) — so it never turned amber when text/voice/engine/pitch changed. Now a `_previewCacheValid` bool flag governs the indicator. `InvalidatePreviewCache()` clears both `_previewCache` and `_previewCacheValid` (and calls `RefreshCacheStatus`) whenever any setting that affects synthesis changes. Indicator turns amber immediately on change, green when cached audio is in memory.
- **Phrase Editor window size not persisting across sessions**: `SizeChanged` was writing dimensions to the config object in memory but `SaveAsync` was never called when the editor closed. `OnClosed` in the code-behind now calls `vm.SaveWindowSize()` which fires `_config.SaveAsync(_config.CurrentConfig)`, persisting the dimensions to `config.json` immediately on close.

---
## [v0.15.1] -- 2026-05-03

### Features

- **Phrase Editor cache-first preview**: "Play Preview" now checks for a cached audio file before synthesizing. On the first press it synthesizes and writes the result to the phrase cache, then plays it. Every subsequent press plays directly from cache — no TTS engine is invoked again, meaning **ElevenLabs credits are only consumed once** per phrase (or after an explicit "Regen Cache"). The status message also updates to note when synthesis is about to spend ElevenLabs credits (`"Synthesizing via ElevenLabs (credits will be used)…"` vs `"Generating preview…"` for Kokoro).
- **Cache status indicator**: The cache indicator in the Preview panel is now always shown on editor open and uses color to communicate state — **green** (`✓ Cached — Play Preview will use stored audio (no re-synthesis).`) when a cache file exists for this phrase, **amber** when no cache is present or the phrase is new.
- **Cloned voice red highlighting in Phrase Editor**: The ElevenLabs voice dropdown in the Voice Override section now renders cloned and AI-generated voices in **red** (matching the Voice tab in Settings). A persistent info panel below the dropdown explains that red-highlighted voices require a Creator plan or higher.

### Improvements

- `RegenVoiceAsync` now calls `RefreshCacheStatus()` after writing the cache (consistent with `PlayPreviewAsync`).
- `RefreshCacheStatus()` now always notifies `HasEditorCache` so the indicator color updates reactively.

---
## [v0.15.0] -- 2026-05-03

### Bug Fixes

- **CRITICAL** — Phrase Editor category and voice ComboBoxes completely non-functional (items never appeared, dropdown never opened on click). Root cause: `EditorComboBox` ControlTemplate had **no `ToggleButton`**. Without one, clicking the arrow area does nothing because every other element in the template is either `IsHitTestVisible="False"` (ContentPresenter, arrow Path) or a TextBox that handles focus but not dropdown-open. The `SettingsComboBox` (which works) uses a `ToggleButton` bound `TwoWay` to `IsDropDownOpen` — that is the correct WPF ComboBox pattern. Rewrote `EditorComboBox` template to match: ToggleButton + `StackPanel IsItemsHost="True"` (reverting the incorrect v0.14.4 `ItemsPresenter` change; `SettingsComboBox` always used `StackPanel IsItemsHost` and works).
- Fixed `PhraseListViewModel.PlaySelectedAsync` fallback path (cache miss): previously synthesised using the global Kokoro voice regardless of per-phrase engine/voice overrides. Now delegates to `PhraseCacheService.GenerateCacheAsync` (which already contains the full override logic) and plays from the resulting cache file, so phrases with ElevenLabs or per-voice overrides are played correctly even on a cold cache.

---
## [v0.14.4] -- 2026-05-03

### Bug Fixes

- Fixed Phrase Editor category (and voice) dropdowns showing no items: the custom `EditorComboBox` ControlTemplate used `<StackPanel IsItemsHost="True"/>` inside the popup, which is unreliable for `ComboBox` item generation. Replaced with the standard `<ItemsPresenter/>` that WPF's ComboBox relies on.
- Fixed Settings window falsely prompting "unsaved changes" when the user only navigated tabs without changing anything: WPF TwoWay-bound ComboBoxes (audio device selectors, voice selectors) write `null` back through their bindings during first render if the persisted ID doesn't match any list item, which fired `PropertyChanged` on child VMs and set `IsDirty = true` immediately on open. The window now calls `vm.ResetDirtyState()` at `DispatcherPriority.ApplicationIdle` after `Loaded` so all initial binding evaluation settles before the dirty flag is considered meaningful.

---
## [v0.14.3] -- 2026-05-03

### Bug Fixes

- Fixed Name & Text column expanding to push Hotkey off the right edge of the screen: `PhraseListView_SizeChanged` had a stale `fixedTotal` (328px) that was missing the Engine (75px) and Voice (95px) columns added in v0.14.1 and used old widths for Category (120→95) and Hotkey (130→110).  Updated to 453px so the auto-fill calculation correctly leaves Name & Text only the leftover space.
- Fixed Category sort header click doing nothing: column index mapping in `PhraseSort_Click` still mapped index 3 → "Category" but Category is now at index 5 after Engine and Voice columns were inserted.

---
## [v0.14.2] — 2026-05-03

### Bug Fixes

- **X button bypasses unsaved-changes prompt after a Save** — `_committed` was set to `true` when the global Save button was clicked and never reset. Any phrase changes made *after* a Save in the same window session would silently bypass the "You have unsaved changes — discard?" dialog when X was pressed. Fix: removed the `vm.Saved → _committed = true` binding. After a genuine Save, both `IsDirty` and `HasSessionChanges` are already `false`, so the guard is satisfied without `_committed`. `_committed` is now only set in the Cancel path (to prevent double-rollback).
- **Phrase list column overflow / horizontal scrollbar** — Removed the "V.Ovr" column and tightened column widths so the phrase list fits within the default 680px settings window without a horizontal scrollbar (total column budget ≈ 616px).

---

## [v0.14.1] — 2026-05-03

### Bug Fixes

- **`PhraseService.Update` data loss** — `Category`, `IsFavorite`, `IsPinned`, `OverrideEngine`, `UseVoiceOverride`, `OverrideVoiceId`, `OverrideVoiceName`, and `OverridePitch` were all silently discarded on every Phrase Editor save for existing phrases. All fields are now copied correctly.
- **ElevenLabs cache generation blocked by Kokoro init check** — `PhraseCacheService.GenerateCacheAsync` used `_tts.IsInitialized` (which returns the *global* engine's readiness, i.e. Kokoro) even for ElevenLabs phrases. This caused every hotkey play to fall through to on-the-fly synthesis, consuming credits. Cache generation for ElevenLabs phrases now proceeds regardless of Kokoro state.
- **Cache regen race condition** — `PhraseService.Update` previously fire-and-forgot its own internal cache regeneration, which could race against Phrase Editor's explicit regen and overwrite the newly generated ElevenLabs audio. Phrase Editor now passes `skipCacheRegen: true` and exclusively owns the regen lifecycle.
- **Category field reverts after typing** — WPF editable `ComboBox` can silently revert the `Text` binding when `SelectedItem` changes. A `LostFocus` handler in `PhraseEditorWindow` now force-pushes the typed text to the ViewModel before save.
- **Phrase hotkeys not live after Phrase Editor save** — New/edited phrase hotkeys now activate immediately after Phrase Editor save via `RegisterPhraseHotkeys()` in `OnEditorCommit`, without requiring a full global settings save.

### Features

- **Phrase Editor window size persistence** — The Phrase Editor now remembers its size between sessions. Stored in `GeneralSettings.PhraseEditorWindowWidth/Height`, persisted to disk on next global save.
- **Three new columns in phrase list** — Engine (Kokoro / ElevenLabs / (Inherited)), V.Ovr (✓ / –), and Voice name columns inserted between "Name & Text" and "Category". Computed as read-only display properties on `PhraseItem`.
- **Dark-theme `ComboBox` control template** — Phrase Editor dropdowns (Category and Voice) now use a proper `ControlTemplate` that renders consistently in the dark VoxBridge theme regardless of Windows system theme. Arrow glyph, dropdown popup, and selected-item text are all styled correctly.

---

## [v0.14.0] -- 2026-05-03

### Features

- **Phrase Editor window**: Full-screen modal editor (`PhraseEditorWindow`) for creating and editing phrases. Opens via "✦ New Phrase" button, "✏ Edit" button, context-menu → Edit, double-click on a phrase row, or pressing Enter on a selected row.
- **Per-phrase voice override**: Each phrase can override the TTS engine (Kokoro / ElevenLabs), select a specific voice, and set a custom pitch independently from global voice settings. Overrides are stored on `PhraseItem` with five new fields: `OverrideEngine`, `UseVoiceOverride`, `OverrideVoiceId`, `OverrideVoiceName`, `OverridePitch`.
- **Preview in Phrase Editor**: Play a live TTS preview (monitor output only, not secondary VRChat cable) directly from the editor with the current settings before committing. Stop, and Regen Cache buttons allow fine-grained control.
- **Hotkey suppression during editor**: Global phrase and overlay hotkeys are suppressed while the Phrase Editor is open to prevent accidental triggers.

### Improvements

- **Phrases tab bottom panel replaced**: The inline Name / Text / Category text boxes and Add / Update buttons have been replaced with a cleaner action bar: `✦ New Phrase`, `✏ Edit`, `▶ Play`, Delete.
- **Context menu "Edit" item**: Right-clicking a phrase now shows "Edit" as the first menu item, opening the Phrase Editor for that phrase.
- **Session tracking for Phrase Editor**: New phrases or cache-regenerated phrases created via the editor are tracked for rollback so Settings → Cancel correctly undoes all session changes.
- **`TtsRouter` engine override**: A new `EngineOverride` field on `TtsRequest` allows per-request engine routing without changing the global config, used by the editor preview and `PhraseCacheService`.

---
## [v0.13.3] -- 2026-05-03

### Bug Fixes

- **Resend button active while audio was already playing**: Clicking "↩ Resend" while audio was playing would incorrectly fill the input and attempt to send — now Resend is disabled under the exact same conditions as the Send button: while audio is playing (`_playbackState.IsPlaying || _audioRouter.IsPlaying`) or while TTS is generating (`IsSending`). Added `CanResend` property to `OverlayViewModel` which is notified in `NotifyCanSubmitChanged()` and at all three `HasRecentMessage` update sites. The button now visually dims to `#45475A` (matching the blocked-send look) when disabled, and the hover highlight only fires when enabled.

- **Phrases tab — column resize could shrink resizable columns to zero**: Added `MinWidth="40"` to the shared `GridView.ColumnHeaderContainerStyle` so Name & Text, Category, and Hotkey columns cannot be collapsed by dragging the resize grip all the way left.

- **Phrases tab — Pin and Star columns could be resized (gripper present)**: Both the 📌 Pin (col 0) and ★ Favorite (col 1) columns now have a per-column `GridViewColumn.HeaderContainerStyle` that omits `PART_HeaderGripper` entirely and locks the column to exactly 28 px (`MinWidth="28"` / `MaxWidth="28"`). These columns cannot be resized.

- **Phrases tab — no confirmation before deleting a phrase**: Right-clicking a phrase and choosing Delete now shows a "Delete the phrase "…"? — This cannot be undone." Yes/No modal before any data is removed. Implemented via a `Func<string, bool>? ConfirmDelete` callback on `PhraseListViewModel`, set by `SettingsWindow` on construction; the ViewModel stays testable without taking a UI dependency.

---

---
## [v0.13.2] -- 2026-05-03

### Bug Fixes

- **ElevenLabs voice selection reset to first voice when switching engines in settings**: When switching from ElevenLabs → Kokoro → ElevenLabs (without saving), the selected ElevenLabs voice was silently replaced with the first voice in the list (e.g. "Roger" instead of the saved "Lily"). Root cause: `FetchElevenLabsVoicesAsync` called `ElevenLabsVoices.Clear()` before repopulating. The ElevenLabs voice ComboBox has a `TwoWay` `SelectedValue` binding — when the collection is cleared, WPF cannot match the current voice ID against the empty list and writes `null`/empty string back through the binding, destroying `ElevenLabsSelectedVoiceId` before the new items arrive. The `All(v => v.Id != ...)` fallback check then found no match and replaced the selection with `voices[0]`. Fixed by snapshotting `ElevenLabsSelectedVoiceId` before `Clear()` and restoring it after repopulation, so the fallback only fires if the voice is genuinely absent from the account.

---

---
## [v0.13.1] -- 2026-05-03

### Bug Fixes

- **Voice selection forgotten on settings reopen (both engines)**: Root cause was a shared `AvailableVoices` collection used by the Kokoro ComboBox. When switching to ElevenLabs, `LoadVoices()` cleared and refilled `AvailableVoices` with ElevenLabs voices. The hidden Kokoro ComboBox's `TwoWay` binding could not match its saved `SelectedVoiceId` against the ElevenLabs items and pushed `null` back, silently destroying the Kokoro voice selection. On the next settings open the ViewModel read `SelectedVoiceId` as empty/wrong and saved the wrong value.
- **Fix — separate permanent collections**: Added `KokoroVoices` (a new `ObservableCollection` bound exclusively to the Kokoro ComboBox, populated once from the Kokoro service and never cleared on engine switch). `ElevenLabsVoices` remains the separate ELevenLabs collection. The XAML Kokoro ComboBox now binds to `KokoroVoices` instead of `AvailableVoices`. `AvailableVoices` is now an alias for `KokoroVoices` for backward compatibility.
- **Fix — removed fragile `_savedKokoroVoiceId` workaround**: The `_savedKokoroVoiceId` field was a sticking-plaster that tried to save/restore the Kokoro voice around engine switches. It is now deleted. `SelectedVoiceId` always reliably holds the Kokoro voice because the Kokoro ComboBox list never changes. `ApplyTo` reads `SelectedVoiceId` directly.
- **Fix — removed `LoadVoices()`**: All calls to the engine-conditional `LoadVoices()` method (which was the source of the shared-collection corruption) are removed. `PopulateKokoroVoices()` fills Kokoro voices once; `FetchElevenLabsVoicesAsync` fills ElevenLabs voices when needed.
- **Auto-fetch on settings open**: If ElevenLabs is the active engine at settings open time and the service cache is empty (e.g. fresh process start), a background voice fetch is triggered automatically so the ElevenLabs voice list is never blank with a valid API key.
- **ElevenLabs voice name sync**: After a fetch, if the saved voice ID is still in the list, the display name is refreshed from the API response in case it was renamed on the ElevenLabs side.

---

---
## [v0.13.0] -- 2026-05-03

### Features

- **DPAPI-encrypted API key storage**: ElevenLabs API key is now encrypted with Windows DPAPI (`ProtectedData`, `CurrentUser` scope) before being written to config. The plain-text `ApiKey` field is replaced by `EncryptedApiKey` (Base64 blob), `ApiKeyTail` (last 4 chars), and `ApiKeyUpdatedDate`. The key is only decrypted in-memory immediately before each API call and never logged.
- **Write-only API key UX**: The Voice tab now has a three-state API key panel:
  - **Saved state** — shows masked tail (`sk_...a1b2 Updated 2026-05-03`) with `Test API`, `Update Key`, and `Clear API` buttons.
  - **Entry/editing state** — shows a `PasswordBox` (masked input), an "⚠ Unsaved" notice, and `Save`, `Test (unsaved)`, and `Cancel` buttons.
  - **Clearing state** — confirmation prompt before deletion, with `Confirm Clear` and `Cancel`.
- **Test API button**: Tests a key (saved or unsaved) against `/v1/user/subscription` and shows a toast with plan tier and characters remaining, or a specific error. The decrypted/pending key is discarded immediately after the call.
- **Automatic migration**: On first load, if a plain-text `ApiKey` value is found in the existing config it is automatically encrypted in-place and a toast confirms "API key re-encrypted for secure storage."
- **`ApiKeyVault` helper** (`Infrastructure/Security/ApiKeyVault.cs`): Internal DPAPI encrypt/decrypt/tail helper. Never exposes decrypted bytes outside `ElevenLabsTtsService`.
- **`ElevenLabsTtsService` key management**: Added `SaveApiKey`, `ClearApiKey`, `HasApiKey`, `GetApiKeyMaskedDisplay`, `MigrateLegacyApiKey`, `TestApiKeyAsync`, and `TestSavedApiKeyAsync` public methods so the ViewModel never handles raw key bytes.

### Improvements

- Auto-fetch voices now triggers on API key **save** rather than on keystroke (debounce removed).
- `ElevenLabsStatus` display unified to a single shared TextBlock; removed duplicate inline status from the Fetch Voices row.
- `SettingsPasswordBox` WPF style added (matching `SettingsTextBox` colours) for consistent look.

---

## [v0.12.1] -- 2026-05-03

### Bug Fixes

- **ElevenLabs Model dropdown was blank**: `ElevenLabsModelOptions` was declared `static`, which WPF's instance-binding path (`{Binding Voice.ElevenLabsModelOptions}`) cannot reach. Changed to a non-static instance property backed by a private static list. Dropdown now populates correctly with all four hard-coded models.

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
