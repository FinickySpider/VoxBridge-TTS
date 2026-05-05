
# Roadmap

Keep this file lightweight. Details live in phases and sprints.

## Milestones

### Phase 1 — Foundation & Core Loop (MVP)
- Solution scaffold, config, logging, tray shell
- Overlay window with hotkey, input, send
- Kokoro TTS integration
- Dual audio routing (monitor + virtual cable)
- Stop playback hotkey
- Settings window (all sections)
- Phrase CRUD and playback
- First-run setup flow
- Error handling and notifications

### Phase 2 — Usability Hardening
- Overlay position adjustment
- Per-output volume controls
- Hotkey conflict detection improvements
- Recent phrases/messages
- Phrase import/export
- Settings organization and onboarding improvements

### Phase 3 — Comfort & Expression
- Theme presets and visual polish
- Phrase categories, search, favorites
- Transcript logging
- Voice speed/pitch controls
- Cached phrase audio
- Optional premium voice path

### Phase 4 — Dynamic Theming MVP
- Full XAML refactor: all hardcoded hex literals → DynamicResource
- ThemeSettings model + ThemeService infrastructure
- Built-in immutable themes (Default Dark / Catppuccin Mocha)
- User theme CRUD with preset dropdown (Save As, Duplicate, Reset)
- Live color editing with native color picker
- Tray icon emergency reset to default theme

### Phase 5 — Theme Polish & Advanced
- Typography controls (UI font, base size, overlay font)
- Shape & density controls (corner radius, border thickness, spacing density)
- WCAG 2.1 AA contrast checker live in Theme tab
- Theme import / export (.ttstheme JSON files)

## Notes
- Strategic changes should be recorded as ADRs.
- Post-MVP phases are tentative and will be refined based on real-world user feedback.
