# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DevHabit is an ASP.NET Core Web API project built with .NET 9.0. The project uses modern C# development practices with strict code analysis and centralized package management.

## Development Commands

### Building and Running
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

## Development Workflow

1. **Adding New Features**: Create controllers in the Controllers/ folder following the existing pattern
2. **Models**: Add domain models at the root level or in appropriate subfolders
3. **Configuration**: Update appsettings files for new configuration requirements
4. **Dependencies**: Add new PackageReference entries and update Directory.Packages.props

## HTTP Client Testing
The project includes DevHabit.Api.http for testing API endpoints directly in compatible editors.