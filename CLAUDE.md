# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DevHabit is an ASP.NET Core Web API project built with .NET 9.0. The project uses modern C# development practices with strict code analysis and centralized package management.

## 📚 Documentation

This project has comprehensive documentation organized in the `/docs` folder:

- **[Application Structure](docs/application-structure.md)**: Complete architectural overview and technology stack
- **[Development Setup Guide](docs/development-setup.md)**: Local development environment setup and workflow
- **[Docker Setup Guide](docs/docker-setup.md)**: Containerized development with Docker Compose
- **[Database Migrations Guide](docs/database-migrations.md)**: EF Core migrations and schema management
- **[Authentication Setup Guide](docs/authentication-setup.md)**: ASP.NET Core Identity configuration and integration
- **[API Reference](docs/api-reference.md)**: Complete API endpoint documentation with examples

## Quick Start

For detailed setup instructions, see the [Development Setup Guide](docs/development-setup.md) or [Docker Setup Guide](docs/docker-setup.md).

### Local Development
```bash
dotnet run --project DevHabit.Api
# Access at: https://localhost:5001
```

### Docker (Recommended)
```bash
./generate-dev-cert.sh  # First time only
docker-compose up --build
# Access at: https://localhost:9001
```

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
  - **AuthController**: User registration and authentication endpoints
  - **HabitsController**: Complete CRUD operations for habit management including tagging support
  - **TagsController**: Complete CRUD operations for tag management
  - **HabitTagsController**: Association management between habits and tags
  - **UsersController**: User retrieval operations
