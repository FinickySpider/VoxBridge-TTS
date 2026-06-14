---
label: File Formats
icon: file-binary
order: 90
---

# File Formats

## Phrase Export JSON

```json
[
  {
    "id": "a1b2c3d4-...",
    "name": "Greeting",
    "text": "Hello everyone!",
    "category": "Social",
    "isFavorite": true,
    "isPinned": false,
    "sortOrder": 1,
    "hotkey": null,
    "overrideEngine": null,
    "useVoiceOverride": false,
    "overrideVoiceId": null,
    "overrideVoiceName": null,
    "overridePitch": null,
    "createdUtc": "2026-01-01T00:00:00Z",
    "updatedUtc": "2026-01-01T00:00:00Z"
  }
]
```

## Theme Export JSON (`.ttstheme`)

```json
{
  "name": "My Theme",
  "windowBackground": "#1E1E2E",
  "panelBackground": "#2B2B3D",
  "deepPanelBackground": "#181825",
  "historyBackground": "#14141E",
  "surface0": "#313244",
  "borderColor": "#45475A",
  "surface2": "#585B70",
  "accent": "#89B4FA",
  "accentHover": "#B4D0FB",
  "primaryText": "#CDD6F4",
  "secondaryText": "#A6ADC8",
  "mutedText": "#6C7086",
  "mutedIcon": "#8B9CC8",
  "infoColor": "#FAB387",
  "errorColor": "#F38BA8",
  "errorHover": "#F5A0B5",
  "warning": "#F9E2AF",
  "success": "#A6E3A1",
  "uiFontFamily": "Segoe UI",
  "baseFontSize": 13,
  "overlayFontFamily": "Segoe UI",
  "overlayFontSize": 18,
  "cornerRadius": 6,
  "borderThickness": 1,
  "controlHeight": 28,
  "spacingDensity": 1,
  "overlayBackgroundHex": "#1E1E2E",
  "overlayBorderHex": "#45475A",
  "overlayOpacity": 0.93,
  "overlayCornerRadius": 12
}
```

## Text Replacement Export JSON

```json
[
  {
    "id": "rule-id",
    "triggerText": "brb",
    "replacementText": "be right back",
    "isEnabled": true,
    "isCaseSensitive": false,
    "wholeWordOnly": true,
    "sortOrder": 0
  }
]
```

## Session Log JSONL

Each line is a JSON object:

```json
{"timestamp":"2026-01-15T14:30:22.123Z","level":"info","category":"tts","event":"synthesis_started","message":"Kokoro TTS synthesis started","metadata":{"voice_id":"af_heart","engine":"kokoro","text_length":12}}
{"timestamp":"2026-01-15T14:30:23.456Z","level":"info","category":"tts","event":"synthesis_completed","message":"Generated 96000 bytes of audio","metadata":{"duration_ms":1234}}
```

## Phrase Cache WAV

Standard WAV file format:
- 44-byte header
- PCM format
- Sample rate: matches TTS engine (24000 Hz for Kokoro, 44100 Hz for ElevenLabs)
- 16-bit mono
