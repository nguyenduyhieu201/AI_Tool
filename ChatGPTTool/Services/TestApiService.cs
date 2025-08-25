using System.Net.Http.Json;

namespace ChatGPTTool.Services;

public interface ITestApiService
{
    Task<string> TestProtectedEndpointAsync();
    Task<string> TestUserProfileAsync();
}

public class TestApiService : ITestApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public TestApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<string> TestProtectedEndpointAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.GetAsync("/api/user/profile");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return $"Success: {content}";
            }
            else
            {
                return $"Error: {response.StatusCode} - {response.ReasonPhrase}";
            }
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }

    public async Task<string> TestUserProfileAsync()
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient("API");
            var response = await httpClient.GetAsync("/api/user/profile");
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return $"Profile loaded successfully: {content}";
            }
            else
            {
                return $"Failed to load profile: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            return $"Error loading profile: {ex.Message}";
        }
    }
}