- **Entities/**: Domain entities including complex value objects
  - **Habit**: Core domain entity with complex value objects (Frequency, Target, Milestone)
  - **Tag**: Tag entity for categorizing habits (Id, Name, Description, timestamps)
  - **HabitTag**: Junction entity for many-to-many habit-tag relationships
  - **User**: User entity for managing user accounts (Id, Email, Name, IdentityId, timestamps)
- **DTOs/**: Data Transfer Objects organized by feature
  - **DTOs/Common/**:
    - **PaginationResult<T>**: Generic pagination wrapper with metadata (page, pageSize, totalCount, etc.) and HATEOS links support
    - **ICollectionResponse<T>**: Base interface for collection responses
    - **LinkDto**: Hypermedia link representation with href, rel, and method properties
    - **ILinksResponse**: Interface for DTOs that include HATEOS links
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
  - **DTOs/Users/**:
    - **UserDto**: Read model for user data
    - **UserQueries**: LINQ expression projections for user queries
    - **UserMappings**: Extension methods for converting RegisterUserDto to User entities
  - **DTOs/Auth/**:
    - **RegisterUserDto**: Input model for user registration with email, name, password, and confirmation
- **Database/**: Entity Framework Core configuration
  - **ApplicationDbContext**: Main database context with Habits, Tags, HabitTags, and Users DbSets (uses `dev_habit` schema)
  - **ApplicationIdentityDbContext**: ASP.NET Core Identity database context for authentication and authorization (uses `identity` schema)
  - **Schemas**: Database schema constants for organizing tables (`dev_habit` for application data, `identity` for authentication)
  - **Configurations/**: Entity configurations using Fluent API
  - **Migrations/**: EF Core migration files for both application and identity schemas
- **Services/**: Application services and infrastructure
  - **Services/Sorting/**: Dynamic sorting infrastructure with type-safe field mapping
  - **DataShapingService**: Field selection service for response customization
  - **LinkService**: HATEOS hypermedia link generation service for API navigation
  - **CustomMediaTypeNames**: Constants for custom media types including HATEOS content negotiation
- **Middleware/**: Custom middleware components
  - **ValidationExceptionHandler**: FluentValidation exception handling
  - **GlobalExceptionHandler**: General exception handling middleware
- **Extensions/**: Extension methods including database setup
- **DependencyInjection.cs**: Organized service registration using extension methods
- **Program.cs**: Clean application entry point using extension methods for configuration with authentication and authorization middleware properly configured

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
- **AddDatabase()**: Configures Entity Framework Core with PostgreSQL, snake case naming conventions, and dual-schema organization (application and identity)
- **AddObservability()**: Registers OpenTelemetry for distributed tracing, metrics collection, and observability with OTLP export
- **AddApplicationServices()**: Registers application-specific services including FluentValidation, dynamic sorting, data shaping, and HATEOS link generation services
- **AddAuthenticationServices()**: Configures ASP.NET Core Identity with Entity Framework stores for user authentication and authorization

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

### HATEOS (Hypermedia as the Engine of Application State)
The project implements HATEOS to provide hypermedia links that guide API consumers through available actions and navigation:
- **LinkService**: Central service for generating hypermedia links using ASP.NET Core's `LinkGenerator`
- **LinkDto**: Standard representation of hypermedia links containing:
  - `Href`: The URL for the linked resource or action
  - `Rel`: The relationship type (e.g., "self", "next-page", "update", "delete")
  - `Method`: HTTP method for the linked action (GET, POST, PUT, PATCH, DELETE)
- **ILinksResponse**: Interface implemented by DTOs that include hypermedia links
- **CustomMediaTypeNames**: Defines custom media types for API responses:
  - `Application.HateoasJson`: `"application/vnd.dev-habit.hateoas+json"` - Custom media type for HATEOS-enabled responses
- **Features:**
  - Automatic URL generation based on route configuration
  - Support for collection navigation (self, next-page, previous-page)
  - Resource action links (create, update, partial-update, delete)
  - Cross-resource relationship links (e.g., upsert-tags for habits)
  - Consistent link structure across all API responses
  - **Content Negotiation**: HATEOS links are conditionally included based on the `Accept` header
    - Links are included when `Accept: application/vnd.dev-habit.hateoas+json` header is present
    - Standard responses without links when using default `Accept: application/json`
- **Implementation**: Links are conditionally included in `HabitDto`, `PaginationResult<T>`, and shaped responses based on Accept header
- **Benefits**: Enables discoverable APIs, reduces client coupling, provides clear navigation paths, and allows clients to control response verbosity

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
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (v9.0.11): ASP.NET Core Identity with Entity Framework integration
- **EFCore.NamingConventions** (v9.0.0): Snake case naming convention for PostgreSQL
- **Npgsql.EntityFrameworkCore.PostgreSQL** (v9.0.4): PostgreSQL database provider
- **OpenTelemetry packages**: Comprehensive observability and monitoring

### Code Analysis Configuration
The project uses comprehensive code analysis:
- All analysis modes enabled
- Code style enforcement during build
- SonarAnalyzer for additional quality checks

### ASP.NET Core Identity Infrastructure
The project implements a fully functional ASP.NET Core Identity system for user authentication and authorization:
- **ApplicationIdentityDbContext**: Separate database context for Identity-related tables using Entity Framework Core
- **Database Schema Separation**: Identity tables are isolated in the `identity` schema, while application data resides in the `dev_habit` schema
- **Standard Identity Tables**: Complete ASP.NET Core Identity table structure including:
  - `asp_net_users`: Core user accounts with username, email, password hash, and security features
  - `asp_net_roles`: Role definitions for authorization
  - `asp_net_user_roles`: Many-to-many mapping between users and roles
  - `asp_net_user_claims`: User-specific claims for fine-grained authorization
  - `asp_net_role_claims`: Role-based claims for group permissions
  - `asp_net_user_logins`: External login provider mappings (OAuth, social logins)
  - `asp_net_user_tokens`: Security tokens for password resets, email confirmations, etc.
- **Snake Case Naming**: All Identity tables follow PostgreSQL conventions with snake_case column names
- **Dual Migration System**: Separate migration histories for application schema and Identity schema
- **Service Registration**: Identity services configured through `AddAuthenticationServices()` extension method
- **Entity Framework Integration**: Uses `IdentityDbContext` with Entity Framework stores for data persistence
- **Middleware Pipeline**: Authentication and authorization middleware properly configured in Program.cs:
  - `UseAuthentication()`: Enables authentication processing for incoming requests
  - `UseAuthorization()`: Enables authorization checks based on policies and roles
  - **Order**: Middleware is correctly ordered after exception handling but before controller mapping
- **Identity Integration**: Active integration between Identity users and application-specific User entities via `IdentityId` property
- **Transactional User Creation**: User registration creates both Identity and application user records atomically using database transactions
- **AuthController**: Fully functional authentication controller with user registration endpoint

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

### User Entity
Domain entity for managing user accounts and identity integration:
- **Id**: Unique identifier using standard string format (max 500 characters)
- **Email**: Required user email address (max 300 characters, unique constraint)
- **Name**: Required user display name (max 100 characters)
- **IdentityId**: External identity provider identifier (max 500 characters, unique constraint)
- **CreatedAtUtc**: Timestamp of user account creation
- **UpdatedAtUtc**: Last modification timestamp

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
- **HATEOS Implementation**: Hypermedia-driven API design with automatic link generation for navigation and actions

## Container Deployment

See the [Docker Setup Guide](docs/docker-setup.md) for comprehensive containerization instructions including:
- Multi-service Docker Compose setup
- HTTPS certificate configuration
- Database container management
- Development workflow with containers

## Development Workflow

See the [Development Setup Guide](docs/development-setup.md) for comprehensive development instructions including:
- Local development environment setup
- Adding new features and following established patterns
- Code quality standards and development patterns
- Package management and configuration

For database-related development:
- See [Database Migrations Guide](docs/database-migrations.md) for EF Core migrations
- See [Authentication Setup Guide](docs/authentication-setup.md) for Identity system details

### Recent Bug Fixes and Resolutions
- **Authentication Middleware Issue** (December 2025): Fixed missing `UseAuthentication()` and `UseAuthorization()` middleware in Program.cs that was preventing auth endpoints from being discoverable
- **Property Name Typos** (December 2025): Corrected `IndentityId` → `IdentityId` throughout the codebase:
  - User entity property name and documentation
  - UserConfiguration Entity Framework mapping
  - AuthController property assignment
  - Database migration to rename column and index
- **File-Scoped Namespace**: Updated migration files to use file-scoped namespace syntax to comply with code analysis rules

## API Endpoints

See the [API Reference](docs/api-reference.md) for comprehensive API documentation including:
- **Authentication API**: User registration and authentication endpoints
- **Habits API**: Complete CRUD operations for habit management with advanced querying
- **Tags API**: Tag management for categorizing habits
- **Users API**: User profile management
- **Habit-Tag Association API**: Managing relationships between habits and tags
- **Usage Examples**: Complete request/response examples with validation scenarios
- **Base URLs**: Development and container endpoint information

### Quick API Reference
- **Local Development**: `https://localhost:5001`
- **Docker Container**: `https://localhost:9001`
- **OpenAPI Spec**: `/openapi/v1.json`
- **Dashboard**: `http://localhost:18888` (container only)

## HTTP Client Testing
The project includes DevHabit.Api.http for testing API endpoints directly in compatible editors. Update endpoint URLs based on your deployment method (container vs. local).