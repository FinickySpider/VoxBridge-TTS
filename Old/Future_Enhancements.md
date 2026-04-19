# TTS Communication Tool - Future Enhancements

## Overview

This document outlines features, improvements, and enhancements that extend beyond the core MVP. These items represent the "polished version" of the application and include quality-of-life upgrades, visual customization, advanced phrase management, additional voice options, and integration features.

All items listed here are based on or naturally extend from the project materials. Features marked as assumptions are logical extensions consistent with the user's needs and current tool usage (TTS Voice Wizard), but were not explicitly requested.

---

## Extra Features

### 1. Voice Preset Switching Hotkey
**Description:**  
Allow the user to quickly switch between different TTS voices (free voices, ElevenLabs voices, or different voice profiles) via a dedicated hotkey without opening settings.

**Source:**  
TTS Voice Wizard screenshot shows "Switch Voice Presets Shortcut" functionality

**Value:**  
Enables the user to change tone or character on the fly during conversations (e.g., serious voice vs. playful voice)

---

### 2. Message Queue System
**Description:**  
Instead of playing one message at a time, allow the user to queue multiple messages that play sequentially. Include options for:
- Queue delay between messages
- Queue character limit
- Smart string splitting for long messages
- Option to clear the entire queue

**Source:**  
TTS Voice Wizard screenshot shows "Message Queue System" with delay settings, character limits, and smart string splitting

**Value:**  
Allows the user to type several responses in advance during fast-paced conversations without interrupting current playback

**Note:**  
The stop TTS hotkey (now in MVP) handles stopping current playback; this feature extends it with multi-message queuing

---

### 3. Typing Indicator
**Description:**  
Optional visual indicator that appears on the overlay while TTS is actively speaking, showing the user what is currently being said.

**Source:**  
TTS Voice Wizard screenshot shows "Typing Indicator" option

**Value:**  
Provides feedback to the user and helps them follow along with what others are hearing

---

### 5. Auto-Clear Text Box After Send
**Description:**  
Option to automatically clear the text input field after successful TTS playback.

**Source:**  
TTS Voice Wizard screenshot shows "Auto Clear TTS Text Box Field"

**Value:**  
Prevents accidental re-sending of previous message and speeds up typing new responses

---

### 6. Auto-Send TTS on Copy/Paste
**Description:**  
Option to automatically trigger TTS when text is copied from another source and pasted into the overlay textbox.

**Source:**  
TTS Voice Wizard screenshot shows "Auto Send TTS (for copy/paste)"

**Value:**  
Enables ultra-fast sharing of text from other sources (e.g., copy a URL and have it spoken immediately)

---

### 5. Minimalist Navbar / Always on Top Mode
**Description:**  
A compact, always-visible mini toolbar that stays on top of all windows, providing quick access to TTS controls without opening the full overlay.

**Source:**  
TTS Voice Wizard screenshot shows "Minimalist Navbar" and "Always on Top" options

**Value:**  
Gives the user persistent visual access to TTS controls and status

---

### 6. Output Transcript in Log
**Description:**  
Automatically log all sent TTS messages to a text file with timestamps for later review.

**Source:**  
TTS Voice Wizard screenshot shows "Output Transcript in Log"

**Value:**  
Allows the user to review conversation history or track frequently used phrases

---

### 7. Button Sounds
**Description:**  
Optional audible feedback (click sounds, confirmation tones) when buttons are pressed or actions are triggered.

**Source:**  
TTS Voice Wizard screenshot shows "Button Sounds" toggle

**Value:**  
Provides additional sensory feedback for user actions

---

### 8. Close Home Screen Banner on Start
**Description:**  
Option to skip a welcome/home screen and go directly to system tray on app launch.

**Source:**  
TTS Voice Wizard screenshot shows "Close Home Screen Banner on Start"

**Value:**  
Faster startup experience for daily use

---

### 9. Disable Windows Media Debug
**Description:**  
Technical setting to disable Windows Media debug messages or notifications.

**Source:**  
TTS Voice Wizard screenshot shows this option

