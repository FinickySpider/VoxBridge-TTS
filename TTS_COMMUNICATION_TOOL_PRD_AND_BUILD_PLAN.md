# TTS Communication Tool

## PRD, Phase-Based Proposal, and Technical Build Plan

---

# 1) Product Requirements Document (PRD)

## Product Name

TTS Communication Tool

## Product Summary

A lightweight Windows desktop text-to-speech application for a mute user who needs fast, reliable voice communication in Discord and VRChat.

The app should function like a dependable communication prosthetic: quick to access, simple to use, low-stress, and pleasant enough for daily use.

## Product Vision

Create a small, focused, always-available TTS tool that reduces friction in live voice conversations and becomes meaningfully better than the user’s current workflow.

## Product Principles

1. Speed over feature breadth.
2. Reliability over novelty.
3. Low cognitive load over dense controls.
4. Emotional comfort matters.
5. Advanced features must not slow or clutter the core flow.

## Primary User

A mute user relying on TTS for daily communication in voice-enabled social apps such as Discord and VRChat.

## Core User Problem

Existing solutions feel clunky, slow, or overly complex during real-time conversations. The user needs a simpler and more responsive tool.

## Key User Needs

- Instant access via hotkey
- Fast type-to-speech loop
- Dual audio routing to monitoring output and virtual cable
- Offline/free default voice
- Quick phrases for common responses
- Easy cancellation of mistakes
- Dark, comfortable interface
- Stable behavior under daily use

## Core Use Case

The user is in Discord or VRChat, presses a hotkey, types a message, presses Enter, and the message is spoken to both local monitoring output and a virtual cable used as microphone input.

## MVP Goal

Deliver a daily-usable communication tool that performs the core speech workflow reliably with minimal friction.

## Non-Goals for MVP

- Deep customization
- Advanced phrase management
- Multiple TTS engines
- Cloud features
- Mobile support
- Voice cloning
- Full VRChat-specific OSC integrations
- Complex automation systems

## MVP Scope

### 1. Global Hotkey Overlay

A configurable global hotkey opens a focused always-on-top overlay.

Requirements:

- User-configurable overlay hotkey
- Opens centered by default
- Auto-focuses the text input
- Esc closes overlay
- Click-outside closes overlay
- X button closes overlay
- Best effort support for borderless fullscreen apps

Note: Do not promise flawless support for every exclusive fullscreen game.

### 2. Text Input and Send

A simple text input allows quick typing and instant speech.

Requirements:

- Enter triggers speech
- Optional Send button
- Text clears after successful send
- Overlay closes after successful send
- Visual feedback for sending / speaking / failed states

### 3. Default Offline TTS Engine

Use Kokoro as the preferred default offline TTS engine, assuming early implementation tests confirm acceptable latency and packaging behavior.

Requirements:

- One bundled default voice
- At least one selectable voice option if feasible
- Local/offline operation after installation
- No recurring cost

### 4. Dual Audio Routing

The app must send the same speech output to:

- Primary monitor output device
- Secondary output device such as VB-Cable

Requirements:

- Device selection in settings
- Test output buttons
- Clear error state if device is unavailable

### 5. Quick Phrase Basics

Basic saved phrases for repeated use.

Requirements:

- Add phrase
- Edit phrase
- Delete phrase
- Play phrase from a simple list
- Optional limited phrase hotkeys

### 6. Stop Speech Hotkey

A global hotkey immediately stops current playback.

### 7. Settings Panel

Simple settings window with sections for:

- Hotkeys
- Audio
- Voice
- Phrases
- Appearance

Minimum settings:

- Overlay hotkey
- Stop hotkey
- Phrase hotkeys
- Monitor output device
- Secondary output device
- Default voice
- Textbox size
- Font size
- Launch behavior

### 8. System Tray Integration

The app runs from the tray and stays unobtrusive.

Requirements:

- Open Settings
- Open Phrase Manager
- Exit

### 9. Dark Mode UI

Dark mode only for MVP.

### 10. Error Handling

The app must fail visibly, not silently.

Errors to handle:

- Hotkey registration failure
- Missing audio device
- TTS generation failure
- Playback failure
- Invalid premium provider settings if later enabled

### 11. Basic Setup / Audio Test Flow

Minimum onboarding support to reduce setup friction.

Requirements:

- Select output devices
- Test primary output
- Test secondary output
- Test both outputs
- Assign overlay hotkey

