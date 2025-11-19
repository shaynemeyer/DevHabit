# DevHabit

A modern ASP.NET Core Web API built with .NET 9.0, designed with strict code quality standards and best practices.

## Features

- **ASP.NET Core Web API** with .NET 9.0
- **OpenAPI/Swagger** documentation
- **Strict Code Analysis** with SonarAnalyzer
- **Central Package Management** for consistent dependency versions
- **Nullable Reference Types** enabled
- **Warnings as Errors** for high code quality

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- IDE of your choice (Visual Studio, VS Code, Rider)

## Getting Started

### 1. Clone the Repository
```bash
git clone <repository-url>
cd DevHabit
```

### 2. Restore Dependencies
```bash
dotnet restore
```

### 3. Build the Project
```bash
dotnet build
```

### 4. Run the Application
```bash
dotnet run --project DevHabit.Api
```

The API will be available at:
- HTTPS: `https://localhost:5001`
- HTTP: `http://localhost:5000`

## Development

### Running with Hot Reload
For development with automatic restart on file changes:
```bash
dotnet watch --project DevHabit.Api
```

### API Documentation
When running in development mode, OpenAPI documentation is available at:
- Swagger UI: `https://localhost:5001/swagger` (if Swagger UI is configured)
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
├── DevHabit.Api/              # Web API project
│   ├── Controllers/           # API controllers
│   ├── Properties/            # Launch settings and configuration
│   ├── appsettings.json       # Application configuration
│   ├── Program.cs            # Application entry point
│   └── DevHabit.Api.csproj   # Project file
├── Directory.Build.props      # Global MSBuild properties
├── Directory.Packages.props   # Central package version management
├── DevHabit.sln              # Solution file
└── README.md                 # This file
```

## Available Commands

| Command | Description |
|---------|-------------|
| `dotnet build` | Build the solution |
| `dotnet run --project DevHabit.Api` | Run the API |
| `dotnet watch --project DevHabit.Api` | Run with hot reload |
| `dotnet test` | Run tests (when test projects are added) |
| `dotnet restore` | Restore NuGet packages |

## Configuration

### Development Settings
- `appsettings.Development.json` - Development-specific configuration
- `Properties/launchSettings.json` - Development server profiles

### Environment Variables
The application uses standard ASP.NET Core configuration patterns. Set environment-specific values using:
- Environment variables
- User secrets (for development)
- Configuration files

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