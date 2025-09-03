using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.Cache;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

namespace Users.Infrastructure.Security
{
    public class RefreshTokenCacheService : IRefreshTokenCacheService
    {
        private readonly IRedisService _redisService;
        private const string CacheKeyPrefix = "refresh_token:";

        public RefreshTokenCacheService(IRedisService redisService)
        {
            _redisService = redisService;
        }

        public async Task<RefreshToken?> GetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            var cacheKey = $"{CacheKeyPrefix}{token}";

            // Only check Redis cache
            var cachedToken = await _redisService.GetAsync<RefreshToken>(cacheKey);
            return cachedToken;
        }

        public async Task<bool> StoreTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            try
            {
                // Only save to Redis cache
                var cacheKey = $"{CacheKeyPrefix}{token.Token}";
                var ttl = token.ExpiresAt - DateTime.UtcNow;
                
                if (ttl > TimeSpan.Zero)
                {
                    await _redisService.SetAsync(cacheKey, token, ttl);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{token.Token}";

                // Only update Redis cache
                if (token.IsActive)
                {
                    var ttl = token.ExpiresAt - DateTime.UtcNow;
                    if (ttl > TimeSpan.Zero)
                    {
                        await _redisService.SetAsync(cacheKey, token, ttl);
                    }
                }
                else
                {
                    // Remove from cache if token is revoked
                    await _redisService.DeleteAsync(cacheKey);
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeTokenAsync(string token, string revokedByIp, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{token}";

                // Only remove from Redis cache
                await _redisService.DeleteAsync(cacheKey);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                // This method requires database access, so it's not suitable for cache-only service
                // The repository should handle this operation
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Redis will automatically expire keys based on TTL
                // No manual cleanup needed for cache service
            }
            catch
            {
                // Log error but don't throw
            }
        }
    }
}
