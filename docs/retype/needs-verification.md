---
label: Needs Verification
icon: question
order: 100
visibility: hidden
---

# Needs Verification

This page documents items that could not be fully confirmed from the codebase and need manual verification.

## TTS Engine Details

- **Kokoro model download URL**: The exact URL where the Kokoro ONNX model is downloaded from is not visible in the code. KokoroSharp handles this internally.
- **Kokoro voice file format**: The exact format and location of Kokoro voice files is handled by KokoroSharp internally.
- **ElevenLabs API rate limits**: The exact rate limit thresholds are not documented in the code. The app handles 429 responses gracefully.

## Audio Pipeline

- **WASAPI device ID format**: The exact format of WASAPI device IDs is platform-dependent. The app stores them as returned by NAudio's `MMDevice.ID`.
- **Silence threshold value**: The silence threshold of 164 (~0.5% of 32767) was determined empirically. The optimal value may vary by voice.

## Configuration

- **Config schema migration**: The `configVersion` field exists for future use, but no migration logic has been implemented yet. Version 1 is the only supported version.
- **Legacy API key migration**: The code references migration of legacy plain-text ElevenLabs keys to encrypted storage, but the exact migration trigger and behavior needs verification.

## Hotkeys

- **Win key rejection**: The `HotkeyValidation` class explicitly rejects the Win key as a modifier, but the `HotkeyBinding` model still has a `Win` property. The `GlobalHotkeyService` will register it if passed, but the UI prevents it.
- **MOD_NOREPEAT**: The `GlobalHotkeyService` uses `MOD_NOREPEAT` (0x4000) to prevent repeated WM_HOTKEY messages when a key is held. This is a Windows Vista+ feature.

## UI

- **Settings window title**: The Settings window title contains a special character "—" (em dash) in "The Traveling Star Swirlotl". This appears to be intentional branding.
- **Overlay window title**: The overlay window title is "TTS Overlay".

## Build

- **net10.0-windows TFM**: The project targets `net10.0-windows` which is a preview TFM. The actual runtime requirement may change.
- **Build script paths**: The build script uses hardcoded absolute paths (`e:\Projects\tts\...`). These may need adjustment on other machines.

## Tests

- **Test project**: The test project scaffold exists but contains no test files. All test directories are empty.

## Documentation

- **Screenshot placeholders**: All screenshot references in this documentation are placeholder tokens. Actual screenshots need to be captured and placed in `docs/retype/screenshots/`.
