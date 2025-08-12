using Microsoft.JSInterop;
using System.Text.Json;

namespace ChatGPTTool.Services;

public interface IIPService
{
    Task<string> GetCurrentIPAsync();
    Task<IPInfo> GetIPInfoAsync();
    Task<ClientInfo> GetClientInfoAsync();
}

public class IPService : IIPService
{
    private readonly IJSRuntime _jsRuntime;

    public IPService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<string> GetCurrentIPAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<string>("getCurrentIP");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting IP: {ex.Message}");
            return "unknown";
        }
    }

    public async Task<IPInfo> GetIPInfoAsync()
    {
        try
        {
            var ipInfoObj = await _jsRuntime.InvokeAsync<object>("getIPInfo");
            if (ipInfoObj != null)
            {
                var json = JsonSerializer.Serialize(ipInfoObj);
                return JsonSerializer.Deserialize<IPInfo>(json) ?? new IPInfo();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting IP info: {ex.Message}");
        }
        
        return new IPInfo();
    }

    public async Task<ClientInfo> GetClientInfoAsync()
    {
        try
        {
            var ip = await GetCurrentIPAsync();
            var ipInfo = await GetIPInfoAsync();
            
            var userAgent = await _jsRuntime.InvokeAsync<string>("navigator.userAgent");
            var screenResolution = await _jsRuntime.InvokeAsync<string>("`${screen.width}x${screen.height}`");
            var timezone = await _jsRuntime.InvokeAsync<string>("Intl.DateTimeFormat().resolvedOptions().timeZone");

            return new ClientInfo
            {
                IP = ip,
                IPInfo = ipInfo,
                UserAgent = userAgent,
                ScreenResolution = screenResolution,
                Timezone = timezone,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting client info: {ex.Message}");
            return new ClientInfo
            {
                IP = "unknown",
                Timestamp = DateTime.UtcNow
            };
        }
    }
}

public class IPInfo
{
    public string IP { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public string ISP { get; set; } = string.Empty;
}

public class ClientInfo
{
    public string IP { get; set; } = string.Empty;
    public IPInfo? IPInfo { get; set; }
    public string UserAgent { get; set; } = string.Empty;
    public string ScreenResolution { get; set; } = string.Empty;
    public string Timezone { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
} 