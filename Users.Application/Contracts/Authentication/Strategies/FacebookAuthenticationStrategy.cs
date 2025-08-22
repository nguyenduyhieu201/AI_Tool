using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

namespace Users.Application.Contracts.Authentication.Strategies
{
    public class FacebookAuthenticationStrategy : IAuthenticationStrategy
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IFacebookAuthService _facebookAuthService;
        private readonly ILogger<FacebookAuthenticationStrategy> _logger;

        public FacebookAuthenticationStrategy(
            IUserRepository userRepository,
            IJwtService jwtService,
            IFacebookAuthService facebookAuthService,
            ILogger<FacebookAuthenticationStrategy> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _facebookAuthService = facebookAuthService ?? throw new ArgumentNullException(nameof(facebookAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AuthenticationResult> AuthenticateAsync(string token)
        {
            try
            {
                if (!await ValidateTokenAsync(token))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Invalid Facebook token"
                    };
                }

                var user = await GetUserInfoAsync(token);
                return await CreateAuthenticationResultAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Facebook");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Error authenticating with Facebook"
                };
            }
        }

        public async Task<User> GetUserInfoAsync(string token)
        {
            var facebookUser = await _facebookAuthService.GetUserInfoAsync(token);
            
            return await GetOrCreateUserAsync(
                facebookUser.Email,
                facebookUser.FirstName,
                facebookUser.LastName,
                "Facebook");
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                return await _facebookAuthService.ValidateTokenAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Facebook token");
                return false;
            }
        }

        private async Task<AuthenticationResult> CreateAuthenticationResultAsync(User user)
        {
            try
            {
                var jwtToken = _jwtService.GenerateToken(user);

                return new AuthenticationResult
                {
                    Success = true,
                    User = user,
                    AccessToken = jwtToken,
                    ExpiresAt = DateTime.UtcNow.AddHours(1)
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating authentication result for user {UserId}", user.Id);
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Error creating authentication tokens"
                };
            }
        }

        private async Task<User> GetOrCreateUserAsync(string email, string firstName, string lastName, string provider)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                return user;
            }

            user = User.Create(
                firstName,
                lastName,
                email,
                string.Empty, // No password for social login
                string.Empty, // No phone number required
                provider
            );

            await _userRepository.AddAsync(user);
            return user;
        }
    }
}
