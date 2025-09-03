using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Users.Application.Contracts.Repositories;
using Users.Application.Contracts.Security;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Security;
using Users.Infrastructure.Security.ExternalAuth.Facebook;
using Users.Infrastructure.Security.ExternalAuth.Google;
using BuildingBlock.Cache;
using StackExchange.Redis;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Users.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Add Redis Connection
            services.AddSingleton<IConnectionMultiplexer>(provider =>
            {
                var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                return ConnectionMultiplexer.Connect(redisConnection);
            });

            // Add Redis Service
            services.AddScoped<IRedisService, RedisService>();

            // Configure GoogleAuth Options
            services.Configure<GoogleAuthOptions>(configuration.GetSection("GoogleAuth"));

            // Configure JWT Options
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));

            // Add Repositories
            services.AddScoped<IUserRepository, UserRepository>();

            // Add Security Services
            services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();
            services.AddScoped<IFacebookAuthService, FacebookAuthService>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();
            services.AddScoped<ITokenGenerator, TokenGenerator>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IRefreshTokenCacheService, RefreshTokenCacheService>();
            services.AddHttpClient<GoogleAuthService>();

            return services;
        }
    }
}
