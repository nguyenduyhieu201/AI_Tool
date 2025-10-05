using System;
using Users.Domain.Abstractions;

namespace Users.Domain.Models
{
    public class PasswordResetToken : Entity<Guid>
    {
        private PasswordResetToken() { } // For EF Core

        public Guid UserId { get; private set; }
        public string Token { get; private set; } = string.Empty;
        public DateTime ExpiresAt { get; private set; }
        public bool IsUsed { get; private set; }
        public DateTime? UsedAt { get; private set; }
        public string IpAddress { get; private set; } = string.Empty;

        // Navigation property
        public User User { get; private set; } = null!;

        public static PasswordResetToken Create(Guid userId, string token, DateTime expiresAt, string ipAddress)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("User ID cannot be empty", nameof(userId));
            if (string.IsNullOrWhiteSpace(token))
                throw new ArgumentException("Token cannot be empty", nameof(token));
            if (string.IsNullOrWhiteSpace(ipAddress))
                throw new ArgumentException("IP address cannot be empty", nameof(ipAddress));

            return new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Token = token,
                ExpiresAt = expiresAt,
                IsUsed = false,
                IpAddress = ipAddress,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsUsed()
        {
            IsUsed = true;
            UsedAt = DateTime.UtcNow;
            LastModified = DateTime.UtcNow;
        }

        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
        public bool IsValid => !IsUsed && !IsExpired;
    }
}

