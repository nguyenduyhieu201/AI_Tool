# Test Redis Connection Script
Write-Host "Testing Redis Connection..." -ForegroundColor Green

# Check if Redis container is running
Write-Host "1. Checking Redis container status..." -ForegroundColor Yellow
$redisContainer = docker ps --filter "name=redis" --format "table {{.Names}}\t{{.Status}}"
if ($redisContainer -match "redis") {
    Write-Host "✅ Redis container is running" -ForegroundColor Green
    Write-Host $redisContainer
} else {
    Write-Host "❌ Redis container is not running" -ForegroundColor Red
    Write-Host "Starting Redis container..." -ForegroundColor Yellow
    docker-compose up redis -d
}

# Test Redis connection
Write-Host "`n2. Testing Redis connection..." -ForegroundColor Yellow
try {
    $pingResult = docker exec redis redis-cli ping
    if ($pingResult -eq "PONG") {
        Write-Host "✅ Redis connection successful: $pingResult" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis connection failed: $pingResult" -ForegroundColor Red
    }
} catch {
    Write-Host "❌ Failed to connect to Redis: $($_.Exception.Message)" -ForegroundColor Red
}

# Test Redis set/get
Write-Host "`n3. Testing Redis set/get operations..." -ForegroundColor Yellow
try {
    docker exec redis redis-cli set test_key "Hello Redis"
    $getResult = docker exec redis redis-cli get test_key
    if ($getResult -eq "Hello Redis") {
        Write-Host "✅ Redis set/get operations successful" -ForegroundColor Green
    } else {
        Write-Host "❌ Redis set/get operations failed" -ForegroundColor Red
    }
    # Clean up
    docker exec redis redis-cli del test_key
} catch {
    Write-Host "❌ Redis operations failed: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`nRedis test completed!" -ForegroundColor Green

