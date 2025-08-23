using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Models;

namespace Users.Application.Contracts.Security
{
    /// <summary>
    /// Interface định nghĩa các phương thức tương tác với Facebook Authentication API
    /// </summary>
    public interface IFacebookAuthService
    {
        /// <summary>
        /// Lấy thông tin user từ Facebook API
        /// </summary>
        /// <param name="token">Access token từ Facebook</param>
        /// <returns>Thông tin user từ Facebook</returns>
        Task<FacebookUserInfo> GetUserInfoAsync(string token);

        /// <summary>
        /// Kiểm tra tính hợp lệ của Facebook token
        /// </summary>
        /// <param name="token">Token cần kiểm tra</param>
        /// <returns>True nếu token hợp lệ, ngược lại là False</returns>
        Task<bool> ValidateTokenAsync(string token);
    }
}
