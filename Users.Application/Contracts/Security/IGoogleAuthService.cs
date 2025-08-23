using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Models;

namespace Users.Application.Contracts.Security
{
    public interface IGoogleAuthService
    {
        public Task<GoogleUserInfo> GetUserInfoAsync(string token);
        public Task<bool> ValidateTokenAsync(string token);


    }
}
