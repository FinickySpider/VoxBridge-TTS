# TTS Communication Tool
## Comprehensive Design Document
### Implementation Handoff for VS Code Copilot Agent

---

# 1. Document Purpose

This document is the complete implementation design for the TTS Communication Tool. It is intended to provide enough detail for an implementation agent to build the application without needing additional project context.

This is not a proposal document. It is an engineering and product handoff document.

The document defines:
- product intent
- target user and constraints
- functional requirements
- non-functional requirements
- system architecture
- project structure
- window and UI behavior
- audio routing behavior
- TTS integration behavior
- data models
- services and interfaces
- state transitions
- settings persistence
- error handling
- testing expectations
- packaging assumptions
- phased roadmap
- implementation order

The implementation target is a Windows-only desktop app intended for daily real-time communication in Discord and VRChat.

---

# 2. Product Overview

## 2.1 Product Name
TTS Communication Tool

## 2.2 Product Summary
A lightweight Windows desktop text-to-speech application for a mute user who needs fast, reliable voice communication in Discord and VRChat.

The app functions like a communication prosthetic. It must feel fast, dependable, simple, and emotionally comfortable to use every day.

## 2.3 Core Use Case
The user is in Discord or VRChat and wants to speak without using their voice.

The workflow is:
1. Press a global hotkey.
2. A small overlay input appears above the current application.
3. Type a message.
4. Press Enter.
5. The message is converted to speech.
6. The speech is played to:
   - the user’s monitoring output device
   - a virtual cable output used as microphone input by Discord or VRChat
7. The overlay clears and closes.

## 2.4 Core Product Principle
The product wins by being:
- faster than the current workflow
- less clunky than existing alternatives
- more dependable under pressure
- pleasant enough to use daily

## 2.5 Design Philosophy
Treat the app as a small accessibility-focused utility, not as a general-purpose TTS platform.

This means:
- keep the core flow minimal
- keep advanced features out of the overlay
- prefer reliability over feature count
- keep settings manageable
- avoid accidental speech triggers
- fail visibly, not silently

---

# 3. Target User

## 3.1 Primary User
A mute user who relies on TTS for daily social communication in voice applications such as Discord and VRChat.

## 3.2 User Priorities
In order of importance:
1. speed of access
2. speed of speaking
3. reliability
4. low stress / low cognitive load
5. acceptable voice quality
6. some voice choice
7. dark and comfortable UI

## 3.3 User Environment
- Windows 10 or Windows 11 desktop/laptop
- Discord and/or VRChat installed
- VB-Cable or another virtual audio cable installed separately by the user
- keyboard-first interaction model
- app running in background while other apps are in focus

## 3.4 User Pain Points This App Must Solve
- current TTS workflow is too slow or awkward
- existing tools are bloated or annoying to navigate
- switching windows breaks conversation flow
- too many clicks or steps between thought and speech
- unreliable routing or setup causes frustration

---

# 4. Scope Definition

## 4.1 MVP Scope
The MVP includes:
- Windows desktop app
- system tray behavior
- global hotkey to open overlay
- focused overlay input
- Enter to speak
- Kokoro offline/local TTS as default engine
- dual audio routing to two output devices
- stop playback hotkey
- basic phrase library
- settings window
- settings persistence
- basic setup and test flow
- dark mode only
- clear error feedback

## 4.2 Explicitly Out of Scope for MVP
Do not implement in MVP unless separately approved:
- multiple TTS engines
- ElevenLabs
- advanced phrase folders/tags/search
- transcript logs
- recent messages
- message queueing
- OSC integration
- voice cloning
- mobile support
- cloud sync
- CSS/theme engine
- cutesy visual customization
- autocomplete/prediction

## 4.3 Post-MVP Scope
Covered later in the roadmap section.

---

# 5. Technical Stack

## 5.1 Required Stack
- Language: C#
- Runtime: .NET
- UI Framework: WPF
- Audio Library: NAudio
- Config Storage: JSON on local filesystem
- Primary TTS: Kokoro local/offline integration

