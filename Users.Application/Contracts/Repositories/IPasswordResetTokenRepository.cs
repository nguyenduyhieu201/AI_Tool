using Users.Domain.Models;

namespace Users.Application.Contracts.Repositories
{
    public interface IPasswordResetTokenRepository
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
        Task<PasswordResetToken?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<PasswordResetToken>> GetValidTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task AddAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
        Task UpdateAsync(PasswordResetToken token, CancellationToken cancellationToken = default);
        Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
        Task RevokeValidTokensForUserAsync(Guid userId, CancellationToken cancellationToken = default);
        Task CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
    }
}

