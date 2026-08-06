using DevtiPosLite.Core.Interfaces;
using DevtiPosLite.Core.Models;

namespace DevtiPosLite.Services;

public class ConfigService : IConfigService
{
    private readonly IRepository<StoreConfig> _repo;

    public ConfigService(IRepository<StoreConfig> repo)
    {
        _repo = repo;
    }

    public async Task<StoreConfig> GetConfigAsync()
    {
        var all = await _repo.GetAllAsync();
        var config = all.FirstOrDefault();
        if (config == null)
        {
            config = new StoreConfig();
            config = await _repo.AddAsync(config);
            await _repo.SaveChangesAsync();
        }
        return config;
    }

    public async Task SaveConfigAsync(StoreConfig config)
    {
        await _repo.UpdateAsync(config);
        await _repo.SaveChangesAsync();
    }
}
