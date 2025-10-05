# AITool Makefile for CI/CD Management

.PHONY: help build test deploy health-check rollback cleanup logs status

# Default target
help:
	@echo "AITool CI/CD Management Commands:"
	@echo ""
	@echo "Development:"
	@echo "  dev-up          Start development environment"
	@echo "  dev-down        Stop development environment"
	@echo "  dev-logs        View development logs"
	@echo ""
	@echo "Production:"
	@echo "  prod-deploy     Deploy to production"
	@echo "  prod-health     Check production health"
	@echo "  prod-rollback   Rollback production deployment"
	@echo "  prod-logs       View production logs"
	@echo "  prod-status     Check production status"
	@echo ""
	@echo "Docker:"
	@echo "  build           Build all Docker images"
	@echo "  build-users     Build Users.API image"
	@echo "  build-chatgpt   Build ChatGPT.API image"
	@echo "  build-tool      Build ChatGPTTool image"
	@echo ""
	@echo "Utilities:"
	@echo "  clean           Clean up Docker resources"
	@echo "  backup          Create backup of current deployment"
	@echo "  restore         Restore from backup"

# Development commands
dev-up:
	@echo "🚀 Starting development environment..."
	docker compose up -d
	@echo "✅ Development environment started!"
	@echo "📊 Services available at:"
	@echo "  - Users API: http://localhost:5011"
	@echo "  - ChatGPT API: http://localhost:5014"
	@echo "  - ChatGPT Tool: http://localhost:5012"
	@echo "  - Nginx Gateway: http://localhost:5010"

dev-down:
	@echo "🛑 Stopping development environment..."
	docker compose down
	@echo "✅ Development environment stopped!"

dev-logs:
	@echo "📋 Viewing development logs..."
	docker compose logs -f

# Production commands
prod-deploy:
	@echo "🚀 Deploying to production..."
	@if [ ! -f "scripts/deploy.sh" ]; then \
		echo "❌ Deploy script not found!"; \
		exit 1; \
	fi
	chmod +x scripts/deploy.sh
	./scripts/deploy.sh
	@echo "✅ Production deployment completed!"

prod-health:
	@echo "🔍 Checking production health..."
	@if [ ! -f "scripts/health-check.sh" ]; then \
		echo "❌ Health check script not found!"; \
		exit 1; \
	fi
	chmod +x scripts/health-check.sh
	./scripts/health-check.sh

prod-rollback:
	@echo "⏪ Rolling back production deployment..."
	@if [ ! -f "scripts/deploy.sh" ]; then \
		echo "❌ Deploy script not found!"; \
		exit 1; \
	fi
	chmod +x scripts/deploy.sh
	./scripts/deploy.sh rollback
	@echo "✅ Production rollback completed!"

prod-logs:
	@echo "📋 Viewing production logs..."
	docker compose -f docker-compose.prod.yml logs -f

prod-status:
	@echo "📊 Production status:"
	docker compose -f docker-compose.prod.yml ps

# Docker build commands
build: build-users build-chatgpt build-tool
	@echo "✅ All images built successfully!"

build-users:
	@echo "🔨 Building Users.API image..."
	docker build -t aitool/users-api:latest -f Users.API/Dockerfile .
	@echo "✅ Users.API image built!"

build-chatgpt:
	@echo "🔨 Building ChatGPT.API image..."
	docker build -t aitool/chatgpt-api:latest -f ChatGPT.API/Dockerfile ./ChatGPT.API
	@echo "✅ ChatGPT.API image built!"

build-tool:
	@echo "🔨 Building ChatGPTTool image..."
	docker build -t aitool/chatgpttool:latest -f ChatGPTTool/Dockerfile .
	@echo "✅ ChatGPTTool image built!"

# Utility commands
clean:
	@echo "🧹 Cleaning up Docker resources..."
	docker system prune -f
	docker volume prune -f
	@echo "✅ Cleanup completed!"

backup:
	@echo "💾 Creating backup..."
	@mkdir -p backups
	@timestamp=$$(date +%Y%m%d-%H%M%S); \
	backup_dir="backups/backup-$$timestamp"; \
	mkdir -p "$$backup_dir"; \
	cp docker-compose.prod.yml "$$backup_dir/" 2>/dev/null || true; \
	cp .env.prod "$$backup_dir/" 2>/dev/null || true; \
	docker images --format "table {{.Repository}}:{{.Tag}}" | grep "aitool\|ghcr.io" > "$$backup_dir/images.txt" 2>/dev/null || true; \
	echo "✅ Backup created: $$backup_dir"

restore:
	@echo "⏪ Available backups:"
	@ls -la backups/ 2>/dev/null || echo "No backups found"
	@echo ""
	@echo "To restore a backup, run:"
	@echo "  cp backups/backup-YYYYMMDD-HHMMSS/docker-compose.prod.yml ."
	@echo "  cp backups/backup-YYYYMMDD-HHMMSS/.env.prod ."

# Test commands
test:
	@echo "🧪 Running tests..."
	@echo "⚠️ Add your test commands here"
	@echo "✅ Tests completed!"

# Setup commands
setup-dev:
	@echo "⚙️ Setting up development environment..."
	@if [ ! -f ".env" ]; then \
		cp env.prod.example .env; \
		echo "📝 Created .env file from template"; \
		echo "⚠️ Please update .env with your development values"; \
	fi
	@echo "✅ Development setup completed!"

setup-prod:
	@echo "⚙️ Setting up production environment..."
	@if [ ! -f ".env.prod" ]; then \
		cp env.prod.example .env.prod; \
		echo "📝 Created .env.prod file from template"; \
		echo "⚠️ Please update .env.prod with your production values"; \
	fi
	@chmod +x scripts/*.sh 2>/dev/null || true
	@echo "✅ Production setup completed!"

# Quick commands
up: dev-up
down: dev-down
logs: dev-logs
deploy: prod-deploy
health: prod-health
rollback: prod-rollback
status: prod-status
