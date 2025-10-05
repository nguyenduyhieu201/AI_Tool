using System.Net;
using System.Net.Http.Headers;

namespace ChatGPTTool.Services;

public class HttpInterceptor : DelegatingHandler
{
    private readonly IAuthService _authService;
    private readonly ILogger<HttpInterceptor> _logger;
    private bool _isRefreshing = false;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public HttpInterceptor(IAuthService authService, ILogger<HttpInterceptor> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Thêm token vào header nếu có
        var token = await _authService.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        // Nếu gặp 401 Unauthorized, thử refresh token
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogInformation("Received 401 Unauthorized, attempting to refresh token");
            
            var refreshSuccess = await RefreshTokenAndRetryAsync(request, cancellationToken);
            if (refreshSuccess)
            {
                // Retry request với token mới
                var newToken = await _authService.GetTokenAsync();
                if (!string.IsNullOrEmpty(newToken))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);
                    response = await base.SendAsync(request, cancellationToken);
                    _logger.LogInformation("Request retried successfully with new token");
                }
            }
        }

        return response;
    }

    private async Task<bool> RefreshTokenAndRetryAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Sử dụng semaphore để tránh multiple refresh requests
        await _semaphore.WaitAsync(cancellationToken);
        
        try
        {
            // Nếu đang refresh, đợi
            if (_isRefreshing)
            {
                // Đợi tối đa 10 giây
                var waitTime = 0;
                while (_isRefreshing && waitTime < 100)
                {
                    await Task.Delay(100, cancellationToken);
                    waitTime++;
                }
                
                // Kiểm tra xem có token mới không
                var newToken = await _authService.GetTokenAsync();
                return !string.IsNullOrEmpty(newToken);
            }

            _isRefreshing = true;
            
            try
            {
                var refreshSuccess = await _authService.RefreshTokenAsync();
                if (refreshSuccess)
                {
                    _logger.LogInformation("Token refreshed successfully");
                    return true;
                }
                else
                {
                    _logger.LogWarning("Failed to refresh token");
                    return false;
                }
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return false;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _semaphore?.Dispose();
        }
        base.Dispose(disposing);
    }
}



