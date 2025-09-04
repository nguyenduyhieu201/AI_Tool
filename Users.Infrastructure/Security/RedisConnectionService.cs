using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Users.Infrastructure.Security
{
    public class RedisConnectionService
    {
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisConnectionService> _logger;

        public RedisConnectionService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisConnectionService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer;
            _logger = logger;
        }

        public async Task<bool> IsConnectedAsync()
        {
            try
            {
                var database = _connectionMultiplexer.GetDatabase();
                await database.PingAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Redis connection failed");
                return false;
            }
        }

        public IDatabase GetDatabase()
        {
            return _connectionMultiplexer.GetDatabase();
        }
    }
}

