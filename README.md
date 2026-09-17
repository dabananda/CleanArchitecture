# CleanArchitecture

A production-ready, **4-Layer Clean Architecture** starter template and backend built with **ASP.NET Core (.NET 10)**.

This solution strictly adheres to Clean Architecture principles, ensuring separation of concerns, maintainability, and testability with inward-pointing dependencies.

---

## Table of Contents

- [Architecture Overview](#architecture-overview)
- [Solution Structure](#solution-structure)
- [Tech Stack](#tech-stack)
- [Key Features](#key-features)
- [API Endpoints](#api-endpoints)
- [Getting Started](#getting-started)
  - [Prerequisites](#prerequisites)
  - [Configuration](#configuration)
  - [Running the Application](#running-the-application)
- [Project Conventions](#project-conventions)
- [Contributing](#contributing)

---

## Architecture Overview

The solution strictly enforces Clean Architecture and the **inward dependency rule**:

```
CleanArchitecture.Api (Presentation / Host / Composition Root)
         │
         ▼
CleanArchitecture.Infrastructure
         │
         ▼
CleanArchitecture.Application
         │
         ▼
CleanArchitecture.Domain
```

- **Domain**: Pure business entities, domain logic, constants, and value objects. Has zero external dependencies (no EF Core, no ASP.NET Core).
- **Application**: CQRS Commands, Queries, Handlers, FluentValidation rules, custom Mediator, and interfaces for repositories/external services.
- **Infrastructure**: Entity Framework Core DbContext, entity type configurations, repository implementations, security services (JWT, BCrypt), and external service clients (Cloudinary, MailKit, Distributed Cache).
- **Api**: ASP.NET Core controllers, exception handling middleware, Swagger documentation, JWT Bearer authentication, rate limiting, and dependency injection composition root.

---

## Solution Structure

```
CleanArchitecture/
├── CleanArchitecture.slnx                               # Modern .NET 10 solution file
├── Directory.Build.props                                # Centralized compiler rules & zero warning policy
├── README.md
├── CONTRIBUTING.md
├── .gitignore
│
├── CleanArchitecture.Domain/                           # Domain Layer (Enterprise Business Rules)
│   ├── Constants/                                      # Roles, DbSchemas
│   └── Entities/                                       # BaseEntity, ApplicationUser, Role, UserRole, etc.
│
├── CleanArchitecture.Application/                      # Application Layer (Application Business Rules)
│   ├── Behaviors/                                      # ValidationBehavior pipeline
│   ├── Common/                                         # Result<T>, ErrorType, PagedResult, Slug, Errors
│   ├── Features/                                       # CQRS vertical slices (Auth, Role)
│   │   ├── Auth/                                       # Login, Register, RefreshToken, PasswordReset, etc.
│   │   └── Role/                                       # Role CRUD, Assign, Remove
│   ├── Interfaces/                                     # Repository and external service contracts
│   ├── Messaging/                                      # Zero-dependency CQRS mediator
│   └── DependencyInjection.cs                          # AddApplication extension
│
├── CleanArchitecture.Infrastructure/                   # Infrastructure Layer (External Concerns)
│   ├── Common/                                         # QueryableExtensions (EF Core pagination)
│   ├── Configurations/                                 # Strongly-typed Settings options
│   ├── Persistence/                                    # ApplicationDbContext, Audit interceptor
│   │   └── Configurations/                             # EF Core entity type configurations
│   ├── Repositories/                                   # AuthRepository, RoleRepository
│   ├── Security/                                       # JwtTokenGenerator, PasswordHasher, SecureTokenGenerator
│   ├── Services/                                       # CloudinaryImageService, EmailService, CurrentUserService, Cache
│   └── DependencyInjection.cs                          # AddInfrastructure extension
│
├── CleanArchitecture.Api/                              # API Layer (Presentation & Host)
│   ├── Controllers/                                    # AuthController, RolesController, ImageController, BaseController
│   ├── Middleware/                                     # GlobalExceptionHandler
│   ├── Properties/                                     # launchSettings.json
│   ├── appsettings.json                                # Central configuration
│   ├── appsettings.Development.json
│   ├── CleanArchitecture.Api.http                      # REST client testing file
│   └── Program.cs                                      # Composition root, Swagger, Serilog, Pipeline
│
└── CleanArchitecture.Tests/                            # Automated test suite
    ├── ArchitectureTests.cs                            # NetArchTest architecture fitness functions
    ├── ApplicationUserDomainTests.cs                   # User domain logic tests
    ├── RoleDomainTests.cs                              # Role domain logic tests
    ├── AuthFeaturesTests.cs                            # CQRS handler tests
    ├── MediatorTests.cs                                # Mediator & validation pipeline tests
    ├── AuditInterceptorTests.cs                        # Auditing & soft-delete tests
    └── CommonTests.cs                                  # Result & utility tests
```

---

## Tech Stack

| Layer / Concern | Technology |
|---|---|
| **Runtime** | .NET 10 (C# 13) |
| **Web Framework** | ASP.NET Core 10 |
| **Data Access / ORM** | Entity Framework Core 10 (PostgreSQL / Npgsql) |
| **Mediator / CQRS** | Built-in zero-dependency Mediator (`ISender`, `IRequest<T>`) |
| **Validation** | FluentValidation 12 with automatic pipeline behaviors |
| **Security & Auth** | JWT Bearer Tokens, Refresh Tokens, BCrypt password hashing |
| **Logging** | Serilog (Structured Console + Daily Rolling File sink) |
| **Caching** | `IDistributedCache` abstraction with in-memory / distributed support |
| **Mailing** | MailKit (SMTP / TLS) |
| **API Documentation** | Swagger / Swashbuckle OpenAPI |
| **Rate Limiting** | ASP.NET Core Rate Limiting (Partitioned Sliding Window + Fixed Window) |

---

## Key Features

### 1. In-House Zero-Dependency CQRS Mediator
Features a lightweight, built-in mediator (`ISender` and `IRequestHandler<TRequest, TResponse>`). Handlers and pipeline behaviors are automatically scanned and registered.

### 2. Functional Result Pattern
Commands and queries return `Result` or `Result<T>` instead of throwing exceptions across layer boundaries:
- `Result.Success(data)`
- `Result.Failure(ErrorType.Validation, "Invalid input")`
- `Result.Failure(ErrorType.NotFound, "Resource not found")`

The `BaseController` automatically translates `ErrorType` into proper HTTP status codes (`400`, `401`, `403`, `404`, `409`, `500`).

### 3. Automatic Validation Pipeline
Every request entering the mediator passes through `ValidationBehavior<TRequest, TResponse>`. If FluentValidation detects validation errors, execution immediately halts and returns a standardized `400 Bad Request` with structured error details.

### 4. Automatic Audit Trail & Soft Delete
All entities deriving from `BaseEntity` are automatically populated with `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt`, and `DeletedBy` via EF Core's `AuditSaveChangesInterceptor`. A global query filter automatically enforces soft-deletion (`!e.IsDeleted`).

---

## API Endpoints

### Authentication (`/api/v1/auth`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/auth/register` | Register a new user |
| `POST` | `/api/v1/auth/login` | Authenticate and obtain JWT + Refresh token |
| `POST` | `/api/v1/auth/refresh-token` | Exchange refresh token for new access token |
| `POST` | `/api/v1/auth/logout` | Revoke active refresh token |
| `GET` | `/api/v1/auth/verify-email` | Verify user email with token |
| `POST` | `/api/v1/auth/forgot-password` | Request password reset token |
| `POST` | `/api/v1/auth/reset-password` | Set new password using reset token |
| `GET` | `/api/v1/auth/me` | Current authenticated user claims |

### Roles (`/api/v1/roles`)
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/v1/roles` | List all roles |
| `GET` | `/api/v1/roles/{id}` | Get role by ID |
| `POST` | `/api/v1/roles` | Create a new role |
| `PUT` | `/api/v1/roles` | Update role details |
| `DELETE` | `/api/v1/roles/{id}` | Soft delete role by ID |
| `POST` | `/api/v1/roles/assign` | Assign role to user |
| `POST` | `/api/v1/roles/remove` | Remove role from user |

### Shared Media (`/api/v1/images`)
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/v1/images/upload` | Upload image to Cloudinary |
| `DELETE` | `/api/v1/images/{id}` | Delete uploaded image |

### System
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/health` | Health check probe |
| `GET` | `/swagger` | OpenAPI Swagger UI |

---

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- PostgreSQL

### Configuration
Update connection strings and secrets in `CleanArchitecture.Api/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "PostgreSql": "Host=localhost;Database=CleanArchitectureDb;Username=postgres;Password=yourpassword"
  },
  "Jwt": {
    "Key": "CleanArchitectureSuperSecretSigningKeyThatIsAtLeast32BytesLong!",
    "Issuer": "CleanArchitecture",
    "Audience": "CleanArchitectureClient",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  }
}
```

### Running the Application

```pwsh
dotnet restore CleanArchitecture.slnx
dotnet build CleanArchitecture.slnx
dotnet test CleanArchitecture.slnx
dotnet run --project CleanArchitecture.Api
```

Once running:
- **Swagger UI**: `http://localhost:5001/swagger` or `https://localhost:5000/swagger`
- **Health Check**: `http://localhost:5001/health`

---

## Project Conventions

| Area | Convention | Example |
|---|---|---|
| **Naming** | `CleanArchitecture.<Layer>` | `CleanArchitecture.Application` |
| **CQRS** | In-house mediator commands and queries | `LoginCommand : IRequest<Result<LoginResponse>>` |
| **Validation** | FluentValidation rules paired with commands | `RegisterCommandValidator : AbstractValidator<RegisterCommand>` |
| **Controllers** | Inherit from `BaseController` using `HandleResult()` | `return HandleResult(await sender.Send(command));` |

---

## Contributing

Please see [CONTRIBUTING.md](CONTRIBUTING.md) for contribution guidelines.
