using System.Security.Cryptography;
using Users.Application.Contracts.Security;

namespace Users.Infrastructure.Security
{
    public class TokenGenerator : ITokenGenerator
    {
        public string GeneratePasswordResetToken()
        {
            return GenerateSecureToken(32);
        }

        public string GenerateEmailVerificationToken()
        {
            return GenerateSecureToken(32);
        }

        private static string GenerateSecureToken(int length)
        {
            var randomBytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .Replace("=", "")
                .Substring(0, Math.Min(length, 32));
        }
    }
}






















