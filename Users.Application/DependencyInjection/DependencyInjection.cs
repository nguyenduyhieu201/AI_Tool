using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Authentication.Factory;
using Users.Application.Contracts.Authentication.Strategies;
using Users.Application.Contracts.Security;

namespace Users.Application.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<GoogleAuthenticationStrategy>();
            services.AddScoped<FacebookAuthenticationStrategy>();
            services.AddScoped<IAuthenticationFactory, AuthenticationFactory>();
            return services;
        }
    }
}
