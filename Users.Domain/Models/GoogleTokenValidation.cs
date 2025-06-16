using System;

namespace Users.Domain.Models
{
    /// <summary>
    /// Thông tin validation token từ Google
    /// </summary>
    public class GoogleTokenValidation
    {
        /// <summary>
        /// ID của ứng dụng được cấp phép sử dụng token
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// Thời gian hết hạn của token (tính bằng giây)
        /// </summary>
        public long ExpiresIn { get; set; }

        /// <summary>
        /// Các quyền được cấp cho token
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Thời gian token được tạo
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}