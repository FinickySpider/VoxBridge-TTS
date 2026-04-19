---
id: FEAT-002
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-01
owner: ""
depends_on: [FEAT-001]
---

# FEAT-002: Config Models and JSON Persistence

## Description

Implement the `AppConfig` data model hierarchy and the `IConfigService` / `JsonConfigService` for loading, saving, and defaulting settings from a JSON file at `%AppData%\TtsCommunicationTool\config.json`. Include schema versioning via `configVersion` field.

## Acceptance Criteria

- [ ] All config model classes created: `AppConfig`, `GeneralSettings`, `HotkeySettings`, `AudioSettings`, `VoiceSettings`, `OverlaySettings`, `PhraseItem`, `HotkeyBinding`
- [ ] `IConfigService` interface defined with Load, Save, and GetDefaults methods
- [ ] `JsonConfigService` loads config from disk or creates defaults if missing
- [ ] Config saved to `%AppData%\TtsCommunicationTool\config.json`
- [ ] `configVersion` field present and set to 1
- [ ] Default values match design doc (overlay 720×140, fontSize 18, Ctrl+Shift+Space, etc.)
- [ ] Unit tests for load, save, and default creation

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Models/AppConfig.cs` | New |
| `src/TtsCommunicationTool.Core/Models/GeneralSettings.cs` | New |
| `src/TtsCommunicationTool.Core/Models/HotkeySettings.cs` | New |
| `src/TtsCommunicationTool.Core/Models/AudioSettings.cs` | New |
| `src/TtsCommunicationTool.Core/Models/VoiceSettings.cs` | New |
| `src/TtsCommunicationTool.Core/Models/OverlaySettings.cs` | New |
| `src/TtsCommunicationTool.Core/Models/PhraseItem.cs` | New |
| `src/TtsCommunicationTool.Core/Models/HotkeyBinding.cs` | New |
| `src/TtsCommunicationTool.Core/Interfaces/IConfigService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Config/JsonConfigService.cs` | New |

## Implementation Notes

- Use `System.Text.Json` for serialization
- Handle missing file gracefully — create defaults
- Handle corrupt file — backup and recreate (detailed in REFACTOR-001)

## Testing

- [ ] Config round-trip: save then load returns same values
- [ ] Missing file creates valid defaults
- [ ] No console errors

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