## 5.2 Why This Stack
This stack is chosen because it is the best fit for:
- native Windows desktop behavior
- system tray utilities
- global hotkeys
- overlay windows
- lower memory overhead than Electron
- easier integration with Windows audio devices
- easier control of always-on-top window behavior

## 5.3 Runtime Assumptions
- Windows-only deployment
- x64 target
- offline operation after installation
- user installs virtual cable separately

---

# 6. Non-Functional Requirements

## 6.1 Performance
- Overlay should appear quickly after hotkey press.
- Target perceived overlay open latency: under 200 ms where possible.
- TTS generation should feel responsive.
- Idle CPU usage should be minimal.
- Idle memory usage should remain modest for a tray utility.

## 6.2 Reliability
- The app must survive long background runtime.
- Missing devices must not crash the app.
- TTS failures must surface clearly.
- Invalid config should be recoverable.

## 6.3 Accessibility and Comfort
- Dark UI only for MVP.
- Readable default font.
- Clear focus state.
- Clear speaking and failure state.
- Keyboard-first interaction.

## 6.4 Safety of Interaction
- No automatic speech unless initiated explicitly by user action.
- Stop hotkey must interrupt active playback quickly.
- Overlay close must never trigger send implicitly.

## 6.5 Maintainability
- Clear service boundaries.
- Testable modules.
- Config schema versioning.
- Logging for failures.

---

# 7. Core Functional Requirements

## 7.1 Global Hotkey Overlay
The app must register a user-configurable global hotkey that opens the overlay.

Requirements:
- Overlay hotkey configurable in settings.
- Overlay appears centered by default.
- Overlay window is always-on-top.
- Overlay input gains keyboard focus immediately.
- Pressing Esc closes overlay without speaking.
- Clicking outside overlay closes it.
- Clicking X closes it.
- Re-triggering overlay hotkey while overlay is open should refocus existing overlay, not open duplicates.

Notes:
- Support for exclusive fullscreen games is best effort only.
- Borderless fullscreen should be targeted.

## 7.2 Text Input Send
The user must be able to type text and send it quickly.

Requirements:
- Text input auto-focused on overlay open.
- Pressing Enter sends message.
- Optional Send button also triggers send.
- Empty or whitespace-only text does not send.
- Text clears after successful send.
- Overlay closes after successful send.
- Overlay remains open if generation or playback fails and user should be able to retry.

## 7.3 Speech Playback
Sending text must generate TTS audio and play it to both configured outputs.

Requirements:
- Generate speech from current text.
- Normalize or convert to format expected by audio pipeline.
- Play simultaneously to monitor output and secondary output.
- Provide visible speaking state.
- Provide stop playback control via hotkey.

## 7.4 Quick Phrase Basics
Support a small saved phrase list.

Requirements:
- User can add phrase.
- User can edit phrase.
- User can delete phrase.
- User can trigger phrase playback from phrase UI.
- A limited number of phrases may optionally have hotkeys.
- Phrase playback uses same TTS path as free text.

## 7.5 Settings
Support configurable settings without editing files manually.

Requirements:
- Settings window accessible from system tray.
- Settings persist across restarts.
- Hotkeys configurable.
- Audio devices selectable.
- Voice selectable.
- Textbox size configurable.
- Font size configurable.
- Launch behavior configurable.

## 7.6 Setup and Audio Test
Support first-run validation of audio routing.

Requirements:
- Select monitor output.
- Select secondary output.
- Test monitor output.
- Test secondary output.
- Test both outputs.
- Assign overlay hotkey.

## 7.7 System Tray
The app must live unobtrusively in system tray.

Requirements:
- Tray icon present while running.
- Tray menu includes Open Settings, Open Phrase Manager, Exit.
- App minimizes/closes to tray as configured.

## 7.8 Error Handling
Common failures must surface clearly.

Must handle:
- hotkey registration failure
- missing audio output device
- TTS generation failure
- playback failure
- invalid voice selection
- invalid config file content

---

# 8. User Experience Specification

## 8.1 UX Goals
The app should feel:
- immediate
- calm
- trustworthy
- unobtrusive
- forgiving

## 8.2 Overlay UX Rules
- Overlay should be visually quiet.
- Overlay should contain only speaking-related controls.
- Avoid clutter.
- Keep interaction steps minimal.
- State should be obvious.

