# DevHabit API Reference

This document provides comprehensive API documentation for the DevHabit application, including all endpoints, request/response formats, and examples.

## Base URLs

### Container Environment (Recommended)
- **HTTPS**: `https://localhost:9001` (secure, with SSL certificate)
- **HTTP**: `http://localhost:9000` (redirects to HTTPS automatically)

### Local Development Environment
- **HTTPS**: `https://localhost:5001` (when running `dotnet run`)
- **HTTP**: `http://localhost:5000` (when running `dotnet run`)

## API Documentation
- **OpenAPI JSON**: Available at `/openapi/v1.json` on any of the above base URLs
- **Database Dashboard**: Aspire Dashboard at `http://localhost:18888` (container only)

## Authentication API
User registration and authentication endpoints for account management:

### Register User
- **POST** `/auth/register`
- Creates a new user account with both Identity and application database entries
- Request Body: `RegisterUserDto`
- Response: Returns JWT access tokens with `200 OK` status on successful registration
- **Validation Rules:**
  - `Email`: Required, valid email format, will be used as username in Identity system
  - `Name`: Required, user's display name (max 100 characters)
  - `Password`: Required, must meet ASP.NET Core Identity password requirements:
    - At least 6 characters long
    - Must contain at least one digit (0-9)
    - Must contain at least one uppercase letter (A-Z)
    - Must contain at least one non-alphanumeric character
  - `ConfirmPassword`: Required, must match the password field
- **Transaction Handling**: Uses database transactions to ensure both Identity and application user records are created atomically
- **Error Responses:**
  - `400 Bad Request`: Returns detailed validation errors if Identity user creation fails
  - Validation errors include specific field-level error messages from ASP.NET Core Identity
- **Integration**: Links Identity user with application User entity via `IdentityId` property
- Returns `400 Bad Request` with detailed Identity validation errors if registration fails

### Login User
- **POST** `/auth/login`
- Authenticates an existing user and returns JWT tokens for API access
- Request Body: `LoginUserDto`
- Response: Returns JWT access tokens with `200 OK` status on successful authentication
- **Request Properties:**
  - `Email`: Required, user's registered email address
  - `Password`: Required, user's password
- **Authentication Process:**
  - Validates email and password against ASP.NET Core Identity
  - Generates JWT access token with user claims (subject, email)
  - Returns structured token response for client authentication
- **Security Features:**
  - Password verification using secure hash comparison
  - Account lockout protection (inherited from Identity configuration)
  - Token expiration based on configured JWT settings
- Returns `401 Unauthorized` if email not found or password is incorrect

### Refresh Access Token
- **POST** `/auth/refresh`
- Exchanges a valid refresh token for new access and refresh tokens
- Request Body: `RefreshTokenDto`
- Response: Returns new JWT access tokens with `200 OK` status on successful refresh
- **Request Properties:**
  - `RefreshToken`: Required, valid refresh token obtained from login or register
- **Authentication Process:**
  - Validates the provided refresh token exists and hasn't expired
  - Generates new JWT access and refresh tokens with updated expiration
  - Updates the refresh token in the database with new value and expiration
  - Returns structured token response for continued API access
- **Security Features:**
  - Refresh token rotation (new refresh token issued with each refresh)
  - Expiration validation (configurable refresh token lifetime)
  - Database-stored refresh tokens for revocation capabilities
  - Automatic cleanup of expired tokens
- **Token Lifetimes:**
  - Access tokens: Configurable via `JwtAuthOptions.ExpirationInMinutes` (typically 15-60 minutes)
  - Refresh tokens: Configurable via `JwtAuthOptions.RefreshTokenExpirationDays` (typically 7-30 days)
- Returns `401 Unauthorized` if refresh token is invalid, expired, or not found

## Habits API
The API provides full CRUD operations for habit management with user-specific resource protection. **All habit endpoints require JWT authentication.**

### Get All Habits
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
- **Headers:**
  - `Accept` (string): Content type preference
    - `application/json` (default): Standard response without HATEOS links
    - `application/vnd.dev-habit.hateoas+json`: Response includes HATEOS hypermedia links
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
  - `links`: Array of hypermedia links for navigation (self, next-page, previous-page, create) - **only included when Accept header is `application/vnd.dev-habit.hateoas+json`**
- Returns `400 Bad Request` if invalid sort or field parameters are provided

### Get Single Habit
- **GET** `/habits/{id}?fields={fields}`
- Retrieves a specific habit by its ID including associated tags
- **Query Parameters:**
  - `fields` (string): Optional comma-separated list of fields to include in response (e.g., `id,name,status`)
- **Headers:**
  - `Accept` (string): Content type preference
    - `application/json` (default): Standard response without HATEOS links
    - `application/vnd.dev-habit.hateoas+json`: Response includes HATEOS hypermedia links
