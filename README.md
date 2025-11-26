# DevHabit

A modern ASP.NET Core Web API built with .NET 9.0, designed with strict code quality standards and best practices.

## Features

- **ASP.NET Core Web API** with .NET 9.0
- **Docker Container Support** with multi-service orchestration
- **HTTPS Support** with self-signed development certificates
- **PostgreSQL Database** integration with Entity Framework Core
- **OpenAPI/Swagger** documentation
- **Strict Code Analysis** with SonarAnalyzer
- **Central Package Management** for consistent dependency versions
- **Nullable Reference Types** enabled
- **Warnings as Errors** for high code quality
- **.NET Aspire Dashboard** for telemetry and monitoring

## Prerequisites

### For Docker Development (Recommended)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine
- [Docker Compose](https://docs.docker.com/compose/) (included with Docker Desktop)

### For Local Development
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/download/) (or use Docker for database only)
- IDE of your choice (Visual Studio, VS Code, Rider)

## Getting Started

### Option 1: Docker Development (Recommended)

#### 1. Clone the Repository
```bash
git clone <repository-url>
cd DevHabit
```

#### 2. Generate HTTPS Certificates (First Time Only)
```bash
./generate-dev-cert.sh
```

#### 3. Start All Services
```bash
docker-compose up --build
```

The API will be available at:
- **HTTPS**: `https://localhost:9001/habits` (secure, recommended)
- **HTTP**: `http://localhost:9000/habits` (redirects to HTTPS)
- **Aspire Dashboard**: `http://localhost:18888` (monitoring)
- **PostgreSQL**: `localhost:5492` (user: postgres, password: postgres)

#### 4. Test the API
```bash
curl -k https://localhost:9001/habits
```

### Option 2: Local Development

#### 1. Clone and Setup
```bash
git clone <repository-url>
cd DevHabit
dotnet restore
```

#### 2. Configure Database
Setup PostgreSQL locally or use Docker for database only:
```bash
docker run --name devhabit-postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:17.2
```

#### 3. Run the Application
```bash
dotnet run --project DevHabit.Api
```

The API will be available at:
- **HTTPS**: `https://localhost:5001/habits`
- **HTTP**: `http://localhost:5000/habits`

## Development

### Docker Development Commands
```bash
# Start all services
docker-compose up

# Rebuild and start (after code changes)
docker-compose up --build

# Run in background
docker-compose up -d

# View logs
docker-compose logs -f devhabit.api

# Stop all services
docker-compose down

# Generate HTTPS certificates (first time)
./generate-dev-cert.sh
```

### Local Development with Hot Reload
For local development with automatic restart on file changes:
```bash
dotnet watch --project DevHabit.Api
```

### API Documentation
OpenAPI documentation is available at:

**Docker Environment:**
- OpenAPI JSON: `https://localhost:9001/openapi/v1.json`

**Local Environment:**
- OpenAPI JSON: `https://localhost:5001/openapi/v1.json`

### Code Quality

This project enforces strict code quality standards:
- **All warnings treated as errors**
- **SonarAnalyzer** static code analysis
- **Latest C# analysis level** with all analysis modes enabled
- **Code style enforcement** during build

### Package Management

