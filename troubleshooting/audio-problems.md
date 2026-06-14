# Audio Problems

## No Sound at All

1. Check Windows volume mixer — ensure the app isn't muted
2. Verify audio devices are selected in Settings → Audio
3. Click **Test Monitor** and **Test Secondary** to isolate the issue
4. Check `%AppData%\TtsCommunicationTool\logs\crash.log` for audio errors

## Audio Only Plays on One Output

- Check per-output volume sliders
- Verify both device IDs are saved (check `config.json`)
- Click **Test Both** to verify dual-output routing

## Audio Crackling or Distortion

- Reduce the sample rate or bit depth (not configurable in current version)
- Check for driver issues with your audio device
- Try a different audio device

## Audio Delay / Lag

- Enable **Trim Trailing Silence** in Settings → Audio
- Set **Silence Retention** to a lower value (e.g., 5%)
- Use Kokoro (offline) instead of ElevenLabs (cloud API has network latency)

## Virtual Cable Not Appearing

1. Reinstall VB-Cable
2. Restart your PC
3. Click **Refresh** in Settings → Audio
4. If still missing, check Windows Sound settings → Playback devices

## Playback Timer Shows Wrong Time

The playback timer estimates remaining time based on audio duration. If the timer seems off:
- It's calculated from `PlaybackDurationSeconds` minus elapsed time
- The timer stops when `PlaybackFinished` fires
- If audio is stopped early (stop hotkey), the timer disappears

## Audio Continues After Stop Hotkey

If pressing the stop hotkey doesn't stop audio:
1. Check the stop hotkey is configured in Settings → Hotkeys
2. Check for hotkey conflicts with other apps
3. Check logs for hotkey registration
4. Restart the app
