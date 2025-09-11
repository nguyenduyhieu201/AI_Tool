using Microsoft.EntityFrameworkCore;
using Users.Application.Contracts.Repositories;
using Users.Domain.Models;
using Users.Infrastructure.Data;

namespace Users.Infrastructure.Repositories
{
    public class PasswordResetTokenRepository : IPasswordResetTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public PasswordResetTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token, cancellationToken);
        }

        public async Task<PasswordResetToken?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.PasswordResetTokens
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.UserId == userId, cancellationToken);
        }

        public async Task<IEnumerable<PasswordResetToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.PasswordResetTokens
                .Where(prt => prt.UserId == userId && !prt.IsUsed && prt.ExpiresAt > DateTime.UtcNow)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            await _context.PasswordResetTokens.AddAsync(token, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default)
        {
            _context.PasswordResetTokens.Update(token);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var token = await _context.PasswordResetTokens.FindAsync(new object[] { id }, cancellationToken);
            if (token != null)
            {
                _context.PasswordResetTokens.Remove(token);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task RevokeValidTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var validTokens = await GetValidTokensByUserIdAsync(userId, cancellationToken);
            foreach (var token in validTokens)
            {
                token.MarkAsUsed();
            }
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
        {
            var expiredTokens = await _context.PasswordResetTokens
                .Where(prt => prt.ExpiresAt < DateTime.UtcNow || prt.IsUsed)
                .ToListAsync(cancellationToken);

            _context.PasswordResetTokens.RemoveRange(expiredTokens);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}






