## 8.3 Overlay States
The overlay has the following states:
- Hidden
- Ready
- Sending
- Speaking
- Error

Hidden: Overlay not visible.

Ready: Overlay visible, input focused, user can type.

Sending: User pressed Enter/Send. TTS generation in progress. UI should disable duplicate sends during this state.

Speaking: Audio playback active. UI may already be closed after successful send in MVP. If overlay remains briefly visible, state indicator should show active speaking.

Error: An operation failed. Display visible error message and preserve user text when possible.

## 8.4 Input Behavior
- Enter triggers send.
- Shift+Enter should insert newline only if multi-line input is enabled. For MVP, prefer single-line or a controlled text box that still treats Enter as send.
- Esc closes overlay.
- Clicking outside closes overlay.
- Tab navigation should be logical.

## 8.5 Feedback Behavior
Minimum visible feedback:
- sending indicator
- speaking indicator
- clear error message on failure

## 8.6 Visual Style
- Dark mode only.
- Clean, soft, minimal.
- No loud visual effects in MVP.
- Readable font.
- Comfortable contrast.

Suggested visual design direction:
- rounded corners
- dark neutral panels
- subtle accent color for active focus
- quiet shadows
- no bright white surfaces

---

# 9. Window and UI Specification

## 9.1 Windows in MVP
The app includes these windows/components:
1. Tray application shell
2. Overlay input window
3. Settings window
4. Phrase manager panel or settings section
5. Optional small toast/notification component

## 9.2 Overlay Window Specification
Purpose: Primary speech entry UI.

Window characteristics:
- borderless
- always-on-top
- non-taskbar window preferred
- centered by default
- fixed or semi-resizable based on settings
- dark theme

Contents:
- Title or subtle label, optional
- text input field
- send button, optional
- close button
- small status text or icon area

Layout priorities:
1. text input must dominate
2. close and status can be minimal
3. no advanced settings in overlay

Behavior:
- opens from hotkey
- closes on Esc, click outside, X, or successful send
- no multiple overlay instances
- maintains last size setting

## 9.3 Settings Window Specification
Purpose: Configuration UI for all non-primary actions.

Sections:
- General
- Hotkeys
- Audio
- Voice
- Phrases
- Appearance

Section details:
- General: launch on startup toggle, optional if implemented; minimize/close to tray behavior; basic onboarding/test actions
- Hotkeys: overlay hotkey; stop playback hotkey; phrase hotkeys if implemented; hotkey validation feedback
- Audio: monitor output device dropdown; secondary output device dropdown; test buttons for each and both; output device status messages
- Voice: voice selection dropdown; engine status text; optional sample test button
- Phrases: list of phrases; add phrase; edit phrase; delete phrase; optional hotkey assignment
- Appearance: textbox width; textbox height; font size; preview if easy, otherwise live apply

## 9.4 Tray Menu Specification
Menu items:
- Open Overlay
- Open Settings
- Open Phrase Manager or Phrases tab
- Exit

Optional later:
- Pause hotkeys
- Restart audio service

---

# 10. Audio System Design

## 10.1 Audio Requirements
The generated TTS audio must be played to two outputs simultaneously:
1. user monitoring output device
2. secondary output device, typically virtual cable

## 10.2 Key Constraints
- Output devices may differ in sample rate expectations.
- Devices may disconnect while app runs.
- Playback must be stoppable.
- Same content must reach both outputs.

## 10.3 Audio Routing Strategy
Use a single generated audio source and fan out to two playback outputs.

Pipeline:
1. Receive generated audio from TTS engine.
2. Convert audio into a normalized in-memory format.
3. Feed same content into two output device playback chains.
4. Start both outputs as close together as possible.
5. Stop both outputs together on stop request.

## 10.4 Audio Format Strategy
Normalize generated audio into a consistent internal format.
Suggested internal format:
- PCM
- 16-bit or 32-bit float depending on NAudio pipeline design
- single sample rate chosen for compatibility

If TTS output differs, resample/convert before playback.

## 10.5 Audio Device Selection
Persist selected device identifiers in settings.