**Value:**  
Reduces console clutter and potential conflicts with media playback APIs

---

### 10. OSC Integration for VRChat
**Description:**  
Add support for VRChat's OSC (Open Sound Control) protocol to:
- Send text to VRChat chatbox
- Show keyboard indicator before sending message
- Enable hide text delay
- Configure OSC send address and port
- Sync with VRChat KAT (chatbox avatar text) system

**Source:**  
TTS Voice Wizard screenshot shows extensive OSC/VRChat integration options including "Send Text to VRChat with VRC Chatbox", "Show Keyboard Before Sending Message", "VRChat Sound on Message Send", "Use Hide Text Delay", OSC address/port config, and KAT integration

**Value:**  
Provides deeper VRChat integration for users who want their TTS messages to also appear as text in VRChat chatbox

**Note:**  
This is a more advanced/niche feature that may be valuable for VRChat power users but not essential for general Discord/VRChat voice communication

---

## Quality-of-Life Improvements

### 1. Phrase Categories / Folders
**Description:**  
Organize quick phrases into categories (e.g., Greetings, Reactions, Questions, Gaming Callouts) with a browsable menu in the overlay.

**Assumption:**  
Natural extension as phrase libraries grow

**Value:**  
Makes phrase selection faster and more organized as the library expands beyond 10-20 items

---

### 2. Phrase Search/Filter
**Description:**  
Quick search box in the overlay to filter phrases by keyword instead of scrolling through a long list.

**Assumption:**  
Logical improvement for users with 50+ saved phrases

**Value:**  
Reduces time to find specific phrases in large libraries

---

### 3. Recent Phrases History
**Description:**  
Track the last 5-10 phrases or freeform messages sent and provide quick access to re-send them.

**Assumption:**  
Common pattern in communication tools

**Value:**  
Allows the user to quickly repeat themselves or recall recently used custom messages

---

### 4. Adjustable Overlay Position
**Description:**  
Allow the user to click-and-drag the overlay to different screen positions and save that preference (e.g., top-center, bottom-right, left-middle).

**Assumption:**  
Standard overlay UX pattern

**Value:**  
Users can position the overlay where it doesn't block important UI elements in their games/apps

---

### 5. Adjustable Overlay Transparency
**Description:**  
Slider in settings to control how transparent the overlay background is (0% = opaque, 100% = invisible).

**Assumption:**  
Natural extension of "transparent with slight dimming" requirement

**Value:**  
Lets users balance visibility and unobtrusiveness based on personal preference

---

### 6. Font Size Adjustment
**Description:**  
Independent control over font size in the text input box (separate from textbox dimensions).

**Assumption:**  
Implied by "Controllable textbox size" — user may want larger text without enlarging the whole box

**Value:**  
Improves readability for users with vision considerations or those using high-DPI displays

---

### 7. Volume Control per Output
**Description:**  
Independent volume sliders for the monitoring output and virtual cable output.

**Assumption:**  
Common need in dual-audio scenarios

**Value:**  
Allows the user to hear themselves quietly while others hear them at normal volume, or vice versa

---

### 8. Quick Phrase Import/Export
**Description:**  
Allow users to export their phrase library to a JSON or CSV file and import from backup or shared files.

**Assumption:**  
Standard data portability feature

**Value:**  
Protects against data loss and enables sharing phrase sets between users or devices

---

### 9. Hotkey Conflict Detection
**Description:**  
Warn the user if a chosen hotkey conflicts with common application shortcuts (e.g., Ctrl+C, Ctrl+V, Windows key combos).

**Assumption:**  
Quality-of-life enhancement for setup experience

**Value:**  
Prevents frustration from non-functional hotkeys

---

### 10. TTS Speed/Pitch Control
**Description:**  
Settings to adjust playback speed and pitch of TTS voices (both free and ElevenLabs).

**Assumption:**  
Standard TTS customization option

**Value:**  
Allows users to personalize their voice delivery style

---

## Polish / UX Refinements

