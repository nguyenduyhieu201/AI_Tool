using System;
using System.Collections.Generic;
using Users.Domain.Abstractions;

namespace Users.Domain.Models
{
    public class User : Entity<Guid>
    {
        private User() { } // For EF Core

        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public string Provider { get; private set; } = "Local"; // Local provider
        public bool IsActive { get; private set; }
        
        public static User Create(
            string firstName,
            string lastName,
            string email,
            string passwordHash,
            string phoneNumber,
            string provider = "Local")
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));
            if (string.IsNullOrWhiteSpace(provider))
                provider = "Local"; // Default to Local provider

            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email.ToLowerInvariant(),
                PasswordHash = passwordHash,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow,
                Provider = provider,
                IsActive = true
            };
        }

        public static User CreateWithProvider(string firstName,
            string lastName,
            string email,
            string provider,
            string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new ArgumentException("First name cannot be empty", nameof(firstName));
            if (string.IsNullOrWhiteSpace(lastName))
                throw new ArgumentException("Last name cannot be empty", nameof(lastName));
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));
            if (string.IsNullOrWhiteSpace(provider))
                throw new ArgumentException("Provider cannot be empty", nameof(provider));

            return new User
            {
                Id = Guid.NewGuid(),
                FirstName = firstName,
                LastName = lastName,
                Email = email.ToLowerInvariant(),
                Provider = provider,
                PhoneNumber = phoneNumber,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };
        }

        public void Update(
            string? firstName = null,
            string? lastName = null,
            string? phoneNumber = null)
        {
            if (firstName != null)
                FirstName = firstName;
            if (lastName != null)
                LastName = lastName;
            if (phoneNumber != null)
                PhoneNumber = phoneNumber;

            LastModified = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            LastModified = DateTime.UtcNow;
        }

        public void Activate()
        {
            IsActive = true;
            LastModified = DateTime.UtcNow;
        }

        public void UpdatePassword(string newPasswordHash)
        {
            if (string.IsNullOrWhiteSpace(newPasswordHash))
                throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

            PasswordHash = newPasswordHash;
            LastModified = DateTime.UtcNow;
        }
    }
}
