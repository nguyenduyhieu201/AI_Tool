using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.Security.Claims;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;

namespace ChatGPTTool.Services;

public interface IAuthService
{
    Task<bool> IsAuthenticatedAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetRefreshTokenAsync();
    Task SignInWithTokenAsync(string token, string refreshToken);
    Task<bool> RegisterAsync(RegisterRequest request);
    Task<bool> LoginAsync(string email, string password, string ipAddress);
    Task<bool> LoginWithGoogleAsync(string googleToken);
    Task<bool> RefreshTokenAsync();
    Task<bool> RequestPasswordResetAsync(string email);
    Task<bool> ResetPasswordAsync(string token, string newPassword);
    event Action<bool>? OnAuthenticationStateChanged;
}

public class AuthService : IAuthService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private UserInfo? _currentUser;
    private bool? _isAuthenticated;
    private const string TokenKey = "jwt_token";
    private const string RefreshTokenKey = "refresh_token";
    public event Action<bool>? OnAuthenticationStateChanged;

    public AuthService(ProtectedSessionStorage sessionStorage, IHttpClientFactory httpClientFactory)
    {
        _sessionStorage = sessionStorage;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        if (_isAuthenticated.HasValue)
            return _isAuthenticated.Value;

        var token = await GetTokenAsync();
        _isAuthenticated = !string.IsNullOrEmpty(token) && !IsTokenExpired(token);
        return _isAuthenticated.Value;
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        if (_currentUser != null)
            return _currentUser;

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            
            _currentUser = new UserInfo
            {
                Id = jwtToken.Claims.FirstOrDefault(x => x.Type == "sub")?.Value ?? "",
                Email = jwtToken.Claims.FirstOrDefault(x => x.Type == "email")?.Value ?? "",
                Name = jwtToken.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "",
                Role = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? ""
            };

            return _currentUser;
        }
        catch
        {
            return null;
        }
    }

    public async Task<string?> GetTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(TokenKey);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(RefreshTokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> LoginAsync(string email, string password, string ipAddress)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var loginRequest = new { Username = email, Password = password , IpAddress = ipAddress};
            
            var response = await httpClient.PostAsJsonAsync("/api/user/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                // Prefer full DTO when backend returns LoginResponseDto
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.AccessToken))
                {
                    await SignInWithTokenAsync(loginResponse.AccessToken, loginResponse.RefreshToken);
                    return true;
                }
                // Fallback: raw token string
                var token = (await response.Content.ReadAsStringAsync())?.Trim('"');
                if (!string.IsNullOrEmpty(token))
                {
                    await SignInWithTokenAsync(token!, "");
                    return true;
                }
            }
            
            return false;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task<bool> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.PostAsJsonAsync("/api/users/register", request);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }
            // Auto-login after successful registration
            return await LoginAsync(request.Email, request.Password, "127.0.0.1");
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> LoginWithGoogleAsync(string googleToken)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var googleLoginRequest = new { Token = googleToken };
            
            var response = await httpClient.PostAsJsonAsync("/api/user/login/google", googleLoginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponseDto>();
                if (loginResponse != null)
                {
                    await SignInWithTokenAsync(loginResponse.AccessToken, loginResponse.RefreshToken);
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RefreshTokenAsync()
    {
        try
        {
            var currentToken = await GetTokenAsync();
            var refreshToken = await GetRefreshTokenAsync();
            
            if (string.IsNullOrEmpty(currentToken) || string.IsNullOrEmpty(refreshToken))
            {
                return false;
            }

            var httpClient = _httpClientFactory.CreateClient("API");
            var refreshRequest = new
            {
                AccessToken = currentToken,
                RefreshToken = refreshToken,
                IpAddress = "127.0.0.1" // Có thể lấy IP thực tế nếu cần
            };
            
            var response = await httpClient.PostAsJsonAsync("/api/user/refresh", refreshRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var refreshResponse = await response.Content.ReadFromJsonAsync<RefreshTokenResponseDto>();
                if (refreshResponse != null)
                {
                    await SignInWithTokenAsync(refreshResponse.AccessToken, refreshResponse.RefreshToken);
                    return true;
                }
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task SignInWithTokenAsync(string token, string refreshToken)
    {
        await _sessionStorage.SetAsync(TokenKey, token);
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _sessionStorage.SetAsync(RefreshTokenKey, refreshToken);
        }
        
        _isAuthenticated = true;
        _currentUser = null; // Clear cache to force refresh
        _currentUser = await GetCurrentUserAsync();
        OnAuthenticationStateChanged?.Invoke(true);
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _sessionStorage.DeleteAsync(TokenKey);
            await _sessionStorage.DeleteAsync(RefreshTokenKey);
            _currentUser = null;
            _isAuthenticated = false;
            OnAuthenticationStateChanged?.Invoke(false);
        }
        catch
        {
            // Ignore errors during logout
        }
    }

    private bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }

    public async Task<bool> RequestPasswordResetAsync(string email)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var request = new { Email = email };
            
            var response = await httpClient.PostAsJsonAsync("/api/users/forgot-password", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ResetPasswordAsync(string token, string newPassword)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var request = new { Token = token, NewPassword = newPassword };
            
            var response = await httpClient.PostAsJsonAsync("/api/users/reset-password", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

// DTOs để deserialize response từ API
public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RefreshTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}

