namespace Users.API.DTOs;

public class ThirdPartyLoginRequest
{
    public string Token { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty; // "Google", "Facebook", etc.
} 