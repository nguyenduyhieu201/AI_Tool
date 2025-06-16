using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Contracts.Authentication.Strategies;

namespace Users.Application.Authentication.Factory
{
    public interface IAuthenticationFactory
    {
        public IAuthenticationStrategy CreateStrategy(string provider);
    }
}
