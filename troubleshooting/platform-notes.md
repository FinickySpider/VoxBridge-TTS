# Platform Notes

## Windows-Only

This application is **Windows-only** and will not run on Linux or macOS. It depends on:
- WPF (Windows Presentation Foundation) for the UI
- NAudio WASAPI for audio device enumeration and playback
- Win32 P/Invoke (`RegisterHotKey`) for global hotkeys
- Windows DPAPI for API key encryption

## Windows 10 vs Windows 11

The app works on both Windows 10 and Windows 11 (x64). No known differences in behavior.

## Exclusive Fullscreen

Support for overlay display over exclusive fullscreen applications (games) is **best-effort only**. The overlay uses `Topmost="True"` which works with borderless fullscreen but may not appear over true exclusive fullscreen.

## Virtual Audio Cable

The user must install a virtual audio cable separately. The app does not bundle or install one. Common options:
- [VB-Cable](https://vb-audio.com/Cable/) (free, recommended)
- [VB-Cable A+B](https://vb-audio.com/VirtualCables/) (paid)
- [Voicemeeter](https://vb-audio.com/Voicemeeter/) (advanced)
- [Virtual Audio Cable](https://vac.muzychenko.net/) (paid)

## Antivirus Considerations

Some antivirus software may:
- Block the Kokoro model download (large file, network activity)
- Flag the app as suspicious (uses global hotkeys, system tray)
- Interfere with config file writing

Add an exception for the app directory if needed.

## .NET Runtime

The app targets `net10.0-windows`. The .NET 10 runtime must be installed. Release builds may bundle the runtime as a self-contained deployment.

## Single Instance

The app uses a named mutex (`Global\TtsCommunicationTool_SingleInstance`) to enforce single-instance. Only one instance can run at a time. Attempting to launch a second instance shows a message box and exits.
