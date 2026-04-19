# TTS Communication Tool - MVP Specification

## Project Overview

This project is a desktop text-to-speech application designed for a mute user who needs fast, reliable voice communication in Discord and VRChat. The app provides a lightweight, always-accessible overlay that allows the user to type text and instantly convert it to speech output that routes to both their speakers (for monitoring) and a virtual audio cable (which acts as a microphone input to other applications).

**Target User:** A mute individual who relies on TTS for daily social communication in voice-enabled applications.

**Core Problem Being Solved:** The user needs a faster, less clunky, and more customizable alternative to their current TTS solution (TTS Voice Wizard) that provides instant access via hotkey, dual audio routing for application compatibility, and better voice options without requiring paid services.

**Client Intent:** Build a focused, practical communication tool that feels natural to use in stressful or fast-paced social situations, with the flexibility to add customization and polish in future versions.

---

## Product Goal

Deliver a lightweight, keyboard-accessible TTS application that:
- Provides **instant access** via global hotkey overlay
- Routes speech to **both monitoring output and virtual microphone**
- Requires **minimal steps** from thought to speech
- Feels **responsive and unobtrusive** during use
- Supports **quick preset phrases** for common responses
- Works reliably with **Discord and VRChat**

The MVP must be immediately usable for daily communication without frustration or delay.

---

## Core User Needs

Based on the project materials, the user needs:

1. **Speed:** Access the TTS input from anywhere instantly via hotkey
2. **Simplicity:** Type → Enter → Speak, with minimal UI friction
3. **Dual Audio Routing:** Hear the speech herself while also sending it to Discord/VRChat as mic input
4. **Quick Phrases:** Pre-saved common responses for faster communication
5. **Free TTS Voice:** Avoid recurring costs while maintaining acceptable voice quality
6. **Dark Mode UI:** Comfortable visual interface (explicitly requested: "fuck lightmodes")
7. **Reliability:** Works consistently across games and applications without breaking focus
8. **Optional Premium Voice:** Ability to use ElevenLabs API when desired, but not required

---

## MVP Features

### 1. Global Hotkey Overlay
**What it does:**  
A user-configurable hotkey (e.g., Ctrl+Shift+Space) opens a transparent text input overlay on top of any running application, including fullscreen games.

**Why it belongs in MVP:**  
This is the primary access method for the entire application. Without instant hotkey access, the tool cannot meet the core need for fast communication.

**Source support:**  
"To have settable hotkey to pull it up over anything at any given time to open the chat box to type in"

---

### 2. Text Input Box with Speech Trigger
**What it does:**  
A simple, resizable textbox where the user types their message. Pressing Enter or clicking a Send button triggers TTS playback and closes the overlay. The textbox automatically clears after each message is sent (default behavior).

**Why it belongs in MVP:**  
The core interaction loop. This is the "box" mentioned in the top 3 priorities: "audio, voices, and the box." Auto-clear prevents accidental re-sending and speeds up the next message.

**Source support:**  
"textbox with enter button (Also functions with Enter key)"  
"Controllable textbox size"

---

### 3. Transparent Overlay Design
**What it does:**  
The overlay appears with a transparent or semi-transparent background, slight edge dimming for visibility, an X button to close, and auto-closes when the user clicks outside the overlay area.

**Why it belongs in MVP:**  
Ensures the overlay is unobtrusive and doesn't block important visual information in games or apps.

**Source support:**  
"Overlay to be transparent with slight dimming around the edges an X in corner... if you dont click on any windows in overlay it closes the overlay"

---

### 4. Dual Audio Output Routing
**What it does:**  
The app outputs TTS audio to two destinations simultaneously:
1. **Primary Output Device** (e.g., headphones/speakers) – so the user can monitor what is being said
2. **Virtual Audio Cable Output** (e.g., VB-Cable, Virtual Audio Cable) – which routes to Discord/VRChat as a microphone input

Both outputs are user-selectable from a list of available audio devices.

**Why it belongs in MVP:**  
This is the technical foundation of how the app integrates with Discord/VRChat. Without dual routing, the app cannot fulfill its primary purpose.

**Source support:**  
"Normal output for her to hear it, so selectable output for audio devices. Second output to be for a Virtual cable"  
Screenshot shows "1st Output Device (your speaker)" and "2nd Output Device (virtual cable 'input')"

---

### 5. Quick Input Phrases (Presets)
**What it does:**  
A library of saved phrases (e.g., "Hello", "One moment please", "I agree") that can be triggered either from a list in the overlay or via assignable hotkeys.

**Why it belongs in MVP:**  
Quick phrases drastically reduce communication time for common responses, which is critical in fast-paced social situations.

**Source support:**  
"hotkeys for pulling up and quick input phrases"  
Screenshots show multiple quick-type shortcut configurations

---

### 6. Free TTS Engine
**What it does:**  
Integrate a free, offline, or low-cost TTS engine (e.g., Windows SAPI voices, eSpeak, Piper TTS, or similar) as the default voice option.

