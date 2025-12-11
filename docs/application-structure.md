# DevHabit Application Structure

## **Project Overview**

DevHabit is an **ASP.NET Core Web API** application built with **.NET 9.0** that focuses on **habit tracking functionality**. It's designed with modern development practices, containerization support, and comprehensive observability.

## **Solution Structure**

```
DevHabit/
├── DevHabit.sln                    # Main solution file
├── DevHabit.Api/                   # Web API project (main application)
├── Directory.Build.props            # Global MSBuild properties & code analysis
├── Directory.Packages.props         # Centralized NuGet package management
├── docker-compose.yml              # Multi-container orchestration
└── CLAUDE.md                       # Project documentation/guidance
```

## **Technology Stack**

### **Core Framework**
- **.NET 9.0** with ASP.NET Core Web API
- **C# 13** with nullable reference types enabled
- **Implicit usings** for reduced boilerplate

### **Database & Persistence**
- **PostgreSQL 17.2** database with multi-schema organization
- **Entity Framework Core 9.0** with Npgsql provider
- **EFCore.NamingConventions** for snake_case database naming
- **Dual-context architecture**: ApplicationDbContext and ApplicationIdentityDbContext
- Database migrations with custom schema support (`dev_habit` and `identity` schemas)

### **Authentication & Authorization**
- **ASP.NET Core Identity** for user management and authentication
- **Entity Framework Identity stores** with PostgreSQL backend
- **Role-based authorization** infrastructure
- **External login providers** support (OAuth, social logins)
- **Security token management** for password resets and confirmations

### **Observability & Monitoring**
- **OpenTelemetry** integration for:
  - Distributed tracing (HTTP, ASP.NET Core, Npgsql)
  - Metrics collection (HTTP, ASP.NET Core, Runtime)
  - Logging with structured data
- **Aspire Dashboard** for development observability

### **Containerization**
- **Docker** support with multi-service composition
- **PostgreSQL container** for database
- **Aspire Dashboard container** for monitoring

## **Application Architecture**

### **DevHabit.Api Project Structure**
```
DevHabit.Api/
├── Program.cs                      # Application entry point & service configuration
├── DependencyInjection.cs         # Organized service registration with extension methods
├── Controllers/                    # API controllers
│   ├── HabitsController.cs        # Complete CRUD operations for habits with advanced querying
│   ├── TagsController.cs          # Complete CRUD operations for tags
│   ├── HabitTagsController.cs     # Habit-tag association management
│   └── UsersController.cs         # User retrieval operations
├── DTOs/                          # Data Transfer Objects organized by feature
│   ├── Common/                    # Shared DTOs and interfaces
│   │   ├── PaginationResult.cs    # Generic pagination wrapper with HATEOS links
│   │   ├── LinkDto.cs            # Hypermedia link representation
│   │   └── ILinksResponse.cs     # Interface for DTOs with HATEOS links
│   ├── Habits/                    # Habit-related DTOs
│   │   ├── HabitDto.cs           # Read models (HabitDto, HabitWithTagsDto)
│   │   ├── CreateHabitDto.cs     # Input model for habit creation
│   │   ├── UpdateHabitDto.cs     # Input model for habit updates
│   │   ├── HabitsQueryParameters.cs # Query parameters for filtering/sorting/pagination
│   │   ├── HabitMappings.cs      # Entity-DTO conversion extensions
│   │   └── HabitQueries.cs       # LINQ expression projections
│   ├── Tags/                      # Tag-related DTOs
│   │   ├── TagDto.cs             # Tag read model
│   │   ├── CreateTagDto.cs       # Tag creation input
│   │   ├── UpdateTagDto.cs       # Tag update input
│   │   ├── TagMappings.cs        # Tag entity-DTO conversions
│   │   └── TagQueries.cs         # Tag LINQ projections
│   ├── HabitTags/                 # Habit-tag association DTOs
│   │   └── UpsertHabitTagsDto.cs # Input for associating tags with habits
│   └── Users/                     # User-related DTOs
│       ├── UserDto.cs            # User read model
│       └── UserQueries.cs        # User LINQ projections
├── Entities/                       # Domain models
│   ├── Habit.cs                   # Habit entity with complex value objects
│   ├── Tag.cs                     # Tag entity for categorizing habits
│   ├── HabitTag.cs               # Junction entity for many-to-many relationships
│   └── User.cs                   # User entity for account management
├── Database/                       # Data access layer
│   ├── ApplicationDbContext.cs     # Main EF Core DbContext (dev_habit schema)
│   ├── ApplicationIdentityDbContext.cs # Identity EF Core DbContext (identity schema)
│   ├── Schemas.cs                 # Database schema constants
│   └── Configurations/            # EF Core entity configurations
│       ├── HabitConfiguration.cs
│       ├── TagConfiguration.cs
│       ├── HabitTagConfiguration.cs
│       └── UserConfiguration.cs
├── Services/                       # Application services and infrastructure
│   ├── Sorting/                   # Dynamic sorting infrastructure
│   │   ├── SortMappingProvider.cs
│   │   ├── SortMappingDefinition.cs
│   │   └── QueryableExtensions.cs
│   ├── DataShapingService.cs      # Field selection for response customization
│   ├── LinkService.cs             # HATEOS hypermedia link generation
│   └── CustomMediaTypeNames.cs   # Custom media type constants
├── Middleware/                     # Custom middleware components
│   ├── ValidationExceptionHandler.cs # FluentValidation exception handling
│   └── GlobalExceptionHandler.cs # General exception handling
├── Extensions/                     # Extension methods
│   └── DatabaseExtensions.cs     # Database setup and migration utilities
├── Migrations/                    # EF Core migrations (dual-schema)
│   ├── Application/               # Application schema migrations (dev_habit)
│   │   ├── 20251121221333_Add_Habits.cs
│   │   ├── 20251126222141_UpdateHabitModel.cs
│   │   ├── 20251203011050_Add_Tags.cs
│   │   ├── 20251203165332_Add_HabitTags.cs
│   │   └── 20251210201633_Add_Users.cs
│   └── Identity/                  # Identity schema migrations (identity)
│       └── 20251210225913_Add_Identity.cs
├── Properties/
│   └── launchSettings.json        # Development server profiles
├── appsettings.json               # Base configuration
├── appsettings.Development.json   # Development overrides (localhost PostgreSQL)
└── appsettings.Docker.json        # Docker-specific settings (container PostgreSQL)
```