The project uses [Central Package Management](https://learn.microsoft.com/en-us/nuget/consume-packages/Central-Package-Management):
- Package versions are defined in `Directory.Packages.props`
- Project files only specify package names without versions
- Ensures consistent package versions across the solution

#### Adding New Packages
1. Add the package reference to the project file:
   ```xml
   <PackageReference Include="PackageName" />
   ```

2. Define the version in `Directory.Packages.props`:
   ```xml
   <PackageVersion Include="PackageName" Version="x.y.z" />
   ```

## Project Structure

```
DevHabit/
├── DevHabit.Api/                    # Web API project
│   ├── Controllers/                 # API controllers
│   │   ├── HabitsController.cs     # Habits API endpoints (GET /habits, GET /habits/{id})
│   │   └── WeatherForecastController.cs # Example controller (template)
│   ├── DTOs/                       # Data Transfer Objects
│   │   └── Habits/                 # Habit-related DTOs
│   │       └── HabitDto.cs        # Habit response models
│   ├── Database/                   # Database context and configuration
│   ├── Entities/                   # Entity models
│   │   └── Habit.cs               # Main habit entity with enums
│   ├── Properties/                 # Launch settings and configuration
│   ├── appsettings.json            # Base application configuration
│   ├── appsettings.Development.json # Local development overrides
│   ├── appsettings.Docker.json     # Docker container configuration
│   ├── Dockerfile                  # Docker container definition
│   ├── Program.cs                  # Application entry point
│   └── DevHabit.Api.csproj        # Project file
├── .certificates/                   # HTTPS development certificates (gitignored)
├── Directory.Build.props           # Global MSBuild properties
├── Directory.Packages.props        # Central package version management
├── docker-compose.yml              # Multi-service container orchestration
├── generate-dev-cert.sh           # HTTPS certificate generation script
├── DevHabit.sln                   # Solution file
├── CLAUDE.md                      # Claude Code project guidance
├── HTTPS-SETUP.md                 # HTTPS configuration guide
└── README.md                      # This file
```

## Available Commands

### Docker Commands
| Command | Description |
|---------|-------------|
| `./generate-dev-cert.sh` | Generate HTTPS development certificates |
| `docker-compose up` | Start all services |
| `docker-compose up --build` | Rebuild and start all services |
| `docker-compose down` | Stop and remove all containers |
| `docker-compose logs devhabit.api` | View API container logs |

### .NET Commands
| Command | Description |
|---------|-------------|
| `dotnet build` | Build the solution |
| `dotnet run --project DevHabit.Api` | Run the API locally |
| `dotnet watch --project DevHabit.Api` | Run with hot reload |
| `dotnet test` | Run tests (when test projects are added) |
| `dotnet restore` | Restore NuGet packages |
| `dotnet ef migrations list --project DevHabit.Api` | View migration status |
| `dotnet ef migrations add {Name} --project DevHabit.Api` | Create new migration |
| `dotnet ef database update --project DevHabit.Api` | Apply pending migrations |

## Configuration

### Development Settings
- `appsettings.json` - Base application settings
- `appsettings.Development.json` - Local development overrides
- `appsettings.Docker.json` - Docker container configuration (includes HTTPS)
- `Properties/launchSettings.json` - Development server profiles

### HTTPS Configuration
The project supports HTTPS in both local and containerized environments:
- **Container**: Uses self-signed certificates generated by `generate-dev-cert.sh`
- **Local**: Uses ASP.NET Core development certificates
- For detailed HTTPS setup and troubleshooting, see [`HTTPS-SETUP.md`](HTTPS-SETUP.md)

### Environment Variables
The application uses standard ASP.NET Core configuration patterns. Set environment-specific values using:
- Environment variables
- User secrets (for development)
- Configuration files

### Database Configuration
- **Docker**: PostgreSQL container with automatic connection setup
- **Local**: Requires local PostgreSQL installation or Docker container
- Connection strings configured per environment in respective `appsettings` files

### Database Migrations
The project uses Entity Framework Core migrations to manage database schema changes:

#### Migration Commands
```bash
# View current migration status
dotnet ef migrations list --project DevHabit.Api

# Create a new migration (when model changes are made)
dotnet ef migrations add {MigrationName} --project DevHabit.Api

# Apply pending migrations to database
dotnet ef database update --project DevHabit.Api
```

#### Migration History
Current database migrations applied:
- **`20251121221333_Add_Habits`** - Initial Habits table creation with full schema
- **`20251126222141_UpdateHabitModel`** - Column rename for naming consistency (`frequency_time_per_period` → `frequency_times_per_period`)

## Troubleshooting

### Common Issues

#### API Fails to Start with Database Connection Error
**Problem**: Getting `Failed to connect to 127.0.0.1:5492` or similar database connection errors.

**Solution**: The API requires PostgreSQL to be running. Choose one of these options:

**Option 1: Start Database Only (Recommended for Local Development)**
```bash
# Start PostgreSQL database container
docker-compose up devhabit.postgres -d

# Then run API locally
dotnet run --project DevHabit.Api
```

**Option 2: Use Full Docker Compose**
```bash
# Generate certificates first (if not done before)
./generate-dev-cert.sh

# Start all services
docker-compose up --build
```

#### Certificate Generation Script Issues
**Problem**: `./generate-dev-cert.sh: bad interpreter: /bin/bash^M`

**Solution**: The script has Windows line endings. Convert line endings:
```bash
# On macOS/Linux
sed -i '' 's/\r$//' generate-dev-cert.sh
chmod +x generate-dev-cert.sh
./generate-dev-cert.sh
```

#### Docker Build Fails with NuGet Connectivity
**Problem**: `NU1900: Error occurred while getting package vulnerability data`

**Solution**: This is a network connectivity issue. Try:
1. Check your internet connection
2. Restart Docker Desktop
3. Use local development instead: `dotnet run --project DevHabit.Api`

### API Endpoints Available

Once the API is running, the following endpoints are available:

**Local Development:**
- `GET http://localhost:5000/habits` - Get all habits
- `GET http://localhost:5000/habits/{id}` - Get habit by ID

**Docker Development:**
- `GET http://localhost:9000/habits` - Get all habits
- `GET http://localhost:9000/habits/{id}` - Get habit by ID

The API returns seeded habit data for testing purposes.

#### Database Migration Issues
**Problem**: Getting `The model for context 'ApplicationDbContext' has pending changes` error.

**Solution**: Your entity models have changed but no migration exists for the changes:
```bash
# Create a migration for the pending changes
dotnet ef migrations add UpdateModel --project DevHabit.Api

# Apply the migration to the database
dotnet ef database update --project DevHabit.Api
```

**Problem**: Migration file fails to build with code analysis errors (e.g., IDE0161 namespace style).

**Solution**: EF Core generates traditional namespaces, but the project requires file-scoped namespaces:
```csharp
// Change from:
namespace DevHabit.Api.Migrations.Application
{
    public partial class MigrationName : Migration
    {
        // ...
    }
}

// To:
namespace DevHabit.Api.Migrations.Application;

public partial class MigrationName : Migration
{
    // ...
}
```

**Problem**: Database connection issues during migration.

**Solution**: Ensure the database is running before applying migrations:
```bash
# Start PostgreSQL container first
docker-compose up devhabit.postgres -d

# Then apply migrations
dotnet ef database update --project DevHabit.Api
```

## Testing

Currently, no test projects are configured. To add tests:

```bash
# Add a test project
dotnet new xunit -n DevHabit.Api.Tests
dotnet sln add DevHabit.Api.Tests

# Add reference to the main project
dotnet add DevHabit.Api.Tests reference DevHabit.Api

# Run tests
dotnet test
```

## Contributing

1. Ensure all code follows the project's quality standards
2. All builds must pass with zero warnings
3. Follow the existing code style and patterns
4. Update documentation as needed

## License

MIT