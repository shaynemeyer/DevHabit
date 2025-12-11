# DevHabit Database Migrations Guide

This guide covers Entity Framework Core migrations, database schema management, and the dual-context architecture used in DevHabit.

## Database Architecture Overview

DevHabit uses a **dual-schema approach** with two separate Entity Framework contexts:

- **ApplicationDbContext**: Application data in the `dev_habit` schema
- **ApplicationIdentityDbContext**: ASP.NET Core Identity data in the `identity` schema

Both contexts share the same PostgreSQL database but maintain separate migration histories and schemas.

## Schema Organization

### dev_habit Schema (Application Data)
Contains business domain entities:
- **habits**: Core habit tracking data
- **tags**: Tag definitions for categorizing habits
- **habit_tags**: Junction table for habit-tag relationships
- **users**: Application user profiles

### identity Schema (Authentication Data)
Contains ASP.NET Core Identity tables:
- **asp_net_users**: User accounts and credentials
- **asp_net_roles**: Role definitions
- **asp_net_user_roles**: User-role assignments
- **asp_net_user_claims**: User-specific claims
- **asp_net_role_claims**: Role-based claims
- **asp_net_user_logins**: External login providers
- **asp_net_user_tokens**: Security tokens

## Migration Commands

### Application Schema Commands

#### Create New Migration
```bash
dotnet ef migrations add {MigrationName} --context ApplicationDbContext --project DevHabit.Api
```

#### Apply Migrations
```bash
# Apply all pending migrations
dotnet ef database update --context ApplicationDbContext --project DevHabit.Api

# Apply to specific migration
dotnet ef database update {MigrationName} --context ApplicationDbContext --project DevHabit.Api
```

#### List Migrations
```bash
dotnet ef migrations list --context ApplicationDbContext --project DevHabit.Api
```

#### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove --context ApplicationDbContext --project DevHabit.Api
```

### Identity Schema Commands

#### Create New Migration
```bash
dotnet ef migrations add {MigrationName} --context ApplicationIdentityDbContext --project DevHabit.Api
```

#### Apply Migrations
```bash
# Apply all pending migrations
dotnet ef database update --context ApplicationIdentityDbContext --project DevHabit.Api

# Apply to specific migration
dotnet ef database update {MigrationName} --context ApplicationIdentityDbContext --project DevHabit.Api
```

#### List Migrations
```bash
dotnet ef migrations list --context ApplicationIdentityDbContext --project DevHabit.Api
```

#### Remove Last Migration (if not applied)
```bash
dotnet ef migrations remove --context ApplicationIdentityDbContext --project DevHabit.Api
```

## Automatic Migration Application

The application automatically applies pending migrations during startup via the `ApplyMigrationsAsync()` extension method in `Program.cs`. This ensures both contexts are updated when the application starts.

### Startup Migration Process
1. **Application migrations** are applied first (dev_habit schema)
2. **Identity migrations** are applied second (identity schema)
3. Success/failure is logged for monitoring

### Disabling Automatic Migrations
For production environments, you may want to disable automatic migrations:
```csharp
// In Program.cs, comment out or remove:
// await app.ApplyMigrationsAsync();
```

## Migration History

### Application Schema (dev_habit)

#### 20251121221333_Add_Habits
- **Purpose**: Initial Habits table creation
- **Changes**:
  - Created `habits` table with full schema
  - Added value objects: Frequency, Target, Milestone
  - Established indexes and constraints

#### 20251126222141_UpdateHabitModel
- **Purpose**: Column rename fix
- **Changes**: Renamed `frequency_time_per_period` → `frequency_times_per_period`

#### 20251203011050_Add_Tags
- **Purpose**: Tags table creation
- **Changes**:
  - Created `tags` table
  - Added unique constraint on tag names
  - Established audit timestamps

#### 20251203165332_Add_HabitTags
- **Purpose**: Junction table for many-to-many relationships
- **Changes**:
  - Created `habit_tags` table
  - Composite primary key (HabitId, TagId)
  - Foreign key constraints with cascade delete

#### 20251210201633_Add_Users
- **Purpose**: Users table creation
- **Changes**:
  - Created `users` table
  - Unique constraints on email and identity_id
  - Integration points with Identity system

#### 20251211181026_FixIdentityIdPropertyName
- **Purpose**: Property name typo fix
- **Changes**:
  - Renamed column: `indentity_id` → `identity_id`
  - Renamed index: `ix_users_indentity_id` → `ix_users_identity_id`

### Identity Schema (identity)

#### 20251210225913_Add_Identity
- **Purpose**: ASP.NET Core Identity tables creation
- **Changes**:
  - Created all standard Identity tables
  - Applied snake_case naming conventions
  - Configured for PostgreSQL compatibility

## Database Configuration

### Connection Strings

#### Development Environment
```json
{
  "ConnectionStrings": {
    "Database": "Host=localhost;Port=5432;Database=dev_habit;Username=postgres;Password=postgres"
  }
}
```

#### Docker Environment
```json
{
  "ConnectionStrings": {
    "Database": "Host=devhabit.postgres;Port=5432;Database=dev_habit;Username=postgres;Password=postgres"
  }
}
```

### Context Configuration

Both contexts are configured in `DependencyInjection.cs`:

```csharp
// Application context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
            HistoryRepository.DefaultTableName, Schemas.Application))
    .UseSnakeCaseNamingConvention()
);