**Why it belongs in MVP:**  
The user explicitly stated: "I dont want to use eleven labs i would like my own voices/api in any way to not have to pay."  
A free option must be the primary voice to make the app economically sustainable for daily use.

**Source support:**  
User preference statement + "Top 3 priorities: audio, voices, and the box"

---

### 7. ElevenLabs API Integration (Optional)
**What it does:**  
Allow the user to optionally configure an ElevenLabs API key in settings. When configured, the app can use ElevenLabs voices instead of the free TTS engine.

**Why it belongs in MVP:**  
While the user prefers free options, they explicitly requested ElevenLabs support as a fallback: "But i would like an eleven labs option."  
This ensures flexibility without locking them into paid services.

**Source support:**  
"I dont want to use elvel labs i would like my own voices/api in any way to not have to pay. But i would like an eleven labs option"

---

### 8. Settings Panel
**What it does:**  
A dedicated settings screen accessible via system tray icon, hotkey, or menu that allows configuration of:
- Global hotkey bindings (overlay activation, stop TTS)
- Audio output device selection (primary + virtual cable)
- TTS engine selection (free vs. ElevenLabs)
- ElevenLabs API key input (if using premium voices)
- Quick phrase management (add/edit/delete presets)
- Textbox size adjustment

**Why it belongs in MVP:**  
Users need basic configuration without editing config files. A settings panel is the minimum viable interface for managing these options.

**Source support:**  
"Set corresponding output devices and hotkeys for pulling up and quick input phrases"  
Screenshots show extensive settings panels in current TTS Voice Wizard

---

### 9. Dark Mode UI
**What it does:**  
All UI elements (overlay, settings panel, system tray menu) use a dark color scheme.

**Why it belongs in MVP:**  
Explicitly and emphatically requested: "Automatic darkmode (fuck lightmodes)"

**Source support:**  
User requirement statement

---

### 10. System Tray Integration
**What it does:**  
The app runs in the system tray with a right-click menu for:
- Open Settings
- Toggle Quick Phrases Window (if applicable)
- Exit Application

The settings window and any other windows can be minimized to the system tray instead of the taskbar.

**Why it belongs in MVP:**  
Keeps the app accessible without cluttering the taskbar, provides a persistent access point for settings, and maintains an unobtrusive presence during use.

**Source support:**  
TTS Voice Wizard screenshot shows "Allow Minimizing to System Tray"

---

### 11. Stop TTS Hotkey
**What it does:**  
A global hotkey that immediately stops current TTS playback if the user needs to cancel mid-speech.

**Why it belongs in MVP:**  
Critical for correcting mistakes or responding to conversation changes. Without this, users cannot interrupt incorrect or inappropriate messages once they start playing.

**Source support:**  
TTS Voice Wizard screenshot shows "Stop TTS Shortcut"

---

### 12. Error Notifications
**What it does:**  
Provide clear visual notifications when TTS fails (e.g., API key expired, audio device disconnected, network error for ElevenLabs, invalid text input).

**Why it belongs in MVP:**  
Prevents silent failures that would leave the user unaware their message wasn't sent. Essential for reliable communication.

**Source support:**  
Basic error handling requirement for communication tools

---

## User Flow

### Primary Flow: Type and Speak
1. User is in Discord voice chat or VRChat
2. User presses the global hotkey (e.g., Ctrl+Shift+Space)
3. Transparent overlay appears with text input box focused
4. User types their message
5. User presses Enter
6. TTS plays through both monitoring output and virtual cable
7. Textbox clears automatically
8. Overlay closes automatically
9. Discord/VRChat receives the audio as microphone input

### Quick Phrase Flow
1. User presses a quick phrase hotkey (e.g., Ctrl+1)
2. Pre-saved phrase immediately plays via TTS
3. No overlay appears (instant execution)

### First-Time Setup Flow
1. User opens settings from system tray
2. User selects their monitoring audio device
3. User selects their virtual cable as second output
4. User configures global overlay hotkey
5. User optionally adds ElevenLabs API key
6. User creates quick phrases and assigns hotkeys
7. User closes settings and begins using the overlay

---

## Functional Requirements

- The app must support Windows 10/11
- The overlay must appear above fullscreen applications and games
- The user can assign any valid key combination as the global hotkey
- The user can select from a list of available audio output devices for both primary and virtual cable outputs
- The user can type text and trigger TTS via Enter key or button click
- The user can create, edit, and delete quick phrases
- The user can assign individual hotkeys to quick phrases (optional per phrase)
- The overlay must close when the user clicks outside its boundary
- The overlay must close after successful TTS playback
- The textbox must automatically clear after each message is sent
- The app must route audio to both selected outputs simultaneously
- The app must support at least one free TTS voice engine by default
- The user can optionally configure an ElevenLabs API key for premium voices
- The user can switch between free and ElevenLabs voices in settings
- The user can stop TTS playback mid-speech via a global hotkey
- The app must display clear error notifications when TTS fails
- The app must persist user settings across restarts
- The textbox size must be adjustable (width/height or font size)
- The UI must use a dark color scheme
- The app must provide visual feedback when TTS is playing
- Settings window can be minimized to system tray

