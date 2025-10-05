#!/bin/bash

# Server Setup Script for AITool Production
# This script sets up the production server for AITool deployment

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Logging function
log() {
    echo -e "$1"
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

# Check if running as root
check_root() {
    if [ "$EUID" -eq 0 ]; then
        warning "This script should not be run as root for security reasons"
        read -p "Do you want to continue? (y/N): " -n 1 -r
        echo
        if [[ ! $REPLY =~ ^[Yy]$ ]]; then
            exit 1
        fi
    fi
}

# Update system packages
update_system() {
    info "Updating system packages..."
    
    if command -v apt-get &> /dev/null; then
        sudo apt-get update
        sudo apt-get upgrade -y
        success "System packages updated"
    elif command -v yum &> /dev/null; then
        sudo yum update -y
        success "System packages updated"
    else
        warning "Package manager not recognized. Please update manually."
    fi
}

# Install Docker
install_docker() {
    info "Installing Docker..."
    
    if command -v docker &> /dev/null; then
        success "Docker is already installed"
        return
    fi
    
    # Install Docker using official script
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    rm get-docker.sh
    
    # Add current user to docker group
    sudo usermod -aG docker $USER
    
    success "Docker installed successfully"
    warning "Please log out and log back in for group changes to take effect"
}

# Install Docker Compose
install_docker_compose() {
    info "Installing Docker Compose..."
    
    if command -v docker-compose &> /dev/null; then
        success "Docker Compose is already installed"
        return
    fi
    
    # Get latest version
    local latest_version=$(curl -s https://api.github.com/repos/docker/compose/releases/latest | grep -Po '"tag_name": "\K.*?(?=")')
    
    # Download and install
    sudo curl -L "https://github.com/docker/compose/releases/download/${latest_version}/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
    
    # Create symlink for docker compose (newer syntax)
    sudo ln -sf /usr/local/bin/docker-compose /usr/local/bin/docker-compose
    
    success "Docker Compose installed successfully"
}

# Install additional tools
install_tools() {
    info "Installing additional tools..."
    
    local tools=("curl" "wget" "git" "htop" "vim" "jq")
    
    for tool in "${tools[@]}"; do
        if ! command -v "$tool" &> /dev/null; then
            if command -v apt-get &> /dev/null; then
                sudo apt-get install -y "$tool"
            elif command -v yum &> /dev/null; then
                sudo yum install -y "$tool"
            fi
        fi
    done
    
    success "Additional tools installed"
}

# Create project directory
create_project_dir() {
    info "Creating project directory..."
    
    local project_dir="/opt/aitool"
    
    if [ ! -d "$project_dir" ]; then
        sudo mkdir -p "$project_dir"
        sudo chown $USER:$USER "$project_dir"
        success "Project directory created: $project_dir"
    else
        success "Project directory already exists: $project_dir"
    fi
}

# Setup firewall
setup_firewall() {
    info "Setting up firewall..."
    
    if command -v ufw &> /dev/null; then
        # Allow SSH
        sudo ufw allow ssh
        
        # Allow HTTP and HTTPS
        sudo ufw allow 80/tcp
        sudo ufw allow 443/tcp
        
        # Allow application ports
        sudo ufw allow 5011/tcp  # Users API
        sudo ufw allow 5012/tcp  # ChatGPT Tool
        sudo ufw allow 5014/tcp  # ChatGPT API
        
        # Allow database ports (optional, for external access)
        # sudo ufw allow 1433/tcp  # SQL Server
        # sudo ufw allow 27017/tcp # MongoDB
        # sudo ufw allow 6379/tcp  # Redis
        # sudo ufw allow 5672/tcp  # RabbitMQ
        
        # Enable firewall
        sudo ufw --force enable
        
        success "Firewall configured"
    else
        warning "UFW not available. Please configure firewall manually."
    fi
}

# Setup log rotation
setup_log_rotation() {
    info "Setting up log rotation..."
    
    sudo tee /etc/logrotate.d/aitool > /dev/null <<EOF
/opt/aitool/logs/*.log {
    daily
    missingok
    rotate 30
    compress
    delaycompress
    notifempty
    create 644 $USER $USER
}
EOF
    
    success "Log rotation configured"
}

# Create systemd service for auto-start
create_systemd_service() {
    info "Creating systemd service..."
    
    sudo tee /etc/systemd/system/aitool.service > /dev/null <<EOF
[Unit]
Description=AITool Application
Requires=docker.service
After=docker.service

[Service]
Type=oneshot
RemainAfterExit=yes
WorkingDirectory=/opt/aitool
ExecStart=/usr/local/bin/docker-compose -f docker-compose.prod.yml up -d
ExecStop=/usr/local/bin/docker-compose -f docker-compose.prod.yml down
TimeoutStartSec=0

[Install]
WantedBy=multi-user.target
EOF
    
    sudo systemctl daemon-reload
    sudo systemctl enable aitool.service
    
    success "Systemd service created and enabled"
}

# Setup monitoring
setup_monitoring() {
    info "Setting up basic monitoring..."
    
    # Create monitoring script
    cat > /opt/aitool/scripts/monitor.sh << 'EOF'
#!/bin/bash

# Simple monitoring script for AITool
LOG_FILE="/opt/aitool/logs/monitor.log"
DATE=$(date '+%Y-%m-%d %H:%M:%S')

echo "[$DATE] Checking AITool services..." >> "$LOG_FILE"

# Check if services are running
if docker compose -f /opt/aitool/docker-compose.prod.yml ps | grep -q "Up"; then
    echo "[$DATE] ✅ Services are running" >> "$LOG_FILE"
else
    echo "[$DATE] ❌ Some services are down" >> "$LOG_FILE"
    # Send alert (customize as needed)
    # curl -X POST "https://hooks.slack.com/your-webhook-url" -d '{"text":"AITool services are down!"}'
fi
EOF
    
    chmod +x /opt/aitool/scripts/monitor.sh
    
    # Add to crontab
    (crontab -l 2>/dev/null; echo "*/5 * * * * /opt/aitool/scripts/monitor.sh") | crontab -
    
    success "Monitoring setup completed"
}

# Main setup function
main() {
    log "${BLUE}🚀 AITool Production Server Setup${NC}"
    log "=================================="
    
    # Check if running as root
    check_root
    
    # Update system
    update_system
    
    # Install Docker
    install_docker
    
    # Install Docker Compose
    install_docker_compose
    
    # Install tools
    install_tools
    
    # Create project directory
    create_project_dir
    
    # Setup firewall
    setup_firewall
    
    # Setup log rotation
    setup_log_rotation
    
    # Create systemd service
    create_systemd_service
    
    # Setup monitoring
    setup_monitoring
    
    # Final instructions
    log ""
    log "${GREEN}🎉 Server setup completed successfully!${NC}"
    log ""
    log "Next steps:"
    log "1. Log out and log back in to apply group changes"
    log "2. Clone your repository to /opt/aitool"
    log "3. Copy env.prod.example to .env.prod and configure"
    log "4. Run: make setup-prod"
    log "5. Run: make prod-deploy"
    log ""
    log "Useful commands:"
    log "  - Check status: make prod-status"
    log "  - View logs: make prod-logs"
    log "  - Health check: make prod-health"
    log "  - Deploy: make prod-deploy"
    log "  - Rollback: make prod-rollback"
}

# Run main function
main "$@"
