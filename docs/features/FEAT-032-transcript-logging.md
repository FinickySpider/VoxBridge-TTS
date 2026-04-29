---
id: FEAT-032
type: feature
phase: PHASE-03
sprint: SPRINT-06
status: planned
dependencies: [FEAT-003]
---

# FEAT-032: Transcript Logging

## Summary

Optionally log every spoken message (timestamp + text) to a persistent plaintext transcript
file. Disabled by default. User can open the file from General settings.

## Acceptance Criteria

- [ ] `GeneralSettings` gains `EnableTranscriptLogging` (bool, default false)
- [ ] When enabled, every successful send appends a line to `%AppData%\TtsCommunicationTool\transcript.txt`
- [ ] Line format: `[2026-04-29 14:30:00] Hello world`
- [ ] `ITranscriptService` interface with `LogAsync(string text)`
- [ ] General settings tab shows toggle + "Open Transcript" button
- [ ] "Open Transcript" opens the file in the default text editor (shell execute)
- [ ] File created on first log entry; no error if it doesn't exist yet

## Implementation Notes

- `ITranscriptService` in Core/Interfaces; `TranscriptService` in Infrastructure
- `TranscriptService` checks `IConfigService.CurrentConfig.GeneralSettings.EnableTranscriptLogging` before writing
- Called from `OverlayViewModel` after successful speech dispatch (both send paths)
- `GeneralSettingsViewModel` + `GeneralSettings` updated with new flag
- XAML: toggle in General tab + "Open Transcript" button (disabled when file not found)
