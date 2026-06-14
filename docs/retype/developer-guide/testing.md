---
label: Testing
icon: beaker
order: 75
---

# Testing

## Test Framework

The project uses **xUnit** with **Moq** for mocking.

## Test Project Structure

```
src/TtsCommunicationTool.Tests/
├── Unit/           # Unit tests (currently empty)
└── Integration/    # Integration tests (currently empty)
```

!!!warning
The test project scaffold exists but contains no test files yet. Tests need to be written.
!!!

## What Should Be Tested

### Unit Tests

Based on the codebase architecture, these areas are prime candidates for unit tests:

| Area | What to Test |
|------|-------------|
| `TextValidation` | Empty text, whitespace, max length, sanitization |
| `PhraseValidation` | Null phrase, empty name/text, max lengths |
| `HotkeyValidation` | Null binding, missing key, reserved keys, missing modifiers, Win key rejection, conflict detection |
| `ContrastCalculator` | Ratio calculation, status thresholds, parse failure handling |
| `SilenceTrimmer` | 16-bit PCM trimming, non-16-bit passthrough, retention fraction, edge cases |
| `TextReplacementService` | Single rule, multiple rules, overlapping rules, case sensitivity, whole word, disabled rules |
| `HotkeyBinding` | Equality, hash code, ToString, FriendlyKeyName, Clone, IsEmpty |
| `RecentMessagesState` | Add, duplicate collapse, max items, thread safety |
| `ApiKeyVault` | Encrypt/decrypt round-trip, null handling, ComputeTail |

### Integration Tests

| Area | What to Test |
|------|-------------|
| `JsonConfigService` | Load/save round-trip, corrupt file recovery, first-run detection |
| `PhraseService` | CRUD operations, cache integration |
| `ThemeService` | Load built-in + user themes, apply, save/delete user themes |
| `TranscriptService` | File creation, append, disabled state |

## Running Tests

```powershell
# Run all tests
dotnet test src\TtsCommunicationTool.Tests\TtsCommunicationTool.Tests.csproj

# Run with verbose output
dotnet test src\TtsCommunicationTool.Tests\TtsCommunicationTool.Tests.csproj -v n
```

## Mocking Patterns

Services should be mocked using Moq:

```csharp
var mockConfig = new Mock<IConfigService>();
mockConfig.Setup(c => c.CurrentConfig).Returns(new AppConfig());

var mockLog = new Mock<ILoggingService>();

var service = new MyService(mockConfig.Object, mockLog.Object);
```
