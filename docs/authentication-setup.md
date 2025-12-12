# DevHabit Authentication Setup Guide

This guide covers the ASP.NET Core Identity authentication system implementation in DevHabit, including setup, configuration, and integration patterns.

## Authentication Architecture Overview

DevHabit implements a **dual-identity approach** that separates authentication concerns from application data:

- **ASP.NET Core Identity**: Handles authentication, authorization, and security (identity schema)
- **Application User Entity**: Manages business-related user data (dev_habit schema)
- **Integration Layer**: Links Identity users with application users via `IdentityId`

## ASP.NET Core Identity Configuration

### Service Registration

Identity services are configured in `DependencyInjection.cs`:

```csharp
public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
{
    builder.Services
        .AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationIdentityDbContext>();
    return builder;
}
```

### Database Context

The `ApplicationIdentityDbContext` manages Identity-specific tables:

```csharp
public class ApplicationIdentityDbContext : IdentityDbContext<IdentityUser>
{
    // Configuration for Identity tables in 'identity' schema
    // Snake case naming for PostgreSQL compatibility
    // Separate migration history from application data
}
```

### Middleware Configuration

Authentication middleware is properly ordered in `Program.cs`:

```csharp
app.UseHttpsRedirection();
app.UseExceptionHandler();

// Authentication middleware MUST come before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
```

**Critical**: The middleware order is essential for proper authentication flow.

## Identity Database Schema

### Core Identity Tables

The Identity system creates these tables in the `identity` schema:

#### asp_net_users
Primary user authentication data:
- **id**: Unique user identifier
- **user_name**: Login username (typically email)
- **email**: User email address
- **password_hash**: Hashed password
- **security_stamp**: Security token for invalidating sessions
- **email_confirmed**: Email verification status
- **lockout_enabled**: Account lockout capability
- **access_failed_count**: Failed login attempts

#### asp_net_roles
Role definitions for authorization:
- **id**: Unique role identifier
- **name**: Role name (Admin, User, etc.)
- **normalized_name**: Uppercase role name for lookups

#### asp_net_user_roles
Many-to-many user-role assignments:
- **user_id**: Reference to asp_net_users
- **role_id**: Reference to asp_net_roles

#### asp_net_user_claims
User-specific claims for fine-grained permissions:
- **user_id**: Reference to asp_net_users
- **claim_type**: Type of claim (permission, attribute)
- **claim_value**: Claim value

#### asp_net_user_logins
External login provider mappings:
- **user_id**: Reference to asp_net_users
- **login_provider**: Provider name (Google, Facebook, etc.)
- **provider_key**: Provider-specific user identifier

#### asp_net_user_tokens
Security tokens for operations:
- **user_id**: Reference to asp_net_users
- **login_provider**: Token provider
- **name**: Token purpose (email confirmation, password reset)
- **value**: Token value

## Application User Integration

### Dual Entity Pattern

DevHabit maintains separate entities for different concerns:

#### Identity User (Authentication)
```csharp
// Built-in ASP.NET Core Identity
public class IdentityUser
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string UserName { get; set; }
    // ... security-related properties
}
```

#### Application User (Business Data)
```csharp
// Custom application entity
public class User
{
    public string Id { get; set; }           // Application user ID
    public string Email { get; set; }        // Business email
    public string Name { get; set; }         // Display name
    public string IdentityId { get; set; }   // Link to Identity user
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
```

### Integration Via IdentityId

The `IdentityId` property links application users to Identity users:
- **Foreign Key**: References `asp_net_users.id`
- **Unique Constraint**: One-to-one relationship
- **Cascade Behavior**: Configured for data consistency

## User Registration Flow

### Registration Process

The `AuthController.Register` method implements atomic user creation:

```csharp
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
{
    // 1. Begin database transaction
    using IDbContextTransaction transaction = await identityDbContext.Database.BeginTransactionAsync();

    // 2. Create Identity user
    var identityUser = new IdentityUser
    {
        Email = registerUserDto.Email,
        UserName = registerUserDto.Email  // Email as username
    };

    IdentityResult identityResult = await userManager.CreateAsync(identityUser, registerUserDto.Password);

    if (!identityResult.Succeeded)
    {
        // Return validation errors from Identity
        return Problem(/* Identity errors */);
    }

    // 3. Create Application user
    User user = registerUserDto.ToEntity();
    user.IdentityId = identityUser.Id;  // Link to Identity user

    applicationDbContext.Users.Add(user);
    await applicationDbContext.SaveChangesAsync();

    // 4. Commit transaction
    await transaction.CommitAsync();

    // 5. Generate JWT tokens for immediate login
    var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);
    AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

    return Ok(accessTokens);
}
```

### Transaction Management

Registration uses database transactions to ensure:
- **Atomicity**: Both Identity and Application users are created or neither
- **Consistency**: No orphaned records if one operation fails
- **Isolation**: Concurrent registrations don't interfere
- **Durability**: Successful registrations are persisted

### Validation and Error Handling

#### Identity Validation Rules
ASP.NET Core Identity enforces password requirements:
- **Minimum length**: 6 characters
- **Uppercase letter**: At least one (A-Z)
- **Lowercase letter**: At least one (a-z)
- **Digit**: At least one (0-9)
- **Non-alphanumeric**: At least one special character

#### Error Response Format
Validation failures return structured error responses:
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Unable to register user, please try again",
  "extensions": {
    "errors": {
      "passwordTooShort": "Passwords must be at least 6 characters.",
      "passwordRequiresDigit": "Passwords must have at least one digit ('0'-'9').",
      // ... other specific errors
    }
  }
}
```

## Security Features

### Password Management
- **Hashing**: Secure password hashing with salt
- **Strength Requirements**: Configurable password policies
- **Reset Functionality**: Token-based password reset (future)
- **History**: Password change tracking (if enabled)

### Account Security
- **Lockout Protection**: Automatic account lockout after failed attempts
- **Email Confirmation**: Email verification workflow (future)
- **Two-Factor Authentication**: 2FA support (future)
- **Security Stamps**: Session invalidation on security changes

### Authorization Framework
- **Role-Based**: Assign roles to users for coarse-grained permissions
- **Claims-Based**: Fine-grained permissions via user claims
- **Policy-Based**: Complex authorization logic via policies
- **Attribute-Based**: Controller/action-level authorization

## Controller Authorization

### Authorization Attributes

```csharp
[ApiController]
[Route("auth")]
[AllowAnonymous]  // Registration is publicly accessible
public sealed class AuthController : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        // Registration logic
    }
}

// Protected controller example
[ApiController]
[Route("habits")]
[Authorize]  // Requires authentication
public sealed class HabitsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits()
    {
        // Only authenticated users can access
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]  // Requires Admin role
    public async Task<IActionResult> DeleteHabit(string id)
    {
        // Only admins can delete habits
    }
}
```

### Policy-Based Authorization

```csharp
// In DependencyInjection.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageHabits", policy =>
        policy.RequireRole("Admin", "Manager")
              .RequireClaim("Permission", "ManageHabits"));

    options.AddPolicy("OwnResourceOnly", policy =>
        policy.Requirements.Add(new OwnerRequirement()));
});

