using System;
using System.Threading;
using System.Threading.Tasks;
using Users.Domain.Models;

namespace Users.Application.Contracts.Security
{
    public interface IRefreshTokenCacheService
    {
        Task<RefreshToken?> GetTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<bool> StoreTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task<bool> UpdateTokenAsync(RefreshToken token, CancellationToken cancellationToken = default);
        Task<bool> RevokeTokenAsync(string token, string revokedByIp, string reason, CancellationToken cancellationToken = default);
        Task<bool> RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default);
        Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}