## Out of Scope for MVP

- Themes and cutesy styling
- Phrase folders/categories/search
- Recent message history
- Transcript logging
- Message queueing
- Multi-engine TTS support
- ElevenLabs in Phase 1 unless it proves trivial
- VRChat OSC integration
- CSS customization
- Cloud sync
- Mobile companion
- Voice cloning
- Autocomplete/prediction

## UX Guidelines

- The overlay must remain visually quiet.
- The user must always know whether the app is ready, speaking, or failed.
- Mistakes must be easy to cancel.
- The overlay is for speaking, not for management.
- The app should feel invisible until needed.

## Success Criteria

The MVP is successful if the user can:

1. Open the overlay reliably via hotkey
2. Type and press Enter to speak
3. Hear output locally
4. Send output to virtual cable
5. Stop speech quickly when needed
6. Use saved phrases for common responses
7. Use the app daily with less friction than their current solution

## Constraints and Risk Notes

- Windows-only MVP
- Overlay behavior over exclusive fullscreen apps may vary
- Dual audio routing is the main technical risk
- Hotkey conflicts must be handled clearly
- Low-latency behavior matters more than maximum voice realism

---

# 2) Post-MVP Roadmap

## Goal

Make the app more comfortable, more expressive, and more joyful to use without damaging the fast core workflow.

## Priority Group A: Usability Hardening

- Overlay position adjustment
- Better font/UI size controls
- Hotkey conflict detection
- Volume per output
- Recent phrases/messages
- Import/export phrases
- Better settings organization
- Improved onboarding flow

## Priority Group B: Joy / Comfort Layer

- Theme presets
- Cutesy visual options
- Better phrase presentation
- Subtle animations
- Better send/success feedback

## Priority Group C: Phrase System Expansion

- Phrase categories / folders
- Phrase search / filter
- Favorites / pinning
- Repeat last / resend
- Transcript logging

## Priority Group D: Voice / Expression Expansion

- Voice preset switching
- Speed / pitch controls
- Optional premium voice path
- Cached audio for quick phrases

## Priority Group E: Advanced / Deferred

- Message queue system
- VRChat OSC integration
- Multiple extra TTS engines
- Voice cloning
- Cloud sync
- Mobile companion
- Phrase prediction / autocomplete

## Post-MVP Guardrails

- Do not slow down the send flow.
- Do not overload the overlay.
- Keep advanced features opt-in.
- Personalization should improve comfort, not clarity.
- Avoid automation that can accidentally speak without clear user intent.

---

# 3) Phase-Based Proposal / Quote / Estimate

## Proposal Summary

This project will be delivered in small, clearly defined phases to reduce risk, control scope, and avoid overpromising.

This is especially important because the app includes several technically sensitive areas:

- Windows overlay behavior
- global hotkeys
- dual audio routing
- local TTS integration

A phased approach ensures the most valuable functionality is delivered first, and later work is shaped by what proves technically reliable and practically useful.

## Recommended Phase Structure

### Phase 1 — Core MVP

Objective: deliver a usable, focused communication tool.

Includes:

- Tray app shell
- Settings persistence
- Global overlay hotkey
- Overlay input window
- Kokoro default offline TTS integration
- Single output playback
- Dual output playback
- Stop playback hotkey
- Basic phrase library
- Dark mode UI
- Basic setup/testing flow
- Error feedback

Deliverable: A working Windows desktop app suitable for real-world testing in Discord/VRChat workflows.

### Phase 2 — Usability and Comfort

Objective: make the app smoother, safer, and easier to live with daily.

Includes:

- Better onboarding/setup flow
- Overlay position controls
- Font size improvements
- Hotkey conflict handling
- Volume per output
- Recent phrase/message support
- Settings organization improvements
- General UX cleanup

Deliverable: A more refined daily-use tool with better setup, better control, and reduced friction.

### Phase 3 — Joy / Personality / Power Features

Objective: add emotional comfort, customization, and selected power-user upgrades.

Potential items:

- Theme presets
- Cutesy customization layer
- Better phrase browsing UI
- Transcript logging
- Voice preset switching
- Optional premium voice integration
- Additional polish based on user feedback

Deliverable: A more personalized and enjoyable tool, shaped by real user use rather than assumptions.

## Budget

- Phase 1: approximately $100
- Phase 2: approximately $100
- Phase 3: approximately $100 optional