## **Core Domain Model**

The application centers around **habit tracking** with a comprehensive domain model across multiple entities:

### **Habit Entity** (`Entities/Habit.cs`)
- **Core Properties**: Id (UUID v7 with "h_" prefix), Name, Description, Type, Status, IsArchived
- **Complex Value Objects**:
  - **Frequency**: Configurable timing (Daily, Weekly, Monthly) with TimesPerPeriod
  - **Target**: Goal setting with numeric value and unit
  - **Milestone**: Progress tracking with target and current values
- **Temporal Properties**: CreatedAtUtc, UpdatedAtUtc, LastCompletedAtUtc, EndDate
- **Enums**:
  - **HabitType**: Binary (yes/no) or Measurable habits
  - **HabitStatus**: Ongoing or Completed states
  - **FrequencyType**: Daily, Weekly, Monthly periods

### **Tag Entity** (`Entities/Tag.cs`)
- **Properties**: Id (UUID v7 with "t_" prefix), Name (unique), Description
- **Timestamps**: CreatedAtUtc, UpdatedAtUtc
- **Purpose**: Categorizing and organizing habits with many-to-many relationships

### **HabitTag Entity** (`Entities/HabitTag.cs`)
- **Junction Entity**: Manages many-to-many relationships between habits and tags
- **Composite Key**: (HabitId, TagId)
- **Properties**: HabitId, TagId, CreatedAtUtc
- **Cascade Behavior**: Automatic cleanup when parent entities are deleted

### **User Entity** (`Entities/User.cs`)
- **Properties**: Id (string), Email (unique), Name, IdentityId (unique for external identity integration)
- **Timestamps**: CreatedAtUtc, UpdatedAtUtc
- **Purpose**: User account management with future integration to ASP.NET Core Identity

## **Code Quality & Standards**

The project enforces **strict quality standards** via `Directory.Build.props`:

- **Warnings treated as errors** for compilation
- **SonarAnalyzer.CSharp** for static code analysis
- **Latest C# analysis level** with all analysis modes enabled
- **Code style enforcement** during build
- **Nullable reference types** project-wide

## **Package Management**

Uses **Central Package Management** via `Directory.Packages.props`:
- All package versions centrally managed
- No version conflicts between projects
- Easy version upgrades across solution

