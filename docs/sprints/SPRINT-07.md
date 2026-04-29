---
id: SPRINT-07
type: sprint
phase: PHASE-03
status: complete
---

# SPRINT-07: Phrase Organization

## Goal

Extend the phrase system with categories, search, favorites, and pinning so users can
efficiently manage large phrase libraries.

## Planned Work

| ID | Title | Status |
|----|-------|--------|
| FEAT-030 | Phrase categories, search, favorites, pinning | planned |

## Notes

- PhraseItem gains: `Category` (string), `IsFavorite` (bool), `IsPinned` (bool).
- PhraseListViewModel gains: `FilterText`, `SelectedCategory`, filtered observable view.
- Settings Phrases tab updated with search box, category selector, and favorite/pin icons.
- PhraseService and config remain backward-compatible (new fields default to empty/false).

## Definition of Done

- Phrases can be assigned to a category
- Category dropdown filters the list
- Search box text-filters by name and text
- Favorite star visible per phrase; favorites-only filter works
- Pinned phrases appear before non-pinned regardless of sort
- Build clean, 0 errors
