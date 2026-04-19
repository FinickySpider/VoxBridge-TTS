
# Conventions

## Status vocabulary (strict)

Use only these:

- `planned`
- `active` (phases and sprints only)
- `in_progress`
- `blocked`
- `review`
- `complete`
- `deprecated`

No other status words are allowed.

## Naming

| Type | Prefix | Example |
|------|--------|---------|
| Phase | `PHASE-XX` | `PHASE-02-core-features.md` |
| Sprint | `SPRINT-XX` | `SPRINT-03.md` |
| Feature | `FEAT-XXX` | `FEAT-014-batch-export.md` |
| Bug | `BUG-XXX` | `BUG-007-export-crash.md` |
| Refactor | `REFACTOR-XXX` | `REFACTOR-004-state-cleanup.md` |
| Decision (ADR) | `ADR-XXXX` | `ADR-0003-storage-choice.md` |

Rules:
- Use zero-padded numbers
- Use kebab-case slugs
- Never reuse or renumber IDs once created

## Cross references

Each work item must reference:
- its phase
- its sprint
- dependencies by ID (if any)

Use standard markdown links with relative paths.

## Claude 4.5 tuning

- Prefer explicit checklists over prose
- Keep scope tight
- Do not invent requirements
- If scope changes, create an ADR
