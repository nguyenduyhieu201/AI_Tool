using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BuildingBlock.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

namespace Users.Infrastructure.Security.ExternalAuth.Facebook
{
    /// <summary>
    /// Service xử lý authentication với Facebook
    /// </summary>
    public class FacebookAuthService : IFacebookAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly FacebookAuthOptions _options;
        private readonly ILogger<FacebookAuthService> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        public FacebookAuthService(
            HttpClient httpClient,
            IOptions<FacebookAuthOptions> options,
            ILogger<FacebookAuthService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        /// <summary>
        /// Lấy thông tin user từ Facebook
        /// </summary>
        public async Task<FacebookUserInfo> GetUserInfoAsync(string token)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var url = $"{_options.GraphApiBaseUrl}/{_options.ApiVersion}/me?fields=id,email,first_name,last_name,picture&access_token={token}";
                    return await _httpClient.GetAsync(url);
                });

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var facebookResponse = JsonSerializer.Deserialize<FacebookGraphResponse>(content);

                if (facebookResponse == null)
                {
                    throw new AuthenticationException("Failed to parse Facebook response");
                }

                return new FacebookUserInfo(
                    facebookResponse.Id,
                    facebookResponse.Email,
                    facebookResponse.FirstName,
                    facebookResponse.LastName,
                    facebookResponse.Picture?.Data?.Url
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting Facebook user info");
                throw new AuthenticationException("Failed to get Facebook user info", ex.Message);
            }
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của token
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                {
                    var url = $"{_options.GraphApiBaseUrl}/{_options.ApiVersion}/debug_token?input_token={token}&access_token={_options.AppId}|{_options.AppSecret}";
                    return await _httpClient.GetAsync(url);
                });

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                var validationResponse = JsonSerializer.Deserialize<FacebookTokenValidationResponse>(content);

                return validationResponse?.Data?.IsValid ?? false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Facebook token");
                return false;
            }
        }
    }

    /// <summary>
    /// Response từ Facebook Graph API
    /// </summary>
    internal class FacebookGraphResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public FacebookPicture Picture { get; set; }
    }

    internal class FacebookPicture
    {
        public FacebookPictureData Data { get; set; }
    }

    internal class FacebookPictureData
    {
        public string Url { get; set; }
    }

    /// <summary>
    /// Response khi validate token
    /// </summary>
    internal class FacebookTokenValidationResponse
    {
        public FacebookTokenData Data { get; set; }
    }

    internal class FacebookTokenData
    {
        public bool IsValid { get; set; }
        public string UserId { get; set; }
        public string AppId { get; set; }
        public string Type { get; set; }
        public string Application { get; set; }
        public long DataAccessExpiresAt { get; set; }
        public long ExpiresAt { get; set; }
        public bool IsAnonymous { get; set; }
        public string[] Scopes { get; set; }
    }
}