- **Response**: `ExpandoObject` containing habit data (shaped based on `fields` parameter), or 404 if not found. HATEOS links are included only when Accept header is `application/vnd.dev-habit.hateoas+json`
- **Parameter**: `id` (string) - The habit identifier
- Returns `400 Bad Request` if invalid field parameters are provided

### Create Habit
- **POST** `/habits`
- Creates a new habit with comprehensive validation
- Request Body: `CreateHabitDto`
- Response: `HabitDto` of the created habit with hypermedia links and `201 Created` status
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

### Update Habit (Full)
- **PUT** `/habits/{id}`
- Completely replaces an existing habit
- Request Body: `UpdateHabitDto`
- Response: `204 No Content` on success, `404 Not Found` if habit doesn't exist
- Parameter: `id` (string) - The habit identifier
- **Note**: Preserves milestone progress (`Current` value) while allowing target updates

### Update Habit (Partial)
- **PATCH** `/habits/{id}`
- Partially modifies an existing habit using JSON Patch operations
- Request Body: `JsonPatchDocument<HabitDto>` with Content-Type `application/json-patch+json`
- Response: `204 No Content` on success, `404 Not Found` if habit doesn't exist, `400 Bad Request` for validation errors
- Parameter: `id` (string) - The habit identifier
- **Current Implementation**: Only updates `Name`, `Description`, and `UpdatedAtUtc` fields
- **Validation**: Full model validation is performed on the patched result before applying changes

### Delete Habit
- **DELETE** `/habits/{id}`
- Permanently deletes a habit and all associated tag relationships
- Response: `204 No Content` on success, `404 Not Found` if habit doesn't exist
- Parameter: `id` (string) - The habit identifier
- **Note**: Cascade deletion automatically removes all HabitTag associations

## Tags API
Complete CRUD operations for tag management with user-specific resource protection. **All tag endpoints require JWT authentication.**

### Get All Tags
- **GET** `/tags`
- Returns a collection of all available tags
- Response: `TagsCollectionDto` containing array of `TagDto` objects

### Get Single Tag
- **GET** `/tags/{id}`
- Retrieves a specific tag by its ID
- Response: `TagDto` object or 404 if not found
- Parameter: `id` (string) - The tag identifier

### Create Tag
- **POST** `/tags`
- Creates a new tag
- Request Body: `CreateTagDto`
- Response: `TagDto` of the created tag with `201 Created` status
- Returns `Location` header pointing to the created resource
- **Validation**: Tag names must be unique (returns `409 Conflict` if duplicate)

### Update Tag
- **PUT** `/tags/{id}`
- Completely replaces an existing tag
- Request Body: `UpdateTagDto`
- Response: `204 No Content` on success, `404 Not Found` if tag doesn't exist
- Parameter: `id` (string) - The tag identifier

### Delete Tag
- **DELETE** `/tags/{id}`
- Permanently deletes a tag and all associated habit relationships
- Response: `204 No Content` on success, `404 Not Found` if tag doesn't exist
- Parameter: `id` (string) - The tag identifier
- **Note**: Cascade deletion automatically removes all HabitTag associations

## Users API
Basic operations for user management and identity integration:

### Get Single User
- **GET** `/users/{id}`
- Retrieves a specific user by their ID
- Response: `UserDto` object or 404 if not found
- Parameter: `id` (string) - The user identifier

## Habit-Tag Association API
Manages the many-to-many relationships between habits and tags:

### Upsert Habit Tags
- **PUT** `/habits/{habitId}/tags`
- Replaces all tag associations for a specific habit
- Request Body: `UpsertHabitTagsDto` containing array of tag IDs
- Response: `200 OK` on success, `204 No Content` if no changes, `404 Not Found` if habit doesn't exist, `400 Bad Request` if any tag IDs are invalid
- Parameter: `habitId` (string) - The habit identifier
- **Behavior**: Removes existing associations and creates new ones based on provided tag IDs

### Remove Tag from Habit
- **DELETE** `/habits/{habitId}/tags/{tagId}`
- Removes a specific tag association from a habit
- Response: `204 No Content` on success, `404 Not Found` if association doesn't exist
- Parameters:
  - `habitId` (string) - The habit identifier
  - `tagId` (string) - The tag identifier

## API Usage Examples

### Querying Habits with Advanced Parameters

#### Search for Habits by Name or Description
```http
GET /habits?q=exercise
```

#### Filter Habits by Type and Status
```http
GET /habits?type=Measurable&status=Ongoing
```

#### Sort Habits by Multiple Fields
```http
GET /habits?sort=name asc,createdAtUtc desc
```

#### Pagination Examples
```http
GET /habits?page=2&pageSize=5
```