Requirements:
- list available output devices
- display friendly names
- preserve selection across restarts where possible
- if saved device not found, show warning and require reselection or fallback

## 10.6 Device Failure Handling
If either device is missing:
- display clear warning
- allow settings correction
- do not crash
- playback should fail gracefully

## 10.7 Stop Behavior
Stop hotkey must:
- stop both output streams
- release active playback resources if appropriate
- reset speaking state

## 10.8 Test Playback
Settings window must provide test playback buttons:
- Test Monitor
- Test Secondary
- Test Both

Prefer spoken phrase to confirm the exact path.

---

# 11. TTS System Design

## 11.1 Default Engine
Use Kokoro as the default offline/local engine.

## 11.2 TTS Requirements
- local/offline after installation
- at least one bundled voice
- fast enough for daily communication
- no recurring cost

## 11.3 TTS Service Responsibilities
- accept input text
- select voice
- generate audio output
- return normalized audio or file path/stream usable by audio router
- surface clear errors

## 11.4 TTS Input Validation
Before generation:
- trim text
- reject empty input
- enforce maximum length if necessary to prevent pathological requests

Suggested initial max length for MVP:
- 500 to 1000 characters

If above limit:
- show user-friendly message
- do not attempt generation

## 11.5 Voice Handling
Persist selected voice in settings.

MVP voice behavior:
- one default bundled voice required
- one alternate voice optional if integration is simple
- no advanced voice preset system in MVP

## 11.6 Phrase Optimization
MVP can generate phrases live through same TTS path.

Optional optimization if easy:
- cache recent generated phrase audio in memory or on disk

Do not make caching required for MVP completion.

## 11.7 Failure Cases
Handle these visibly:
- engine unavailable
- model/voice missing
- generation timeout or failure
- invalid voice name

---

# 12. Hotkey System Design

## 12.1 Hotkeys in MVP
Required hotkeys:
- overlay toggle/open
- stop playback

Optional:
- phrase hotkeys

## 12.2 Hotkey Requirements
- global registration
- configurable by user
- clear failure message on registration conflict
- no silent registration failures

## 12.3 Hotkey Conflict Handling
If registration fails:
- inform user which hotkey failed
- keep old working hotkey if possible
- suggest choosing a different combination

## 12.4 Overlay Hotkey Behavior
When overlay hotkey pressed:
- if overlay hidden, show overlay
- if overlay visible but not focused, bring to front/focus
- if overlay already focused, keep focused

Avoid treating overlay hotkey as a destructive toggle in MVP. Prefer reliable focus/open behavior.

## 12.5 Stop Hotkey Behavior
When stop hotkey pressed:
- stop active playback immediately if speaking
- no effect if idle

---

# 13. Phrase System Design

## 13.1 MVP Phrase Model
A phrase is a saved reusable text item.

## 13.2 Phrase Fields
Each phrase should store:
- unique id
- display name
- text content
- optional hotkey
- sort order or creation timestamp

## 13.3 Phrase UI Requirements
- list phrases
- create phrase
- edit phrase
- delete phrase
- trigger playback

## 13.4 Phrase Limits
Reasonable initial limit may be set if needed, but not required.

## 13.5 Phrase Execution
Phrase send should follow same pipeline as free text:
- validate text
- generate speech
- route audio
- show state/errors

---

# 14. Persistence and Configuration

## 14.1 Storage Format
Use local JSON config file.

## 14.2 Storage Location
Use a per-user application data path, for example under AppData.

## 14.3 Configuration Requirements
- load on startup
- save on settings changes or explicit save action
- tolerate missing config file
- tolerate corrupted config file gracefully
- support schema version field

## 14.4 Top-Level Config Structure
Suggested top-level config object:
- configVersion
- generalSettings
- hotkeySettings
- audioSettings
- voiceSettings
- overlaySettings
- phraseSettings
- phrases[]

## 14.5 Corrupt Config Handling
If config file is malformed:
- back it up if possible
- create fresh default config
- notify user that settings were reset due to invalid config

---

# 15. Data Model Specification

