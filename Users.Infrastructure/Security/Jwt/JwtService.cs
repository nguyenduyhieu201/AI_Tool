using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Users.Core.Entities;
using Users.Core.Interfaces;
using Users.Domain.Models;

namespace Users.Infrastructure.Security.Jwt
{
    public class JwtService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtOptions _options;

        public JwtService(IUserRepository userRepository, JwtOptions options)
        {
            _userRepository = userRepository;
            _options = options;
        }

        /// <summary>
        /// Tạo JWT token cho user
        /// </summary>
        public async Task<string> GenerateTokenAsync(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Tạo refresh token cho user
        /// </summary>
        public async Task<string> GenerateRefreshTokenAsync(User user)
        {
            // Tạo refresh token ngẫu nhiên
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var refreshToken = Convert.ToBase64String(randomNumber);

            // Cập nhật refresh token cho user
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays);
            await _userRepository.UpdateAsync(user);

            return refreshToken;
        }
    }
} 