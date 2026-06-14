# Adding Features

## Development Workflow

1. **Create a feature work item** — Follow the naming convention `FEAT-XXX-description.md` in `docs/features/`
2. **Update tracking** — Add to `MASTER_INDEX.md` and the active sprint
3. **Implement** — Follow MVVM patterns, service interfaces, and async/await
4. **Build** — Run `.\build.ps1` to verify compilation
5. **Update CHANGELOG** — Document changes under the appropriate version
6. **Update docs** — Keep the Retype documentation in sync

## Adding a New Setting

1. Add the property to the appropriate model in Core (e.g., `GeneralSettings`)
2. Add the property to the corresponding ViewModel
3. Add the XAML control in the appropriate Settings tab
4. Wire up load/save in the ViewModel's `LoadFrom`/`ApplyTo` methods
5. Register any new service in `ServiceRegistration.Configure()`

## Adding a New Service

1. Define the interface in `TtsCommunicationTool.Core/Interfaces/`
2. Implement the interface in `TtsCommunicationTool.Infrastructure/`
3. Register in `ServiceRegistration.Configure()` as singleton
4. Inject where needed via constructor DI

## Adding a New View

1. Create the XAML view in `TtsCommunicationTool.UI/Views/`
2. Create the ViewModel in `TtsCommunicationTool.UI/ViewModels/`
3. Register the ViewModel as transient in `ServiceRegistration.Configure()`
4. Open the view from the appropriate coordinator or event handler

## Adding a New TTS Engine

See [TTS Engine Integration](tts-engine-integration.md) for detailed steps.

## Build Script

Always use the build script before committing:

```powershell
# Development iteration (no version bump)
.\build.ps1

# Before committing (bump patch version)
.\build.ps1 -VersionBump patch -Configuration Release
```

## Code Conventions

- Use **file-scoped namespaces**
- Use **primary constructors** where appropriate
- Use **async/await** for all I/O operations
- Never block the UI thread with `.Result` or `.Wait()`
- All errors must be **visible to the user** and **logged to file**
- Follow **MVVM pattern** for all WPF UI work
- Use **service interfaces** (defined in Core) with **implementations** (in Infrastructure)