```http
GET /habits?page=1&pageSize=20&sort=name asc
```

#### Complex Query with All Parameters
```http
GET /habits?q=daily&type=Measurable&status=Ongoing&sort=target.value desc,name asc&page=1&pageSize=15
```

#### Field Selection Examples
```http
GET /habits?fields=id,name,status
```

```http
GET /habits/{id}?fields=name,description,type
```

```http
GET /habits?q=exercise&fields=id,name,target&page=1&pageSize=5
```

#### HATEOS Content Negotiation Examples
```http
GET /habits
Accept: application/vnd.dev-habit.hateoas+json
```

```http
GET /habits/{id}
Accept: application/vnd.dev-habit.hateoas+json
```

```http
GET /habits?q=exercise&fields=id,name,status&page=1&pageSize=5
Accept: application/vnd.dev-habit.hateoas+json
```

### Validation Error Example
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

### Creating a Daily Exercise Habit
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

### Response Example (HabitWithTagsDto from GET /habits/{id})
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
  "createdAtUtc": "2025-11-25T12:00:00Z",
  "updatedAtUtc": null,
  "lastCompletedAtUtc": null,
  "tags": ["Fitness", "Health", "Morning Routine"],
  "links": [
    {
      "href": "https://localhost:9001/habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "https://localhost:9001/habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R",
      "rel": "update",
      "method": "PUT"
    },
    {
      "href": "https://localhost:9001/habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R",
      "rel": "partial-update",
      "method": "PATCH"
    },
    {
      "href": "https://localhost:9001/habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R",
      "rel": "delete",
      "method": "DELETE"
    },
    {
      "href": "https://localhost:9001/habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R/tags",
      "rel": "upsert-tags",
      "method": "PUT"
    }
  ]
}
```

### Paginated Response Example (PaginationResult<ExpandoObject> from GET /habits)
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
      "createdAtUtc": "2025-11-25T12:00:00Z",
      "updatedAtUtc": null,
      "lastCompletedAtUtc": "2025-12-01T10:30:00Z"
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
      "createdAtUtc": "2025-11-20T15:00:00Z",
      "updatedAtUtc": "2025-11-25T16:00:00Z",
      "lastCompletedAtUtc": "2025-12-02T20:45:00Z"
    }
  ],
  "page": 1,
  "pageSize": 10,
  "totalCount": 25,
  "totalPages": 3,
  "hasPreviousPage": false,
  "hasNextPage": true,
  "links": [
    {
      "href": "https://localhost:9001/habits?page=1&pageSize=10",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "https://localhost:9001/habits",
      "rel": "create",
      "method": "POST"
    },
    {
      "href": "https://localhost:9001/habits?page=2&pageSize=10",
      "rel": "next-page",
      "method": "GET"
    }
  ]
}
```

### Field Selection Response Example (GET /habits?fields=id,name,status)
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
  "hasNextPage": true,
  "links": [
    {
      "href": "https://localhost:9001/habits?page=1&pageSize=10&fields=id,name,status",
      "rel": "self",
      "method": "GET"
    },
    {
      "href": "https://localhost:9001/habits",
      "rel": "create",
      "method": "POST"
    },
    {
      "href": "https://localhost:9001/habits?page=2&pageSize=10&fields=id,name,status",
      "rel": "next-page",
      "method": "GET"
    }
  ]
}
```

### Updating a Habit (Full Replacement)
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

### Partially Updating a Habit (JSON Patch)
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

### Creating a Tag
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
  "createdAtUtc": "2025-12-03T12:00:00Z",
  "updatedAtUtc": null
}
```

### Associating Tags with a Habit
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

### Removing a Tag from a Habit
```json
DELETE /habits/h_01JDQM7Z8K2X3Y4W5V6U7T8S9R/tags/t_01JDQM8A9L3N4P5Q6R7S8T9U0V
```

**Response**: `204 No Content`

### Getting a User by ID
```json
GET /users/u_01JDQM9D2O6R7S8T9U0V1W2X3Y
```

**Response**: `200 OK`
```json
{
  "id": "u_01JDQM9D2O6R7S8T9U0V1W2X3Y",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "createdAtUtc": "2025-12-10T12:00:00Z",
  "updatedAtUtc": "2025-12-10T14:30:00Z"
}
```

### Registering a New User
```json
POST /auth/register
Content-Type: application/json

{
  "email": "newuser@example.com",
  "name": "New User",
  "password": "SecurePassword123!",
  "confirmPassword": "SecurePassword123!"
}
```