## 15.1 AppConfig
Fields:
- int ConfigVersion
- GeneralSettings General
- HotkeySettings Hotkeys
- AudioSettings Audio
- VoiceSettings Voice
- OverlaySettings Overlay
- List<PhraseItem> Phrases

## 15.2 GeneralSettings
Fields:
- bool MinimizeToTray
- bool StartWithWindows (optional in MVP)
- bool CloseToTray
- bool ShowNotifications

## 15.3 HotkeySettings
Fields:
- HotkeyBinding OverlayHotkey
- HotkeyBinding StopHotkey
- Dictionary<string, HotkeyBinding> PhraseHotkeys or phrase hotkeys stored directly in phrases

## 15.4 AudioSettings
Fields:
- string MonitorOutputDeviceId
- string SecondaryOutputDeviceId
- string MonitorOutputDeviceNameCache
- string SecondaryOutputDeviceNameCache

Optional future fields:
- float MonitorVolume
- float SecondaryVolume

## 15.5 VoiceSettings
Fields:
- string EngineName
- string SelectedVoiceId
- string SelectedVoiceDisplayName

## 15.6 OverlaySettings
Fields:
- double Width
- double Height
- double FontSize
- double PositionX
- double PositionY

## 15.7 PhraseItem
Fields:
- string Id
- string Name
- string Text
- HotkeyBinding Hotkey
- int SortOrder
- DateTime CreatedUtc
- DateTime UpdatedUtc

## 15.8 HotkeyBinding
Fields:
- bool Ctrl
- bool Alt
- bool Shift
- bool Win
- string Key

---

# 16. Application Architecture

## 16.1 Architectural Style
Use a layered architecture with clearly separated responsibilities.

Suggested pattern:
- WPF + MVVM
- service layer
- domain/config models
- infrastructure layer for OS/audio/TTS integration

## 16.2 Core Modules
1. App Shell
2. Tray Manager
3. Overlay UI
4. Settings UI
5. Hotkey Service
6. Config Service
7. Phrase Service
8. TTS Service
9. Audio Router
10. Notification Service
11. Logging Service

## 16.3 Module Responsibilities
- App Shell: startup, bootstrap, global exception handling
- Tray Manager: tray icon lifecycle and commands
- Overlay UI: input flow, send command, cancel command, view state
- Settings UI: display and edit settings, audio tests, phrase management
- Hotkey Service: register/unregister hotkeys and route events
- Config Service: load/save config, schema versioning, defaults
- Phrase Service: CRUD and validation
- TTS Service: generate audio, enumerate voices, validate engine state
- Audio Router: playback, stop, device enumeration, validation
- Notification Service: user-facing status and errors
- Logging Service: file logging and diagnostics

---

# 17. Suggested Project Structure

```txt
TtsCommunicationTool/
├─ src/
│  ├─ TtsCommunicationTool.App/
│  ├─ TtsCommunicationTool.UI/
│  ├─ TtsCommunicationTool.Core/
│  ├─ TtsCommunicationTool.Infrastructure/
│  └─ TtsCommunicationTool.Tests/
└─ docs/
   └─ ComprehensiveDesignDocument.md
```

If a smaller single-project structure is preferred for speed, keep logical folder boundaries equivalent to the above.

---

# 18. Service Interface Guidance

Required services:
- IConfigService
- IHotkeyService
- IAudioDeviceService
- IAudioRouterService
- ITtsService
- IPhraseService
- INotificationService
- IOverlayCoordinator

Each service should have clear single-responsibility boundaries and be testable.

---

# 19. State Machine and Workflow Design

## 19.1 High-Level App States
- Starting
- Idle
- OverlayOpen
- GeneratingSpeech
- PlayingSpeech
- ErrorRecoverable
- ShuttingDown

## 19.2 Main Workflow

Startup Workflow:
1. App launches.
2. Load config.
3. Initialize services.
4. Create tray icon.
5. Register hotkeys.
6. Validate selected devices and voice.
7. Enter Idle.

Overlay Send Workflow:
1. User presses overlay hotkey.
2. Overlay opens and input gains focus.
3. User enters text.
4. User presses Enter.
5. Validate text.
6. Set state to GeneratingSpeech.
7. Generate TTS audio.
8. If generation fails, show error and return to Ready/Error state.
9. If generation succeeds, route audio to outputs.
10. Close overlay after successful send start.
11. Set state to PlayingSpeech.
12. On completion, return to Idle.