## Framing

This work is structured as:

- clearly defined phases
- each phase has a specific deliverable
- later phases depend on technical feasibility and user feedback
- final scope may adjust based on what proves realistic in testing

This is safer than promising a full polished tool upfront for a flat amount.

## Estimate Notes

- This estimate is intentionally conservative on promises
- Full fullscreen-game compatibility cannot be guaranteed
- Advanced features are not included in Phase 1
- Voice/provider breadth is intentionally limited early
- Later features should be prioritized after real user testing

---

# 4) Technical Build Plan

## Recommended Stack

- C#
- .NET
- WPF
- NAudio
- Kokoro for default local TTS
- Local JSON configuration

## Why This Stack

This is the best fit for:

- Windows-first desktop behavior
- system tray integration
- native hotkeys
- overlay windows
- lower-overhead background utility behavior
- easier audio routing than a web-desktop shell

## High-Level Architecture

### Module 1: App Shell

Responsibilities:

- application startup
- tray icon/menu
- lifecycle management
- window launching

### Module 2: Settings / Persistence

Responsibilities:

- store hotkeys
- store audio device selections
- store voice settings
- store phrase library
- load/save config safely

### Module 3: Hotkey Service

Responsibilities:

- register overlay hotkey
- register stop hotkey
- register phrase hotkeys
- detect/report registration failures

### Module 4: Overlay UI

Responsibilities:

- show/hide overlay
- focus input
- send/cancel behavior
- visible state feedback

### Module 5: Phrase Service

Responsibilities:

- CRUD for phrases
- phrase playback requests
- future support for recent/favorites

### Module 6: TTS Service

Responsibilities:

- generate speech from text using Kokoro
- manage voice selection
- handle generation failures
- future caching of phrase audio

### Module 7: Audio Router

Responsibilities:

- normalize output format
- duplicate playback to two outputs
- stop playback instantly
- device testing
- recover from missing/unavailable devices

### Module 8: Notification / Status Layer

Responsibilities:

- toasts or banners
- speaking status
- error feedback

## Technical Risks and Mitigations

### Risk 1: Overlay compatibility over games

Mitigation:

- target standard apps and borderless fullscreen first
- clearly document support expectations

### Risk 2: Dual output stability

Mitigation:

- build and test the audio path early
- normalize output format before playback
- provide manual routing tests in settings

### Risk 3: Kokoro integration complexity

Mitigation:

- validate generation speed and packaging early
- standardize on one integration path
- bundle one known-good voice where practical

### Risk 4: Hotkey conflicts

Mitigation:

- detect failed registration
- inform the user clearly
- provide recommended fallback combinations

### Risk 5: Silent failure / trust loss

Mitigation:

- surface all important failures clearly
- always show sending/speaking/failed states

## Suggested Build Sequence

### Milestone 1: Project Skeleton

- Create app shell
- Add tray behavior
- Add settings model and local persistence

### Milestone 2: Overlay Flow

- Build overlay window
- Add show/hide behavior
- Add text input send/cancel flow

### Milestone 3: TTS Bring-Up

- Integrate Kokoro generation
- Generate speech for typed input
- Validate latency and output format

### Milestone 4: Audio Playback

- Add single-device playback
- Add dual-device playback
- Add stop playback handling
- Add test-play functionality

### Milestone 5: Phrase Basics

- Add phrase CRUD
- Add phrase playback
- Add limited phrase hotkeys

### Milestone 6: Stabilization

- Improve error handling
- Add speaking status
- Validate config persistence
- Test across target use cases

### Milestone 7: Follow-Up Phase Work

- Improve onboarding
- Improve settings organization
- Add overlay placement / font controls
- Add comfort and polish features

## Definition of Done for Phase 1

Phase 1 is complete when:

- The app launches and lives in the system tray
- The overlay opens by hotkey and focuses the input
- Text can be spoken with Enter
- Speech can play to both selected outputs
- The user can stop speech with a hotkey
- Basic phrases can be created and used
- Settings persist across restarts
- Failure states are visible and understandable
- The app is good enough for real user testing in the intended workflow

---

# 5) Project Philosophy and Approach

This should be treated as a small accessibility-focused utility, not a general-purpose TTS platform.

That means the build strategy should be:

- small scope
- strong core loop
- honest promises
- polish based on real use

The product wins by being dependable and pleasant, not by having the most checkboxes.
