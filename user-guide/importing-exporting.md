# Importing & Exporting

The app supports importing and exporting data for portability and backup.

## What Can Be Imported/Exported

| Data | Format | File Extension | Where |
|------|--------|---------------|-------|
| Phrases | JSON | `.json` | Settings → Phrases tab |
| Text Replacements | JSON | `.json` | Settings → Replacements tab |
| Themes | JSON | `.ttstheme` | Settings → Theme tab |

## Import/Export Buttons

Import and Export buttons appear in the bottom action bar of the Settings window when the active tab supports them:

| Tab | Import | Export |
|-----|--------|--------|
| Replacements (tab 5) | ✓ | ✓ |
| Phrases (tab 6) | ✓ | ✓ |
| Theme (tab 7) | ✓ | ✓ |

## Phrase Import/Export

### Export
Exports all phrases as a JSON array. Each phrase includes all fields (name, text, category, favorites, voice overrides, etc.).

### Import
- Phrases are **added** to the existing list
- Conflicting names are adjusted automatically
- Imported hotkeys are cleared to prevent conflicts
- A summary dialog shows: count added, names changed, hotkeys cleared

## Theme Import/Export

### Export
Saves the current theme as a `.ttstheme` JSON file. All colour, typography, shape, and overlay settings are included.

### Import
- Validates all required fields on load
- Returns a user-editable theme (not applied automatically)
- Must be saved via the theme editor to persist

## Text Replacement Import/Export

### Export
Exports all replacement rules as a JSON array.

### Import
Replaces the current rule list with the imported rules.