Stop Workflow:
1. User presses stop hotkey.
2. If audio is playing, stop both outputs.
3. Release playback resources.
4. Set state to Idle.

Phrase Playback Workflow:
1. User selects phrase or phrase hotkey triggers.
2. Retrieve phrase text.
3. Run same validation and generation workflow as normal send.
4. Route and play.

---

# 20. Error Handling Specification

## 20.1 Error Handling Principles
- Never fail silently.
- Prefer user-understandable wording.
- Preserve user input when practical.
- Log technical detail separately from user message.

## 20.2 User-Facing Error Messages
Required scenarios:
- hotkey conflict
- missing device
- TTS failure
- playback failure
- invalid config

## 20.3 Logging Requirements
Log at minimum:
- startup failures
- config parse failures
- hotkey registration failures
- TTS generation exceptions
- audio playback exceptions
- unhandled exceptions

---

# 21. Logging and Diagnostics

## 21.1 Logging Purpose
Logging exists to help diagnose failures without burdening the user.

## 21.2 MVP Logging Scope
- write internal logs to local file
- include timestamps
- include severity
- include exception details

## 21.3 Log Levels
- Info
- Warning
- Error

---

# 22. Setup and First-Run Experience

## 22.1 First-Run Goals
- minimize confusion
- get audio working quickly
- avoid hidden setup failure

## 22.2 Minimum First-Run Flow
1. Launch app.
2. Open settings automatically on first run or prompt from tray.
3. Select monitor output device.
4. Select secondary output device.
5. Test monitor output.
6. Test secondary output.
7. Test both outputs.
8. Confirm overlay hotkey.
9. Save settings.

## 22.3 First-Run Defaults
- dark mode enabled
- reasonable overlay size
- bundled default voice selected
- safe default hotkey suggested

---

# 23. Packaging and Deployment Assumptions

## 23.1 Packaging Goals
- simple installation
- local bundled assets where practical
- no complex external setup beyond virtual cable requirement

## 23.2 Installation Assumptions
Installer should include:
- application binaries
- required runtime if self-contained publish used
- Kokoro integration dependencies
- at least one default voice/model if feasible

## 23.3 External Dependency Not Installed by App
- VB-Cable or equivalent virtual cable must be installed by user

## 23.4 Publish Strategy
Preferred options:
- self-contained Windows x64 publish
- standard installer or portable distribution depending time constraints

## 23.5 Upgrade Considerations
- config migration via configVersion
- preserve phrases/settings between updates

---

# 24. Testing Specification

## 24.1 Testing Priorities
This app is more sensitive to integration issues than to pure business logic issues, so test both unit logic and real environment behavior.

## 24.2 Unit Test Targets
- config load/save
- config migration/defaulting
- phrase CRUD validation
- hotkey binding validation
- text validation
- state transitions in view models

## 24.3 Integration Test Targets
- overlay opens from hotkey
- overlay focuses input correctly
- text send calls TTS service
- TTS output reaches audio router
- dual output playback works with selected devices
- stop hotkey interrupts playback
- phrase playback uses same path as typed text

## 24.4 Manual Test Matrix
Test manually on:
- Windows 10 and/or 11
- Discord open and using virtual cable input
- VRChat if available
- no secondary device present
- unplugged/reconnected audio device
- conflicting hotkey
- corrupted config file
- long text input
- repeated rapid sends

## 24.5 Regression Focus Areas
After any core change, re-test:
- overlay hotkey
- Enter send
- stop hotkey
- dual routing
- settings persistence
- phrase playback

---

# 25. Acceptance Criteria

## 25.1 Phase 1 Acceptance Criteria
The phase is complete when all of the following are true:
- app launches and runs in system tray
- overlay opens reliably from configured hotkey
- overlay input is automatically focused
- user can type text and press Enter to speak
- empty input is rejected safely
- speech is generated using Kokoro
- speech plays to both configured outputs
- stop hotkey interrupts playback
- user can choose output devices in settings
- user can test outputs in settings
- user can create/edit/delete/play phrases
- settings persist across restart
- app handles missing devices and hotkey conflicts visibly
- app is usable for real-world trial in Discord/VRChat workflow

