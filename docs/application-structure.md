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
- **PostgreSQL 17.2** database
- **Entity Framework Core 9.0** with Npgsql provider
- **EFCore.NamingConventions** for snake_case database naming
- Database migrations with custom schema support

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
├── Controllers/                    # API controllers
│   └── WeatherForecastController.cs  # Example controller (template)
├── Entities/                       # Domain models
│   └── Habit.cs                   # Main habit entity with enums
├── Database/                       # Data access layer
│   ├── ApplicationDbContext.cs     # EF Core DbContext
│   ├── Schemas.cs                 # Database schema definitions
│   ├── Configurations/            # EF Core entity configurations
│   │   └── HabitConfiguration.cs
│   └── Extensions/
│       └── DatabaseExtensions.cs  # Database setup extensions
├── Migrations/Application/         # EF Core migrations
├── Properties/
│   └── launchSettings.json        # Development server profiles
├── appsettings.json               # Base configuration
├── appsettings.Development.json   # Development overrides
└── appsettings.Docker.json        # Docker-specific settings
```

## **Core Domain Model**

The application centers around **habit tracking** with a rich domain model in `Entities/Habit.cs`:

- **Habit entity** with properties for tracking habits
- **HabitType enum**: Binary (yes/no) or Measurable habits
- **Frequency class**: Configurable timing (daily, weekly, monthly)
- **Target class**: Goal setting with value and unit
- **Milestone class**: Progress tracking
- **HabitStatus enum**: Ongoing or Completed states

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

- **PostgreSQL connection** configured in `Program.cs`
- **Snake case naming convention** for database objects
- **Custom migration history table** with application schema
- **Automatic migrations** applied in development mode

## **Key Features**

1. **Modern C# practices** with latest language features
2. **Comprehensive observability** with OpenTelemetry
3. **Container-first deployment** strategy
4. **Strict code quality** enforcement
5. **Centralized configuration** management
6. **Rich domain modeling** for habit tracking

The application is well-structured for **scalability**, **maintainability**, and **production deployment** with modern DevOps practices.