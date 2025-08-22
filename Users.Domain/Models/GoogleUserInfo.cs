using System;
using System.Text.Json.Serialization;

namespace Users.Domain.Models
{
    /// <summary>
    /// Thông tin user từ Google
    /// </summary>
    public class GoogleUserInfo
    {
        /// <summary>
        /// ID của user trên Google
        /// </summary>
        public string Id { get; set; }


        /// <summary>
        /// Email của user
        /// </summary>
        [JsonPropertyName("email")]

        public string Email { get; set; }

        /// <summary>
        /// Tên của user
        /// </summary>
        public string GivenName { get; set; }

        /// <summary>
        /// Họ của user
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// URL ảnh đại diện
        /// </summary>
        public string Picture { get; set; }

        /// <summary>
        /// Trạng thái xác thực email
        /// </summary>
        public bool VerifiedEmail { get; set; }

        /// <summary>
        /// Thời gian tạo thông tin
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
} 