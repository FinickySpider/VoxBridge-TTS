# TTS Communication Tool — Copilot Agent Instructions

## Identity

You are a coding agent working on the **TTS Communication Tool**, a Windows-only desktop text-to-speech application built with C# / .NET / WPF / NAudio / Kokoro TTS. The app is a communication prosthetic for a mute user who needs fast, reliable voice communication in Discord and VRChat.

---

## Start Procedure (MANDATORY)

1. Read `docs/index/MASTER_INDEX.md` — this is your single source of truth.
2. Read `docs/design/DESIGN.md` — the authoritative design document.
3. Read all files in `docs/_system/` — conventions, lifecycle, and operating rules.
4. Identify the **active phase** and **active sprint** from MASTER_INDEX.
5. Work ONLY on items listed in the active sprint.

If `DESIGN.md` is missing or incomplete, **stop and request it**. Do not invent product direction.

---

## Work Rules (STRICT)

### Scope Control
- **Only work on items listed in the active sprint.** No exceptions.
- **Do not silently expand scope.** If something seems missing, create a new work item with an ID — do not fold it into an existing task.
- **Do not invent requirements.** Every implementation must trace back to the design document or an explicit work item.
- If scope changes are necessary, **create an ADR** documenting the decision.

### Implementation Standards
- Prefer **minimal viable implementation** that satisfies acceptance criteria.
- Keep changes **traceable**: acceptance criteria → implementation → verification.
- Follow **MVVM pattern** for all WPF UI work.
- Use **service interfaces** (defined in Core) with **implementations** (in Infrastructure).
- Use **async/await** for I/O and TTS operations — never block the UI thread.
- All errors must be **visible to the user** and **logged to file**. No silent failures.

### Document Maintenance (CRITICAL)
After completing ANY task, update ALL of the following:
- The **feature/bug/refactor** work item file (check off acceptance criteria, update status)
- The **sprint** document (update item status)
- The **phase** document (check completion criteria if applicable)
- `docs/index/MASTER_INDEX.md` (update status, move items in/out of "In Progress")
- `docs/decisions/DECISION_LOG.md` (if any ADRs were created)

### Sprint Boundaries
- At the **start of every new sprint**, read through ALL files in `/docs` and create any missing files (features, ADRs, sprints, phases).
- Use templates from `docs/templates/` when creating new files.
- Follow naming conventions from `docs/_system/conventions.md` exactly.

---

## Naming Conventions

| Type | Prefix | Example |
|------|--------|---------|
| Phase | `PHASE-XX` | `PHASE-02-usability-hardening.md` |
| Sprint | `SPRINT-XX` | `SPRINT-03.md` |
| Feature | `FEAT-XXX` | `FEAT-014-settings-hotkeys.md` |
| Bug | `BUG-XXX` | `BUG-007-export-crash.md` |
| Refactor | `REFACTOR-XXX` | `REFACTOR-004-state-cleanup.md` |
| ADR | `ADR-XXXX` | `ADR-0003-naudio-dual-routing.md` |

- Zero-padded numbers, kebab-case slugs.
- **Never reuse or renumber IDs once created.**

---

## Status Vocabulary (STRICT)

Use ONLY these status values — no synonyms, no variations:

- `planned`
- `active` (phases and sprints only)
- `in_progress`
- `blocked`
- `review`
- `complete`
- `deprecated`

---

## Lifecycle Rules

| Entity | Flow |
|--------|------|
| Phase | planned → active → complete |
| Sprint | planned → active → complete |
| Feature | planned → in_progress → review → complete (can enter blocked) |
| Bug | planned → in_progress → complete |
| Refactor | planned → in_progress → complete |
| ADR | planned → complete → deprecated (if superseded) |

A sprint is complete when all items are either **complete** or **explicitly deferred** to another sprint (recorded in the sprint doc).

---

## Technical Context

- **Language:** C# on .NET 8+ (Windows x64)
- **UI:** WPF with MVVM, dark theme only
- **Audio:** NAudio (WASAPI device enumeration, dual-output playback)
- **TTS:** Kokoro (local/offline, bundled voice models)
- **Config:** JSON at `%AppData%\TtsCommunicationTool\config.json`
- **Logs:** `%AppData%\TtsCommunicationTool\logs\app.log`
- **Hotkeys:** Windows `RegisterHotKey` via P/Invoke