### **Key Dependencies**
- **FluentValidation.DependencyInjectionExtensions** (v12.1.1): Comprehensive input validation
- **System.Linq.Dynamic.Core** (v1.7.1): Dynamic LINQ for sorting functionality
- **Microsoft.AspNetCore.JsonPatch** (v9.0.11): JSON Patch support for partial updates
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson** (v9.0.11): Newtonsoft.Json integration for JSON Patch
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (v9.0.11): Identity with Entity Framework stores
- **EFCore.NamingConventions** (v9.0.0): Snake case naming for PostgreSQL
- **Npgsql.EntityFrameworkCore.PostgreSQL** (v9.0.4): PostgreSQL database provider
- **OpenTelemetry packages**: Comprehensive observability and monitoring
- **SonarAnalyzer.CSharp** (v10.4.0): Static code analysis

## **Development & Deployment**

### **Local Development**
- `dotnet run --project DevHabit.Api` → runs on https://localhost:5001
- `dotnet watch` → hot reload support
- **OpenAPI/Swagger** available in development mode

### **Containerized Deployment**
Via `docker-compose.yml`:
- **API container** (ports 9000/9001)
- **PostgreSQL container** (port 5492)
- **Aspire Dashboard** (port 18888) for observability
- **Volume persistence** for PostgreSQL data

## **Database Configuration**

### **Dual-Schema Architecture**
- **PostgreSQL 17.2** with multi-schema organization
- **`dev_habit` schema**: Application domain entities (habits, tags, users)
- **`identity` schema**: ASP.NET Core Identity tables (authentication/authorization)
- **Snake case naming convention** for all database objects
- **Dual DbContext architecture**:
  - `ApplicationDbContext`: Main application data
  - `ApplicationIdentityDbContext`: Identity and authentication data

### **Migration Management**
The project maintains separate migration histories for each schema:

#### **Application Schema Migrations** (`dev_habit`)
- **Migration files**: `Migrations/Application/` directory
- **History table**: `dev_habit.__EFMigrationsHistory`
- **Current migrations** (6 applied):
  1. `20251121221333_Add_Habits` - Initial Habits table with full entity structure
  2. `20251126222141_UpdateHabitModel` - Column rename: `frequency_times_per_period`
  3. `20251203011050_Add_Tags` - Tags table with unique name constraint
  4. `20251203165332_Add_HabitTags` - Junction table for habit-tag relationships
  5. `20251210201633_Add_Users` - Users table with unique email/identity constraints

#### **Identity Schema Migrations** (`identity`)
- **Migration files**: `Migrations/Identity/` directory
- **History table**: `identity.__EFMigrationsHistory`
- **Current migrations** (1 applied):
  1. `20251210225913_Add_Identity` - Complete ASP.NET Core Identity table structure

#### **Migration Commands**
```bash
# Application schema
dotnet ef migrations add {Name} --context ApplicationDbContext
dotnet ef database update --context ApplicationDbContext

# Identity schema
dotnet ef migrations add {Name} --context ApplicationIdentityDbContext
dotnet ef database update --context ApplicationIdentityDbContext
```

#### **Automatic Migration Application**
- **Development mode**: Both contexts automatically migrated via `ApplyMigrationsAsync()`
- **Production**: Manual migration application recommended for safety

## **Service Registration & Dependency Injection**

The application uses a **modular approach** to service registration through organized extension methods in `DependencyInjection.cs`:

### **Service Registration Categories**
```csharp
builder
    .AddControllers()           // MVC controllers, JSON/XML serialization, OpenAPI
    .AddErrorHandling()         // Problem details, exception handlers
    .AddDatabase()              // Dual EF contexts, PostgreSQL configuration
    .AddObservability()         // OpenTelemetry tracing, metrics, logging
    .AddApplicationServices()   // FluentValidation, sorting, data shaping, HATEOS
    .AddAuthenticationServices(); // ASP.NET Core Identity with EF stores
```

### **Extension Method Details**

#### **AddControllers()**
- **MVC Configuration**: Controllers with content negotiation (`ReturnHttpNotAcceptable = true`)
- **JSON Serialization**: Newtonsoft.Json with camelCase properties for JSON Patch support
- **XML Support**: XML serializers for additional content types
- **Custom Media Types**: HATEOS JSON media type (`application/vnd.dev-habit.hateoas+json`)
- **OpenAPI Integration**: Swagger/OpenAPI documentation generation

