# CI/CD Pipeline Documentation

## 🚀 Overview

This project uses GitHub Actions for CI/CD with a 2-job pipeline:
1. **Build Job**: Builds Docker images and pushes to GitHub Container Registry
2. **Deploy Job**: Deploys the application to production server

## 📁 Pipeline Structure

```
.github/
├── workflows/
│   └── ci-cd.yml          # Main CI/CD pipeline
scripts/
├── deploy.sh              # Deployment script
└── health-check.sh        # Health check script
docker-compose.prod.yml    # Production Docker Compose
env.prod.example          # Environment variables template
```

## 🔧 Setup Instructions

### 1. GitHub Secrets Configuration

Add the following secrets to your GitHub repository:

#### Required Secrets:
- `PROD_HOST`: Production server IP address
- `PROD_USER`: SSH username for production server
- `PROD_SSH_KEY`: Private SSH key for production server access
- `PROD_PORT`: SSH port (usually 22)

#### Optional Secrets:
- `SA_PASSWORD`: SQL Server password
- `RABBITMQ_PASS`: RabbitMQ password
- `JWT_SECRET_KEY`: JWT signing key
- `OPENAI_API_KEY`: OpenAI API key
- `DEEPSEEK_API_KEY`: DeepSeek API key

### 2. Production Server Setup

#### Install Docker and Docker Compose:
```bash
# Install Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Install Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

#### Create Project Directory:
```bash
sudo mkdir -p /opt/aitool
sudo chown $USER:$USER /opt/aitool
cd /opt/aitool
```

#### Clone Repository:
```bash
git clone https://github.com/your-username/your-repo.git .
```

#### Setup Environment:
```bash
cp env.prod.example .env.prod
# Edit .env.prod with your production values
```

#### Make Scripts Executable:
```bash
chmod +x scripts/*.sh
```

### 3. SSH Key Setup

#### Generate SSH Key Pair:
```bash
ssh-keygen -t rsa -b 4096 -C "your-email@example.com" -f ~/.ssh/aitool_deploy
```

#### Add Public Key to Server:
```bash
# Copy public key to server
ssh-copy-id -i ~/.ssh/aitool_deploy.pub user@your-server-ip
```

#### Add Private Key to GitHub Secrets:
```bash
# Copy private key content
cat ~/.ssh/aitool_deploy
# Add this content to PROD_SSH_KEY secret in GitHub
```

## 🔄 Pipeline Workflow

### Build Job
1. **Checkout Code**: Gets the latest code from repository
2. **Setup Docker Buildx**: Enables multi-platform builds
3. **Login to Registry**: Authenticates with GitHub Container Registry
4. **Extract Metadata**: Generates image tags
5. **Run Tests**: Executes automated tests (if any)
6. **Build Images**: Builds Docker images for all services
7. **Push Images**: Pushes images to GitHub Container Registry
8. **Generate Artifacts**: Creates deployment files

### Deploy Job (Only on main branch)
1. **Download Artifacts**: Gets deployment files from Build job
2. **SSH to Server**: Connects to production server
3. **Login to Registry**: Authenticates with container registry
4. **Pull Images**: Downloads new images
5. **Stop Services**: Gracefully stops current services
6. **Update Compose**: Updates docker-compose.prod.yml with new images
7. **Start Services**: Launches updated services
8. **Health Check**: Verifies all services are running
9. **Cleanup**: Removes old images

## 🛠️ Manual Deployment

### Deploy to Production:
```bash
# On production server
cd /opt/aitool
./scripts/deploy.sh
```

### Health Check:
```bash
./scripts/health-check.sh
```

### Rollback:
```bash
./scripts/deploy.sh rollback
```

### View Logs:
```bash
# All services
docker compose -f docker-compose.prod.yml logs

# Specific service
docker compose -f docker-compose.prod.yml logs users.api
```

## 🔍 Monitoring and Troubleshooting

### Health Check Endpoints:
- Users API: `http://your-server:5011/health`
- ChatGPT API: `http://your-server:5014/health`
- ChatGPT Tool: `http://your-server:5012/`
- Nginx Gateway: `http://your-server/health`

### Common Issues:

#### 1. Build Failures:
- Check Dockerfile syntax
- Verify all dependencies are installed
- Check GitHub Actions logs

#### 2. Deployment Failures:
- Verify SSH connection to production server
- Check if all required secrets are set
- Verify server has enough resources

#### 3. Service Health Issues:
- Check service logs: `docker compose -f docker-compose.prod.yml logs [service]`
- Verify all dependencies are running
- Check network connectivity between services

### Log Locations:
- Application logs: `docker compose -f docker-compose.prod.yml logs`
- Deployment logs: `/opt/aitool/logs/deploy.log`
- System logs: `/var/log/syslog`

## 🔒 Security Considerations

1. **Environment Variables**: Never commit `.env.prod` to repository
2. **SSH Keys**: Use dedicated deployment keys with limited permissions
3. **Registry Access**: Use GitHub tokens with minimal required permissions
4. **Network Security**: Configure firewall rules for required ports only
5. **SSL/TLS**: Use HTTPS in production with valid certificates

## 📊 Performance Optimization

1. **Docker Images**: Use multi-stage builds to reduce image size
2. **Caching**: Enable Docker layer caching in GitHub Actions
3. **Resource Limits**: Set appropriate CPU/memory limits for containers
4. **Database**: Optimize database queries and indexes
5. **Monitoring**: Implement application performance monitoring

## 🚨 Emergency Procedures

### Quick Rollback:
```bash
cd /opt/aitool
./scripts/deploy.sh rollback
```

### Stop All Services:
```bash
docker compose -f docker-compose.prod.yml down
```

### Restart Specific Service:
```bash
docker compose -f docker-compose.prod.yml restart [service-name]
```

### Emergency Access:
```bash
# Access container shell
docker exec -it $(docker compose -f docker-compose.prod.yml ps -q [service-name]) /bin/bash
```

## 📞 Support

For issues with the CI/CD pipeline:
1. Check GitHub Actions logs
2. Review deployment logs on production server
3. Verify all secrets are correctly configured
4. Test SSH connection manually
5. Check Docker and Docker Compose installation