// Identity context
builder.Services.AddDbContext<ApplicationIdentityDbContext>(options =>
    options.UseNpgsql(connectionString,
        npgsqlOptions => npgsqlOptions.MigrationsHistoryTable(
            HistoryRepository.DefaultTableName, Schemas.Identity))
    .UseSnakeCaseNamingConvention()
);
```

## Working with Migrations

### Creating a New Migration

1. **Make entity changes** in your domain models
2. **Update configurations** if needed (in `Database/Configurations/`)
3. **Generate migration**:
   ```bash
   dotnet ef migrations add YourMigrationName --context ApplicationDbContext --project DevHabit.Api
   ```
4. **Review generated migration** files
5. **Test migration**:
   ```bash
   dotnet ef database update --context ApplicationDbContext --project DevHabit.Api
   ```

### Migration Best Practices

#### Naming Conventions
- Use descriptive names: `Add_HabitCategories`, `Update_UserSchema`
- Include action: `Add`, `Update`, `Remove`, `Fix`
- Use PascalCase for consistency

#### Code Review
- **Review generated SQL** in migration files
- **Check for breaking changes** (column drops, renames)
- **Verify indexes and constraints**
- **Test rollback scenarios**

#### Data Migrations
For complex data transformations:
```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // Schema changes first
    migrationBuilder.AddColumn<string>("new_column", "habits");

    // Data migration
    migrationBuilder.Sql(@"
        UPDATE dev_habit.habits
        SET new_column = old_column
        WHERE old_column IS NOT NULL
    ");

    // Remove old column
    migrationBuilder.DropColumn("old_column", "habits");
}
```

### Rolling Back Migrations

#### Rollback to Previous Migration
```bash
# Get migration list
dotnet ef migrations list --context ApplicationDbContext --project DevHabit.Api

# Rollback to specific migration
dotnet ef database update PreviousMigrationName --context ApplicationDbContext --project DevHabit.Api
```

#### Remove Unapplied Migration
```bash
dotnet ef migrations remove --context ApplicationDbContext --project DevHabit.Api
```

## Production Migration Strategy

### Manual Migration Application
In production, apply migrations manually for better control:

```bash
# Generate SQL scripts for review
dotnet ef migrations script --context ApplicationDbContext --project DevHabit.Api

# Apply to production database
dotnet ef database update --context ApplicationDbContext --project DevHabit.Api --connection "ProductionConnectionString"
```

### Blue-Green Deployment
1. **Deploy new version** to staging environment
2. **Test migrations** thoroughly
3. **Generate migration scripts** for production
4. **Apply migrations** during maintenance window
5. **Deploy application** after successful migration

### Monitoring Migration Performance
```sql
-- Monitor long-running migrations
SELECT * FROM pg_stat_activity
WHERE query LIKE '%ALTER TABLE%' OR query LIKE '%CREATE INDEX%';

-- Check migration history
SELECT * FROM dev_habit."__EFMigrationsHistory" ORDER BY migration_id;
SELECT * FROM identity."__EFMigrationsHistory" ORDER BY migration_id;
```

## Troubleshooting

### Common Issues

#### Migration Conflicts
```bash
# Error: Migration already exists
dotnet ef migrations remove --context ApplicationDbContext --project DevHabit.Api
# Then create migration with different name
```

#### Context Mismatch
```bash
# Error: Wrong context specified
# Ensure you're using the correct context:
# --context ApplicationDbContext (for app data)
# --context ApplicationIdentityDbContext (for auth data)
```

#### Connection Issues
```bash
# Verify database connectivity
docker exec -it devhabit-postgres pg_isready -U postgres

# Test connection string
dotnet ef database drop --context ApplicationDbContext --project DevHabit.Api --dry-run
```

#### Schema Conflicts
```sql
-- Check schema existence
SELECT schema_name FROM information_schema.schemata
WHERE schema_name IN ('dev_habit', 'identity');

-- View tables in each schema
SELECT table_name FROM information_schema.tables
WHERE table_schema = 'dev_habit';
```

### Recovery Scenarios

#### Corrupted Migration State
```bash
# Drop and recreate database (⚠️ DATA LOSS)
dotnet ef database drop --context ApplicationDbContext --project DevHabit.Api
dotnet ef database update --context ApplicationDbContext --project DevHabit.Api

# Or restore from backup
psql -h localhost -p 5432 -U postgres dev_habit < backup.sql
```

#### Failed Migration
```bash
# Check what failed
dotnet ef migrations list --context ApplicationDbContext --project DevHabit.Api

# Rollback to last working migration
dotnet ef database update LastWorkingMigration --context ApplicationDbContext --project DevHabit.Api

# Fix issues and try again
```

## Development Workflow

### Adding New Entities
1. Create entity class in `Entities/`
2. Add configuration in `Database/Configurations/`
3. Add DbSet to appropriate context
4. Generate migration
5. Apply and test

### Modifying Existing Entities
1. Update entity properties
2. Update configuration if needed
3. Generate migration
4. Review generated SQL
5. Test migration and rollback

### Entity Relationships
- Use Fluent API for complex relationships
- Configure cascade delete behavior
- Add indexes for foreign keys
- Consider performance implications

## Related Documentation

- [Development Setup Guide](development-setup.md): Local development environment
- [Docker Setup Guide](docker-setup.md): Containerized database setup
- [Authentication Setup Guide](authentication-setup.md): Identity context details
- [Application Structure](application-structure.md): Overall architecture overview