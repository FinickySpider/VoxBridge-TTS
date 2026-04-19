---
id: FEAT-008
type: feature
status: complete
priority: medium
phase: PHASE-01
sprint: SPRINT-02
owner: ""
depends_on: [FEAT-005]
---

# FEAT-008: Text Input Validation

## Description

Implement text validation logic used before TTS generation. Trim input, reject empty/whitespace-only text, enforce maximum character length (500–1000 chars). Show user-friendly feedback on rejection. Validation used by both overlay send and phrase playback paths.

## Acceptance Criteria

- [ ] `TextValidation` class with `Validate(string)` returning `OperationResult`
- [ ] Empty or whitespace-only text rejected
- [ ] Text exceeding max length rejected with user message
- [ ] Text is trimmed before validation
- [ ] Validation result includes user-friendly error message on failure
- [ ] Unit tests cover empty, whitespace, max-length, and valid cases

## Files Touched

| File | Change |
|------|--------|
| `src/TtsCommunicationTool.Core/Validation/TextValidation.cs` | New |
| `src/TtsCommunicationTool.Core/Models/OperationResult.cs` | New |

## Implementation Notes

- Initial max length: 500 characters (adjustable based on Kokoro latency testing)
- Shared by overlay send command and phrase playback command

## Testing

- [ ] Empty string → rejected
- [ ] Whitespace only → rejected
- [ ] 501+ chars → rejected
- [ ] Valid text → accepted
- [ ] Trimming works correctly

## Done When

- [ ] Acceptance criteria met
- [ ] Verified manually
