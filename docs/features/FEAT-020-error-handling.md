---
id: FEAT-020
type: feature
status: planned
priority: high
phase: PHASE-01
sprint: SPRINT-03
owner: ""
depends_on: [FEAT-003, FEAT-005, FEAT-006, FEAT-010]
---

# FEAT-020: Error Handling and User Notifications

## Description

Implement `INotificationService` / `NotificationService` for user-facing error messages, warnings, and status toasts. Wire error handling for all critical failure paths: hotkey registration failures, missing audio devices, TTS generation failures, playback failures, and invalid config. Ensure errors are logged and user is informed with clear, non-technical messages.

## Acceptance Criteria

- [ ] `INotificationService` interface defined with Info, Warning, Error notification methods
- [ ] `NotificationService` shows dark-themed toast/banner notifications
- [ ] Hotkey registration failure shows user-facing message
- [ ] Missing audio device shows warning notification
- [ ] TTS generation failure shows error with useful message
- [ ] Playback failure shows error notification
- [ ] Invalid config triggers reset notification (see REFACTOR-001)
- [ ] All errors also logged via `ILoggingService`
- [ ] No blocking message boxes for recoverable errors
- [ ] Unhandled exceptions caught at app level and logged

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Interfaces/INotificationService.cs` | New |
| `src/TtsCommunicationTool.Infrastructure/Notifications/NotificationService.cs` | New |
| `src/TtsCommunicationTool.App/App.xaml.cs` | Global exception handler |

## Implementation Notes

- Prefer non-blocking dark toasts over system dialogs
- Notification types: info, warning, error
- Keep user messages simple and actionable

## Testing

- [ ] Force TTS failure → error notification shown
- [ ] Unplug audio device → warning shown
- [ ] Conflicting hotkey → error shown
- [ ] No console errors from notification service itself

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
