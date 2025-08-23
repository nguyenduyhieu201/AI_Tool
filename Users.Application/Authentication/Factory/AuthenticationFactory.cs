using System;
using System.Collections.Generic;
using Users.Application.Contracts.Authentication.Strategies;

namespace Users.Application.Authentication.Factory
{
    /// <summary>
    /// Factory class để tạo các authentication strategies
    /// </summary>
    public class AuthenticationFactory : IAuthenticationFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, Type> _strategyTypes;

        public AuthenticationFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _strategyTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
            {
                { "Google", typeof(GoogleAuthenticationStrategy) },
                { "Facebook", typeof(FacebookAuthenticationStrategy) },
                //{ "Apple", typeof(AppleAuthenticationStrategy) }
            };
        }

        /// <summary>
        /// Tạo authentication strategy dựa trên provider
        /// </summary>
        /// <param name="provider">Tên provider (Google, Facebook, Apple)</param>
        /// <returns>IAuthenticationStrategy instance</returns>
        /// <exception cref="ArgumentException">Khi provider không được hỗ trợ</exception>
        public IAuthenticationStrategy CreateStrategy(string provider)
        {
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ArgumentException("Provider cannot be null or empty", nameof(provider));
            }

            if (!_strategyTypes.TryGetValue(provider, out var strategyType))
            {
                throw new ArgumentException($"Authentication provider '{provider}' is not supported", nameof(provider));
            }

            var strategy = _serviceProvider.GetService(strategyType) as IAuthenticationStrategy;
            if (strategy == null)
            {
                throw new InvalidOperationException($"Failed to create authentication strategy for provider '{provider}'");
            }

            return strategy;
        }

        /// <summary>
        /// Kiểm tra xem provider có được hỗ trợ không
        /// </summary>
        public bool IsProviderSupported(string provider)
        {
            return !string.IsNullOrWhiteSpace(provider) && _strategyTypes.ContainsKey(provider);
        }

        /// <summary>
        /// Lấy danh sách các providers được hỗ trợ
        /// </summary>
        public IEnumerable<string> GetSupportedProviders()
        {
            return _strategyTypes.Keys;
        }
    }
} 