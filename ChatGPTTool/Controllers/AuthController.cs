using Microsoft.AspNetCore.Mvc;
using ChatGPTTool.Services;

namespace ChatGPTTool.Controllers
{
	[ApiController]
	[Route("auth")]
	public class AuthController : ControllerBase
	{
		private readonly IGoogleAuthService _googleAuthService;
		private readonly IAuthService _authService;
		private readonly IHttpClientFactory _httpClientFactory;

		public AuthController(IGoogleAuthService googleAuthService, IAuthService authService, IHttpClientFactory httpClientFactory)
		{
			_googleAuthService = googleAuthService;
			_authService = authService;
			_httpClientFactory = httpClientFactory;
		}

		[HttpGet("google")]
		public IActionResult GoogleAuth([FromQuery] string? returnUrl)
		{
			try
			{
				var authUrl = _googleAuthService.GetGoogleAuthUrl();
				return Redirect(authUrl);
			}
			catch (Exception ex)
			{
				return BadRequest($"Lỗi khi tạo Google OAuth URL: {ex.Message}");
			}
		}

		[HttpGet("google/callback")]
		public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string? error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				return Redirect($"/login?error=Google+OAuth+error:+{error}");
			}

			if (string.IsNullOrEmpty(code))
			{
				return Redirect("/login?error=Authorization+code+not+received");
			}

			try
			{
				// 1) Đổi code lấy Google access_token
				var googleAccessToken = await _googleAuthService.ExchangeCodeForAccessTokenAsync(code);

				// 2) Gọi Users.API để sinh JWT của app
				var http = _httpClientFactory.CreateClient("API");
				var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "";
				var payload = new { Token = googleAccessToken, IpAddress = ip };
				var response = await http.PostAsJsonAsync("/api/auth/login/google", payload);
				if (!response.IsSuccessStatusCode)
				{
					return Redirect("/login?error=Login+with+Google+failed");
				}

				var loginResponse = await response.Content.ReadFromJsonAsync<LoginResponse>();
				if (loginResponse == null || string.IsNullOrWhiteSpace(loginResponse.AccessToken))
				{
					return Redirect("/login?error=Invalid+JWT+response");
				}

				// 3) Redirect về /login kèm cả jwt và refresh token
				var jwt = Uri.EscapeDataString(loginResponse.AccessToken);
				var refreshToken = Uri.EscapeDataString(loginResponse.RefreshToken);
				return Redirect($"/login?jwt={jwt}&refreshToken={refreshToken}");
			}
			catch (Exception ex)
			{
				return Redirect($"/login?error=Failed+to+complete+Google+login:+{Uri.EscapeDataString(ex.Message)}");
			}
		}
	}

	public class LoginResponse
	{
		public string AccessToken { get; set; } = string.Empty;
		public string RefreshToken { get; set; } = string.Empty;
		public DateTime ExpiresAt { get; set; }
	}
}




