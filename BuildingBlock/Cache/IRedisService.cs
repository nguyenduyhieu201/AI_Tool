using System;
using System.Threading.Tasks;

namespace BuildingBlock.Cache
{
    public interface IRedisService
    {
        Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null);
        Task<T?> GetAsync<T>(string key);
        Task<bool> DeleteAsync(string key);
        Task<bool> ExistsAsync(string key);
        Task<bool> ExpireAsync(string key, TimeSpan expiry);
    }
}

