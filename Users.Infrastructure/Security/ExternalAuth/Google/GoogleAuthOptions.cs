using System;

namespace Users.Infrastructure.Security.ExternalAuth.Google
{
    /// <summary>
    /// Cấu hình cho Google Authentication
    /// </summary>
    public class GoogleAuthOptions
    {
        /// <summary>
        /// Google Client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Google Client Secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Redirect URI sau khi xác thực
        /// </summary>
        public string RedirectUri { get; set; }

        /// <summary>
        /// Các scope cần thiết cho Google API
        /// </summary>
        public string[] Scopes { get; set; } = new[]
        {
            "https://www.googleapis.com/auth/userinfo.email",
            "https://www.googleapis.com/auth/userinfo.profile"
        };

        /// <summary>
        /// Thời gian timeout cho các request đến Google API (mặc định: 30 giây)
        /// </summary>
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Số lần retry khi gọi Google API thất bại (mặc định: 3 lần)
        /// </summary>
        public int RetryCount { get; set; } = 3;

        /// <summary>
        /// Thời gian delay giữa các lần retry (mặc định: 1 giây)
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    }
} 