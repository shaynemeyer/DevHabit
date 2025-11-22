# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DevHabit is an ASP.NET Core Web API project built with .NET 9.0. The project uses modern C# development practices with strict code analysis and centralized package management.

## Development Commands

### Building and Running (Local Development)
- `dotnet build` - Build the entire solution
- `dotnet build --configuration Release` - Build for production
- `dotnet run --project DevHabit.Api` - Run the API locally (default: https://localhost:5001)
- `dotnet watch --project DevHabit.Api` - Run with hot reload for development

### Docker Container Commands
- `docker-compose up` - Start all services (API, PostgreSQL, Aspire Dashboard)
- `docker-compose up --build` - Rebuild images and start services
- `docker-compose down` - Stop and remove all containers
- `docker-compose logs devhabit.api` - View API container logs
- `./generate-dev-cert.sh` - Generate HTTPS development certificates (run once)

### Testing
- `dotnet test` - Run all tests (no test projects exist yet)
- `dotnet test --configuration Release` - Run tests in release mode

### Package Management
- `dotnet restore` - Restore NuGet packages
- `dotnet list package` - List installed packages
- `dotnet add DevHabit.Api package [PackageName]` - Add a package (update Directory.Packages.props for version)

## Architecture and Code Structure

### Project Organization
- **DevHabit.sln** - Main solution file
- **DevHabit.Api/** - Web API project containing controllers, models, and startup configuration
- **Directory.Build.props** - Global MSBuild properties and code analysis settings
- **Directory.Packages.props** - Centralized NuGet package version management

### Code Quality Standards
The project enforces strict code quality standards:
- **Warnings as Errors**: All warnings are treated as compilation errors
- **SonarAnalyzer**: Static code analysis with SonarAnalyzer.CSharp
- **Latest Analysis Level**: Uses the latest C# analysis features
- **Nullable Reference Types**: Enabled project-wide
- **Implicit Usings**: Enabled to reduce boilerplate

### Current Structure
- **Controllers/**: API controllers (currently contains WeatherForecastController as template)
- **Models**: Domain models (WeatherForecast model exists as example)
- **Program.cs**: Application entry point with service configuration

### Configuration
- **appsettings.json**: Base application settings
- **appsettings.Development.json**: Development-specific overrides
- **appsettings.Docker.json**: Docker container-specific configuration (includes HTTPS setup)
- **Properties/launchSettings.json**: Development server launch profiles

## Key Architectural Patterns

### Dependency Injection
The project uses ASP.NET Core's built-in dependency injection container. Services are configured in Program.cs.

### API Documentation
- OpenAPI/Swagger is configured and available in development mode
- Access via `/openapi/v1.json` endpoint when running locally

### Central Package Management
All package versions are managed centrally through Directory.Packages.props. When adding new packages:
1. Add PackageReference in the project file without version
2. Define the version in Directory.Packages.props

### Code Analysis Configuration
The project uses comprehensive code analysis:
- All analysis modes enabled
- Code style enforcement during build
- SonarAnalyzer for additional quality checks

## Container Deployment

### Docker Configuration
The project includes full Docker support with:
- **docker-compose.yml**: Multi-service orchestration (API, PostgreSQL, Aspire Dashboard)
- **DevHabit.Api/Dockerfile**: Multi-stage build for API container
- **HTTPS Support**: Self-signed certificates for development
- **Volume Mounting**: Certificate files mounted securely

### Container Services
- **devhabit.api**: Main API service (ports 9000/HTTP, 9001/HTTPS)
- **devhabit.postgres**: PostgreSQL 17.2 database (port 5492)
- **devhabit.aspire-dashboard**: .NET Aspire telemetry dashboard (port 18888)

### HTTPS Configuration
For HTTPS support in containers:
1. Run `./generate-dev-cert.sh` to create development certificates
2. Certificates are automatically mounted to `/https` in the container
3. Access API via `https://localhost:9001/habits` (secure) or `http://localhost:9000/habits` (redirects to HTTPS)
4. See `HTTPS-SETUP.md` for detailed configuration and troubleshooting

### Database Connection
- **Development**: Uses localhost connection (`appsettings.Development.json`)
- **Docker**: Uses container networking to `devhabit.postgres` (`appsettings.Docker.json`)

## Development Workflow

### Local Development
1. **Adding New Features**: Create controllers in the Controllers/ folder following the existing pattern
2. **Models**: Add domain models at the root level or in appropriate subfolders
3. **Configuration**: Update appsettings files for new configuration requirements
4. **Dependencies**: Add new PackageReference entries and update Directory.Packages.props

### Container Development
1. **First Time Setup**: Run `./generate-dev-cert.sh` to create HTTPS certificates
2. **Start Services**: Use `docker-compose up --build` to start all services
3. **Development**: Make code changes and rebuild containers as needed
4. **Testing**: Access API at `https://localhost:9001` or `http://localhost:9000`
5. **Logs**: Monitor with `docker-compose logs -f devhabit.api`
6. **Database**: PostgreSQL available at `localhost:5492` (user: postgres, password: postgres)

## API Endpoints

### Container Environment (Recommended)
- **HTTPS**: `https://localhost:9001/habits` (secure, with SSL certificate)
- **HTTP**: `http://localhost:9000/habits` (redirects to HTTPS automatically)

### Local Development Environment
- **HTTPS**: `https://localhost:5001/habits` (when running `dotnet run`)
- **HTTP**: `http://localhost:5000/habits` (when running `dotnet run`)

### API Documentation
- **OpenAPI JSON**: Available at `/openapi/v1.json` on any of the above endpoints
- **Database Dashboard**: Aspire Dashboard at `http://localhost:18888` (container only)

## HTTP Client Testing
The project includes DevHabit.Api.http for testing API endpoints directly in compatible editors. Update endpoint URLs based on your deployment method (container vs. local).