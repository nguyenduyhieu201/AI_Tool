using System;
using System.Text.Json;
using System.Threading.Tasks;
using BuildingBlock.Cache;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Users.Infrastructure.Security
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _database;
        private readonly IConnectionMultiplexer _connectionMultiplexer;
        private readonly ILogger<RedisService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RedisService(IConnectionMultiplexer connectionMultiplexer, ILogger<RedisService> logger)
        {
            _connectionMultiplexer = connectionMultiplexer ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _database = _connectionMultiplexer.GetDatabase();
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task<bool> SetAsync<T>(string key, T value, TimeSpan? expiry = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.StringSetAsync(key, serializedValue, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting value for key {Key}", key);
                return false;
            }
        }

        public async Task<T?> GetAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var value = await _database.StringGetAsync(key);
                
                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting value for key {Key}", key);
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                return await _database.KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting key {Key}", key);
                return false;
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                return await _database.KeyExistsAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking existence of key {Key}", key);
                return false;
            }
        }

        public async Task<bool> ExpireAsync(string key, TimeSpan expiry)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                return await _database.KeyExpireAsync(key, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting expiry for key {Key}", key);
                return false;
            }
        }

        // Additional utility methods for enhanced functionality
        public async Task<bool> SetStringAsync(string key, string value, TimeSpan? expiry = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                return await _database.StringSetAsync(key, value, expiry);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting string value for key {Key}", key);
                return false;
            }
        }

        public async Task<string?> GetStringAsync(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var value = await _database.StringGetAsync(key);
                return value.HasValue ? value.ToString() : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting string value for key {Key}", key);
                return null;
            }
        }

        public async Task<long> IncrementAsync(string key, long value = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                return await _database.StringIncrementAsync(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error incrementing value for key {Key}", key);
                return 0;
            }
        }

        public async Task<long> DecrementAsync(string key, long value = 1)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                return await _database.StringDecrementAsync(key, value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error decrementing value for key {Key}", key);
                return 0;
            }
        }

        public async Task<bool> SetHashAsync<T>(string key, string field, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));
                if (string.IsNullOrWhiteSpace(field))
                    throw new ArgumentException("Field cannot be null or empty", nameof(field));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.HashSetAsync(key, field, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting hash value for key {Key}, field {Field}", key, field);
                return false;
            }
        }

        public async Task<T?> GetHashAsync<T>(string key, string field)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));
                if (string.IsNullOrWhiteSpace(field))
                    throw new ArgumentException("Field cannot be null or empty", nameof(field));

                var value = await _database.HashGetAsync(key, field);
                
                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting hash value for key {Key}, field {Field}", key, field);
                return default;
            }
        }

        public async Task<bool> DeleteHashAsync(string key, string field)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));
                if (string.IsNullOrWhiteSpace(field))
                    throw new ArgumentException("Field cannot be null or empty", nameof(field));

                return await _database.HashDeleteAsync(key, field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting hash field {Field} for key {Key}", field, key);
                return false;
            }
        }

        public async Task<bool> ListLeftPushAsync<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var result = await _database.ListLeftPushAsync(key, serializedValue);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing value to left of list {Key}", key);
                return false;
            }
        }

        public async Task<bool> ListRightPushAsync<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                var result = await _database.ListRightPushAsync(key, serializedValue);
                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pushing value to right of list {Key}", key);
                return false;
            }
        }

        public async Task<T?> ListLeftPopAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var value = await _database.ListLeftPopAsync(key);
                
                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error popping value from left of list {Key}", key);
                return default;
            }
        }

        public async Task<T?> ListRightPopAsync<T>(string key)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var value = await _database.ListRightPopAsync(key);
                
                if (!value.HasValue)
                    return default;

                return JsonSerializer.Deserialize<T>(value!, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error popping value from right of list {Key}", key);
                return default;
            }
        }

        public async Task<bool> SetAddAsync<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.SetAddAsync(key, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding value to set {Key}", key);
                return false;
            }
        }

        public async Task<bool> SetRemoveAsync<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.SetRemoveAsync(key, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing value from set {Key}", key);
                return false;
            }
        }

        public async Task<bool> SetContainsAsync<T>(string key, T value)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("Key cannot be null or empty", nameof(key));

                var serializedValue = JsonSerializer.Serialize(value, _jsonOptions);
                return await _database.SetContainsAsync(key, serializedValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if set {Key} contains value", key);
                return false;
            }
        }

        public async Task<bool> PingAsync()
        {
            try
            {
                var result = await _database.PingAsync();
                return result.TotalMilliseconds >= 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error pinging Redis server");
                return false;
            }
        }

        public async Task FlushDatabaseAsync()
        {
            try
            {
                var server = _connectionMultiplexer.GetServer(_connectionMultiplexer.GetEndPoints().First());
                await server.FlushDatabaseAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing Redis database");
                throw;
            }
        }

        public void Dispose()
        {
            _connectionMultiplexer?.Dispose();
        }
    }
}