### 1. Custom Themes and Color Schemes
**Description:**  
Allow users to customize overlay and settings panel colors, backgrounds, and accent colors beyond basic dark mode.

**Source:**  
User explicitly wants "ability to make customizable and cutesy"

**Value:**  
Personalization makes the app feel more owned and enjoyable to use

---

### 2. "Cutesy" Visual Options
**Description:**  
Add optional visual flourishes such as:
- Rounded corners and soft shadows
- Subtle animations (fade-in, bounce)
- Icon sets or mascot graphics
- Particle effects or sparkles on send
- Customizable fonts

**Source:**  
User explicitly requested: "She does really want ability to make customizable and cutesy"

**Value:**  
Makes the app more emotionally engaging and fun to use, increasing user satisfaction

---

### 3. CSS Customization Support
**Description:**  
If the app is built with web technologies (Electron, WebView2), expose a custom CSS file that users can edit to fully customize appearance.

**Source:**  
Client suggested: "Could always rely on CSS input for customization stuff"

**Value:**  
Empowers advanced users to deeply personalize the interface without developer intervention

---

### 4. Overlay Entry/Exit Animations
**Description:**  
Smooth fade-in or slide-in animation when overlay appears, smooth fade-out when closing.

**Assumption:**  
Standard polish for overlay UX

**Value:**  
Makes the experience feel more refined and less jarring

---

### 5. Visual Feedback on Send
**Description:**  
Brief animation, color flash, or icon change when TTS successfully triggers (e.g., checkmark, pulsing border).

**Assumption:**  
Improves perceived responsiveness

**Value:**  
Confirms the action was registered, reducing user uncertainty

---

### 6. Phrase List Visual Organization
**Description:**  
Display quick phrases in an attractive grid or card layout with icons, colors, or tags for easy visual scanning.

**Assumption:**  
Extension of customization desire

**Value:**  
Makes phrase selection faster and more pleasant

---

### 7. Onboarding Tutorial
**Description:**  
First-launch guide that walks the user through:
- Setting up virtual cable output
- Configuring global hotkey
- Creating first quick phrase
- Testing TTS output

**Assumption:**  
Reduces initial setup friction

**Value:**  
Lowers barrier to entry for non-technical users

---

### 8. Settings Panel Organization
**Description:**  
Group settings into logical tabs or sections (General, Audio, Hotkeys, Voices, Quick Phrases, Appearance).

**Assumption:**  
Becomes necessary as feature count grows

**Value:**  
Prevents settings panel from becoming overwhelming

---

## Advanced / Later-Phase Ideas

### 1. Additional TTS Engine Support
**Description:**  
Integrate more free or open-source TTS engines such as:
- Google Cloud TTS
- Azure Cognitive Services TTS
- Coqui TTS
- Piper TTS
- Tortoise TTS

**Assumption:**  
User wants "more voices to play with" and prefers free options

**Value:**  
Expands voice variety without requiring payment

---

### 2. Voice Cloning (Free)
**Description:**  
Integrate an open-source voice cloning solution (e.g., Coqui TTS, OpenVoice) that allows the user to train a custom voice from audio samples.

**Assumption:**  
User may want a personalized voice that sounds natural to them

**Value:**  
Provides a deeply personal communication experience

**Note:**  
Technically complex and may require significant development time

---

### 3. Emotion/Tone Presets
**Description:**  
Multi-voice system where different emotional tones (happy, sad, excited, serious) are mapped to different voices or TTS settings.

**Assumption:**  
Logical extension of voice switching for expressive communication

**Value:**  
Allows nuanced emotional expression in voice communication

---

### 4. Cloud Sync for Settings and Phrases
**Description:**  
Optional cloud backup/sync for user settings and phrase library (via Google Drive, Dropbox, or custom backend).

**Assumption:**  
Useful for users who switch between multiple PCs

**Value:**  
Prevents data loss and enables seamless multi-device usage

---

### 5. Shared Phrase Libraries
**Description:**  
Community-contributed phrase packs (e.g., "Gaming Callouts", "D&D Phrases", "Common Greetings") that users can download and import.

