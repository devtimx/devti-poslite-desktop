using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Core.Interfaces;

public interface IConfigService
{
    Task<StoreConfig> GetConfigAsync();
    Task SaveConfigAsync(StoreConfig config);
}
