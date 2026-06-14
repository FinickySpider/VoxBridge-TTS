# Project Structure

```
TtsCommunicationTool/
├── .github/
│   └── copilot-instructions.md          # Agent instructions for Copilot
├── docs/
│   ├── design/DESIGN.md                 # Authoritative design document
│   ├── index/MASTER_INDEX.md            # Project tracking index
│   ├── features/                        # Feature work item files
│   ├── sprints/                         # Sprint tracking documents
│   ├── phases/                          # Phase tracking documents
│   ├── decisions/                       # Architecture Decision Records
│   ├── templates/                       # Document templates
│   ├── _system/                         # Operating rules and conventions
│   └── retype/                          # Retype documentation site (this)
├── src/
│   ├── TtsCommunicationTool.App/        # Bootstrap, tray, resources
│   │   ├── App.xaml / App.xaml.cs       # Application entry point
│   │   ├── ServiceRegistration.cs       # DI container setup
│   │   ├── TrayIconManager.cs           # System tray icon and menu
│   │   ├── HotkeyHostWindow.cs          # Invisible window for hotkey messages
│   │   ├── OverlayCoordinator.cs        # Overlay singleton management
│   │   ├── SplashWindow.xaml            # Startup splash screen
│   │   ├── WinFormsColorPickerService.cs # Windows colour picker dialog
│   │   ├── Themes/Default.xaml          # Default theme resource dictionary
│   │   └── icon.ico                     # Application icon
│   ├── TtsCommunicationTool.UI/         # Views, ViewModels, commands
│   │   ├── Views/
│   │   │   ├── OverlayWindow.xaml       # Main input overlay
│   │   │   ├── SettingsWindow.xaml      # Settings (8 tabs)
│   │   │   ├── PhraseEditorWindow.xaml  # Per-phrase editor
│   │   │   ├── ToastWindow.xaml         # Toast notifications
│   │   │   ├── ImportProgressWindow.xaml # Import progress dialog
│   │   │   └── ScreenColorPickerWindow.xaml # Eyedropper tool
│   │   ├── ViewModels/                  # MVVM ViewModels
│   │   ├── Converters/                  # WPF value converters
│   │   ├── Commands/                    # RelayCommand, AsyncRelayCommand
│   │   └── Services/
│   │       └── WpfNotificationService.cs # Toast notification implementation
│   ├── TtsCommunicationTool.Core/       # Models, interfaces, validation
│   │   ├── Interfaces/                  # All service interfaces
│   │   ├── Models/                      # Data models (AppConfig, PhraseItem, etc.)
│   │   ├── State/                       # Runtime state singletons
│   │   ├── Validation/                  # Validation logic
│   │   └── Utilities/                   # ContrastCalculator, SilenceTrimmer
│   ├── TtsCommunicationTool.Infrastructure/ # Service implementations
│   │   ├── Config/JsonConfigService.cs  # JSON config persistence
│   │   ├── Tts/
│   │   │   ├── KokoroTtsService.cs      # Kokoro offline TTS
│   │   │   ├── ElevenLabsTtsService.cs  # ElevenLabs cloud TTS
│   │   │   ├── TtsRouter.cs             # Routes between engines
│   │   │   └── StubTtsService.cs        # Test stub
│   │   ├── Audio/
│   │   │   ├── DualOutputAudioRouter.cs # Dual-output playback
│   │   │   └── WasapiAudioDeviceService.cs # Device enumeration
│   │   ├── Hotkeys/GlobalHotkeyService.cs # Win32 RegisterHotKey
│   │   ├── Logging/FileLoggingService.cs # Structured + crash logging
│   │   ├── Phrases/
│   │   │   ├── PhraseService.cs         # Phrase CRUD
│   │   │   └── PhraseCacheService.cs    # WAV file caching
│   │   ├── Themes/ThemeService.cs       # Theme load/apply/persist
│   │   ├── Transcript/TranscriptService.cs # Text transcript logging
│   │   ├── TextReplacement/TextReplacementService.cs # Text substitution
│   │   └── Security/ApiKeyVault.cs      # DPAPI encryption
│   └── TtsCommunicationTool.Tests/      # Unit + integration tests
│       ├── Unit/                        # Unit tests (empty)
│       └── Integration/                 # Integration tests (empty)
├── build.ps1                            # Build script
├── CHANGELOG.md                         # Version history
├── README.md                            # Project overview
├── TtsCommunicationTool.slnx            # Solution file
├── kokoro.onnx                          # Kokoro TTS model file
├── icon.ico.backup                      # Icon backup
└── Splashscreen.png.backup              # Splash screen backup
```
