---
id: FEAT-003
type: feature
status: complete
priority: medium
phase: PHASE-01
sprint: SPRINT-01
owner: ""
depends_on: [FEAT-001]
---

# FEAT-003: File Logging Service

## Description

Implement `ILoggingService` / `FileLoggingService` that writes timestamped diagnostic logs to `%AppData%\TtsCommunicationTool\logs\app.log`. Support Info, Warning, and Error severity levels. Used by all services to record failures and diagnostics.

## Acceptance Criteria

- [ ] `ILoggingService` interface defined with methods for Info, Warning, Error
- [ ] `FileLoggingService` writes to `%AppData%\TtsCommunicationTool\logs\app.log`
- [ ] Log entries include timestamp, severity, and message
- [ ] Error entries include exception details when provided
- [ ] Log file created on first write if directory/file doesn't exist
- [ ] No crashes from logging failures (logging is best-effort)

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/ILoggingService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Logging/FileLoggingService.cs` | New |

## Implementation Notes

- Keep implementation simple for MVP — no external logging framework required
- Consider log rotation later if file grows large

## Testing

- [ ] Log file created with correct content
- [ ] No crash when log directory is missing (auto-create)

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
