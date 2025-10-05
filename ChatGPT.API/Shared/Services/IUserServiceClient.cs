namespace ChatGPT.API.Shared.Services;

public interface IUserServiceClient
{
    Task<UserInfoDto?> GetUserByIdAsync(string userId, string token);
    Task<UserInfoDto?> GetUserByEmailAsync(string email, string token);
    Task<bool> ValidateTokenAsync(string token);
    Task<RefreshTokenResponse?> RefreshTokenAsync(string refreshToken);
}

public class UserInfoDto
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RefreshTokenResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}