#### **AddErrorHandling()**
- **Problem Details**: RFC 7807 compliant error responses with request correlation
- **Exception Handlers**: Validation and global exception handling middleware
- **Structured Errors**: Field-level validation error grouping

#### **AddDatabase()**
- **Dual Contexts**: ApplicationDbContext and ApplicationIdentityDbContext registration
- **PostgreSQL Configuration**: Connection strings with schema-specific migration history
- **Snake Case Naming**: Automatic snake_case conversion for PostgreSQL compatibility
- **Migration History**: Separate history tables for application and Identity schemas

#### **AddObservability()**
- **OpenTelemetry Configuration**: Distributed tracing with HTTP, ASP.NET Core, and Npgsql instrumentation
- **Metrics Collection**: HTTP, ASP.NET Core, and runtime metrics
- **OTLP Export**: OpenTelemetry Protocol export for observability platforms
- **Structured Logging**: OpenTelemetry logging with scopes and formatted messages

#### **AddApplicationServices()**
- **FluentValidation**: Automatic validator discovery and registration from assembly
- **Dynamic Sorting**: SortMappingProvider with type-safe field mapping definitions
- **Data Shaping**: Field selection service for response customization
- **HATEOS Links**: LinkService with HttpContextAccessor for URL generation

#### **AddAuthenticationServices()**
- **ASP.NET Core Identity**: IdentityUser and IdentityRole configuration
- **Entity Framework Stores**: Identity data persistence through ApplicationIdentityDbContext
- **Future Integration**: Foundation for authentication, authorization, and user management

## **Current API Endpoints**

The API provides comprehensive CRUD operations across multiple resource controllers:

### **HabitsController** - Complete habit management with advanced querying
- **GET /habits** - Paginated collection with advanced filtering and sorting
  - **Query Parameters**: `q` (search), `type`, `status`, `sort`, `fields`, `page`, `pageSize`
  - **Content Negotiation**: HATEOS links with `Accept: application/vnd.dev-habit.hateoas+json`
  - **Response**: `PaginationResult<ExpandoObject>` with metadata and navigation links
- **GET /habits/{id}** - Single habit by ID with optional field selection
  - **Query Parameters**: `fields` (comma-separated field names)
  - **Response**: `HabitWithTagsDto` including associated tags array
- **POST /habits** - Create new habit with comprehensive validation
  - **Request**: `CreateHabitDto` with FluentValidation rules
  - **Response**: `201 Created` with Location header
- **PUT /habits/{id}** - Complete habit replacement
  - **Request**: `UpdateHabitDto`
  - **Response**: `204 No Content`
- **PATCH /habits/{id}** - Partial updates via JSON Patch
  - **Content-Type**: `application/json-patch+json`
  - **Response**: `204 No Content`
- **DELETE /habits/{id}** - Remove habit and all tag associations
  - **Response**: `204 No Content`

### **TagsController** - Tag management for categorization
- **GET /tags** - Collection of all available tags
  - **Response**: `TagsCollectionDto`
- **GET /tags/{id}** - Single tag by ID
  - **Response**: `TagDto`
- **POST /tags** - Create new tag
  - **Request**: `CreateTagDto`
  - **Response**: `201 Created` with Location header
  - **Validation**: Unique name constraint (409 Conflict for duplicates)
- **PUT /tags/{id}** - Update existing tag
  - **Request**: `UpdateTagDto`
  - **Response**: `204 No Content`
- **DELETE /tags/{id}** - Remove tag and all habit associations
  - **Response**: `204 No Content`

### **HabitTagsController** - Many-to-many relationship management
- **PUT /habits/{habitId}/tags** - Replace all tag associations for a habit
  - **Request**: `UpsertHabitTagsDto` with array of tag IDs
  - **Response**: `200 OK` or `204 No Content`
  - **Validation**: Ensures all tag IDs exist (400 Bad Request for invalid IDs)
- **DELETE /habits/{habitId}/tags/{tagId}** - Remove specific tag association
  - **Response**: `204 No Content`

### **UsersController** - User account operations
- **GET /users/{id}** - Single user by ID
  - **Response**: `UserDto`

### **API Features**

