---
label: Dependency Problems
icon: package-dependencies
order: 80
---

# Dependency Problems

## .NET Runtime

**Symptoms**: "Failed to load runtime" or app won't start

**Solution**: Install .NET 10 runtime (x64) from [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/10.0)

## VB-Cable / Virtual Audio Cable

**Symptoms**: No secondary output device available in settings

**Solution**: Download and install from [VB-Audio](https://vb-audio.com/Cable/)

## Kokoro Model

**Symptoms**: "TTS engine failed to initialize"

**Solution**: The model (~320MB) auto-downloads on first run. Ensure:
- Internet connection for first download
- Sufficient disk space
- Antivirus isn't blocking the download

## NuGet Packages

If building from source and getting build errors:

```powershell
dotnet restore
```

Key packages:
| Package | Version | Purpose |
|---------|---------|---------|
| `KokoroSharp.CPU` | 0.6.7 | Kokoro TTS engine |
| `NAudio` | 2.2.* | Audio playback and device enumeration |
| `Serilog` | 4.* | Crash log output |
| `Serilog.Sinks.File` | 6.* | File logging sink |
| `Microsoft.Extensions.DependencyInjection` | 9.* | DI container |
| `System.Security.Cryptography.ProtectedData` | 9.* | DPAPI encryption |
| `xunit` | 2.* | Testing framework |
| `Moq` | 4.* | Mocking library |
