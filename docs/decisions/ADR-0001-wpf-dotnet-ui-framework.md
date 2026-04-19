---
id: ADR-0001
type: decision
status: complete
date: 2026-04-18
supersedes: ""
superseded_by: ""
---

# ADR-0001: WPF and C#/.NET for Desktop UI Framework

## Context

The application is a Windows-only desktop text-to-speech utility that requires system tray integration, global hotkeys, always-on-top overlay windows, and low-level audio device access. Framework options considered: WPF/.NET, WinUI 3, Electron, and MAUI.

## Decision

Use **WPF on .NET (C#)** as the UI framework, with NAudio for audio, and Windows native interop for global hotkeys.

## Consequences

### Positive

- Mature, stable framework with excellent Windows integration
- Native support for borderless windows, topmost behavior, system tray
- Lower memory footprint than Electron
- Direct access to Windows audio APIs via NAudio and P/Invoke
- Strong MVVM ecosystem
- Single-platform focus avoids cross-platform complexity

### Negative

- No cross-platform path (acceptable — Windows-only is a stated constraint)
- WPF is older than WinUI 3 — some modern UI patterns require more effort
- Limited to .NET ecosystem for dependencies

## Links

- Related items:
  - FEAT-001
  - FEAT-005
