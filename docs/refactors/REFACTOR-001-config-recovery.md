---
id: REFACTOR-001
type: refactor
status: complete
risk: low
phase: PHASE-01
sprint: SPRINT-03
owner: ""
---

# REFACTOR-001: Config Recovery and Corrupt-File Handling

## Purpose

Ensure the config service handles corrupt or malformed JSON config files gracefully: back up the invalid file, create a fresh default config, and notify the user that settings were reset. This prevents startup crashes from bad config state.

## Scope

### In Scope

- Detect JSON parse failures during config load
- Copy corrupt file to `config.invalid.backup.json`
- Generate fresh default config
- Notify user via `INotificationService` that settings were reset
- Log the error via `ILoggingService`
- Continue app startup normally

### Out of Scope

- UX changes
- Feature additions
- Config migration between schema versions (future work)

## Plan

1. Wrap `JsonConfigService.Load()` in try/catch for deserialization exceptions
2. On catch: copy current file to backup path
3. Call `CreateDefaults()` and save new config
4. Emit notification and log entry
5. Return default config to caller

## Validation

- [ ] Corrupt JSON file → app starts with defaults, backup created
- [ ] Missing file → app starts with defaults (no backup needed)
- [ ] Valid file → app loads normally

## Done When

- [ ] Validation complete