---

## Accessibility / UX Considerations

### Speed and Efficiency
- Minimize steps from hotkey press to speech output
- Keep overlay simple and focused (no distracting elements)
- Provide instant visual feedback when overlay opens
- Fast TTS processing with minimal latency

### Ease of Use
- Clear, readable fonts in the overlay and settings
- Logical grouping of settings
- Obvious close/cancel options (X button, Esc key, click-outside)
- No required configuration before first use (sensible defaults)

### Cognitive Load
- Single-purpose overlay (type and speak, nothing else)
- Quick phrases reduce typing fatigue for common responses
- Clean, uncluttered UI to avoid overwhelming the user

### Stress Resilience
- Must work reliably under pressure (fast-paced conversations)
- Forgiving interaction (easy to cancel or retry)
- No blocking errors or crashes that interrupt communication flow

---

## Technical Notes

### Platform
- **OS:** Windows 10/11 (desktop application)
- **Likely Tech Stack:** Electron, WPF, or similar framework that supports overlays and global hotkeys

### Audio Routing
- Requires detection and listing of available audio output devices
- Must support simultaneous playback to two separate audio devices
- May require third-party libraries for dual-output functionality
- Virtual cable software (e.g., VB-Cable) must be installed separately by user

### Overlay Implementation
- Must use topmost/always-on-top window flags
- Must detect and handle fullscreen game compatibility
- Should be frameless/borderless for clean appearance
- Must capture click-outside events to auto-close

### TTS Integration
- Free option: Integrate Windows SAPI, eSpeak, Piper TTS, or similar
- Paid option: ElevenLabs REST API integration with user-provided API key
- TTS processing should be asynchronous to avoid UI blocking

### Data Persistence
- Settings stored in local config file (JSON or similar)
- Quick phrases stored persistently
- Settings should reload on app restart

### Performance
- Low memory footprint (runs continuously in background)
- Fast overlay launch time (<200ms from hotkey to visible overlay)
- Minimal CPU usage when idle

---

## Assumptions

1. **Virtual Cable Setup:** The user is expected to install and configure virtual audio cable software separately (e.g., VB-Cable). The app will detect it as an available audio device but won't handle installation.

2. **Windows Platform:** Based on the context (mention of Windows-specific challenges), this is assumed to be a Windows-only application for MVP.

3. **Keyboard-First Interaction:** The user prefers keyboard control (hotkeys, Enter to send) over mouse-heavy workflows.

4. **Single User:** The app is designed for personal use, not multi-user or shared device scenarios.

5. **No Mobile Version:** The MVP is desktop-only. Mobile support was asked about and deemed non-essential ("It doesnt need to but that would be cool").

6. **Internet Not Required:** The free TTS engine should work offline. ElevenLabs requires internet but is optional.

7. **Basic Voice Selection:** MVP will provide voice selection but not advanced voice cloning or custom voice features.

---

## Recommended MVP Boundary

### Include in MVP
- Global hotkey overlay with text input
- Transparent overlay design with auto-close behavior
- Dual audio output routing (monitoring + virtual cable)
- Free TTS engine as default
- ElevenLabs API integration as optional upgrade
- Quick phrase library with hotkey support
- Basic settings panel (hotkeys, audio devices, TTS engine, API key, phrases, textbox size)
- Dark mode UI
- System tray integration
- Enter key to trigger speech

### Exclude from MVP (Save for Future Versions)
- Visual customization (themes, colors, fonts)
- "Cutesy" aesthetic options
- Advanced phrase organization (categories, folders, search)
- Voice switching hotkeys during playback
- Message queuing system
- OSC integration for VRChat-specific features
- Advanced overlay animations or transitions
- Multiple overlay layouts
- Voice cloning or custom voice training
- Cloud sync for phrases/settings
- Analytics or usage statistics
- Mobile companion app
- Speech-to-text features
- In-app voice previews before sending

---

## Success Criteria for MVP

The MVP is considered successful if:
1. The user can reliably access the overlay via hotkey from Discord or VRChat
2. Text-to-speech plays through both monitoring output and virtual cable without audio dropouts
3. Quick phrases can be triggered instantly via hotkeys
4. The app runs stably in the background without crashes
5. The user finds it faster and less clunky than their current solution (TTS Voice Wizard)
6. The free TTS voice is acceptable quality for daily use
7. Settings changes persist across app restarts
8. The overlay does not interfere with game performance or visibility

---

*This MVP is intentionally scoped to deliver a functional, daily-usable communication tool without over-engineering. Polish, customization, and advanced features are reserved for post-MVP iterations.*
