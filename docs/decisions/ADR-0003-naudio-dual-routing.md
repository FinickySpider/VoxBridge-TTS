---
id: ADR-0003
type: decision
status: complete
date: 2026-04-18
supersedes: ""
superseded_by: ""
---

# ADR-0003: NAudio for Dual Audio Routing

## Context

The application must play the same TTS audio simultaneously to two different output devices (a monitoring device and a virtual cable). This is the primary technical risk of the project. Options considered: NAudio, CSCore, Windows Audio Session API (WASAPI) direct, and Bass.NET.

## Decision

Use **NAudio** for audio device enumeration, format normalization, and dual-output playback.

## Consequences

### Positive

- Well-documented .NET audio library with large community
- Supports WASAPI and WaveOut output modes
- Device enumeration via `MMDeviceEnumerator`
- Supports simultaneous playback to multiple devices via separate output instances
- Format conversion and resampling built in

### Negative

- Dual-output synchronization is application responsibility (start both outputs close together)
- Some edge cases with device disconnection require careful handling
- Memory management of playback sessions must be explicit

## Links

- Related items:
  - FEAT-009
  - FEAT-010
  - FEAT-011
