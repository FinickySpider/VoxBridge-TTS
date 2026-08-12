# TTS Engine Pipeline and SAPI5 Support Proposal

**Status:** proposed
**Date:** 2026-08-09
**Scope:** Add SAPI5 as a third backend and establish an extensible engine/manifest architecture.

## Executive recommendation

Add SAPI5 through a dedicated `Sapi5TtsService`, but do not add another engine-specific branch to `TtsRouter` as the long-term design. First introduce a registry-based `TtsEngineDescriptor` and `ITtsEngineProvider` abstraction, then register Kokoro, ElevenLabs, and SAPI5 through dependency injection.

The pipeline should have three layers:

1. **Engine registry and manifest layer** — discovers engines, exposes capabilities, validates engine settings, and supplies UI metadata.
2. **Provider adapter layer** — converts the common synthesis request into the engine's native API and returns normalized audio.
3. **Shared audio pipeline** — owns speed/pitch normalization, silence trimming, format conversion, dual-output playback, cancellation, and stop behavior.

The key persistence rule is: **global selection is separate from per-engine profiles**. Switching engines changes only the active engine pointer; it must never overwrite another engine's voice, rate, pitch, speed, volume, or provider-specific settings.

## Current state and impact

The current implementation has a workable seam but is not yet plugin-oriented:

- [ITtsService](../../src/TtsCommunicationTool.Core/Interfaces/ITtsService.cs) is the common synthesis interface.
- [TtsRouter](../../src/TtsCommunicationTool.Infrastructure/Tts/TtsRouter.cs) has hard-coded Kokoro and ElevenLabs dependencies and a `VoiceEngine` switch.
- [VoiceSettings](../../src/TtsCommunicationTool.Core/Models/VoiceSettings.cs) stores one global voice selection, pitch, and speed.
- ElevenLabs settings are already separated into [ElevenLabsSettings](../../src/TtsCommunicationTool.Core/Models/ElevenLabsSettings.cs), which is the right direction for engine-specific persistence.
- [PhraseItem](../../src/TtsCommunicationTool.Core/Models/PhraseItem.cs) stores a nullable engine override, so adding SAPI5 requires preserving phrase-level engine identity and cache metadata.
- [ServiceRegistration](../../src/TtsCommunicationTool.App/ServiceRegistration.cs) registers concrete providers directly.

The minimum SAPI5 change is moderate. The extensible redesign is a larger but worthwhile refactor because the existing router and settings view model currently encode engine names and engine-specific fields in multiple places.

## SAPI5 feasibility

### Recommended implementation

Use `System.Speech.Synthesis.SpeechSynthesizer` as the managed SAPI5 adapter on Windows:

- `GetInstalledVoices()` discovers installed SAPI voices.
- `SelectVoice()` selects a voice using its stable voice name.
- `Rate` supports SAPI's native speaking-rate control.
- `Volume` supports native voice volume.
- `SetOutputToWaveStream()` produces a WAV stream instead of sending audio directly to the default device.
- The resulting WAV is decoded to the common PCM representation used by the existing `IAudioRouterService`.

This keeps SAPI5 inside the same TTS-to-audio pipeline as Kokoro and ElevenLabs. The provider must never call `SetOutputToDefaultAudioDevice()` for normal application speech because that would bypass the application's monitor/virtual-cable routing.

Microsoft documents `SpeechSynthesizer` as an interface to installed speech synthesis engines, including installed voice enumeration, voice selection, rate/volume controls, and WAV stream output:

