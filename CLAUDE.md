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
- **DevHabit.Api/** - Web API project containing controllers, entities, DTOs, and database configuration
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
- **Controllers/**: API controllers including HabitsController for habit management
- **Entities/**: Domain entities including Habit with complex value objects (Frequency, Target, Milestone)
- **DTOs/**: Data Transfer Objects organized by feature (e.g., DTOs/Habits/)
  - **HabitDto**: Read model for habit data
  - **CreateHabitDto**: Input model for habit creation
  - **UpdateHabitDto**: Input model for full habit updates (PUT operations)
  - **UpdateMilestoneDto**: Simplified milestone update model (only Target, preserves Current progress)
  - **HabitMappings**: Extension methods for entity-DTO conversions including UpdateFromDto
  - **HabitQueries**: LINQ expression projections for efficient database queries
- **Database/**: Entity Framework Core configuration
  - **ApplicationDbContext**: Main database context with Habits DbSet
  - **Configurations/**: Entity configurations using Fluent API
  - **Migrations/**: EF Core migration files
- **Extensions/**: Extension methods including database setup
- **Program.cs**: Application entry point with service configuration

### Configuration
- **appsettings.json**: Base application settings
- **appsettings.Development.json**: Development-specific overrides
- **appsettings.Docker.json**: Docker container-specific configuration (includes HTTPS setup)
- **Properties/launchSettings.json**: Development server launch profiles

## Key Architectural Patterns

### Dependency Injection
The project uses ASP.NET Core's built-in dependency injection container. Services are configured in Program.cs.

### JSON Patch Support
The API supports JSON Patch operations for partial updates:
- **Microsoft.AspNetCore.JsonPatch** (v9.0.11): Provides JSON Patch functionality
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson** (v9.0.11): Enables Newtonsoft.Json serialization required for JSON Patch
- **Configuration**: Program.cs includes `.AddNewtonsoftJson()` to enable JSON Patch document support

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

## Domain Model

### Habit Entity
The core domain entity that represents a habit with the following properties:
- **Id**: Unique identifier using Version 7 GUIDs with "h_" prefix
- **Name**: Required habit name (max 100 characters)
- **Description**: Optional description (max 500 characters)
- **Type**: Enum defining habit type (Binary, Measurable)
- **Frequency**: Complex value object defining how often the habit should be performed
  - **Type**: Daily, Weekly, or Monthly
  - **TimesPerPeriod**: Number of times per the specified period
- **Target**: Value object defining the habit goal
  - **Value**: Numeric target value
  - **Unit**: Unit of measurement (max 100 characters)
- **Status**: Current habit status (Ongoing, Completed)
- **IsArchived**: Boolean flag for archiving habits
- **EndDate**: Optional end date for time-bounded habits
- **Milestone**: Optional progress tracking
  - **Target**: Milestone goal
  - **Current**: Current progress toward milestone
- **CreatedAtUtc**: Timestamp of creation
- **UpdatedAtUtc**: Last modification timestamp
- **LastCompletedAtUtc**: Last completion timestamp

### Data Architecture Patterns
- **Domain-Driven Design**: Clear separation between entities and DTOs
- **Value Objects**: Frequency, Target, and Milestone as owned entities
- **Efficient Projections**: Using Expression<Func<T, TResult>> for database queries
- **Mapping Extensions**: Clean entity-DTO conversions with extension methods
- **UUID v7 Identifiers**: Time-ordered identifiers for better database performance

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
1. **Adding New Features**: Follow the established patterns:
   - **Entities**: Add domain entities to the `Entities/` folder with proper value objects
   - **DTOs**: Create feature-specific DTOs in `DTOs/{FeatureName}/` folders
   - **Controllers**: Add controllers to `Controllers/` following REST conventions
   - **Database**: Configure entity mappings in `Database/Configurations/`
   - **Mappings**: Create extension methods for entity-DTO conversions
   - **Queries**: Use expression projections for efficient database queries
2. **Database Changes**: Use EF Core migrations for schema modifications
   - `dotnet ef migrations add {Name}` - Create a new migration
   - `dotnet ef database update` - Apply pending migrations to database
   - `dotnet ef migrations list` - View migration history and status
   - **Current Migration History:**
     - `20251121221333_Add_Habits` - Initial Habits table creation with full schema
     - `20251126222141_UpdateHabitModel` - Column rename: `frequency_time_per_period` → `frequency_times_per_period`
3. **Configuration**: Update appsettings files for new configuration requirements
4. **Dependencies**: Add new PackageReference entries and update Directory.Packages.props

### Development Patterns
- **Entity Configuration**: Use `IEntityTypeConfiguration<T>` for Fluent API configurations
- **Value Objects**: Model complex properties as owned entities (e.g., `Frequency`, `Target`)
- **DTO Projections**: Use `Expression<Func<T, TResult>>` for efficient database queries
- **Mapping Extensions**: Create `ToDto()`, `ToEntity()`, and `UpdateFromDto()` extension methods
- **Controller Actions**: Follow REST conventions with proper HTTP status codes
- **Progress Tracking Separation**: Milestone progress (`Current`) is intentionally separated from general updates to preserve progress tracking integrity

### Container Development
1. **First Time Setup**: Run `./generate-dev-cert.sh` to create HTTPS certificates
2. **Start Services**: Use `docker-compose up --build` to start all services
3. **Development**: Make code changes and rebuild containers as needed
4. **Testing**: Access API at `https://localhost:9001` or `http://localhost:9000`
5. **Logs**: Monitor with `docker-compose logs -f devhabit.api`
6. **Database**: PostgreSQL available at `localhost:5492` (user: postgres, password: postgres)

## API Endpoints

### Habits API
The API provides full CRUD operations for habit management:

#### Get All Habits
- **GET** `/habits`
- Returns a collection of all habits with pagination support
- Response: `HabitsCollectionDto` containing array of `HabitDto` objects

#### Get Single Habit
- **GET** `/habits/{id}`
- Retrieves a specific habit by its ID
- Response: `HabitDto` object or 404 if not found
- Parameter: `id` (string) - The habit identifier

#### Create Habit
- **POST** `/habits`
- Creates a new habit
- Request Body: `CreateHabitDto`
- Response: `HabitDto` of the created habit with `201 Created` status
- Returns `Location` header pointing to the created resource

#### Update Habit (Full)
- **PUT** `/habits/{id}`
- Completely replaces an existing habit
- Request Body: `UpdateHabitDto`
- Response: `204 No Content` on success, `404 Not Found` if habit doesn't exist
- Parameter: `id` (string) - The habit identifier
- **Note**: Preserves milestone progress (`Current` value) while allowing target updates

#### Update Habit (Partial)
- **PATCH** `/habits/{id}`
- Partially modifies an existing habit using JSON Patch operations
- Request Body: `JsonPatchDocument<HabitDto>` with Content-Type `application/json-patch+json`
- Response: `204 No Content` on success, `404 Not Found` if habit doesn't exist, `400 Bad Request` for validation errors
- Parameter: `id` (string) - The habit identifier
- **Current Implementation**: Only updates `Name`, `Description`, and `UpdatedAtUtc` fields
- **Validation**: Full model validation is performed on the patched result before applying changes

### Example API Usage

#### Creating a Daily Exercise Habit
```json
POST /habits
{
  "name": "Daily Exercise",
  "description": "30 minutes of physical activity",
  "type": "Measurable",
  "frequency": {
    "type": "Daily",
    "timesPerPeriod": 1
  },
  "target": {
    "value": 30,
    "unit": "minutes"
  },
  "milestone": {
    "target": 100,
    "current": 0
  }
}
```

#### Response Example
```json
{
  "id": "h_01JDQM7Z8K2X3Y4W5V6U7T8S9R",
  "name": "Daily Exercise",
  "description": "30 minutes of physical activity",
  "type": "Measurable",
  "frequency": {
    "type": "Daily",
    "timesPerPeriod": 1
  },
  "target": {
    "value": 30,
    "unit": "minutes"
  },
  "status": "Ongoing",
  "isArchived": false,
  "endDate": null,
  "milestone": {
    "target": 100,
    "current": 0
  },
  "createdAtUtc": "2024-11-25T12:00:00Z",
  "updatedAtUtc": null,
  "lastCompletedAtUtc": null
}
```

#### Updating a Habit (Full Replacement)
```json
PUT /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R
Content-Type: application/json

{
  "name": "Updated Daily Exercise",
  "description": "45 minutes of physical activity",
  "type": "Measurable",
  "frequency": {
    "type": "Daily",
    "timesPerPeriod": 1
  },
  "target": {
    "value": 45,
    "unit": "minutes"
  },
  "milestone": {
    "target": 150
  }
}
```

**Response**: `204 No Content`

#### Partially Updating a Habit (JSON Patch)
```json
PATCH /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R
Content-Type: application/json-patch+json

[
  {
    "op": "replace",
    "path": "/name",
    "value": "Morning Exercise"
  },
  {
    "op": "replace",
    "path": "/description",
    "value": "30-minute workout routine"
  }
]
```

**Response**: `204 No Content`

**Note**: Currently, only `name` and `description` operations are fully implemented in the PATCH endpoint.

### Base URLs

#### Container Environment (Recommended)
- **HTTPS**: `https://localhost:9001` (secure, with SSL certificate)
- **HTTP**: `http://localhost:9000` (redirects to HTTPS automatically)

#### Local Development Environment
- **HTTPS**: `https://localhost:5001` (when running `dotnet run`)
- **HTTP**: `http://localhost:5000` (when running `dotnet run`)

### API Documentation
- **OpenAPI JSON**: Available at `/openapi/v1.json` on any of the above base URLs
- **Database Dashboard**: Aspire Dashboard at `http://localhost:18888` (container only)

## HTTP Client Testing
The project includes DevHabit.Api.http for testing API endpoints directly in compatible editors. Update endpoint URLs based on your deployment method (container vs. local).