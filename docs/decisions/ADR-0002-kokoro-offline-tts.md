---
id: ADR-0002
type: decision
status: complete
date: 2026-04-18
supersedes: ""
superseded_by: ""
---

# ADR-0002: Kokoro as Default Offline TTS Engine

## Context

The application needs a text-to-speech engine that works offline after installation, incurs no recurring cost, and produces acceptable voice quality for daily communication. Options considered: Kokoro (local), Windows SAPI, Piper TTS, and cloud-based APIs (ElevenLabs, Azure, Google).

## Decision

Use **Kokoro** as the sole MVP TTS engine for local/offline speech generation.

## Consequences

### Positive

- Fully offline after installation — no network dependency
- No recurring cost or API keys
- Acceptable voice quality for communication use
- Bundleable with the application
- Single engine keeps MVP scope tight

### Negative

- Integration complexity depends on Kokoro runtime (Python subprocess, ONNX, or native binding) — requires early spike
- Packaging may increase installer size (model files)
- Limited voice selection compared to cloud providers
- Latency characteristics need validation

## Links

- Related items:
  - FEAT-007
  - OQ-01 (Kokoro packaging strategy)