- [SpeechSynthesizer class](https://learn.microsoft.com/dotnet/api/system.speech.synthesis.speechsynthesizer)
- [GetInstalledVoices](https://learn.microsoft.com/dotnet/api/system.speech.synthesis.speechsynthesizer.getinstalledvoices)
- [SelectVoice](https://learn.microsoft.com/dotnet/api/system.speech.synthesis.speechsynthesizer.selectvoice)
- [SetOutputToWaveStream](https://learn.microsoft.com/dotnet/api/system.speech.synthesis.speechsynthesizer.setoutputtowavestream)

### Project/package considerations

- The application is already Windows-only and WPF-based, so SAPI5's platform constraint is compatible with the product.
- Add the `System.Speech` package to the infrastructure project only. Keep `System.Speech` types out of Core and UI.
- Verify the package/runtime combination against the repository's target framework (`net10.0-windows`) before implementation. Microsoft Learn currently exposes modern `System.Speech` APIs, but package/runtime compatibility should be confirmed in a small spike rather than assumed.
- SAPI voices are installed outside the application. The app must handle zero installed voices, disabled voices, removed voices, and voice-name changes as recoverable configuration states.
- A 32-bit/64-bit mismatch can affect visibility of legacy SAPI components. The shipping x64 target should be tested with both inbox Windows voices and common third-party SAPI5 voice packages.

### Audio and control limitations

SAPI5 is not equivalent to Kokoro:

| Capability | SAPI5 approach | Compatibility decision |
|---|---|---|
| Voice selection | Installed voice name/ID | Supported |
| Speed | Map common multiplier to SAPI `Rate` (`-10..10`) | Supported, with provider-specific mapping |
| Volume | SAPI `Volume` (`0..100`) | Supported if exposed by common settings |
| Pitch | Use SSML only where the selected voice supports it; otherwise neutral | Capability-dependent |
| Raw PCM output | WAV stream then NAudio decode | Supported |
| Cancellation | `SpeakAsync` plus `SpeakAsyncCancelAll`, or isolate synchronous synthesis on a worker | Must be wrapped and tested |
| Offline operation | Yes, when the voice is installed locally | Supported |
| Voice metadata | Name, culture, gender, age, description, supported formats | Supported |

Do not fake unsupported capabilities in the UI. The manifest should mark pitch as unsupported or provider-native when appropriate, and the UI should hide or disable controls accordingly.

## Proposed common pipeline

```text
User action
  -> TtsRequestFactory resolves active engine + persisted profile
  -> TtsEngineRegistry resolves descriptor/provider
  -> provider validates request and synthesizes engine-native audio
  -> AudioNormalizer decodes WAV/MP3/PCM and applies common format rules
  -> PlaybackTransform applies speed/pitch only when requested and supported
  -> IAudioRouterService fans out the normalized buffer to both outputs
  -> PlaybackState / cancellation / logging
```

### Provider contract

Keep the provider contract small and engine-neutral:

```csharp
public interface ITtsEngineProvider : IAsyncDisposable
{
    TtsEngineManifest Manifest { get; }
    bool IsAvailable { get; }
    Task InitializeAsync(CancellationToken ct = default);
    Task<IReadOnlyList<TtsVoice>> GetVoicesAsync(CancellationToken ct = default);
    Task<TtsSynthesisResult> SynthesizeAsync(
        TtsSynthesisRequest request,
        CancellationToken ct = default);
}
```

The provider returns audio plus format metadata. It does not route to devices, own phrase caches, show notifications, or mutate global configuration.

`ITtsService` can remain temporarily as a compatibility facade over the registry while callers migrate. The facade should resolve the provider by engine ID rather than by an enum switch.

### Common request/result types

Use a request model that distinguishes common settings from provider settings:

```csharp
public sealed record TtsSynthesisRequest
{
    public required string Text { get; init; }
    public required string EngineId { get; init; }
    public string? VoiceId { get; init; }
    public float Speed { get; init; } = 1.0f;
    public float Pitch { get; init; } = 1.0f;
    public int VolumePercent { get; init; } = 100;
    public JsonObject ProviderOptions { get; init; } = new();
}
```

The shared pipeline, not each provider, should own the final normalized playback contract. A provider may report that it has already applied speed or pitch so the transform is not accidentally applied twice.

## Manifest/schema design

A manifest should describe capabilities and settings, not contain executable code. Providers remain compiled and dependency-injected; manifests make them discoverable and allow the UI to render compatible settings without engine-name conditionals.

### Manifest shape

```json
{
  "$schema": "https://voxbridge.local/schemas/tts-engine-manifest.v1.json",
  "id": "sapi5",
  "displayName": "SAPI5",
  "version": "1.0.0",
  "platforms": ["windows"],
  "providerType": "TtsCommunicationTool.Infrastructure.Tts.Sapi5TtsEngineProvider",
  "availability": {
    "requiresInstalledVoice": true,
    "requiresNetwork": false
  },
  "audio": {
    "output": "wave-stream",
    "normalization": "pcm16-mono",
    "providerAppliesSpeed": true,
    "providerAppliesPitch": false
  },
  "capabilities": {
    "voiceSelection": true,
    "speed": true,
    "pitch": false,
    "volume": true,
    "ssml": true,
    "pauseResume": false
  },
  "settingsSchema": {
    "type": "object",
    "properties": {
      "voiceId": { "type": "string" },
      "rate": { "type": "integer", "minimum": -10, "maximum": 10 },
      "volume": { "type": "integer", "minimum": 0, "maximum": 100 }
    }
  }
}
```

### Schema requirements

The manifest schema should require:

- Stable `id` using lowercase kebab-case; never use display names as identifiers.
- Semantic `version` for manifest compatibility.
- Supported platforms and availability requirements.
- Audio input/output format and whether transformations are provider-applied.
- Capability flags with explicit unsupported behavior.
- A JSON Schema for provider-specific settings, defaults, ranges, and display metadata.
- A provider type or registry key resolved only from a trusted built-in registry. Do not load arbitrary types or assemblies from untrusted config.

The schema should reject unknown top-level fields only when the manifest version requires strict validation. Provider-specific settings should permit forward-compatible fields where appropriate.

## Configuration and engine-switch persistence

### Core rule

Do not store the active engine's settings in one shared `VoiceSettings` object. Store a stable active engine ID plus an independent profile for every engine.

Recommended model:

```csharp
public sealed class VoiceSettings
{
    public string ActiveEngineId { get; set; } = "kokoro";
    public Dictionary<string, EngineProfile> Profiles { get; set; } = new();
}

public sealed class EngineProfile
{
    public string? VoiceId { get; set; }
    public float Speed { get; set; } = 1.0f;
    public float Pitch { get; set; } = 1.0f;
    public int VolumePercent { get; set; } = 100;
    public JsonObject ProviderSettings { get; set; } = new();
}
```

The persisted JSON should look conceptually like this:

```json
{
  "voiceSettings": {
    "activeEngineId": "sapi5",
    "profiles": {
      "kokoro": {
        "voiceId": "af_heart",
        "speed": 1.15,
        "pitch": 0.95,
        "providerSettings": { "modelPath": "kokoro.onnx" }
      },
      "elevenlabs": {
        "voiceId": "21m00Tcm4TlvDq8ikWAM",
        "speed": 0.9,
        "pitch": 1.0,
        "providerSettings": { "modelId": "eleven_multilingual_v2" }
      },
      "sapi5": {
        "voiceId": "Microsoft David Desktop",
        "speed": 1.0,
        "pitch": 1.0,
        "providerSettings": { "rate": 0, "volume": 100 }
      }
    }
  }
}
```

### Switching algorithm

1. Read the target engine ID from the registry.
2. Load that engine's profile from `Profiles[targetEngineId]`.
3. If missing, create it from the manifest defaults and save only after the user confirms settings or global Save is clicked.
4. Validate the saved voice ID against the provider's current voice list.
5. If unavailable, preserve the stored ID, show a visible unavailable warning, and select a temporary fallback for preview only.
6. Never copy the prior engine's voice or provider settings into the target profile.
7. On Save, persist the active engine ID and all profile dictionaries atomically.
8. On Cancel, restore the complete profile snapshot and active engine ID, not just the active voice.

This guarantees that switching Kokoro → SAPI5 → ElevenLabs → Kokoro restores the prior Kokoro profile exactly, while still allowing a removed SAPI5 voice to be reported as unavailable.

### Migration from the current model

Use a versioned config migration rather than changing fields in place:

- Copy the current `VoiceSettings.Engine` or `EngineName` into `ActiveEngineId` (`kokoro` or `elevenlabs`).
- Move the current global voice, pitch, and speed into the corresponding engine profile.
- Move current [ElevenLabsSettings](../../src/TtsCommunicationTool.Core/Models/ElevenLabsSettings.cs) into the `elevenlabs` profile's provider settings, or retain a compatibility property until migration is complete.
- Keep legacy properties read-only/obsolete for one migration cycle if external tools or old configs may still exist.
- Increment `AppConfig.ConfigVersion` and make migration idempotent.
- Back up the config before migration, consistent with current corrupt-config recovery behavior.

## Phrase cache and engine identity

Phrase cache keys must include the engine ID, voice ID, provider settings fingerprint, and all transformations that affect audio. A suitable cache identity is:

```text
phrase text hash
+ engine id/version
+ voice id
+ provider settings hash
+ speed
+ pitch
+ volume
+ audio pipeline version
```

When an engine is switched, existing phrase caches must not be silently reused for another engine. A phrase with an explicit engine override should continue using that engine profile even when the global active engine changes.

For SAPI5 specifically:

- Store the stable voice ID/name in the phrase snapshot or override profile.
- If the voice is removed, mark the cache unavailable and show an actionable error instead of silently synthesizing with another voice.
- Do not spend network credits for SAPI5; it is local-only.

## UI design

The Voice tab should be manifest-driven:

- Engine selector binds to registry descriptors.
- A shared section renders common controls only when the manifest advertises them.
- A provider-specific settings panel is generated from the validated settings schema, or uses a provider-owned view model for richer controls.
- Voice selection uses the provider's `GetVoicesAsync()` result and displays availability state.
- Unsupported controls are hidden or disabled with an explanation; they are never silently applied.
- Changing engine changes the displayed profile, not the values stored for other engines.
- Save/Cancel operates on a full settings snapshot containing every engine profile.

A practical first iteration can use typed provider view models while the manifest supplies capability flags. Full JSON-schema-generated UI can follow after three providers prove the model.

## Proposed implementation phases

### Phase A — SAPI5 spike

- Add a small `System.Speech` proof of concept.
- Enumerate installed voices and verify stable identifiers.
- Synthesize to a memory-backed WAV stream.
- Decode to the project's common PCM format.
- Verify cancellation, repeated synthesis, voice removal, and x64 packaging.
- Confirm SAPI rate mapping and whether pitch should be marked unsupported.

### Phase B — Provider registry seam

- Introduce `TtsEngineManifest`, `ITtsEngineProvider`, `TtsEngineRegistry`, and common request/result models.
- Wrap existing Kokoro and ElevenLabs implementations without changing user-visible behavior.
- Replace `TtsRouter`'s enum switch with registry lookup.
- Add provider capability metadata and availability diagnostics.

### Phase C — Profile persistence migration

- Add config schema migration to per-engine profiles.
- Snapshot/restore all profiles in settings Cancel.
- Update phrase cache identity and phrase engine overrides.
- Add tests for switching between all engines in every order.

### Phase D — SAPI5 production integration

- Register the SAPI5 provider through DI.
- Add SAPI5 manifest and Voice tab controls.
- Add user-facing installation/availability diagnostics.
- Add integration tests on Windows with at least one installed SAPI5 voice.

### Phase E — Manifest hardening

- Validate manifests at startup.
- Add manifest version compatibility checks.
- Add a trusted built-in provider registry.
- Add a diagnostics view showing provider availability and capability mismatches.

## Testing and acceptance criteria

### Provider tests

- Enumerates installed SAPI5 voices without blocking the UI.
- Selects a saved voice by stable ID.
- Synthesizes valid WAV output for normal text and punctuation.
- Returns a clear failure when no voices are installed or the saved voice is unavailable.
- Honors cancellation and releases `SpeechSynthesizer` resources.
- Does not send audio directly to the default device.

### Pipeline tests

- Every provider returns a normalized audio result accepted by `IAudioRouterService`.
- Speed/pitch are applied exactly once according to manifest flags.
- Stop interrupts SAPI5 playback through the shared audio router.
- Provider failures preserve overlay input and generate a visible notification plus diagnostic log.

### Persistence tests

- Configure unique settings for Kokoro, ElevenLabs, and SAPI5.
- Switch through every engine and verify each profile is restored byte-for-byte where valid.
- Cancel restores active engine and all profiles.
- Restart reloads every profile.
- Missing voices preserve the saved ID and show unavailable status rather than overwriting it.
- Config migration from the current schema is idempotent and creates a backup.

### Compatibility tests

- Windows 10 and Windows 11.
- x64 release build.
- Inbox Microsoft voices and at least one third-party SAPI5 voice.
- No installed SAPI5 voice.
- Voice disabled or removed after configuration.
- Concurrent stop during synthesis and during playback.

## Risks and mitigations

| Risk | Mitigation |
|---|---|
| `System.Speech` package/runtime incompatibility with the target framework | Complete Phase A as an isolated spike before changing the production router |
| SAPI voice IDs are not stable across reinstall or architecture changes | Store ID plus display name; validate on load; never silently overwrite |
| SAPI rate differs perceptually by voice | Store provider-native rate in provider settings and map common speed only as a default |
| Provider-specific UI becomes a large conditional view | Use manifest capabilities and provider-specific settings view models |
| A provider applies speed/pitch and the shared pipeline applies it again | Require explicit `providerApplies*` flags and test the transform matrix |
| Config migration loses settings | Backup, atomic write, idempotent migration, and round-trip tests |
| Arbitrary manifest/provider loading becomes unsafe | Use trusted compiled provider registrations; manifests describe registered providers only |

## Decision requested

Approve the following architectural direction before implementation:

1. SAPI5 is implemented through `System.Speech.Synthesis.SpeechSynthesizer` and WAV-stream output.
2. Providers are registered by stable engine ID through a registry, not a growing enum switch.
3. Engine settings are stored in independent per-engine profiles.
4. Manifests describe capabilities and provider settings schemas, while trusted compiled providers execute synthesis.
5. The shared pipeline owns routing, normalization, cancellation, logging, and cache identity.
