using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IRefreshTokenCacheService _cacheService;

        public RefreshTokenRepository(ApplicationDbContext context, IRefreshTokenCacheService cacheService)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _cacheService = cacheService ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            // Direct database access
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
        }

        public async Task<RefreshToken?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // This method still needs direct database access for user-specific queries
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            // This method still needs direct database access for user-specific queries
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            // 1. Save to database
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            // 2. Update cache
            await _cacheService.StoreTokenAsync(refreshToken, cancellationToken);
        }

        public async Task UpdateAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
        {
            // 1. Update database
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            // 2. Update cache
            await _cacheService.UpdateTokenAsync(refreshToken, cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var refreshToken = await _context.RefreshTokens.FindAsync(new object[] { id }, cancellationToken);
            if (refreshToken != null)
            {
                // 1. Remove from cache
                await _cacheService.RevokeTokenAsync(refreshToken.Token, "Deleted", "Token deleted", cancellationToken);
                
                // 2. Remove from database
                _context.RefreshTokens.Remove(refreshToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            // Direct database cleanup
            var expiredTokens = await _context.RefreshTokens
                .Where(rt => rt.ExpiresAt < DateTime.UtcNow)
                .ToListAsync(cancellationToken);
            
            _context.RefreshTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, string revokedByIp, string reason, CancellationToken cancellationToken = default)
        {
            // 1. Get all active tokens for this user
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync(cancellationToken);
            
            // 2. Revoke in database
            foreach (var token in userTokens)
            {
                token.Revoke(revokedByIp, reason);
            }
            await _context.SaveChangesAsync(cancellationToken);
            
            // 3. Remove from cache
            foreach (var token in userTokens)
            {
                await _cacheService.RevokeTokenAsync(token.Token, revokedByIp, reason, cancellationToken);
            }
        }
    }
}