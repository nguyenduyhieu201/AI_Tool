using System.Threading.Tasks;

namespace ChatGPT.API.Shared.Services
{
    // Dummy implementation to unblock DI and local testing
    public class DummyUserServiceClient : IUserServiceClient
    {
        public Task<UserInfoDto?> GetUserByIdAsync(string userId, string token)
        {
            // Return a fake user for local testing
            var user = new UserInfoDto
            {
                Id = string.IsNullOrWhiteSpace(userId) ? "demo-user" : userId,
                Email = "demo@example.com",
                FirstName = "Demo",
                LastName = "User",
                Role = "User"
            };
            return Task.FromResult<UserInfoDto?>(user);
        }

        public Task<UserInfoDto?> GetUserByEmailAsync(string email, string token)
        {
            var user = new UserInfoDto
            {
                Id = "demo-user",
                Email = string.IsNullOrWhiteSpace(email) ? "demo@example.com" : email,
                FirstName = "Demo",
                LastName = "User",
                Role = "User"
            };
            return Task.FromResult<UserInfoDto?>(user);
        }

        public Task<bool> ValidateTokenAsync(string token)
        {
            // Always valid in dummy mode
            return Task.FromResult(true);
        }

        public Task<RefreshTokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            var res = new RefreshTokenResponse
            {
                AccessToken = "dummy-access-token",
                RefreshToken = refreshToken ?? "dummy-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            };
            return Task.FromResult<RefreshTokenResponse?>(res);
        }
    }
}
