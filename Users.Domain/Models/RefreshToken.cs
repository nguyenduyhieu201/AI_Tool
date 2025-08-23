using System;
using Users.Domain.Abstractions;

namespace Users.Domain.Models
{
    public class RefreshToken : Entity<Guid>
    {
        private RefreshToken() { } // For EF Core

        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public string CreatedByIp { get; private set; } = string.Empty;
        public DateTime? RevokedAt { get; private set; }
        public string? RevokedByIp { get; private set; }
        public string? ReplacedByToken { get; private set; }
        public string? ReasonRevoked { get; private set; }
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;

        // Foreign key
        public Guid UserId { get; private set; }
        public User User { get; private set; } = null!;

        public static RefreshToken Create(
            string token,
            DateTime expiresAt,
            Guid userId,
            string createdByIp)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty", nameof(token));
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(createdByIp))
                throw new ArgumentException("Created by IP cannot be empty", nameof(createdByIp));

            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Token = token,
                ExpiresAt = expiresAt,
                UserId = userId,
                CreatedByIp = createdByIp,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void Revoke(string revokedByIp, string? reason = null, string? replacedByToken = null)
        {
            RevokedAt = DateTime.UtcNow;
            RevokedByIp = revokedByIp;
            ReasonRevoked = reason;
            ReplacedByToken = replacedByToken;
            LastModified = DateTime.UtcNow;
        }
    }
}