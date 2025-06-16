using System;
using System.Threading.Tasks;
using Users.Domain.Models;

namespace Users.Application.Contracts.Authentication.Strategies
{
    /// <summary>
    /// Interface định nghĩa các phương thức cần thiết cho việc xác thực với các provider bên thứ ba
    /// </summary>
    public interface IAuthenticationStrategy
    {
        /// <summary>
        /// Xác thực token từ provider và trả về kết quả xác thực
        /// </summary>
        /// <param name="token">Token từ provider (Facebook/Google)</param>
        /// <returns>Kết quả xác thực bao gồm thông tin user và token</returns>
        Task<AuthenticationResult> AuthenticateAsync(string token);

        /// <summary>
        /// Lấy thông tin user từ provider
        /// </summary>
        /// <param name="token">Token từ provider</param>
        /// <returns>Thông tin user</returns>
        Task<User> GetUserInfoAsync(string token);

        /// <summary>
        /// Kiểm tra tính hợp lệ của token
        /// </summary>
        /// <param name="token">Token cần kiểm tra</param>
        /// <returns>True nếu token hợp lệ, ngược lại là False</returns>
        Task<bool> ValidateTokenAsync(string token);
    }

    /// <summary>
    /// Kết quả trả về sau khi xác thực
    /// </summary>
    public class AuthenticationResult
    {
        /// <summary>
        /// Trạng thái xác thực
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Thông báo lỗi nếu có
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Thông tin user
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// JWT token
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token
        /// </summary>
        public string RefreshToken { get; set; } = string.Empty;

        /// <summary>
        /// Thời gian hết hạn của token
        /// </summary>
        public DateTime ExpiresAt { get; set; }
    }
} 