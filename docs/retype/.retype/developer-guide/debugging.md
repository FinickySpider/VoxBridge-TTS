# Debugging

## Logging System

The app has a two-tier logging system:

### 1. Always-On Crash Log
- **File**: `%AppData%\TtsCommunicationTool\logs\crash.log`
- **Level**: WARN and above only
- **Format**: Plain text via Serilog
- **Purpose**: Catastrophic failure capture — always active

### 2. Structured Session Log (Optional)
- **File**: `%AppData%\TtsCommunicationTool\logs\session_<timestamp>_<sessionId>.jsonl`
- **Format**: JSONL (one JSON object per line)
- **Enabled**: Settings → General → Enable Diagnostic Logging

### Log Levels

| Level | Description |
|-------|-------------|
| TRACE | Very detailed — all events (very chatty) |
| DEBUG | Detailed diagnostic events |
| INFO | Normal operational events |
| WARN | Warning conditions (also written to crash log) |
| ERROR | Error conditions (also written to crash log) |
| FATAL | Fatal errors (also written to crash log) |

### Structured Event Fields

Each JSONL event contains:

| Field | Description |
|-------|-------------|
| `timestamp` | ISO 8601 UTC timestamp |
| `level` | Log level (info, warn, error, etc.) |
| `category` | Event category (tts, api, cache, app, hotkey) |
| `event` | Event name (synthesis_started, api_request_failed, etc.) |
| `message` | Human-readable description |
| `metadata` | Structured data object |

### Request ID Correlation

When enabled, a short request ID is attached to TTS pipeline events, allowing you to correlate synthesis start → completion → playback across log entries.

## Diagnostic Logging Settings

| Setting | Effect |
|---------|--------|
| Enable Diagnostic Logging | Turns on JSONL session logging |
| Verbose Logging | Includes DEBUG-level events |
| Trace Logging | Includes TRACE-level events (implies Verbose) |
| Include Request IDs | Attaches correlation IDs to TTS events |
| Log Raw Text | **Warning**: writes actual spoken text to logs |

## Common Debugging Tasks

### Check if TTS Engine Initialized
Look for log entries:
```
[INFO] Kokoro TTS engine initialized successfully. X voices loaded.
```
or
```
[ERROR] Failed to initialize Kokoro TTS engine.
```

### Check Hotkey Registration
```
[INFO] Registered overlay hotkey: Ctrl + Shift + Space
[WARN] Failed to register overlay hotkey: ...
```

### Check Audio Playback
```
[INFO] Generated 96000 bytes of audio (48000 samples at 24000Hz).
```

### Check Phrase Cache
```
[DEBUG] cache_miss: Phrase audio cache miss
[DEBUG] cache_hit: Phrase audio cache hit
```

## Debug Build

Build in Debug configuration for easier debugging:

```powershell
.\build.ps1
```

The Debug build outputs to:
```
src\TtsCommunicationTool.App\bin\Debug\net10.0-windows\TtsCommunicationTool.App.exe
```

## Visual Studio Debugging

1. Open `TtsCommunicationTool.slnx` in Visual Studio
2. Set `TtsCommunicationTool.App` as the startup project
3. Press F5 to start debugging
4. Set breakpoints in ViewModels, services, or XAML code-behind

## Common Breakpoints

| File | Line/Method | What to inspect |
|------|-------------|-----------------|
| `OverlayViewModel.cs` | `SendAsync()` | Text validation, TTS request building |
| `KokoroTtsService.cs` | `SynthesizeAsync()` | Audio generation |
| `DualOutputAudioRouter.cs` | `PlayAsync()` | Audio playback routing |
| `HotkeyHostWindow.cs` | `OnHotkeyPressed()` | Hotkey dispatch |
| `JsonConfigService.cs` | `LoadAsync()` | Config loading/recovery |
