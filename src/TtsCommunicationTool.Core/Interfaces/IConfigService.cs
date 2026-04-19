using TtsCommunicationTool.Core.Models;

namespace TtsCommunicationTool.Core.Interfaces;

public interface IConfigService
{
    AppConfig CurrentConfig { get; }
    Task<AppConfig> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(AppConfig config, CancellationToken ct = default);
    AppConfig GetDefaults();
}
