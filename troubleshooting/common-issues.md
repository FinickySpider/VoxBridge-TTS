# Common Issues

## "TTS engine failed to initialize"

**Symptoms**: App starts but shows error: "TTS engine failed to initialize"

**Likely causes**:
- Kokoro model file (`kokoro.onnx`) is missing or corrupt
- Insufficient disk space for model download (~320MB)
- Antivirus blocking the model download

**Fixes**:
1. Ensure `kokoro.onnx` exists in the app directory
2. Check `%AppData%\TtsCommunicationTool\logs\crash.log` for details
3. Temporarily disable antivirus and restart
4. Re-download the app (the model is bundled)

## "Configuration file was corrupt"

**Symptoms**: Notification on startup: "Configuration file was corrupt and has been reset to defaults"

**Likely causes**:
- Manual editing of `config.json` while the app was running
- Disk error or power loss during config save
- Corrupt JSON syntax

**Fixes**:
1. A backup was saved as `config.corrupt.<timestamp>.json`
2. Restore settings from the backup if needed
3. Re-configure the app through the Settings window

## Hotkey not working

**Symptoms**: Pressing the overlay hotkey does nothing

**Likely causes**:
- Another application has registered the same hotkey
- The hotkey was not configured (empty binding)
- Win32 `RegisterHotKey` failed (check logs)

**Fixes**:
1. Check `%AppData%\TtsCommunicationTool\logs\crash.log` for hotkey registration errors
2. Try a different hotkey combination in Settings → Hotkeys
3. Restart the app
4. Check for conflicting apps (e.g., Discord, OBS, game overlays)

## No audio from monitor output

**Symptoms**: Speech plays but you can't hear it

**Likely causes**:
- Monitor output device not selected or disconnected
- Monitor volume set to 0%
- Wrong device selected

**Fixes**:
1. Go to Settings → Audio
2. Verify the Monitor Output dropdown has a device selected
3. Click **Test Monitor** to test
4. Check Monitor Volume slider
5. Click **Refresh** to re-enumerate devices

## No audio in Discord/VRChat

**Symptoms**: You can hear speech through your headphones, but others can't hear you

**Likely causes**:
- Secondary output device not configured
- Virtual cable not installed
- Voice app microphone set to wrong device
- Secondary volume set to 0%

**Fixes**:
1. Verify VB-Cable is installed (check Windows Sound settings)
2. Go to Settings → Audio
3. Select **CABLE Input (VB-Audio Virtual Cable)** as Secondary Output
4. Click **Test Secondary** to verify
5. In Discord/VRChat, set Microphone to **CABLE Output (VB-Audio Virtual Cable)**
6. Check Secondary Volume slider

## "ElevenLabs API returned 401"

**Symptoms**: Error when trying to use ElevenLabs voice

**Likely causes**:
- Invalid API key
- API key has been revoked
- API key was entered incorrectly

**Fixes**:
1. Go to Settings → Voice
2. Re-enter your ElevenLabs API key
3. Verify the key is active in your ElevenLabs account
4. Check your ElevenLabs subscription status

## "ElevenLabs API returned 429"

**Symptoms**: Error during ElevenLabs synthesis

**Likely causes**:
- Rate limited — too many requests in a short time
- Character quota exceeded

**Fixes**:
1. Wait a few seconds and try again
2. Check your ElevenLabs subscription character usage in Settings → Voice
3. Consider upgrading your ElevenLabs plan

## "ElevenLabs API returned 404"

**Symptoms**: Error with voice ID in the message

**Likely causes**:
- The selected voice no longer exists (deleted from ElevenLabs)
- Voice ID mismatch

**Fixes**:
1. Go to Settings → Voice
2. Click to refresh the voice list
3. Re-select a voice

## Phrase not playing

**Symptoms**: Clicking a phrase does nothing

**Likely causes**:
- TTS engine not initialized
- Phrase text is empty
- Phrase cache generation failed

**Fixes**:
1. Check the app is fully started (tray icon visible)
2. Verify the phrase has text
3. Open the Phrase Editor and click **Play Preview**
4. Check logs for cache generation errors

## Overlay not appearing

**Symptoms**: Hotkey pressed but no overlay

**Likely causes**:
- Hotkey conflict with another app
- Overlay already open (re-triggering refocuses)
- App minimized to tray but not responding

**Fixes**:
1. Check if the overlay is already open (it may be behind another window)
2. Try a different hotkey
3. Restart the app
4. Check logs for hotkey registration errors

## App won't start

**Symptoms**: Double-clicking the EXE does nothing

**Likely causes**:
- Another instance is already running (single-instance enforcement)
- Missing .NET runtime
- Corrupt installation

**Fixes**:
1. Check if the app is already running (check system tray)
2. Install .NET 10 runtime
3. Re-download the app
4. Check Windows Event Viewer for crash details

## "Already Running" message

**Symptoms**: Message box: "TTS Swirlotl is already running."

**Cause**: The app uses a named mutex (`Global\TtsCommunicationTool_SingleInstance`) to enforce single-instance. Only one instance can run at a time.

**Fix**: Look for the running instance in the system tray.
