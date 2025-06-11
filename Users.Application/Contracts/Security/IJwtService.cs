using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Domain.Models;

namespace Users.Application.Contracts.Security
{
    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
