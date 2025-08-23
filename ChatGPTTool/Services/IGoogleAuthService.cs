namespace ChatGPTTool.Services
{
    public interface IGoogleAuthService
    {
        string GetGoogleAuthUrl();
        Task<string> ExchangeCodeForAccessTokenAsync(string code);
        Task<GoogleUserInfo> GetUserInfoAsync(string code);
    }

    public class GoogleUserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Picture { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
    }
}