**Assumption:**  
Builds on import/export functionality

**Value:**  
Saves users time in building comprehensive phrase libraries

---

### 6. Phrase Prediction / Autocomplete
**Description:**  
As the user types, suggest frequently used phrases or recently sent messages for quick completion.

**Assumption:**  
Natural evolution of quick phrase functionality

**Value:**  
Reduces typing effort for semi-custom messages

---

### 7. Multi-Language Support
**Description:**  
UI localization and support for TTS in multiple languages (Spanish, French, Japanese, etc.).

**Assumption:**  
Expands potential user base

**Value:**  
Makes the tool accessible to non-English speakers

**Note:**  
Not requested by current user but a logical product expansion

---

### 8. Mobile Companion App
**Description:**  
Android/iOS app that can trigger TTS on the desktop app or function as a standalone mobile TTS tool.

**Source:**  
Asked about but deemed non-essential: "Does it need to work on mobile? ...It doesnt need to but that would be cool"

**Value:**  
Extends communication capability to mobile scenarios

**Note:**  
Significantly expands scope; consider only in later phases

---

### 9. Wearable Integration
**Description:**  
Support for smartwatch quick phrase triggers or voice command activation.

**Assumption:**  
Speculative accessibility enhancement

**Value:**  
Hands-free operation for users who may have limited keyboard access

**Note:**  
Highly speculative; only consider if user expresses interest

---

### 10. Caregiver / Shared Management Mode
**Description:**  
Allow a secondary user (caregiver, family member) to remotely manage phrase libraries or settings.

**Assumption:**  
Potential accessibility need for users with additional mobility challenges

**Value:**  
Expands usability for users who need communication assistance

**Note:**  
Not relevant to current user's stated needs

---

## Priority Suggestions

### High-Value Next Steps (Post-MVP)
These features directly enhance the core communication experience and align with explicit user requests:

1. **Custom Themes and "Cutesy" Visuals** — Explicitly requested by user
2. **Voice Preset Switching Hotkey** — Supported by reference app screenshots
3. **Message Queue System** — Supported by reference app, useful for fast-paced chats
4. **Phrase Categories/Folders** — Natural evolution as phrase library grows
5. **TTS Speed/Pitch Control** — Standard personalization option
6. **CSS Customization Support** — Suggested by client, enables deep personalization

### Nice-to-Have Upgrades
These improve usability but are not critical:

1. Adjustable overlay position and transparency
2. Recent phrases history
3. Font size adjustment
4. Volume control per output
5. Visual feedback on send
6. Auto-send TTS on paste (eliminates Enter after paste)
7. Phrase search/filter
8. Import/export phrase library
9. Onboarding tutorial
10. Settings panel organization

### Long-Term Ideas
These are valuable but complex or speculative:

1. Additional TTS engine support (Piper, Coqui, Azure, Google)
2. Voice cloning (free, open-source)
3. Emotion/tone presets
4. Cloud sync
5. Phrase prediction/autocomplete
6. Mobile companion app
7. Shared phrase libraries (community packs)
8. Multi-language support
9. OSC integration for VRChat (power users only)

---

## Assumptions

### Confirmed via Source Material
- User wants customization and "cutesy" options (explicitly stated)
- User is familiar with TTS Voice Wizard's feature set (reference screenshots provided)
- User prefers free voice options but wants premium as fallback
- User values speed and simplicity over feature complexity

### Inferred from Context
- Phrase libraries will grow over time, necessitating better organization
- Users may want to adjust overlay position/transparency for different games
- Volume control may be needed to balance monitoring vs. output levels
- Error handling and notifications improve reliability perception
- Visual polish increases user satisfaction and daily enjoyment

### Speculative but Aligned
- Community phrase sharing could save time for new users
- Voice cloning could provide deeply personal communication experience
- Multi-language support expands potential user base (but not requested by current user)

---

*This enhancement roadmap is designed to guide iterative development beyond MVP. Features should be prioritized based on user feedback, usage patterns, and development complexity. Start with high-value items that directly address user requests before moving to speculative features.*