## 25.2 Quality Bar
The MVP does not need to be visually fancy. It does need to feel:
- stable
- responsive
- understandable
- less frustrating than the current tool

---

# 26. Build Order and Milestones

## 26.1 Milestone 1: Foundation
Implement:
- solution structure
- config models
- config service
- app shell
- tray icon/menu
- logging

## 26.2 Milestone 2: Overlay
Implement:
- overlay window
- overlay view model
- open/focus/close logic
- input handling

## 26.3 Milestone 3: Hotkeys
Implement:
- global hotkey registration
- overlay hotkey
- stop hotkey
- failure reporting

## 26.4 Milestone 4: TTS Bring-Up
Implement:
- Kokoro integration
- voice enumeration or fixed default voice
- text to audio generation

## 26.5 Milestone 5: Audio Playback
Implement:
- device enumeration
- single output playback
- dual output playback
- stop playback
- test buttons

## 26.6 Milestone 6: Phrase System
Implement:
- phrase model
- phrase CRUD UI
- phrase playback
- optional phrase hotkeys

## 26.7 Milestone 7: Hardening
Implement:
- error messaging
- missing device handling
- invalid config recovery
- polish of settings flow

---

# 27. Future Roadmap

## 27.1 Priority Group A: Usability Hardening
- overlay position adjustment
- better font/UI size controls
- hotkey conflict detection improvements
- volume per output
- recent phrases/messages
- import/export phrases
- better settings organization
- improved onboarding flow

## 27.2 Priority Group B: Joy / Comfort Layer
- theme presets
- cutesy visual options
- better phrase presentation
- subtle animations
- better send/success feedback

## 27.3 Priority Group C: Phrase System Expansion
- phrase categories/folders
- phrase search/filter
- favorites/pinning
- repeat last/resend
- transcript logging

## 27.4 Priority Group D: Voice / Expression Expansion
- voice preset switching
- speed/pitch controls
- optional premium voice path
- cached audio for quick phrases

## 27.5 Priority Group E: Advanced / Deferred
- message queue system
- VRChat OSC integration
- multiple extra TTS engines
- voice cloning
- cloud sync
- mobile companion
- phrase prediction/autocomplete

---

# 28. Implementation Guardrails

- Protect the core loop.
- Keep the overlay focused.
- No silent failures.
- No accidental speech.
- Avoid engine sprawl.
- Be honest about fullscreen support.

---

# 29. Recommended Defaults

- Dark mode enabled
- centered overlay
- medium overlay width
- readable font size
- bundled default Kokoro voice
- safe default overlay hotkey
- safe default stop hotkey
- close to tray enabled

Suggested safe default hotkeys:
- Overlay: Ctrl + Shift + Space
- Stop: Ctrl + Shift + Backspace

---

# 30. Final Definition of Success

This project is successful when the user can communicate more smoothly and with less stress than before.

Technically, that means:
- the app is always ready
- the overlay opens fast
- the user can type and speak quickly
- the speech is heard locally and sent through virtual cable
- mistakes can be canceled easily
- the UI feels calm and comfortable
- the app is dependable enough for real daily use

The application should feel like a caring, minimal, dependable tool rather than a feature-heavy control panel.

---

# 31. Addendum References

The following companion files are part of this implementation handoff and should be read together with this document:

1. `ADDENDUM_A_FILE_BY_FILE_SCAFFOLD.md`
2. `ADDENDUM_B_JSON_SCHEMA_AND_SAMPLE_CONFIG.md`
3. `ADDENDUM_C_UI_LAYOUT_AND_WINDOW_SPEC.md`
4. `ADDENDUM_D_COMMAND_EVENT_FLOW_AND_STATE_MAP.md`
5. `ADDENDUM_E_IMPLEMENTATION_CHECKLIST_AND_ACCEPTANCE_MATRIX.md`

These addenda provide concrete implementation structure, schema examples, UI layout guidance, event flow detail, and delivery checklists for the app.
