using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlock.Cache;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;
using Microsoft.Extensions.Logging;

namespace Users.Infrastructure.Security
{
    public class RefreshTokenCacheService : IRefreshTokenCacheService
    {
        private readonly IRedisService _redisService;
        private readonly ILogger<RefreshTokenCacheService> _logger;
        private const string CacheKeyPrefix = "refresh_token:";

        // Lightweight DTO to avoid serializing full EF entities with navigation properties
        private sealed class CachedRefreshToken
        {
            public string Token { get; init; } = string.Empty;
            public DateTime ExpiresAt { get; init; }
            public DateTime CreatedAt { get; init; }
            public string CreatedByIp { get; init; } = string.Empty;
            public Guid UserId { get; init; }
            public DateTime? RevokedAt { get; init; }
            public string? RevokedByIp { get; init; }
            public string? ReplacedByToken { get; init; }
            public string? ReasonRevoked { get; init; }
        }

        public RefreshTokenCacheService(IRedisService redisService, ILogger<RefreshTokenCacheService> logger)
        {
            _redisService = redisService;
            _logger = logger;
        }

        public async Task<RefreshToken?> GetTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            try
            {
                // We store DTO in cache; repository reads DB, so return null here.
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get token from cache: {Token}", token);
                return null;
            }
        }

        public async Task<bool> StoreTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{token.Token}";
                var ttl = token.ExpiresAt - DateTime.UtcNow;

                if (ttl > TimeSpan.Zero)
                {
                    var dto = new CachedRefreshToken
                    {
                        Token = token.Token,
                        ExpiresAt = token.ExpiresAt,
                        CreatedAt = token.CreatedAt,
                        CreatedByIp = token.CreatedByIp,
                        UserId = token.UserId,
                        RevokedAt = null,
                        RevokedByIp = null,
                        ReplacedByToken = null,
                        ReasonRevoked = null
                    };
                    await _redisService.SetAsync(cacheKey, dto, ttl);
                    _logger.LogDebug("Token cached successfully: {Token}", token.Token);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to store token in cache: {Token}", token.Token);
                return false; // Cache failure is not critical - database is still available
            }
        }

        public async Task<bool> UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{token.Token}";

                if (token.IsActive)
                {
                    var ttl = token.ExpiresAt - DateTime.UtcNow;
                    if (ttl > TimeSpan.Zero)
                    {
                        var dto = new CachedRefreshToken
                        {
                            Token = token.Token,
                            ExpiresAt = token.ExpiresAt,
                            CreatedAt = token.CreatedAt,
                            CreatedByIp = token.CreatedByIp,
                            UserId = token.UserId,
                            RevokedAt = null,
                            RevokedByIp = null,
                            ReplacedByToken = token.ReplacedByToken,
                            ReasonRevoked = null
                        };
                        await _redisService.SetAsync(cacheKey, dto, ttl);
                        _logger.LogDebug("Token updated in cache: {Token}", token.Token);
                    }
                }
                else
                {
                    await _redisService.DeleteAsync(cacheKey);
                    _logger.LogDebug("Token removed from cache: {Token}", token.Token);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update token in cache: {Token}", token.Token);
                return false; // Cache failure is not critical
            }
        }

        public async Task<bool> RevokeTokenAsync(string token, string revokedByIp, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheKey = $"{CacheKeyPrefix}{token}";
                await _redisService.DeleteAsync(cacheKey);
                _logger.LogDebug("Token revoked from cache: {Token}", token);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke token from cache: {Token}", token);
                return false; // Cache failure is not critical
            }
        }

        public async Task<bool> RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default)
        {
            try
            {
                // This method requires database access, so it's not suitable for cache-only service
                // The repository should handle this operation
                _logger.LogDebug("RevokeAllUserTokensAsync called for user: {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to revoke all user tokens from cache: {UserId}", userId);
                return false;
            }
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                // Redis will automatically expire keys based on TTL
                // No manual cleanup needed for cache service
                _logger.LogDebug("CleanupExpiredTokensAsync called");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup expired tokens from cache");
            }
        }
    }
}