### Solution Structure
```
src/
├─ TtsCommunicationTool.App/           # Bootstrap, tray, resources
├─ TtsCommunicationTool.UI/            # Views, ViewModels, commands
├─ TtsCommunicationTool.Core/          # Models, interfaces, validation, state
├─ TtsCommunicationTool.Infrastructure/ # Config, hotkeys, audio, TTS, logging
└─ TtsCommunicationTool.Tests/         # Unit + integration tests
```

### Key Service Interfaces
- `IConfigService` — config load/save/defaults
- `IHotkeyService` — global hotkey registration
- `IAudioDeviceService` — device enumeration
- `IAudioRouterService` — dual-output playback
- `ITtsService` — speech generation
- `IPhraseService` — phrase CRUD
- `INotificationService` — user-facing messages
- `IOverlayCoordinator` — overlay singleton management
- `ILoggingService` — file-based diagnostics

---

## Cross-Reference Requirements

Every work item must reference:
- Its **phase**
- Its **sprint**
- **Dependencies** by ID (if any)

Use standard markdown links with relative paths.

---

## Global Tracking Documents

Always keep these up to date:
- `docs/decisions/DECISION_LOG.md`
- `docs/index/MASTER_INDEX.md`
- `docs/index/ROADMAP.md`

And local checklists within:
- Feature files (acceptance criteria)
- Sprint files (planned work status)
- Phase files (completion criteria)

---

## Error Handling Philosophy

- Never fail silently.
- Prefer user-understandable wording.
- Preserve user input when practical.
- Log technical detail separately from user messages.
- No automatic speech unless explicitly initiated by user action.

---

## Testing Expectations

- **Unit tests** for: config load/save, text validation, phrase CRUD, hotkey validation, state transitions
- **Integration tests** for: overlay open/close, TTS generation, dual audio playback, stop hotkey
- **Manual testing** on Windows 10/11 with Discord and VB-Cable

---

## Build Automation (MANDATORY FOR EVERY TASK)

### Build Script

**Location:** `build.ps1` at the root of the workspace (`e:\Projects\tts\build.ps1`)

**What it does:**
1. Kills any running `TtsCommunicationTool.App.exe` process
2. (Optional) Bumps the version in `.csproj` based on arguments
3. (Optional) Updates `CHANGELOG.md` with new version header
4. Runs `dotnet build` to compile the project

### Build Script Usage

Run from PowerShell in the project root:

```powershell
# No version bump (build with current version)
.\build.ps1

# With specific configuration
.\build.ps1 -Configuration Release
.\build.ps1 -Configuration Debug

# Bump PATCH version (0.9.2 → 0.9.3)
.\build.ps1 -VersionBump patch

# Bump MINOR version (0.9.2 → 0.10.0, PATCH resets to 0)
.\build.ps1 -VersionBump minor

# Bump MAJOR version (0.9.2 → 1.0.0, MINOR/PATCH reset to 0)
.\build.ps1 -VersionBump major

# Combine options
.\build.ps1 -VersionBump minor -Configuration Release
```

### When Version is Bumped

When any `VersionBump` argument (patch/minor/major) is used:
1. `.csproj` version fields are updated (Version, AssemblyVersion, FileVersion)
2. `CHANGELOG.md` gets a new version header with template sections for Features, Bug Fixes, and Improvements
3. **YOU MUST edit `CHANGELOG.md` after the build** to fill in the actual changes
4. The build proceeds and succeeds

### Building Strategy

- **During development:** Use `.\build.ps1` (no bump) to iterate
- **Before committing fixes:** Use `.\build.ps1 -VersionBump patch` (or minor/major as appropriate)
- **Always update CHANGELOG.md** after version bumps to describe changes in detail
- **Always run the build script before ending a task** to verify compilation succeeds

### Post-Task Checklist

After implementing any feature, bug fix, or refactor, **ALWAYS execute:**

```powershell
# Example: Bump patch after a bug fix, build Release config
.\build.ps1 -VersionBump patch -Configuration Release
```

Then:
1. ✅ Verify build output shows **0 errors, 0 warnings**
2. ✅ Update `CHANGELOG.md` with detailed change descriptions
3. ✅ Confirm the executable exists at `src\TtsCommunicationTool.App\bin\Release\net10.0-windows\TtsCommunicationTool.App.exe`

---

## When in Doubt

- Check `docs/index/MASTER_INDEX.md` first.
- Check `docs/design/DESIGN.md` for product intent.
- Check `docs/_system/conventions.md` for naming and status rules.
- If something isn't in the design, **don't build it**.
- If the design is ambiguous, **create an ADR** to record the interpretation.
