using System;
using System.Threading.Tasks;
using Users.Domain.Models;

namespace Users.Application.Contracts.Security
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        Task<bool> ValidateTokenAsync(string token);
    }
} 