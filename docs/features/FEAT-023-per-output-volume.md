---
id: FEAT-023
type: feature
status: complete
priority: medium
phase: PHASE-02
sprint: SPRINT-04
owner: ""
depends_on: [FEAT-010, FEAT-015]
---

# FEAT-023: Per-Output Volume Controls

## Description
Add independent volume sliders for the monitor output and secondary (virtual cable) output in the Audio settings tab. Volume is applied as a multiplier to PCM audio before playback.

## Acceptance Criteria
- [ ] Monitor output has a volume slider (0â€“100%, default 100%)
- [ ] Secondary output has a volume slider (0â€“100%, default 100%)
- [ ] Volume is applied to audio data before it is sent to each output device
- [ ] Volume settings persist in config
- [ ] Test buttons respect the volume setting

## Files Touched
| File | Change |
|------|--------|
| `src/.../Core/Models/AudioSettings.cs` | Add `MonitorVolume`, `SecondaryVolume` (float 0.0â€“1.0, default 1.0) |
| `src/.../Core/Interfaces/IAudioRouterService.cs` | Add volume params to PlayAsync |
| `src/.../Infrastructure/Audio/DualOutputAudioRouter.cs` | Apply volume multiplier to PCM samples |
| `src/.../UI/ViewModels/AudioSettingsViewModel.cs` | Add MonitorVolume, SecondaryVolume properties |
| `src/.../Views/SettingsWindow.xaml` | Add sliders in Audio tab |

## Implementation Notes
- Apply volume as PCM sample multiplication (16-bit: multiply short samples by factor)
- Clamp result to Int16 range to avoid clipping
- Sliders should show percentage label alongside

## Testing
- [ ] Set monitor to 50%, secondary to 100% â€” monitor is quieter
- [ ] Set both to 0% â€” silence on both
- [ ] Persists after save and restart

## Done When
- [ ] Acceptance criteria met
- [ ] Verified manually
