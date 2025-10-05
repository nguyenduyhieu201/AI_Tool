#!/bin/bash

# Deploy Script for AITool Production
# This script handles the deployment process

set -e

# Configuration
COMPOSE_FILE="docker-compose.prod.yml"
BACKUP_DIR="/opt/aitool/backups"
LOG_FILE="/opt/aitool/logs/deploy.log"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "$1" | tee -a "$LOG_FILE"
}

# Error handling
error_exit() {
    log "${RED}❌ Error: $1${NC}"
    exit 1
}

# Success message
success() {
    log "${GREEN}✅ $1${NC}"
}

# Info message
info() {
    log "${BLUE}ℹ️ $1${NC}"
}

# Warning message
warning() {
    log "${YELLOW}⚠️ $1${NC}"
}

# Create necessary directories
create_directories() {
    info "Creating necessary directories..."
    mkdir -p "$BACKUP_DIR"
    mkdir -p "$(dirname "$LOG_FILE")"
    mkdir -p /opt/aitool/deployment
}

# Backup current deployment
backup_current() {
    info "Creating backup of current deployment..."
    
    local backup_name="backup-$(date +%Y%m%d-%H%M%S)"
    local backup_path="$BACKUP_DIR/$backup_name"
    
    mkdir -p "$backup_path"
    
    # Backup docker-compose file
    if [ -f "$COMPOSE_FILE" ]; then
        cp "$COMPOSE_FILE" "$backup_path/"
        success "Backed up docker-compose.prod.yml"
    fi
    
    # Backup environment variables
    if [ -f ".env" ]; then
        cp ".env" "$backup_path/"
        success "Backed up .env file"
    fi
    
    # Export current images
    info "Exporting current images..."
    docker images --format "table {{.Repository}}:{{.Tag}}" | grep "ghcr.io" > "$backup_path/images.txt" || true
    
    success "Backup created: $backup_name"
}

# Pull new images
pull_images() {
    info "Pulling new images..."
    
    # Login to registry
    info "Logging in to GitHub Container Registry..."
    echo "$GITHUB_TOKEN" | docker login ghcr.io -u "$GITHUB_ACTOR" --password-stdin || error_exit "Failed to login to registry"
    
    # Pull images
    local images=(
        "$USERS_API_IMAGE"
        "$CHATGPT_API_IMAGE"
        "$CHATGPTTOOL_IMAGE"
    )
    
    for image in "${images[@]}"; do
        if [ -n "$image" ]; then
            info "Pulling $image..."
            docker pull "$image" || error_exit "Failed to pull $image"
            success "Pulled $image"
        fi
    done
}

# Update docker-compose file
update_compose_file() {
    info "Updating docker-compose.prod.yml with new images..."
    
    # Create backup
    cp "$COMPOSE_FILE" "$COMPOSE_FILE.backup.$(date +%Y%m%d-%H%M%S)"
    
    # Update images
    if [ -n "$USERS_API_IMAGE" ]; then
        sed -i "s|image: .*/users-api:.*|image: $USERS_API_IMAGE|g" "$COMPOSE_FILE"
        success "Updated users-api image"
    fi
    
    if [ -n "$CHATGPT_API_IMAGE" ]; then
        sed -i "s|image: .*/chatgpt-api:.*|image: $CHATGPT_API_IMAGE|g" "$COMPOSE_FILE"
        success "Updated chatgpt-api image"
    fi
    
    if [ -n "$CHATGPTTOOL_IMAGE" ]; then
        sed -i "s|image: .*/chatgpttool:.*|image: $CHATGPTTOOL_IMAGE|g" "$COMPOSE_FILE"
        success "Updated chatgpttool image"
    fi
}

# Deploy services
deploy_services() {
    info "Deploying services..."
    
    # Stop current services
    info "Stopping current services..."
    docker compose -f "$COMPOSE_FILE" down || warning "Some services were not running"
    
    # Start new services
    info "Starting new services..."
    docker compose -f "$COMPOSE_FILE" up -d || error_exit "Failed to start services"
    
    success "Services started successfully"
}

# Health check
health_check() {
    info "Running health check..."
    
    # Wait for services to start
    info "Waiting for services to start (60 seconds)..."
    sleep 60
    
    # Run health check script
    if [ -f "./scripts/health-check.sh" ]; then
        chmod +x ./scripts/health-check.sh
        ./scripts/health-check.sh || error_exit "Health check failed"
    else
        warning "Health check script not found, skipping..."
    fi
}

# Cleanup old images
cleanup() {
    info "Cleaning up old images..."
    
    # Remove unused images
    docker image prune -f || warning "Failed to prune images"
    
    # Remove old backups (keep last 5)
    info "Cleaning up old backups..."
    ls -t "$BACKUP_DIR" | tail -n +6 | xargs -I {} rm -rf "$BACKUP_DIR/{}" || true
    
    success "Cleanup completed"
}

# Rollback function
rollback() {
    warning "Rolling back to previous version..."
    
    # Find latest backup
    local latest_backup=$(ls -t "$BACKUP_DIR" | head -n 1)
    
    if [ -n "$latest_backup" ]; then
        info "Rolling back to: $latest_backup"
        
        # Restore docker-compose file
        if [ -f "$BACKUP_DIR/$latest_backup/docker-compose.prod.yml" ]; then
            cp "$BACKUP_DIR/$latest_backup/docker-compose.prod.yml" "$COMPOSE_FILE"
            success "Restored docker-compose.prod.yml"
        fi
        
        # Restart services
        docker compose -f "$COMPOSE_FILE" down
        docker compose -f "$COMPOSE_FILE" up -d
        
        success "Rollback completed"
    else
        error_exit "No backup found for rollback"
    fi
}

# Main deployment function
main() {
    info "Starting deployment process..."
    info "Deployment started at: $(date)"
    
    # Check if we're in the right directory
    if [ ! -f "$COMPOSE_FILE" ]; then
        error_exit "docker-compose.prod.yml not found. Please run this script from the project root."
    fi
    
    # Create directories
    create_directories
    
    # Backup current deployment
    backup_current
    
    # Pull new images
    pull_images
    
    # Update compose file
    update_compose_file
    
    # Deploy services
    deploy_services
    
    # Health check
    if health_check; then
        success "Deployment completed successfully!"
        
        # Cleanup
        cleanup
        
        info "Deployment finished at: $(date)"
        exit 0
    else
        error_exit "Health check failed. Deployment may have issues."
    fi
}

# Handle script arguments
case "${1:-}" in
    "rollback")
        rollback
        ;;
    "health-check")
        health_check
        ;;
    "cleanup")
        cleanup
        ;;
    *)
        main
        ;;
esac
