# DevHabit Docker Setup Guide

This guide covers containerized deployment of the DevHabit application using Docker and Docker Compose.

## Prerequisites

- **Docker Desktop** - Latest version with Docker Compose support
- **Git** for cloning the repository
- **Basic understanding** of Docker containers and networking

## Quick Start

### 1. Clone the Repository
```bash
git clone <repository-url>
cd DevHabit
```

### 2. Generate HTTPS Certificates (First Time Only)
```bash
./generate-dev-cert.sh
```
This creates self-signed certificates for HTTPS support in development.

### 3. Start All Services
```bash
# Build and start all containers
docker-compose up --build

# Or run in the background
docker-compose up --build -d
```

### 4. Access the Application
- **API (HTTPS)**: `https://localhost:9001`
- **API (HTTP)**: `http://localhost:9000` (redirects to HTTPS)
- **Aspire Dashboard**: `http://localhost:18888`
- **PostgreSQL**: `localhost:5492` (user: postgres, password: postgres)

## Docker Configuration

### Container Services

The project includes a multi-service Docker Compose setup:

#### devhabit.api
- **Ports**: 9000 (HTTP), 9001 (HTTPS)
- **Description**: Main API service
- **Health Check**: Built-in health checks for container orchestration
- **Volume Mounts**: HTTPS certificates mounted to `/https`

#### devhabit.postgres
- **Port**: 5492
- **Version**: PostgreSQL 17.2
- **Database**: `dev_habit`
- **Credentials**: postgres/postgres
- **Volume**: Persistent data storage

#### devhabit.aspire-dashboard
- **Port**: 18888
- **Description**: .NET Aspire telemetry dashboard for monitoring
- **Purpose**: Development observability and metrics visualization

### Docker Compose Commands

```bash
# Start all services
docker-compose up

# Build and start services
docker-compose up --build

# Start in background (detached mode)
docker-compose up -d

# Stop all services
docker-compose down

# Stop and remove volumes (⚠️ This deletes database data)
docker-compose down -v

# View logs for all services
docker-compose logs

# View logs for specific service
docker-compose logs devhabit.api
docker-compose logs devhabit.postgres
docker-compose logs devhabit.aspire-dashboard

# Follow log output in real-time
docker-compose logs -f devhabit.api

# Restart a specific service
docker-compose restart devhabit.api

# Scale services (if configured)
docker-compose up --scale devhabit.api=2
```

## HTTPS Configuration

### Development Certificates

The project supports HTTPS in containers using self-signed certificates:

1. **Generate certificates** (run once):
   ```bash
   ./generate-dev-cert.sh
   ```

2. **Certificate locations**:
   - Host: `./certs/` directory
   - Container: `/https/` directory (mounted)

3. **Configuration files**:
   - `appsettings.Docker.json`: Container-specific HTTPS settings

### Certificate Management

#### Regenerate Certificates
```bash
# Remove existing certificates
rm -rf certs/

# Generate new certificates
./generate-dev-cert.sh

# Restart containers to pick up new certificates
docker-compose restart devhabit.api
```

#### Troubleshooting Certificate Issues
- Ensure certificates exist in `./certs/` directory
- Check file permissions (certificates should be readable)
- Verify volume mounts in docker-compose.yml
- Check container logs for certificate-related errors

## Database Configuration

### Schema Organization
The application uses a single PostgreSQL database with multiple schemas:
- **dev_habit**: Application data (habits, tags, users)
- **identity**: ASP.NET Core Identity tables (authentication)

### Database Access
```bash
# Connect via psql
docker exec -it devhabit-postgres psql -U postgres -d dev_habit

# Or connect from host
psql -h localhost -p 5492 -U postgres -d dev_habit
```

### Database Persistence
Database data persists across container restarts unless volumes are explicitly removed:
```bash
# ⚠️ WARNING: This deletes all database data
docker-compose down -v
```