**Response**: `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5YmJlNTFmYy0xMDBjLTQ5NDAtYmY2NC1hYTk2ZTJmZmI1MTciLCJlbWFpbCI6Im5ld3VzZXJAZXhhbXBsZS5jb20iLCJpc3MiOiJkZXYtaGFiaXQtYXBpIiwiYXVkIjoiZGV2LWhhYml0LWNsaWVudCIsImV4cCI6MTczMzk5NjExNCwiaWF0IjoxNzMzOTkyNTE0fQ.K5xP2sY8JqN9LMwF3QRx7Tv4QnU8ZrW2Ht1Bc9VmXdE",
  "refreshToken": "xYz9K3mP2qR4sT6uV8wX0yA1bC2dE3fG4hI5jK6lM7nO8pQ9rS0tU1vW2xY3zA4B"
}
```

### Logging In a User
```json
POST /auth/login
Content-Type: application/json

{
  "email": "newuser@example.com",
  "password": "SecurePassword123!"
}
```

**Response**: `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5YmJlNTFmYy0xMDBjLTQ5NDAtYmY2NC1hYTk2ZTJmZmI1MTciLCJlbWFpbCI6Im5ld3VzZXJAZXhhbXBsZS5jb20iLCJpc3MiOiJkZXYtaGFiaXQtYXBpIiwiYXVkIjoiZGV2LWhhYml0LWNsaWVudCIsImV4cCI6MTczMzk5NjExNCwiaWF0IjoxNzMzOTkyNTE0fQ.K5xP2sY8JqN9LMwF3QRx7Tv4QnU8ZrW2Ht1Bc9VmXdE",
  "refreshToken": "aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5A6B7C8D9E0F1G2H3I4J5K6L7M"
}
```

### Login with Invalid Credentials
```json
POST /auth/login
Content-Type: application/json

{
  "email": "nonexistent@example.com",
  "password": "WrongPassword"
}
```

**Response**: `401 Unauthorized`

### Refreshing Access Token
```json
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "aB3cD4eF5gH6iJ7kL8mN9oP0qR1sT2uV3wX4yZ5A6B7C8D9E0F1G2H3I4J5K6L7M"
}
```

**Response**: `200 OK`
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5YmJlNTFmYy0xMDBjLTQ5NDAtYmY2NC1hYTk2ZTJmZmI1MTciLCJlbWFpbCI6Im5ld3VzZXJAZXhhbXBsZS5jb20iLCJpc3MiOiJkZXYtaGFiaXQtYXBpIiwiYXVkIjoiZGV2LWhhYml0LWNsaWVudCIsImV4cCI6MTczMzk5ODcyNCwiaWF0IjoxNzMzOTk1MTI0fQ.N9mE8qP7rL2wF6QxY3zA4B5cD6eF7gH8iJ9kL0mN1oP2qR3sT4uV5wX6yZ7A8B9C",
  "refreshToken": "pQ9rS0tU1vW2xY3zA4B5cD6eF7gH8iJ9kL0mN1oP2qR3sT4uV5wX6yZ7A8B9C0dE"
}
```

### Refresh Token Invalid or Expired
```json
POST /auth/refresh
Content-Type: application/json

{
  "refreshToken": "invalid-or-expired-token"
}
```

**Response**: `401 Unauthorized`

### Using JWT Tokens for Authentication
After successful login or registration, include the access token in subsequent API requests:

```http
GET /habits
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI5YmJlNTFmYy0xMDBjLTQ5NDAtYmY2NC1hYTk2ZTJmZmI1MTciLCJlbWFpbCI6Im5ld3VzZXJAZXhhbXBsZS5jb20iLCJpc3MiOiJkZXYtaGFiaXQtYXBpIiwiYXVkIjoiZGV2LWhhYml0LWNsaWVudCIsImV4cCI6MTczMzk5NjExNCwiaWF0IjoxNzMzOTkyNTE0fQ.K5xP2sY8JqN9LMwF3QRx7Tv4QnU8ZrW2Ht1Bc9VmXdE
```

### Registration with Validation Errors
```json
POST /auth/register
Content-Type: application/json

{
  "email": "invalid-email",
  "name": "",
  "password": "weak",
  "confirmPassword": "different"
}
```

**Response**: `400 Bad Request`
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Unable to register user, please try again",
  "traceId": "00-c15a58566b484e11b471bb2ff00a8163-891517bac7706d47-01",
  "requestId": "0HNHOS2MBA6E2:00000001",
  "extensions": {
    "errors": {
      "passwordTooShort": "Passwords must be at least 6 characters.",
      "passwordRequiresNonAlphanumeric": "Passwords must have at least one non alphanumeric character.",
      "passwordRequiresDigit": "Passwords must have at least one digit ('0'-'9').",
      "passwordRequiresUpper": "Passwords must have at least one uppercase ('A'-'Z')."
    }
  }
}
```

## HTTP Client Testing
The project includes DevHabit.Api.http for testing API endpoints directly in compatible editors. Update endpoint URLs based on your deployment method (container vs. local).