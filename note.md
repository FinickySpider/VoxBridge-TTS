# TTS Communication Tool (VoxBridge) - Release v1.0.3

## 🎉 Official Release Announcement

After extensive development and refinement, we're proud to announce the official release of **TTS Communication Tool (VoxBridge) v1.0.3** - a comprehensive Windows desktop application designed as a communication prosthetic for mute users who need fast, reliable voice communication in Discord and VRChat.

## 📋 What This Program Does

TTS Communication Tool is a Windows-only desktop text-to-speech application built with C#/.NET/WPF that provides:

- **Instant Voice Communication**: Generate speech from typed text with a single hotkey
- **Dual Audio Routing**: Send TTS output to both your headphones and virtual microphone (VB-Cable) simultaneously
- **Global Hotkeys**: System-wide keyboard shortcuts that work even when the app is minimized
- **Phrase Management**: Save and organize frequently used phrases for quick access
- **Customizable Overlay**: Floating text input window that stays on top of other applications
- **Offline TTS**: Uses Kokoro TTS engine with bundled voice models (no internet required)
- **Theme System**: Fully customizable appearance with live preview and WCAG contrast checking

## 🚀 Key Features in v1.0.3

### Core Functionality
- **Overlay Window**: Floating text input that stays on top of games/applications
- **Global Hotkeys**: RegisterHotKey system for instant speech activation
- **Dual Audio Playback**: NAudio-based routing to both playback and recording devices
- **Kokoro TTS**: Local, offline speech synthesis with bundled voice models
- **Phrase System**: Save, organize, and quick-access frequently used phrases
- **Config Persistence**: JSON configuration with automatic backup/restore

### Theme & Customization
- **Complete Theme System**: 17-color palette with live preview
- **Typography Controls**: Separate UI and overlay font settings
- **Shape & Density**: Corner radius, border thickness, spacing density controls
- **WCAG 2.1 Contrast Checker**: Automatic accessibility compliance checking
- **Theme Import/Export**: Save and share custom themes as `.ttstheme` files
- **Built-in Presets**: Multiple pre-configured themes included

### Settings & Configuration
- **Audio Device Selection**: WASAPI device enumeration and selection
- **Per-Output Volume**: Independent volume control for each audio output
- **Hotkey Configuration**: Customizable global keyboard shortcuts
- **Appearance Settings**: Overlay position persistence, font customization
- **First-Run Setup**: Guided initial configuration wizard
- **Error Handling**: User-friendly error messages with detailed logging

## 🛠️ Technical Specifications

- **Platform**: Windows 10/11 (x64)
- **Framework**: .NET 8+ with WPF UI
- **Audio Engine**: NAudio for WASAPI device enumeration and dual-output playback
- **TTS Engine**: Kokoro (local/offline, bundled voice models)
- **Configuration**: JSON at `%AppData%\TtsCommunicationTool\config.json`
- **Logs**: `%AppData%\TtsCommunicationTool\logs\app.log`
- **Hotkeys**: Windows `RegisterHotKey` via P/Invoke

## 📁 Project Structure

```
src/
├─ TtsCommunicationTool.App/           # Bootstrap, tray, resources
├─ TtsCommunicationTool.UI/            # Views, ViewModels, commands
├─ TtsCommunicationTool.Core/          # Models, interfaces, validation, state
├─ TtsCommunicationTool.Infrastructure/ # Config, hotkeys, audio, TTS, logging
└─ TtsCommunicationTool.Tests/         # Unit + integration tests
```

## 🔧 Recent Improvements (v1.0.3)

### Bug Fixes
- **Symbol Corruption**: Fixed encoding issues with special characters (em dash, warning symbols, etc.)
- **Overlay Corner Radius**: Status bar now correctly uses dynamic corner radius resources
- **Build Configuration**: All releases now use Release binaries (not Debug)
- **Theme Persistence**: Fixed theme name persistence after preset switches

### Theme System Enhancements
- **Live Preview Panel**: Instant visual feedback for all theme changes
- **Contrast Calculator**: WCAG 2.1 compliance checking with status indicators
- **Typography Separation**: Independent UI and overlay font controls
- **Shape Controls**: Corner radius, border thickness, and spacing density adjustments

## 🎯 Target Use Cases

1. **Discord Communication**: For mute users who need to communicate in voice channels
2. **VRChat Voice Chat**: Text-to-speech for virtual reality social platforms
3. **Gaming Communication**: In-game voice chat alternatives
4. **Accessibility Tool**: Speech assistance for various communication needs
5. **Streaming Tool**: Text-to-speech for live streaming commentary

## 📦 Installation & Setup

1. **Download**: Get the latest release from the GitHub repository
2. **First Run**: The setup wizard guides you through audio device selection
3. **Audio Configuration**: Select your headphones and virtual microphone (VB-Cable)
4. **Hotkey Setup**: Configure global shortcuts for instant speech
5. **Phrase Organization**: Save your most frequently used phrases

## 🔄 Build & Deployment

The project includes a comprehensive build system:
- **Build Script**: `build.ps1` with version bumping and changelog management
- **Release Configuration**: Automatic Release builds for distribution
- **Version Management**: Semantic versioning with automatic assembly version updates
- **Changelog Automation**: Template-based changelog generation

## 📚 Documentation

Complete end-user documentation is available including:
- Getting Started guides
- User guides for all features
- Troubleshooting assistance
- Screenshot-based walkthroughs

## 🏆 Why This Release Matters

This v1.0.3 release represents a stable, production-ready version of the TTS Communication Tool with:

- **Reliability**: Extensive error handling and logging
- **Customization**: Complete theme system with accessibility features
- **Performance**: Optimized audio routing and TTS generation
- **Usability**: Intuitive interface with comprehensive settings
- **Maintainability**: Clean architecture with separation of concerns

## 🔮 Future Roadmap

While v1.0.3 is a complete release, future enhancements may include:
- Additional voice models
- Enhanced phrase management
- Advanced audio processing
- Integration with more communication platforms
- Mobile companion applications

---

**TTS Communication Tool (VoxBridge) v1.0.3** is now ready for production use, providing a reliable, customizable, and accessible communication solution for mute users across various platforms and applications.