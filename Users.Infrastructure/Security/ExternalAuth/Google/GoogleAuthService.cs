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

namespace Users.Infrastructure.Security.ExternalAuth.Google
{
    /// <summary>
    /// Service xử lý authentication với Google
    /// </summary>
    public class GoogleAuthService : IGoogleAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly GoogleAuthOptions _options;
        private readonly ILogger<GoogleAuthService> _logger;
        private readonly IAsyncPolicy<HttpResponseMessage> _retryPolicy;

        public GoogleAuthService(
            HttpClient httpClient,
            IOptions<GoogleAuthOptions> options,
            ILogger<GoogleAuthService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Cấu hình retry policy
            _retryPolicy = Policy<HttpResponseMessage>
                .Handle<HttpRequestException>()
                .Or<TimeoutException>()
                .WaitAndRetryAsync(
                    _options.RetryCount,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) * _options.RetryDelay.TotalSeconds),
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.LogWarning(exception.Exception,
                            "Retry {RetryCount} after {Delay}ms due to {Error}",
                            retryCount,
                            timeSpan.TotalMilliseconds,
                            exception.Exception?.Message ?? "unknow error");
                    });

        }

        /// <summary>
        /// Lấy thông tin user từ Google
        /// </summary>
        public async Task<GoogleUserInfo> GetUserInfoAsync(string accessToken)
        {
            try
            {
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.GetAsync("https://openidconnect.googleapis.com/v1/userinfo"));

                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var userInfo = JsonSerializer.Deserialize<GoogleUserInfo>(content);

                if (string.IsNullOrEmpty(userInfo?.Email))
                {
                    throw new GoogleAuthException("Email is required");
                }

                return userInfo;
            }
            catch (Exception ex) when (ex is not GoogleAuthException)
            {
                _logger.LogError(ex, "Error getting user info from Google");
                throw new GoogleAuthException("Failed to get user info from Google", ex.ToString());
            }
        }

        /// <summary>
        /// Validate token với Google
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(async () =>
                    await _httpClient.GetAsync($"https://www.googleapis.com/oauth2/v3/tokeninfo?access_token={token}"));

                if (!response.IsSuccessStatusCode)
                {
                    return false; 
                }

                var content = await response.Content.ReadAsStringAsync();
                var tokenInfo = JsonSerializer.Deserialize<GoogleTokenValidation>(content);

                return tokenInfo?.Audience == _options.ClientId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token with Google");
                return false;
            }
        }
    }
} 