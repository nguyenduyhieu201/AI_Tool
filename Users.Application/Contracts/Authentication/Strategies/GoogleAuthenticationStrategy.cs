using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Domain.Models;

namespace Users.Application.Contracts.Authentication.Strategies
{
    /// <summary>
    /// Strategy xử lý đăng nhập bằng Google
    /// </summary>
    public class GoogleAuthenticationStrategy : IAuthenticationStrategy
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly ILogger<GoogleAuthenticationStrategy> _logger;

        public GoogleAuthenticationStrategy(
            IUserRepository userRepository,
            IJwtService jwtService,
            IGoogleAuthService googleAuthService,
            ILogger<GoogleAuthenticationStrategy> logger)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
            _googleAuthService = googleAuthService ?? throw new ArgumentNullException(nameof(googleAuthService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Xác thực token từ Google và trả về kết quả
        /// </summary>
        public async Task<AuthenticationResult> AuthenticateAsync(string token)
        {
            try
            {
                if (!await ValidateTokenAsync(token))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Invalid Google token"
                    };
                }

                var user = await GetUserInfoAsync(token);
                return await CreateAuthenticationResultAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error authenticating with Google");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Error authenticating with Google"
                };
            }
        }

        /// <summary>
        /// Lấy thông tin user từ Google
        /// </summary>
        public async Task<User> GetUserInfoAsync(string token)
        {
            var googleUser = await _googleAuthService.GetUserInfoAsync(token);
            
            return await GetOrCreateUserAsync(
                googleUser.Email,
                googleUser.GivenName,
                googleUser.FamilyName,
                "Google");
        }

        /// <summary>
        /// Kiểm tra tính hợp lệ của token
        /// </summary>
        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                return await _googleAuthService.ValidateTokenAsync(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating Google token");
                return false;
            }
        }

        /// <summary>
        /// Tạo kết quả authentication
        /// </summary>
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

        /// <summary>
        /// Lấy hoặc tạo user mới
        /// </summary>
        private async Task<User> GetOrCreateUserAsync(string email, string firstName, string lastName, string provider)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                return user;
            }

            //user = User.Create()
            //{
            //    Email = email,
            //    FirstName = firstName,
            //    LastName = lastName,
            //    IsActive = true,
            //    CreatedAt = DateTime.UtcNow,
            //    ExternalProvider = provider
            //};

            await _userRepository.AddAsync(user);
            return user;
        }
    }
} 