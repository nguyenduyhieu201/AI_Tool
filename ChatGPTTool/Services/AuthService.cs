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
    Task SignInWithTokenAsync(string token);
    Task<bool> LoginAsync(string email, string password, string ipAddress);
    Task<bool> LoginWithGoogleAsync(string googleToken);
    event Action<bool>? OnAuthenticationStateChanged;
}

public class AuthService : IAuthService
{
    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly IHttpClientFactory _httpClientFactory;
    private UserInfo? _currentUser;
    private bool? _isAuthenticated;
    private const string TokenKey = "jwt_token";
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
            
            var response = await httpClient.PostAsJsonAsync("/api/auth/login", loginRequest);
            
            if (response.IsSuccessStatusCode)
            {
                var token = await response.Content.ReadAsStringAsync();
                token = token.Trim('"'); // Remove JSON quotes
                
                await SignInWithTokenAsync(token);
                return true;
            }
            
            return false;
        }
        catch (Exception ex)
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
                var token = await response.Content.ReadAsStringAsync();
                token = token.Trim('"'); // Remove JSON quotes
                
                await SignInWithTokenAsync(token);
                return true;
            }
            
            return false;
        }
        catch
        {
            return false;
        }
    }

    public async Task SignInWithTokenAsync(string token)
    {
        await _sessionStorage.SetAsync(TokenKey, token);
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

    public async Task SignInWithTokenAsync(string token)
    {
        // l?u token b?ng ProtectedSessionStorage
        await _sessionStorage.SetAsync(TokenKey, token);
        _isAuthenticated = true;

        // Clear cached user info to force refresh
        _currentUser = null;

        // Get user info immediately
        _currentUser = await GetCurrentUserAsync();

        // raise event T?I service
        OnAuthenticationStateChanged?.Invoke(true);
    }
}

public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

