# Configuration

The app stores all settings in a JSON configuration file. Most settings are managed through the Settings window, but understanding the file structure helps with troubleshooting and backup.

## Config File Location

```
%AppData%\TtsCommunicationTool\config.json
```

On most systems this resolves to:
```
C:\Users\<YourUsername>\AppData\Roaming\TtsCommunicationTool\config.json
```

## Config File Structure

```json
{
  "configVersion": 1,
  "generalSettings": {
    "startWithWindows": false,
    "showNotifications": true,
    "showSplashScreen": true,
    "enableTranscriptLogging": false,
    "keepOverlayText": false,
    "enableCharacterLimit": true,
    "maxOverlayInputLength": 500,
    "settingsWindowWidth": 880,
    "settingsWindowHeight": 580,
    "themePreviewColumnWidth": 300,
    "phraseEditorWindowWidth": 560,
    "phraseEditorWindowHeight": 760,
    "showPlaybackTimer": true
  },
  "hotkeySettings": {
    "overlayHotkey": { "ctrl": true, "shift": true, "key": "Space" },
    "stopHotkey": { "ctrl": true, "shift": true, "key": "Back" },
    "settingsHotkey": { "ctrl": false, "alt": false, "shift": false, "win": false, "key": "" },
    "resendHotkey": { "ctrl": false, "alt": false, "shift": false, "win": false, "key": "" },
    "overrideHotkey": { "ctrl": true, "key": "Return" }
  },
  "audioSettings": {
    "monitorOutputDeviceId": "",
    "secondaryOutputDeviceId": "",
    "monitorVolume": 1.0,
    "secondaryVolume": 1.0,
    "trimTrailingSilence": false,
    "silenceRetentionPercent": 100
  },
  "voiceSettings": {
    "engine": "Kokoro",
    "selectedVoiceId": "af_heart",
    "selectedVoiceDisplayName": "AF Heart",
    "globalPitch": 1.0
  },
  "overlaySettings": {
    "width": 720,
    "height": 160,
    "fontSize": 18,
    "overlayOpacity": 0.93,
    "overlayFontFamily": "Segoe UI",
    "left": null,
    "top": null
  },
  "phrases": [],
  "textReplacements": {
    "isEnabled": true,
    "rules": []
  },
  "elevenLabs": {
    "encryptedApiKey": null,
    "apiKeyTail": null,
    "selectedVoiceId": "",
    "selectedVoiceName": "",
    "modelId": "eleven_multilingual_v2",
    "subscriptionCharacterCount": 0,
    "subscriptionCharacterLimit": 0,
    "totalCharactersUsed": 0
  },
  "diagnosticLogging": {
    "enableDiagnosticLogging": false,
    "verboseLogging": false,
    "traceLogging": false,
    "includeRequestIds": false,
    "logRawText": false
  },
  "activeThemeName": "Default Dark"
}
```

## Config Recovery

If the config file becomes corrupt (e.g., from a disk error or manual editing mistake):

1. The app detects the corruption on next launch
2. A backup is saved as `config.corrupt.<timestamp>.json`
3. The config is reset to defaults
4. A notification is shown: *"Configuration file was corrupt and has been reset to defaults. A backup was saved."*

!!!warning
Do not edit the config file while the app is running. The app overwrites the file on save, which could cause data loss.
!!!

## Data Storage Locations

| Data | Location |
|------|----------|
| Config file | `%AppData%\TtsCommunicationTool\config.json` |
| Log files | `%AppData%\TtsCommunicationTool\logs\` |
| Phrase audio cache | `%AppData%\TtsCommunicationTool\phrase_cache\` |
| User themes | `%AppData%\TtsCommunicationTool\themes\` |
| Transcript | `%AppData%\TtsCommunicationTool\transcript.txt` |
| Config backups | `%AppData%\TtsCommunicationTool\config.corrupt.*.json` |
