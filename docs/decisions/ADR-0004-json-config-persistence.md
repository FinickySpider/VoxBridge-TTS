---
id: ADR-0004
type: decision
status: complete
date: 2026-04-18
supersedes: ""
superseded_by: ""
---

# ADR-0004: Local JSON File for Settings Persistence

## Context

The application needs to persist user settings (hotkeys, audio devices, voice selection, overlay dimensions, phrases) across restarts. Options considered: JSON file, SQLite, Windows Registry, and XML config.

## Decision

Use a **local JSON config file** at `%AppData%\TtsCommunicationTool\config.json` with schema versioning via a `configVersion` field.

## Consequences

### Positive

- Human-readable and easy to debug
- Simple to implement with `System.Text.Json`
- Schema versioning enables future migrations
- Portable — easy to back up or transfer
- No external dependencies

### Negative

- No transactional writes (risk of corruption on crash during save)
- Flat file — not suitable for large datasets (acceptable for config + small phrase list)
- No built-in query capability (not needed for this use case)

## Links

- Related items:
  - FEAT-002
  - REFACTOR-001