#### **Advanced Querying (Habits)**
- **Search**: Full-text search across name and description (`q` parameter)
- **Filtering**: By type (Binary/Measurable) and status (Ongoing/Completed)
- **Dynamic Sorting**: Multi-field sorting with direction control
  - Supported fields: `name`, `description`, `type`, `status`, `endDate`
  - Nested fields: `frequency.type`, `frequency.timesPerPeriod`, `target.value`, `target.unit`
  - Timestamps: `createdAtUtc`, `updatedAtUtc`, `lastCompletedAtUtc`
- **Field Selection**: Custom response shaping via `fields` parameter
- **Pagination**: Offset-based with configurable page size (default: 10 items per page)

#### **HATEOS (Hypermedia as the Engine of Application State)**
- **Content Negotiation**: Links included with `Accept: application/vnd.dev-habit.hateoas+json`
- **Navigation Links**: Collection navigation (self, next-page, previous-page)
- **Action Links**: Resource operations (create, update, partial-update, delete)
- **Cross-Resource Links**: Related operations (upsert-tags for habits)

#### **Comprehensive Validation**
- **FluentValidation**: Input validation with detailed error responses
- **Business Rules**: Unit compatibility validation, future date constraints
- **Problem Details**: Structured error responses following RFC 7807
- **Field-Level Errors**: Grouped validation errors by property name

#### **Sample API Usage**
```bash
# Advanced habit querying
GET /habits?q=exercise&type=Measurable&status=Ongoing&sort=target.value desc,name asc&page=1&pageSize=15

# Field selection for bandwidth optimization
GET /habits?fields=id,name,status,target

# HATEOS-enabled responses
GET /habits
Accept: application/vnd.dev-habit.hateoas+json

# JSON Patch partial updates
PATCH /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R
Content-Type: application/json-patch+json
[{"op": "replace", "path": "/name", "value": "Updated Name"}]

# Tag association management
PUT /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R/tags
{"tagIds": ["t_01JDQM8A9L3N4P5Q6R7S8T9U0V", "t_01JDQM8B0M4O5P6Q7R8S9T0U1W"]}
```

## **Key Features & Architectural Patterns**

### **Core Development Practices**
1. **Modern C# practices** with .NET 9.0 and latest language features
2. **Domain-Driven Design** with rich domain entities and value objects
3. **Clean Architecture** with clear separation of concerns
4. **Strict code quality** enforcement with warnings-as-errors and SonarAnalyzer
5. **Centralized package management** for consistent dependency versions

### **Advanced API Features**
6. **HATEOS implementation** with hypermedia-driven navigation and content negotiation
7. **Dynamic sorting system** with type-safe field mapping and validation
8. **Data shaping service** for field selection and bandwidth optimization
9. **Comprehensive pagination** with metadata and navigation links
10. **FluentValidation framework** with business rule enforcement and structured error responses
11. **JSON Patch support** for efficient partial resource updates

### **Database & Persistence**
12. **Dual-schema database architecture** separating application and Identity data
13. **Entity Framework Core** with PostgreSQL and snake_case naming conventions
14. **Separate migration contexts** for application and Identity schemas
15. **Complex value objects** as owned entities (Frequency, Target, Milestone)
16. **UUID v7 identifiers** for time-ordered, efficient database indexing

### **Authentication & Authorization Infrastructure**
17. **ASP.NET Core Identity** integration with Entity Framework stores
18. **Role-based authorization** foundation for future implementation
19. **External login provider** support for OAuth and social authentication
20. **Security token management** for password resets and email confirmations

### **Observability & Monitoring**
21. **Comprehensive OpenTelemetry** integration for distributed tracing and metrics
22. **Aspire Dashboard** for development-time observability
23. **Structured logging** with correlation IDs and request tracking

### **Containerization & Deployment**
24. **Container-first deployment** strategy with Docker Compose
25. **Multi-service orchestration** (API, PostgreSQL, Aspire Dashboard)
26. **HTTPS support** with development certificates and secure communication
27. **Environment-specific configuration** for development and container deployments

### **Service Architecture**
28. **Modular dependency injection** with organized extension methods
29. **Custom middleware pipeline** for validation and global exception handling
30. **Service layer abstraction** with repository pattern and LINQ projections
31. **DTO mapping extensions** for clean entity-DTO conversions

The application demonstrates **enterprise-grade architecture** suitable for **scalability**, **maintainability**, and **production deployment** with modern DevOps and development practices. The foundation supports future enhancements including real-time features, advanced analytics, and mobile API consumption.