// In controller
[HttpPut("{id}")]
[Authorize(Policy = "CanManageHabits")]
public async Task<IActionResult> UpdateHabit(string id, UpdateHabitDto dto)
{
    // Only users with CanManageHabits policy can access
}
```

## JWT Token Authentication

DevHabit implements JWT (JSON Web Token) based authentication for API access, providing stateless authentication suitable for mobile apps and SPA frontends.

### JWT Service Configuration

JWT authentication is configured in `DependencyInjection.cs`:

```csharp
public static WebApplicationBuilder AddAuthenticationServices(this WebApplicationBuilder builder)
{
    builder.Services
        .AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationIdentityDbContext>();

    // Configure JWT settings from appsettings.json
    builder.Services.Configure<JwtAuthOptions>(builder.Configuration.GetSection("Jwt"));

    JwtAuthOptions jwtAuthOptions = builder.Configuration.GetSection("Jwt").Get<JwtAuthOptions>()!;

    // Configure JWT Bearer authentication
    builder.Services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = jwtAuthOptions.Issuer,
                ValidAudience = jwtAuthOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtAuthOptions.Key))
            };
        });

    builder.Services.AddAuthorization();

    // Register JWT token provider service
    builder.Services.AddTransient<TokenProvider>();

    return builder;
}
```

### JWT Configuration Options

JWT settings are defined in the `JwtAuthOptions` class:

```csharp
public sealed class JwtAuthOptions
{
    public string Issuer { get; init; }
    public string Audience { get; init; }
    public string Key { get; init; }
    public int ExpirationInMinutes { get; init; }
    public int RefreshTokenExpirationDays { get; init; }
}
```

These settings should be configured in `appsettings.json`:

```json
{
  "Jwt": {
    "Issuer": "dev-habit-api",
    "Audience": "dev-habit-client",
    "Key": "your-super-secret-jwt-signing-key-here",
    "ExpirationInMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

**Security Note**: The JWT signing key should be a strong, randomly generated string and stored securely (environment variables in production).

### Token Provider Service

The `TokenProvider` service handles JWT token generation:

```csharp
public sealed class TokenProvider(IOptions<JwtAuthOptions> options)
{
    public AccessTokensDto Create(TokenRequest tokenRequest)
    {
        return new AccessTokensDto(GenerateAccessToken(tokenRequest), GenerateRefreshToken());
    }

    private string GenerateAccessToken(TokenRequest tokenRequest)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims = [
            new(JwtRegisteredClaimNames.Sub, tokenRequest.UserId),
            new(JwtRegisteredClaimNames.Email, tokenRequest.Email)
        ];

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience
        };

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(tokenDescriptor);
    }
}
```

### Authentication Endpoints

#### User Login
The `AuthController` provides a login endpoint that returns JWT tokens:

```csharp
[HttpPost("login")]
public async Task<ActionResult<AccessTokensDto>> Login(LoginUserDto loginUserDto)
{
    IdentityUser? identityUser = await userManager.FindByEmailAsync(loginUserDto.Email);

    if (identityUser is null || !await userManager.CheckPasswordAsync(identityUser, loginUserDto.Password))
    {
        return Unauthorized();
    }

    var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email!);
    AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

    return Ok(accessTokens);
}
```

#### Updated User Registration
The registration endpoint now returns JWT tokens upon successful registration:

```csharp
[HttpPost("register")]
public async Task<ActionResult<AccessTokensDto>> Register(RegisterUserDto registerUserDto)
{
    // ... Identity and Application user creation logic ...

    // Generate and return JWT tokens
    var tokenRequest = new TokenRequest(identityUser.Id, identityUser.Email);
    AccessTokensDto accessTokens = tokenProvider.Create(tokenRequest);

    return Ok(accessTokens);
}
```

### JWT Token Structure

#### Access Token Claims
Generated JWT access tokens include:
- **sub** (Subject): The Identity user ID
- **email**: User's email address
- **iss** (Issuer): Configured issuer value
- **aud** (Audience): Configured audience value
- **exp** (Expires): Token expiration timestamp
- **iat** (Issued At): Token generation timestamp

#### Token Response Format
Authentication endpoints return tokens in the `AccessTokensDto` format:

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": ""
}
```

**Note**: Refresh token functionality is currently a placeholder and returns an empty string.

### Client Authentication

#### Making Authenticated Requests
Clients should include the JWT access token in the Authorization header:

```http
GET /habits
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Token Validation
The JWT Bearer middleware automatically:
- Validates the token signature using the configured signing key
- Checks token expiration
- Verifies issuer and audience claims
- Populates the `HttpContext.User` with claims from the token

### Protected Controllers

Controllers requiring authentication use the `[Authorize]` attribute:

```csharp
[ApiController]
[Route("habits")]
[Authorize]  // Requires valid JWT token
public sealed class HabitsController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetHabits()
    {
        // Access authenticated user via HttpContext.User
        string userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // ... implementation
    }
}
```

## Authentication Extensions (Future)

### External Login Providers
Social login integration:
```csharp
builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"];
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
    })
    .AddFacebook(options =>
    {
        options.AppId = configuration["Authentication:Facebook:AppId"];
        options.AppSecret = configuration["Authentication:Facebook:AppSecret"];
    });
```

### Cookie Authentication
For web application scenarios:
```csharp
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
});
```

## Development and Testing

### Seeding Test Users
For development and testing:
```csharp
public static class DataSeeder
{
    public static async Task SeedTestUsers(UserManager<IdentityUser> userManager)
    {
        if (!await userManager.Users.AnyAsync())
        {
            var testUser = new IdentityUser
            {
                UserName = "test@example.com",
                Email = "test@example.com",
                EmailConfirmed = true
            };

            await userManager.CreateAsync(testUser, "Test123!");
            await userManager.AddToRoleAsync(testUser, "User");
        }
    }
}
```

### Testing Authentication
```csharp
[Test]
public async Task Register_ValidUser_ReturnsSuccess()
{
    var registerDto = new RegisterUserDto
    {
        Email = "test@example.com",
        Name = "Test User",
        Password = "SecurePass123!",
        ConfirmPassword = "SecurePass123!"
    };

    var response = await client.PostAsJsonAsync("/auth/register", registerDto);

    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var userId = await response.Content.ReadAsStringAsync();
    userId.Should().NotBeNullOrEmpty();
}
```

## Troubleshooting

### Common Issues

#### Middleware Order Problems
**Symptom**: 401 Unauthorized on all endpoints
**Solution**: Ensure `UseAuthentication()` comes before `UseAuthorization()`

#### Missing Identity Services
**Symptom**: Services not registered errors
**Solution**: Verify `AddAuthenticationServices()` is called in `Program.cs`

#### Database Context Issues
**Symptom**: Identity tables not found
**Solution**: Ensure Identity migrations are applied

#### Transaction Conflicts
**Symptom**: Registration fails with transaction errors
**Solution**: Check database connection and transaction configuration

### Debugging Authentication

#### Check User Claims
```csharp
[HttpGet("claims")]
[Authorize]
public IActionResult GetClaims()
{
    var claims = User.Claims.Select(c => new { c.Type, c.Value });
    return Ok(claims);
}
```

#### Verify Identity Configuration
```csharp
[HttpGet("identity-info")]
public IActionResult GetIdentityInfo()
{
    return Ok(new
    {
        IsAuthenticated = User.Identity?.IsAuthenticated,
        UserName = User.Identity?.Name,
        AuthenticationType = User.Identity?.AuthenticationType
    });
}
```

## Security Best Practices

### Configuration Security
- **Never hardcode secrets** in source code
- **Use environment variables** for sensitive configuration
- **Implement proper key rotation** for JWT signing keys
- **Configure HTTPS** for all authentication endpoints

### Data Protection
- **Encrypt sensitive data** at rest
- **Use secure communication** (HTTPS only)
- **Implement proper session management**
- **Log security events** for monitoring

### Monitoring and Auditing
- **Log authentication attempts** (success and failure)
- **Monitor for suspicious patterns** (brute force, etc.)
- **Implement account lockout** policies
- **Track privilege escalation** events

## Related Documentation

- [Database Migrations Guide](database-migrations.md): Identity schema migrations
- [API Reference](api-reference.md): Authentication endpoint documentation
- [Development Setup Guide](development-setup.md): Local development configuration
- [Docker Setup Guide](docker-setup.md): Container authentication setup