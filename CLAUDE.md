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
- **Controllers/**: API controllers for resource management
  - **HabitsController**: Complete CRUD operations for habit management including tagging support
  - **TagsController**: Complete CRUD operations for tag management
  - **HabitTagsController**: Association management between habits and tags
- **Entities/**: Domain entities including complex value objects
  - **Habit**: Core domain entity with complex value objects (Frequency, Target, Milestone)
  - **Tag**: Tag entity for categorizing habits (Id, Name, Description, timestamps)
  - **HabitTag**: Junction entity for many-to-many habit-tag relationships
- **DTOs/**: Data Transfer Objects organized by feature
  - **DTOs/Common/**:
    - **PaginationResult<T>**: Generic pagination wrapper with metadata (page, pageSize, totalCount, etc.)
    - **ICollectionResponse<T>**: Base interface for collection responses
  - **DTOs/Habits/**:
    - **HabitDto**: Basic read model for habit data
    - **HabitWithTagsDto**: Enhanced read model including associated tags array
    - **CreateHabitDto**: Input model for habit creation
    - **UpdateHabitDto**: Input model for full habit updates (PUT operations)
    - **HabitsQueryParameters**: Query parameters for filtering, searching, sorting, pagination, and field selection
    - **HabitMappings**: Extension methods for entity-DTO conversions including UpdateFromDto
    - **HabitQueries**: LINQ expression projections for efficient database queries
  - **DTOs/Tags/**:
    - **TagDto**: Read model for tag data
    - **CreateTagDto**: Input model for tag creation
    - **UpdateTagDto**: Input model for tag updates
    - **TagsCollectionDto**: Collection wrapper for tag arrays
    - **TagMappings**: Extension methods for tag entity-DTO conversions
    - **TagQueries**: LINQ expression projections for tag queries
  - **DTOs/HabitTags/**:
    - **UpsertHabitTagsDto**: Input model for associating tags with habits
- **Database/**: Entity Framework Core configuration
  - **ApplicationDbContext**: Main database context with Habits, Tags, and HabitTags DbSets
  - **Configurations/**: Entity configurations using Fluent API
  - **Migrations/**: EF Core migration files
- **Services/**: Application services and infrastructure
  - **Services/Sorting/**: Dynamic sorting infrastructure with type-safe field mapping
  - **DataShapingService**: Field selection service for response customization
- **Middleware/**: Custom middleware components
  - **ValidationExceptionHandler**: FluentValidation exception handling
  - **GlobalExceptionHandler**: General exception handling middleware
- **Extensions/**: Extension methods including database setup
- **DependencyInjection.cs**: Organized service registration using extension methods
- **Program.cs**: Clean application entry point using extension methods for configuration

### Configuration
- **appsettings.json**: Base application settings
- **appsettings.Development.json**: Development-specific overrides
- **appsettings.Docker.json**: Docker container-specific configuration (includes HTTPS setup)
- **Properties/launchSettings.json**: Development server launch profiles

## Key Architectural Patterns

### Dependency Injection
The project uses ASP.NET Core's built-in dependency injection container with a clean, organized approach using extension methods defined in `DependencyInjection.cs`:

- **AddControllers()**: Configures MVC controllers with JSON/XML serialization, OpenAPI support, and proper content negotiation
- **AddErrorHandling()**: Sets up problem details framework and exception handlers for validation and global error handling
- **AddDatabase()**: Configures Entity Framework Core with PostgreSQL, snake case naming conventions, and proper schema organization
- **AddObservability()**: Registers OpenTelemetry for distributed tracing, metrics collection, and observability with OTLP export
- **AddApplicationServices()**: Registers application-specific services including FluentValidation, dynamic sorting, and data shaping services

This modular approach in `Program.cs` provides clear separation of concerns and makes the application startup configuration easy to understand and maintain.

### Dynamic Sorting Service
The project includes a flexible sorting system that provides type-safe, dynamic sorting capabilities:
- **SortMappingProvider**: Main service that manages field mappings between DTOs and entities
- **SortMappingDefinition**: Defines which DTO fields map to which entity properties
- **SortMapping**: Individual field mapping configuration
- **QueryableExtensions**: Extension method `ApplySort()` for applying dynamic sorting to IQueryable
- **Features:**
  - Type-safe mapping validation at request time
  - Support for nested property sorting (e.g., `frequency.type`, `target.value`)
  - Multiple field sorting with direction control (asc/desc)
  - Automatic validation of sort parameter field names
- **Usage**: Sort parameters use format: `field1 asc,field2 desc,field3` (default direction is ascending)

### Data Shaping Service
The project includes a flexible data shaping system for field selection in API responses:
- **DataShapingService**: Service that allows clients to specify which fields to include in responses
- **Field Selection**: Uses reflection with caching for performance optimization
- **Dynamic Response Structure**: Returns `ExpandoObject` containing only requested fields
- **Validation**: Validates field names against DTO properties to prevent errors
- **Features:**
  - Property-level caching using `ConcurrentDictionary` for performance
  - Case-insensitive field name matching
  - Support for both single entity and collection shaping
  - Automatic validation of field parameter values
- **Usage**: Field parameters use format: `field1,field2,field3` (comma-separated field names)
- **Benefits**: Reduces bandwidth usage and allows clients to customize response payloads

### FluentValidation Framework
The project implements comprehensive input validation using FluentValidation:
- **FluentValidation.DependencyInjectionExtensions**: Automatic validator discovery and registration
- **Custom Validators**: Type-specific validators for DTOs (e.g., `CreateHabitDtoValidator`, `CreateTagDtoValidator`)
- **Validation Rules**: Comprehensive validation including:
  - Field length constraints and required field validation
  - Enum value validation for habit types and frequency periods
  - Business rule validation (e.g., unit compatibility with habit type)
  - Custom validation logic (e.g., future date validation for end dates)
- **Exception Handling**: Custom `ValidationExceptionHandler` middleware that:
  - Catches `ValidationException` from FluentValidation
  - Converts validation errors to structured problem details format
  - Returns `400 Bad Request` with detailed error information grouped by field
  - Integrates with ASP.NET Core's problem details framework

### Pagination Infrastructure
The API implements offset-based pagination for collection endpoints:
- **PaginationResult<T>**: Generic pagination wrapper providing metadata and items
- **Pagination Properties**: `Page`, `PageSize`, `TotalCount`, `TotalPages`, `HasPreviousPage`, `HasNextPage`
- **Default Values**: Page 1, PageSize 10 (configurable via query parameters)
- **Implementation**: Uses Entity Framework's `Skip()` and `Take()` methods for efficient database queries
- **Usage**: Currently implemented on the `GET /habits` endpoint with plans for other collection endpoints

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

#### Key Package Dependencies
- **FluentValidation.DependencyInjectionExtensions** (v12.1.1): Comprehensive input validation framework
- **System.Linq.Dynamic.Core** (v1.7.1): Dynamic LINQ expression parsing for sorting functionality
- **Microsoft.AspNetCore.JsonPatch** (v9.0.11): JSON Patch support for partial updates
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson** (v9.0.11): Newtonsoft.Json integration for JSON Patch
- **EFCore.NamingConventions** (v9.0.0): Snake case naming convention for PostgreSQL
- **Npgsql.EntityFrameworkCore.PostgreSQL** (v9.0.4): PostgreSQL database provider
- **OpenTelemetry packages**: Comprehensive observability and monitoring

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
- **HabitTags**: Collection navigation property for associated tags

### Tag Entity
Domain entity for categorizing and organizing habits:
- **Id**: Unique identifier using Version 7 GUIDs with "t_" prefix
- **Name**: Required tag name (max 50 characters, unique constraint)
- **Description**: Optional tag description (max 500 characters)
- **CreatedAtUtc**: Timestamp of creation
- **UpdatedAtUtc**: Last modification timestamp

### HabitTag Entity
Junction entity establishing many-to-many relationships between habits and tags:
- **HabitId**: Foreign key to Habit entity
- **TagId**: Foreign key to Tag entity
- **CreatedAtUtc**: Timestamp when association was created
- **Composite Primary Key**: (HabitId, TagId)
- **Cascade Delete**: Automatically removes associations when parent entities are deleted

### Data Architecture Patterns
- **Domain-Driven Design**: Clear separation between entities and DTOs
- **Value Objects**: Frequency, Target, and Milestone as owned entities
- **Efficient Projections**: Using Expression<Func<T, TResult>> for database queries
- **Mapping Extensions**: Clean entity-DTO conversions with extension methods
- **UUID v7 Identifiers**: Time-ordered identifiers for better database performance
- **Query Parameter Objects**: Dedicated DTOs for request parameters with model binding attributes
- **Dynamic Sorting**: Type-safe field mapping system with validation and flexible query building
- **Data Shaping**: Field selection service allowing clients to customize response payloads
- **Offset-Based Pagination**: Generic pagination system with metadata and configurable page sizes
- **Comprehensive Validation**: FluentValidation-based input validation with business rule enforcement
- **Structured Exception Handling**: Middleware-based validation error handling with problem details format

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
     - `20251203011050_Add_Tags` - Tags table creation with unique name constraint
     - `20251203165332_Add_HabitTags` - Junction table for many-to-many habit-tag relationships
3. **Configuration**: Update appsettings files for new configuration requirements
4. **Dependencies**: Add new PackageReference entries and update Directory.Packages.props

### Development Patterns
- **Entity Configuration**: Use `IEntityTypeConfiguration<T>` for Fluent API configurations
- **Value Objects**: Model complex properties as owned entities (e.g., `Frequency`, `Target`)
- **DTO Projections**: Use `Expression<Func<T, TResult>>` for efficient database queries
- **Mapping Extensions**: Create `ToDto()`, `ToEntity()`, and `UpdateFromDto()` extension methods
- **Controller Actions**: Follow REST conventions with proper HTTP status codes
- **Progress Tracking Separation**: Milestone progress (`Current`) is intentionally separated from general updates to preserve progress tracking integrity
- **Service Registration**: Use extension methods in `DependencyInjection.cs` to organize service configuration by concern (controllers, database, observability, etc.) and maintain a clean `Program.cs`

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
- Returns a paginated collection of habits with advanced querying capabilities
- **Query Parameters:**
  - `q` (string): Search term to filter habits by name or description (case-insensitive)
  - `type` (HabitType): Filter habits by type (`Binary` or `Measurable`)
  - `status` (HabitStatus): Filter habits by status (`Ongoing` or `Completed`)
  - `sort` (string): Sort results by specified fields. Supports multiple fields comma-separated with optional direction (e.g., `name asc,createdAtUtc desc`)
  - `fields` (string): Comma-separated list of fields to include in response (e.g., `id,name,status`)
  - `page` (int): Page number for pagination (default: 1)
  - `pageSize` (int): Number of items per page (default: 10)
- **Supported Sort Fields:**
  - `name`, `description`, `type`, `status`, `endDate`
  - `frequency.type`, `frequency.timesPerPeriod`
  - `target.value`, `target.unit`
  - `createdAtUtc`, `updatedAtUtc`, `lastCompletedAtUtc`
- **Response**: `PaginationResult<ExpandoObject>` containing:
  - `items`: Array of filtered and sorted habit objects (shaped based on `fields` parameter)
  - `page`: Current page number
  - `pageSize`: Number of items per page
  - `totalCount`: Total number of habits matching the query
  - `totalPages`: Total number of pages available
  - `hasPreviousPage`: Boolean indicating if there's a previous page
  - `hasNextPage`: Boolean indicating if there's a next page
- Returns `400 Bad Request` if invalid sort or field parameters are provided

#### Get Single Habit
- **GET** `/habits/{id}?fields={fields}`
- Retrieves a specific habit by its ID including associated tags
- **Query Parameters:**
  - `fields` (string): Optional comma-separated list of fields to include in response (e.g., `id,name,status`)
- **Response**: `ExpandoObject` containing habit data (shaped based on `fields` parameter) or 404 if not found
- **Parameter**: `id` (string) - The habit identifier
- Returns `400 Bad Request` if invalid field parameters are provided

#### Create Habit
- **POST** `/habits`
- Creates a new habit with comprehensive validation
- Request Body: `CreateHabitDto`
- Response: `HabitDto` of the created habit with `201 Created` status
- Returns `Location` header pointing to the created resource
- **Validation Rules:**
  - `Name`: Required, 3-100 characters
  - `Description`: Optional, max 500 characters
  - `Type`: Must be valid `HabitType` enum value
  - `Frequency.TimesPerPeriod`: Must be greater than 0
  - `Target.Value`: Must be greater than 0
  - `Target.Unit`: Must be one of: minutes, hours, steps, km, cal, pages, books, tasks, sessions
  - `EndDate`: Must be in the future (if provided)
  - `Milestone.Target`: Must be greater than 0 (if milestone provided)
  - **Unit Compatibility**: Binary habits can only use "sessions" or "tasks" units; Measurable habits can use any allowed unit
- Returns `400 Bad Request` with detailed validation errors if validation fails

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

#### Delete Habit
- **DELETE** `/habits/{id}`
- Permanently deletes a habit and all associated tag relationships
- Response: `204 No Content` on success, `404 Not Found` if habit doesn't exist
- Parameter: `id` (string) - The habit identifier
- **Note**: Cascade deletion automatically removes all HabitTag associations

### Tags API
Complete CRUD operations for tag management:

#### Get All Tags
- **GET** `/tags`
- Returns a collection of all available tags
- Response: `TagsCollectionDto` containing array of `TagDto` objects

#### Get Single Tag
- **GET** `/tags/{id}`
- Retrieves a specific tag by its ID
- Response: `TagDto` object or 404 if not found
- Parameter: `id` (string) - The tag identifier

#### Create Tag
- **POST** `/tags`
- Creates a new tag
- Request Body: `CreateTagDto`
- Response: `TagDto` of the created tag with `201 Created` status
- Returns `Location` header pointing to the created resource
- **Validation**: Tag names must be unique (returns `409 Conflict` if duplicate)

#### Update Tag
- **PUT** `/tags/{id}`
- Completely replaces an existing tag
- Request Body: `UpdateTagDto`
- Response: `204 No Content` on success, `404 Not Found` if tag doesn't exist
- Parameter: `id` (string) - The tag identifier

#### Delete Tag
- **DELETE** `/tags/{id}`
- Permanently deletes a tag and all associated habit relationships
- Response: `204 No Content` on success, `404 Not Found` if tag doesn't exist
- Parameter: `id` (string) - The tag identifier
- **Note**: Cascade deletion automatically removes all HabitTag associations

### Habit-Tag Association API
Manages the many-to-many relationships between habits and tags:

#### Upsert Habit Tags
- **PUT** `/habits/{habitId}/tags`
- Replaces all tag associations for a specific habit
- Request Body: `UpsertHabitTagsDto` containing array of tag IDs
- Response: `200 OK` on success, `204 No Content` if no changes, `404 Not Found` if habit doesn't exist, `400 Bad Request` if any tag IDs are invalid
- Parameter: `habitId` (string) - The habit identifier
- **Behavior**: Removes existing associations and creates new ones based on provided tag IDs

#### Remove Tag from Habit
- **DELETE** `/habits/{habitId}/tags/{tagId}`
- Removes a specific tag association from a habit
- Response: `204 No Content` on success, `404 Not Found` if association doesn't exist
- Parameters:
  - `habitId` (string) - The habit identifier
  - `tagId` (string) - The tag identifier

### Example API Usage

#### Querying Habits with Advanced Parameters

##### Search for Habits by Name or Description
```http
GET /habits?q=exercise
```

##### Filter Habits by Type and Status
```http
GET /habits?type=Measurable&status=Ongoing
```

##### Sort Habits by Multiple Fields
```http
GET /habits?sort=name asc,createdAtUtc desc
```

##### Pagination Examples
```http
GET /habits?page=2&pageSize=5
```

```http
GET /habits?page=1&pageSize=20&sort=name asc
```

##### Complex Query with All Parameters
```http
GET /habits?q=daily&type=Measurable&status=Ongoing&sort=target.value desc,name asc&page=1&pageSize=15
```

##### Field Selection Examples
```http
GET /habits?fields=id,name,status
```

```http
GET /habits/{id}?fields=name,description,type
```

```http
GET /habits?q=exercise&fields=id,name,target&page=1&pageSize=5
```

#### Validation Error Example
```http
POST /habits
Content-Type: application/json

{
  "name": "Ex",  // Too short (min 3 characters)
  "type": "Binary",
  "target": {
    "value": -1,  // Invalid (must be > 0)
    "unit": "minutes"  // Invalid for Binary type
  },
  "frequency": {
    "type": "Daily",
    "timesPerPeriod": 0  // Invalid (must be > 0)
  }
}
```

**Response**: `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "extensions": {
    "requestId": "0HN7KQJQV2QQT:00000001",
    "errors": {
      "name": ["Habit name must be between 3 and 100 characters"],
      "target.value": ["Target value must be greater than 0"],
      "target.unit": ["Target unit is not compatible with the habit type"],
      "frequency.timesperperiod": ["Frequency must be greater than 0"]
    }
  }
}
```

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

#### Response Example (HabitWithTagsDto from GET /habits/{id})
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
  "lastCompletedAtUtc": null,
  "tags": ["Fitness", "Health", "Morning Routine"]
}
```

#### Paginated Response Example (PaginationResult<ExpandoObject> from GET /habits)
```json
{
  "items": [
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
        "current": 15
      },
      "createdAtUtc": "2024-11-25T12:00:00Z",
      "updatedAtUtc": null,
      "lastCompletedAtUtc": "2024-12-01T10:30:00Z"
    },
    {
      "id": "h_01JDQM8A9L3N4P5Q6R7S8T9U0V",
      "name": "Read Daily",
      "description": "Read for at least 20 minutes",
      "type": "Measurable",
      "frequency": {
        "type": "Daily",
        "timesPerPeriod": 1
      },
      "target": {
        "value": 20,
        "unit": "minutes"
      },
      "status": "Ongoing",
      "isArchived": false,
      "endDate": null,
      "milestone": {
        "target": 50,
        "current": 23
      },
      "createdAtUtc": "2024-11-20T15:00:00Z",
      "updatedAtUtc": "2024-11-25T16:00:00Z",
      "lastCompletedAtUtc": "2024-12-02T20:45:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

#### Field Selection Response Example (GET /habits?fields=id,name,status)
```json
{
  "items": [
    {
      "id": "h_01JDQM7Z8K2X3Y4W5V6U7T8S9R",
      "name": "Daily Exercise",
      "status": "Ongoing"
    },
    {
      "id": "h_01JDQM8A9L3N4P5Q6R7S8T9U0V",
      "name": "Read Daily",
      "status": "Ongoing"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true
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

#### Creating a Tag
```json
POST /tags
{
  "name": "Fitness",
  "description": "Health and fitness related habits"
}
```

**Response**: `201 Created` with `TagDto`
```json
{
  "id": "t_01JDQM8A9L3N4P5Q6R7S8T9U0V",
  "name": "Fitness",
  "description": "Health and fitness related habits",
  "createdAtUtc": "2024-12-03T12:00:00Z",
  "updatedAtUtc": null
}
```

#### Associating Tags with a Habit
```json
PUT /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R/tags
{
  "tagIds": [
    "t_01JDQM8A9L3N4P5Q6R7S8T9U0V",
    "t_01JDQM8B0M4O5P6Q7R8S9T0U1W",
    "t_01JDQM8C1N5P6Q7R8S9T0U1W2X"
  ]
}
```

**Response**: `200 OK`

#### Removing a Tag from a Habit
```json
DELETE /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R/tags/t_01JDQM8A9L3N4P5Q6R7S8T9U0V
```

**Response**: `204 No Content`

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