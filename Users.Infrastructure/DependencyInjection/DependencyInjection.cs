using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Contracts.Security;
using Users.Infrastructure.Security;

namespace Users.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();

            return services;
        }
    }
}