### Database Backup and Restore
```bash
# Backup database
docker exec devhabit-postgres pg_dump -U postgres dev_habit > backup.sql

# Restore database
docker exec -i devhabit-postgres psql -U postgres dev_habit < backup.sql
```

## Network Configuration

### Port Mapping
- **9000**: HTTP (redirects to HTTPS)
- **9001**: HTTPS (recommended)
- **5492**: PostgreSQL database
- **18888**: Aspire Dashboard

### Service Communication
Containers communicate via Docker's internal network:
- API connects to PostgreSQL at `devhabit.postgres:5432`
- All services share the same Docker network
- External access via mapped ports on localhost

### Firewall Considerations
Ensure the following ports are accessible:
- 9000, 9001 (API)
- 5492 (Database, if external access needed)
- 18888 (Dashboard)

## Development Workflow

### Container Development Cycle
1. **Make code changes** in your IDE
2. **Rebuild containers**:
   ```bash
   docker-compose up --build
   ```
3. **Test changes** via API endpoints
4. **Check logs** for issues:
   ```bash
   docker-compose logs -f devhabit.api
   ```

### Debugging in Containers
```bash
# Execute commands inside API container
docker exec -it devhabit-api bash

# View container environment variables
docker exec devhabit-api env

# Check container health
docker inspect devhabit-api

# Monitor resource usage
docker stats
```

### Hot Reload in Containers
The Docker configuration supports development scenarios:
- Volume mounts for source code (if configured)
- Automatic rebuilds on file changes
- Environment-specific configurations

## Monitoring and Observability

### Aspire Dashboard
Access the dashboard at `http://localhost:18888` to monitor:
- **Distributed traces**: Request flows across services
- **Metrics**: Performance counters and custom metrics
- **Logs**: Structured logging from all services
- **Health checks**: Service health and availability

### Application Logs
```bash
# View structured logs
docker-compose logs devhabit.api | grep -E "(info|warn|error)"

# Follow logs in real-time
docker-compose logs -f devhabit.api

# Filter logs by level
docker-compose logs devhabit.api | grep "ERROR"
```

### Health Checks
```bash
# Check service health via Docker
docker ps

# API health endpoint
curl http://localhost:9000/health
```

## Production Considerations

### Security
- **Replace self-signed certificates** with valid SSL certificates
- **Change default database passwords**
- **Configure proper firewall rules**
- **Use secrets management** for sensitive configuration

### Performance
- **Resource limits**: Configure memory and CPU limits
- **Database tuning**: Optimize PostgreSQL configuration
- **Load balancing**: Scale API containers as needed
- **Caching**: Implement Redis or similar for session/cache data

### Deployment
```bash
# Production deployment example
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## Troubleshooting

### Common Issues

#### Port Conflicts
```bash
# Check what's using ports
netstat -an | grep :9000
lsof -i :9001

# Stop conflicting services or change ports in docker-compose.yml
```

#### Container Startup Issues
```bash
# Check container logs
docker-compose logs devhabit.api

# Inspect container details
docker inspect devhabit-api

# Check Docker daemon status
docker system info
```

#### Database Connection Issues
```bash
# Test database connectivity
docker exec devhabit-postgres pg_isready -U postgres

# Check database logs
docker-compose logs devhabit.postgres

# Verify connection from API container
docker exec devhabit-api nc -zv devhabit.postgres 5432
```

#### Certificate Issues
```bash
# Verify certificate files exist
ls -la certs/

# Check certificate validity
openssl x509 -in certs/localhost.crt -text -noout

# Regenerate certificates
./generate-dev-cert.sh
```

### Performance Issues
```bash
# Monitor resource usage
docker stats

# Check disk usage
docker system df

# Clean up unused resources
docker system prune
```

## Related Documentation

- [Development Setup Guide](development-setup.md): Local development without containers
- [Database Migrations Guide](database-migrations.md): Managing database schema changes
- [API Reference](api-reference.md): Complete API documentation
- [Authentication Setup](authentication-setup.md): Identity configuration details

For HTTPS setup details, see the project's `HTTPS-SETUP.md` file.