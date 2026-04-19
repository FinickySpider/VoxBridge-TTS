# ADDENDUM B
## JSON Schema and Sample Config

## Storage Location

Recommended path:

```txt
%AppData%\TtsCommunicationTool\config.json
```

Log file:

```txt
%AppData%\TtsCommunicationTool\logs\app.log
```

## Example JSON Config

```json
{
  "configVersion": 1,
  "generalSettings": {
    "minimizeToTray": true,
    "startWithWindows": false,
    "closeToTray": true,
    "showNotifications": true
  },
  "hotkeySettings": {
    "overlayHotkey": {
      "ctrl": true,
      "alt": false,
      "shift": true,
      "win": false,
      "key": "Space"
    },
    "stopHotkey": {
      "ctrl": true,
      "alt": false,
      "shift": true,
      "win": false,
      "key": "Back"
    }
  },
  "audioSettings": {
    "monitorOutputDeviceId": "{MONITOR_DEVICE_ID}",
    "secondaryOutputDeviceId": "{SECONDARY_DEVICE_ID}",
    "monitorOutputDeviceNameCache": "Headphones (Realtek(R) Audio)",
    "secondaryOutputDeviceNameCache": "CABLE Input (VB-Audio Virtual Cable)"
  },
  "voiceSettings": {
    "engineName": "Kokoro",
    "selectedVoiceId": "af_heart",
    "selectedVoiceDisplayName": "AF Heart"
  },
  "overlaySettings": {
    "width": 720,
    "height": 140,
    "fontSize": 18
  },
  "phrases": [
    {
      "id": "phrase_001",
      "name": "Hello",
      "text": "Hello",
      "hotkey": null,
      "sortOrder": 0,
      "createdUtc": "2026-04-18T00:00:00Z",
      "updatedUtc": "2026-04-18T00:00:00Z"
    },
    {
      "id": "phrase_002",
      "name": "One moment please",
      "text": "One moment please",
      "hotkey": {
        "ctrl": true,
        "alt": false,
        "shift": true,
        "win": false,
        "key": "D1"
      },
      "sortOrder": 1,
      "createdUtc": "2026-04-18T00:00:00Z",
      "updatedUtc": "2026-04-18T00:00:00Z"
    }
  ]
}
```

## Schema Notes

### `configVersion`
Integer schema version. Use this for migrations later.

### `generalSettings`
- `minimizeToTray`: if true, minimizing sends app to tray
- `startWithWindows`: optional for MVP
- `closeToTray`: if true, clicking close keeps app in tray
- `showNotifications`: enables non-critical toasts

### `hotkeySettings`
Only overlay and stop hotkeys are required in MVP.

### `audioSettings`
Store both persistent device IDs and cached human-readable names for display and debugging.

### `voiceSettings`
Use `engineName = "Kokoro"` in MVP. Future expansion can support other engines without changing the rest of the config shape much.

### `overlaySettings`
MVP requires width, height, and font size. Position can be added later if needed.

### `phrases`
Keep phrase list flat in MVP. Do not introduce folders or tags yet.

## C# Model Mapping

Use direct DTO-style models for config serialization. Suggested shape:

```csharp
public sealed class AppConfig
{
    public int ConfigVersion { get; set; } = 1;
    public GeneralSettings GeneralSettings { get; set; } = new();
    public HotkeySettings HotkeySettings { get; set; } = new();
    public AudioSettings AudioSettings { get; set; } = new();
    public VoiceSettings VoiceSettings { get; set; } = new();
    public OverlaySettings OverlaySettings { get; set; } = new();
    public List<PhraseItem> Phrases { get; set; } = new();
}
```

## Default Values

Recommended defaults on first run:

- `overlayHotkey = Ctrl + Shift + Space`
- `stopHotkey = Ctrl + Shift + Backspace`
- `voiceSettings.engineName = "Kokoro"`
- `overlaySettings.width = 720`
- `overlaySettings.height = 140`
- `overlaySettings.fontSize = 18`
- `showNotifications = true`

## Config Failure Recovery

If config load fails:
1. Copy invalid file to `config.invalid.backup.json`
2. Create a new default config
3. Notify the user that settings were reset
4. Continue app startup without crashing
