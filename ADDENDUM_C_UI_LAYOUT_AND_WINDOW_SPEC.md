# ADDENDUM C
## UI Layout and Window Specification

This addendum describes exact MVP window contents and behavior.

## 1. Overlay Window

### Purpose
Fast message entry and send.

### Window Rules
- Borderless
- Topmost
- Hidden from taskbar
- Centered by default
- Single instance only
- Close on Esc
- Close on click outside
- Close on X
- Close on successful send
- Stay open on error

### Recommended Size
- Width: 720 px default
- Height: 140 px default
- User-adjustable via settings

### Overlay Layout

```txt
┌──────────────────────────────────────────────────────────────────────┐
│ TTS Input                                                     [X]   │
│                                                                      │
│  [ Text input box................................................. ] │
│                                                                      │
│  Status: Ready / Sending / Error            [Send]                   │
└──────────────────────────────────────────────────────────────────────┘
```

### Controls
- `TextBox`: main input, auto-focused
- `Button Send`: optional but included for discoverability
- `Button Close`: top-right close button
- `TextBlock Status`: shows Ready, Sending, Speaking, Error

### Input Behavior
- Enter sends
- Esc closes
- Empty text is rejected
- On error, preserve text in the textbox
- On success, clear text and close overlay

## 2. Settings Window

### Purpose
All configuration lives here.

### Layout Pattern
Use left navigation or top tabs. Either is acceptable, but sections must be clear.

Recommended sections:
1. General
2. Hotkeys
3. Audio
4. Voice
5. Phrases
6. Appearance

### General Section
Controls:
- checkbox: Close to tray
- checkbox: Minimize to tray
- optional checkbox: Start with Windows
- button: Run audio setup test

### Hotkeys Section
Controls:
- hotkey capture input: Overlay hotkey
- hotkey capture input: Stop hotkey
- optional list: Phrase hotkeys
- inline validation warning area

### Audio Section
Controls:
- dropdown: Monitor output device
- dropdown: Secondary output device
- button: Test Monitor
- button: Test Secondary
- button: Test Both
- status line for device warnings

### Voice Section
Controls:
- dropdown: Voice
- label: Engine = Kokoro
- button: Test Voice

### Phrases Section
Two-pane or single-pane list layout is fine.

Recommended layout:

```txt
┌──────────────────────────────────────────────────────────────────────┐
│ Phrases                                                              │
│ ┌──────────────────────┐  ┌───────────────────────────────────────┐ │
│ │ Hello                │  │ Name: [ Hello                      ] │ │
│ │ One moment please    │  │ Text: [ One moment please.........] │ │
│ │ I agree              │  │ Hotkey: [ none ]                   │ │
│ │                      │  │                                     │ │
│ └──────────────────────┘  │ [Save] [Play] [Delete]              │ │
│                            └───────────────────────────────────────┘ │
└──────────────────────────────────────────────────────────────────────┘
```

Minimum actions:
- Add
- Edit
- Delete
- Play

### Appearance Section
Controls:
- numeric input or slider: Overlay width
- numeric input or slider: Overlay height
- numeric input or slider: Font size

## 3. Phrase Editor Dialog

If phrase editing is done in a dialog instead of inline editing, use:
- Name field
- Text field
- Optional hotkey field
- Save button
- Cancel button

## 4. Notification UI

Use subtle dark toasts or banners.
Do not use blocking message boxes unless required for severe errors.

### Notification Types
- info
- warning
- error

### Examples
- "Overlay hotkey updated"
- "Secondary output device is missing"
- "Speech generation failed"

## 5. Visual Style Guide

MVP style:
- dark only
- soft corners
- muted background
- subtle focus outline
- readable text
- minimal chrome
- low visual noise

Suggested palette direction:
- background: charcoal
- panel: slightly lighter neutral
- accent: soft blue or violet
- error: muted red
- success/info: muted teal or blue

Avoid:
- pure white panels
- bright saturated colors
- heavy animations
- visual clutter
