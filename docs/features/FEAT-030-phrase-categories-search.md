---
id: FEAT-030
type: feature
phase: PHASE-03
sprint: SPRINT-07
status: planned
dependencies: [FEAT-018]
---

# FEAT-030: Phrase Categories, Search, Favorites, Pinning

## Summary

Extend the phrase system so users can organize phrases into named categories, mark favorites,
pin important phrases to the top, and filter/search the list by text or category.

## Acceptance Criteria

- [ ] `PhraseItem` has `Category` (string, nullable), `IsFavorite` (bool), `IsPinned` (bool)
- [ ] Phrases tab: search box filters by name + text (case-insensitive)
- [ ] Phrases tab: category dropdown ("All" + distinct categories from data) filters list
- [ ] Favorites-only filter toggle button (★) in phrase toolbar
- [ ] Pinned phrases sorted to top regardless of other sort
- [ ] Favorite/pin toggleable from selection controls in settings UI
- [ ] New fields default gracefully in existing config (backward compatible)
- [ ] Export/import JSON includes new fields

## Implementation Notes

- `PhraseItem` (Core model): add `Category`, `IsFavorite`, `IsPinned`
- `PhraseListViewModel`: add `FilterText`, `SelectedCategory`, `ShowFavoritesOnly`; expose `FilteredPhrases` (computed `ObservableCollection` or `CollectionView`)
- `PhraseService.GetAll()` returns full list; filtering done in VM
- `SettingsWindow.xaml` Phrases tab: add search TextBox, category ComboBox, ★ toggle button above the ListBox; replace `ItemsSource` binding with `FilteredPhrases`
