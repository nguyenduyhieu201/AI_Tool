# Redis Connection Fix Guide

## 🚨 Problem
```
StackExchange.Redis.RedisConnectionException: It was not possible to connect to the redis server(s). Error connecting right now. To allow this multiplexer to continue retrying until it's able to connect, use abortConnect=false in your connection string or AbortOnConnectFail=false; in your code.
```

## ✅ Solutions Applied

### 1. Updated Redis Connection Configuration
**File**: `Users.Infrastructure/DependencyInjection/DependencyInjection.cs`

```csharp
// Add Redis Connection with retry configuration
services.AddSingleton<IConnectionMultiplexer>(provider =>
{
    var redisConnection = configuration.GetConnectionString("Redis") ?? "localhost:6379";
    var configurationOptions = ConfigurationOptions.Parse(redisConnection);
    configurationOptions.AbortOnConnectFail = false;  // Don't abort on connect fail
    configurationOptions.ConnectRetry = 3;            // Retry 3 times
    configurationOptions.ConnectTimeout = 5000;       // 5 second timeout
    configurationOptions.SyncTimeout = 5000;          // 5 second sync timeout
    configurationOptions.AsyncTimeout = 5000;         // 5 second async timeout
    return ConnectionMultiplexer.Connect(configurationOptions);
});
```

### 2. Updated Connection Strings
**File**: `Users.API/appsettings.json`

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379,abortConnect=false,connectRetry=3,connectTimeout=5000"
  }
}
```

### 3. Updated Docker Compose
**File**: `docker-compose.yml`

```yaml
users.api:
  environment:
    - ConnectionStrings__Redis=redis:6379,abortConnect=false,connectRetry=3,connectTimeout=5000
```

### 4. Added Error Handling
**File**: `Users.Infrastructure/Security/RefreshTokenCacheService.cs`

- Added try-catch blocks around all Redis operations
- Cache failures are logged but don't crash the application
- Application can run without Redis (fallback to database only)

## 🔧 How to Test

### Option 1: Use PowerShell Script
```powershell
.\test-redis.ps1
```

### Option 2: Manual Testing
```bash
# 1. Start Redis container
docker-compose up redis -d

# 2. Check Redis status
docker ps | grep redis

# 3. Test Redis connection
docker exec redis redis-cli ping

# 4. Test Redis operations
docker exec redis redis-cli set test "hello"
docker exec redis redis-cli get test
```

### Option 3: Run Application
```bash
# Start all services
docker-compose up

# Or start just the API
cd Users.API
dotnet run
```

## 🎯 What's Fixed

1. **Connection Retry**: Redis will retry connection 3 times
2. **No Abort on Fail**: Application won't crash if Redis is down
3. **Timeout Configuration**: 5-second timeouts for all operations
4. **Graceful Degradation**: App works with or without Redis
5. **Better Logging**: All Redis errors are logged but not fatal

## 🚀 Expected Behavior

- **With Redis**: Full caching functionality
- **Without Redis**: Application still works, just no caching
- **Redis Down**: Application continues running, logs warnings

## 📋 Troubleshooting

### If Redis still doesn't connect:

1. **Check Redis container**:
   ```bash
   docker ps | grep redis
   ```

2. **Check Redis logs**:
   ```bash
   docker logs redis
   ```

3. **Restart Redis**:
   ```bash
   docker-compose restart redis
   ```

4. **Check port conflicts**:
   ```bash
   netstat -an | grep 6379
   ```

### If application still crashes:

1. **Disable Redis temporarily**:
   - Comment out Redis services in `DependencyInjection.cs`
   - Application will run without caching

2. **Check connection string**:
   - Verify `ConnectionStrings__Redis` in environment variables
   - Check `appsettings.json` format

## ✅ Success Indicators

- Application starts without Redis connection errors
- Redis operations work when Redis is available
- Application continues working when Redis is down
- Logs show Redis connection status

