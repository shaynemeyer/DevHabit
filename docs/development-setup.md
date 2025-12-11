# DevHabit Development Setup Guide

This guide covers everything you need to set up and work with the DevHabit application in a development environment.

## Prerequisites

- **.NET 9.0 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
- **PostgreSQL 17.2** (or use Docker container - see [Docker Setup Guide](docker-setup.md))
- **Git** for version control
- **IDE/Editor**: Visual Studio, VS Code, or JetBrains Rider

## Development Commands

### Building and Running (Local Development)
- `dotnet build` - Build the entire solution
- `dotnet build --configuration Release` - Build for production
- `dotnet run --project DevHabit.Api` - Run the API locally (default: https://localhost:5001)
- `dotnet watch --project DevHabit.Api` - Run with hot reload for development

### Testing
- `dotnet test` - Run all tests (no test projects exist yet)
- `dotnet test --configuration Release` - Run tests in release mode

### Package Management
- `dotnet restore` - Restore NuGet packages
- `dotnet list package` - List installed packages
- `dotnet add DevHabit.Api package [PackageName]` - Add a package (update Directory.Packages.props for version)

## Project Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd DevHabit
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Database Setup
See the [Database Migrations Guide](database-migrations.md) for detailed database setup instructions.

### 4. Configuration
- **appsettings.json**: Base application settings
- **appsettings.Development.json**: Development-specific overrides
- Update connection strings in appsettings files as needed

### 5. Run the Application
```bash
# Run with hot reload for development
dotnet watch --project DevHabit.Api

# Or run normally
dotnet run --project DevHabit.Api
```

The API will be available at:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

## Development Workflow

### Adding New Features
Follow the established patterns when adding new functionality:

1. **Entities**: Add domain entities to the `Entities/` folder with proper value objects
2. **DTOs**: Create feature-specific DTOs in `DTOs/{FeatureName}/` folders
3. **Controllers**: Add controllers to `Controllers/` following REST conventions
4. **Database**: Configure entity mappings in `Database/Configurations/`
5. **Mappings**: Create extension methods for entity-DTO conversions
6. **Queries**: Use expression projections for efficient database queries

### Code Quality Standards
The project enforces strict code quality standards:
- **Warnings as Errors**: All warnings are treated as compilation errors
- **SonarAnalyzer**: Static code analysis with SonarAnalyzer.CSharp
- **Latest Analysis Level**: Uses the latest C# analysis features
- **Nullable Reference Types**: Enabled project-wide
- **Implicit Usings**: Enabled to reduce boilerplate

### Development Patterns
- **Entity Configuration**: Use `IEntityTypeConfiguration<T>` for Fluent API configurations
- **Value Objects**: Model complex properties as owned entities (e.g., `Frequency`, `Target`)
- **DTO Projections**: Use `Expression<Func<T, TResult>>` for efficient database queries
- **Mapping Extensions**: Create `ToDto()`, `ToEntity()`, and `UpdateFromDto()` extension methods
- **Controller Actions**: Follow REST conventions with proper HTTP status codes
- **Progress Tracking Separation**: Milestone progress (`Current`) is intentionally separated from general updates to preserve progress tracking integrity
- **Service Registration**: Use extension methods in `DependencyInjection.cs` to organize service configuration by concern (controllers, database, observability, etc.) and maintain a clean `Program.cs`
- **Middleware Order**: Ensure authentication and authorization middleware are properly ordered in the pipeline after exception handling but before controller mapping

## Central Package Management

All package versions are managed centrally through Directory.Packages.props. When adding new packages:
1. Add PackageReference in the project file without version
2. Define the version in Directory.Packages.props

### Key Package Dependencies
- **FluentValidation.DependencyInjectionExtensions** (v12.1.1): Comprehensive input validation framework
- **System.Linq.Dynamic.Core** (v1.7.1): Dynamic LINQ expression parsing for sorting functionality
- **Microsoft.AspNetCore.JsonPatch** (v9.0.11): JSON Patch support for partial updates
- **Microsoft.AspNetCore.Mvc.NewtonsoftJson** (v9.0.11): Newtonsoft.Json integration for JSON Patch
- **Microsoft.AspNetCore.Identity.EntityFrameworkCore** (v9.0.11): ASP.NET Core Identity with Entity Framework integration
- **EFCore.NamingConventions** (v9.0.0): Snake case naming convention for PostgreSQL
- **Npgsql.EntityFrameworkCore.PostgreSQL** (v9.0.4): PostgreSQL database provider
- **OpenTelemetry packages**: Comprehensive observability and monitoring

## Configuration and Settings

### appsettings.json Structure
```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5492;Database=dev_habit;Username=postgres;Password=postgres"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Environment-Specific Configuration
- **Development**: Uses localhost PostgreSQL connection
- **Docker**: Uses container networking to `devhabit.postgres`
- **Production**: Configure secure connection strings

## Debugging and Troubleshooting

### Common Issues

#### Build Errors
- Ensure all package references have versions defined in Directory.Packages.props
- Check for nullable reference type warnings
- Verify code analysis rules compliance

#### Database Connection Issues
- Verify PostgreSQL is running
- Check connection string configuration
- Ensure database migrations are applied

#### Authentication Issues
- Verify `UseAuthentication()` and `UseAuthorization()` middleware are configured
- Check middleware order in Program.cs
- Ensure Identity services are registered

### Logging and Observability
The application includes comprehensive logging and observability:
- **OpenTelemetry** for distributed tracing and metrics
- **Structured logging** with configurable levels
- **Request/response logging** for debugging API issues

### Testing API Endpoints
- Use the included `DevHabit.Api.http` file for testing endpoints
- Access OpenAPI documentation at `/openapi/v1.json`
- Use tools like Postman, curl, or HTTP clients built into IDEs

## IDE Configuration

### Visual Studio Code
Recommended extensions:
- C# Dev Kit
- REST Client (for .http files)
- GitLens
- Docker

### Visual Studio
- Ensure latest version with .NET 9.0 support
- Enable nullable reference type warnings
- Configure code analysis rules

### JetBrains Rider
- Enable .NET 9.0 support
- Configure code inspections for nullable reference types
- Set up database tool integration for PostgreSQL

## Performance and Optimization

### Database Performance
- Use LINQ projections with `Expression<Func<T, TResult>>`
- Implement efficient pagination with Skip/Take
- Utilize database indexes for frequently queried fields

### API Performance
- Enable response caching where appropriate
- Use field selection to reduce payload size
- Implement proper HTTP status codes and headers

### Memory Management
- Dispose database contexts properly
- Use async/await patterns consistently
- Monitor memory usage during development

## Next Steps

After completing the development setup:
1. Review the [API Reference](api-reference.md) for endpoint documentation
2. Check the [Authentication Setup Guide](authentication-setup.md) for auth configuration
3. See the [Docker Setup Guide](docker-setup.md) for containerized development
4. Explore the [Database Migrations Guide](database-migrations.md) for schema management

For architectural details, see the main [Application Structure](application-structure.md) documentation.