---
label: Config Files
icon: file-code
order: 100
---

# Configuration Files

## Main Config: `config.json`

**Location**: `%AppData%\TtsCommunicationTool\config.json`

The main configuration file stores all user settings. See [Configuration](../getting-started/configuration.md) for the full schema.

### Schema Versioning

The `configVersion` field enables future schema migrations. Currently at version `1`.

### Config Recovery

If the config file is corrupt:
1. A backup is saved as `config.corrupt.<timestamp>.json`
2. The config is reset to defaults
3. The user is notified

## User Themes: `*.ttstheme`

**Location**: `%AppData%\TtsCommunicationTool\themes\*.ttstheme`

User-created themes are stored as individual JSON files with the `.ttstheme` extension. The format matches the `ThemeSettings` model:

```json
{
  "name": "My Custom Theme",
  "windowBackground": "#1E1E2E",
  "panelBackground": "#2B2B3D",
  "accent": "#89B4FA",
  "primaryText": "#CDD6F4",
  "cornerRadius": 6,
  "spacingDensity": 1,
  ...
}
```

## Phrase Cache: `*.wav`

**Location**: `%AppData%\TtsCommunicationTool\phrase_cache\{phraseId}.wav`

Pre-generated TTS audio for quick phrases. Standard WAV format with 44-byte header.

## Session Logs: `*.jsonl`

**Location**: `%AppData%\TtsCommunicationTool\logs\session_<timestamp>_<sessionId>.jsonl`

Structured diagnostic logs in JSONL format (one JSON object per line). Only created when diagnostic logging is enabled.

## Crash Log: `crash.log`

**Location**: `%AppData%\TtsCommunicationTool\logs\crash.log`

Always-on plain-text log for WARN-level and above events. Never rolls over.

## Transcript: `transcript.txt`

**Location**: `%AppData%\TtsCommunicationTool\transcript.txt`

Timestamped log of all spoken text. Only created when transcript logging is enabled.

```
[2026-01-15 14:30:22] Hello everyone!
[2026-01-15 14:30:45] How are you doing today?
```
