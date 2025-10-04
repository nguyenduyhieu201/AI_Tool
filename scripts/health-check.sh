#!/bin/bash

# Health Check Script for AITool Production
# This script checks if all services are running and healthy

set -e

echo "🔍 Starting health check for AITool services..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to check service health
check_service() {
    local service_name=$1
    local health_url=$2
    local max_attempts=${3:-10}
    local delay=${4:-5}
    
    echo -n "Checking $service_name... "
    
    for i in $(seq 1 $max_attempts); do
        if curl -f -s "$health_url" > /dev/null 2>&1; then
            echo -e "${GREEN}✅ Healthy${NC}"
            return 0
        else
            echo -n "."
            sleep $delay
        fi
    done
    
    echo -e "${RED}❌ Unhealthy${NC}"
    return 1
}

# Function to check Docker service
check_docker_service() {
    local service_name=$1
    local max_attempts=${2:-10}
    local delay=${3:-5}
    
    echo -n "Checking Docker service $service_name... "
    
    for i in $(seq 1 $max_attempts); do
        if docker compose -f docker-compose.prod.yml ps "$service_name" | grep -q "healthy\|Up"; then
            echo -e "${GREEN}✅ Running${NC}"
            return 0
        else
            echo -n "."
            sleep $delay
        fi
    done
    
    echo -e "${RED}❌ Not running${NC}"
    return 1
}

# Check Docker services
echo -e "${YELLOW}📦 Checking Docker services...${NC}"
check_docker_service "sqlserver"
check_docker_service "redis"
check_docker_service "mongo"
check_docker_service "rabbitmq"
check_docker_service "users.api"
check_docker_service "chatgpt-api"
check_docker_service "chatgpttool"
check_docker_service "nginx"

# Check HTTP endpoints
echo -e "${YELLOW}🌐 Checking HTTP endpoints...${NC}"
check_service "Users API" "http://localhost:5011/health"
check_service "ChatGPT API" "http://localhost:5016/health"
check_service "ChatGPT Tool" "http://localhost:5012/"
check_service "Nginx Gateway" "http://localhost/health"

# Check database connections
echo -e "${YELLOW}🗄️ Checking database connections...${NC}"

# Check SQL Server
echo -n "Checking SQL Server connection... "
if docker exec $(docker compose -f docker-compose.prod.yml ps -q sqlserver) /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "${SA_PASSWORD:-SwN12345678}" -Q "SELECT 1" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Connected${NC}"
else
    echo -e "${RED}❌ Connection failed${NC}"
fi

# Check Redis
echo -n "Checking Redis connection... "
if docker exec $(docker compose -f docker-compose.prod.yml ps -q redis) redis-cli ping | grep -q "PONG"; then
    echo -e "${GREEN}✅ Connected${NC}"
else
    echo -e "${RED}❌ Connection failed${NC}"
fi

# Check MongoDB
echo -n "Checking MongoDB connection... "
if docker exec $(docker compose -f docker-compose.prod.yml ps -q mongo) mongosh --eval "db.adminCommand('ping')" > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Connected${NC}"
else
    echo -e "${RED}❌ Connection failed${NC}"
fi

# Check RabbitMQ
echo -n "Checking RabbitMQ connection... "
if docker exec $(docker compose -f docker-compose.prod.yml ps -q rabbitmq) rabbitmq-diagnostics ping > /dev/null 2>&1; then
    echo -e "${GREEN}✅ Connected${NC}"
else
    echo -e "${RED}❌ Connection failed${NC}"
fi

# Summary
echo -e "${YELLOW}📊 Health Check Summary:${NC}"
echo "=================================="

# Count healthy services
healthy_count=$(docker compose -f docker-compose.prod.yml ps --format "table {{.Service}}\t{{.Status}}" | grep -c "healthy\|Up" || true)
total_count=$(docker compose -f docker-compose.prod.yml ps --format "table {{.Service}}" | wc -l)
total_count=$((total_count - 1)) # Subtract header row

echo "Docker Services: $healthy_count/$total_count healthy"

if [ "$healthy_count" -eq "$total_count" ]; then
    echo -e "${GREEN}🎉 All services are healthy!${NC}"
    exit 0
else
    echo -e "${RED}⚠️ Some services are not healthy. Please check the logs.${NC}"
    echo ""
    echo "To check logs:"
    echo "  docker compose -f docker-compose.prod.yml logs [service-name]"
    echo ""
    echo "To restart services:"
    echo "  docker compose -f docker-compose.prod.yml restart [service-name]"
    exit 1
fi
