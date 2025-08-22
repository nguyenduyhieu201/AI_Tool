using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.PeopleService.v1;
using Google.Apis.Services;
using Microsoft.Extensions.Configuration;

namespace ChatGPTTool.Services
{
	public class GoogleAuthService : IGoogleAuthService
	{
		private readonly IConfiguration _configuration;
		private readonly string _clientId;
		private readonly string _clientSecret;
		private readonly string _redirectUri;

		public GoogleAuthService(IConfiguration configuration)
		{
			_configuration = configuration;
			_clientId = _configuration["GoogleOAuth:ClientId"] ?? string.Empty;
			_clientSecret = _configuration["GoogleOAuth:ClientSecret"] ?? string.Empty;
			_redirectUri = _configuration["GoogleOAuth:RedirectUri"] ?? string.Empty;
		}

		public string GetGoogleAuthUrl()
		{
			var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
			{
				ClientSecrets = new ClientSecrets
				{
					ClientId = _clientId,
					ClientSecret = _clientSecret
				},
				Scopes = new[]
				{
					"openid",
					"email",
					"profile",
					"https://www.googleapis.com/auth/userinfo.profile",
					"https://www.googleapis.com/auth/userinfo.email"
				}
			});

			var authUrl = flow.CreateAuthorizationCodeRequest(_redirectUri);
			// Explicit parameters to avoid invalid_request
			authUrl.ResponseType = "code";
			authUrl.Scope = "openid email profile https://www.googleapis.com/auth/userinfo.profile https://www.googleapis.com/auth/userinfo.email";
			return authUrl.Build().AbsoluteUri;
		}

		public async Task<string> ExchangeCodeForAccessTokenAsync(string code)
		{
			var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
			{
				ClientSecrets = new ClientSecrets
				{
					ClientId = _clientId,
					ClientSecret = _clientSecret
				},
				Scopes = new[]
				{
					"openid",
					"email",
					"profile",
					"https://www.googleapis.com/auth/userinfo.profile",
					"https://www.googleapis.com/auth/userinfo.email"
				}
			});

			var token = await flow.ExchangeCodeForTokenAsync("", code, _redirectUri, CancellationToken.None);
			return token.AccessToken;
		}

		public async Task<GoogleUserInfo> GetUserInfoAsync(string code)
		{
			var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
			{
				ClientSecrets = new ClientSecrets
				{
					ClientId = _clientId,
					ClientSecret = _clientSecret
				},
				Scopes = new[]
				{
					"https://www.googleapis.com/auth/userinfo.profile",
					"https://www.googleapis.com/auth/userinfo.email"
				}
			});

			var token = await flow.ExchangeCodeForTokenAsync("", code, _redirectUri, CancellationToken.None);

			// Fetch profile via Google People API
			var credential = new UserCredential(flow, "", token);
			var service = new PeopleServiceService(new BaseClientService.Initializer
			{
				HttpClientInitializer = credential,
				ApplicationName = "ChatGPT Tool"
			});

			var request = service.People.Get("people/me");
			request.PersonFields = "names,emailAddresses,photos";
			var profile = await request.ExecuteAsync();

			return new GoogleUserInfo
			{
				Id = profile.ResourceName?.Replace("people/", "") ?? string.Empty,
				Email = profile.EmailAddresses?.FirstOrDefault()?.Value ?? string.Empty,
				Name = profile.Names?.FirstOrDefault()?.DisplayName ?? string.Empty,
				GivenName = profile.Names?.FirstOrDefault()?.GivenName ?? string.Empty,
				FamilyName = profile.Names?.FirstOrDefault()?.FamilyName ?? string.Empty,
				Picture = profile.Photos?.FirstOrDefault()?.Url ?? string.Empty
			};
		}
	}
}


