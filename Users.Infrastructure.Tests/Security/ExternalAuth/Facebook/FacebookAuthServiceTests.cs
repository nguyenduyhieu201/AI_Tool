using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BuildingBlock.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Users.Application.Contracts.Security;
using Users.Domain.Models;
using Users.Infrastructure.Security.ExternalAuth.Facebook;
using Xunit;

namespace Users.Infrastructure.Tests.Security.ExternalAuth.Facebook
{
    public class FacebookAuthServiceTests
    {
        private readonly Mock<ILogger<FacebookAuthService>> _loggerMock;
        private readonly Mock<IOptions<FacebookAuthOptions>> _optionsMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly FacebookAuthService _facebookAuthService;
        private readonly string _validToken = "valid_facebook_token";
        private readonly string _invalidToken = "invalid_facebook_token";

        public FacebookAuthServiceTests()
        {
            _loggerMock = new Mock<ILogger<FacebookAuthService>>();
            _optionsMock = new Mock<IOptions<FacebookAuthOptions>>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _optionsMock.Setup(x => x.Value).Returns(new FacebookAuthOptions
            {
                AppId = "test_app_id",
                AppSecret = "test_app_secret"
            });

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _facebookAuthService = new FacebookAuthService(
                httpClient,
                _optionsMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetUserInfoAsync_WithValidToken_ReturnsUserInfo()
        {
            // Arrange
            var expectedUserInfo = new
            {
                id = "123456789",
                email = "test@example.com",
                first_name = "John",
                last_name = "Doe",
                picture = new { data = new { url = "https://example.com/photo.jpg" } }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(expectedUserInfo))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _facebookAuthService.GetUserInfoAsync(_validToken);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedUserInfo.id, result.Id);
            Assert.Equal(expectedUserInfo.email, result.Email);
            Assert.Equal(expectedUserInfo.first_name, result.FirstName);
            Assert.Equal(expectedUserInfo.last_name, result.LastName);
            Assert.Equal(expectedUserInfo.picture.data.url, result.PictureUrl);
        }

        [Fact]
        public async Task GetUserInfoAsync_WithInvalidToken_ThrowsAuthenticationException()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act & Assert
            await Assert.ThrowsAsync<AuthenticationException>(
                () => _facebookAuthService.GetUserInfoAsync(_invalidToken));
        }

        [Fact]
        public async Task ValidateTokenAsync_WithValidToken_ReturnsTrue()
        {
            // Arrange
            var validationResponse = new
            {
                data = new { is_valid = true }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(validationResponse))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _facebookAuthService.ValidateTokenAsync(_validToken);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithInvalidToken_ReturnsFalse()
        {
            // Arrange
            var validationResponse = new
            {
                data = new { is_valid = false }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(validationResponse))
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _facebookAuthService.ValidateTokenAsync(_invalidToken);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ValidateTokenAsync_WithHttpError_ReturnsFalse()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Act
            var result = await _facebookAuthService.ValidateTokenAsync(_validToken);

            // Assert
            Assert.False(result);
        }
    }
} 