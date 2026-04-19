using System.Text.Json;
using System.Text.Json.Serialization;
using TtsCommunicationTool.Core.Interfaces;
using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Infrastructure.Config;

public sealed class JsonConfigService : IConfigService
{
    private static readonly string ConfigDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TtsCommunicationTool");

    private static readonly string ConfigPath = Path.Combine(ConfigDir, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private AppConfig _current;

    /// <summary>True if the last LoadAsync call detected a fresh/default config (no file existed).</summary>
    public bool IsFirstRun { get; private set; }

    /// <summary>True if the last LoadAsync call recovered from a corrupt config file.</summary>
    public bool WasRecovered { get; private set; }

    public JsonConfigService()
    {
        _current = GetDefaults();
    }

    public AppConfig CurrentConfig => _current;

    public async Task<AppConfig> LoadAsync(CancellationToken ct = default)
    {
        IsFirstRun = false;
        WasRecovered = false;

        if (!File.Exists(ConfigPath))
        {
            IsFirstRun = true;
            _current = GetDefaults();
            await SaveAsync(_current, ct);
            return _current;
        }

        try
        {
            var json = await File.ReadAllTextAsync(ConfigPath, ct);
            var config = JsonSerializer.Deserialize<AppConfig>(json, JsonOptions);
            _current = config ?? GetDefaults();
        }
        catch
        {
            // Config file is corrupt — back it up and reset to defaults
            WasRecovered = true;
            await BackupCorruptFileAsync(ct);
            _current = GetDefaults();
            await SaveAsync(_current, ct);
        }

        return _current;
    }

    public async Task SaveAsync(AppConfig config, CancellationToken ct = default)
    {
        Directory.CreateDirectory(ConfigDir);
        var json = JsonSerializer.Serialize(config, JsonOptions);
        await File.WriteAllTextAsync(ConfigPath, json, ct);
        _current = config;
    }

    public AppConfig GetDefaults() => new();

    private static async Task BackupCorruptFileAsync(CancellationToken ct)
    {
        try
        {
            if (!File.Exists(ConfigPath)) return;
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(ConfigDir, $"config.corrupt.{stamp}.json");
            // Use async copy via read + write to support cancellation
            var bytes = await File.ReadAllBytesAsync(ConfigPath, ct);
            await File.WriteAllBytesAsync(backupPath, bytes, ct);
        }
        catch
        {
            // Best-effort backup; if this fails we still reset to defaults
        }
    }
}
