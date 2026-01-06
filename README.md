# Dilcore Platform

[![CodeRabbit Reviews](https://img.shields.io/coderabbit/prs/github/aytymchuk/Dilcore-Platform?utm_source=oss&utm_medium=github&utm_campaign=aytymchuk%2FDilcore-Platform&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)](https://coderabbit.ai)
[![🚀 .NET CI](https://github.com/aytymchuk/Dilcore-Platform/actions/workflows/ci-dotnet-v1.yml/badge.svg)](https://github.com/aytymchuk/Dilcore-Platform/actions/workflows/ci-dotnet-v1.yml)

A new platform based on .NET 10.

## Domains

### Tenancy
Enables multitenancy capabilities within the platform. This domain manages tenants and controls user access to specific tenant environments.

### Identity
Handles user management at the platform level. It manages users and their relationships to tenants, including defining access levels and permissions.


### Common
Shared libraries and infrastructure components used across all domains.
- **MediatR**: Provides CQRS infrastructure with built-in logging and distributed tracing.
- **MultiTenant**: Common infrastructure for tenant resolution, context management, and logging. Supports header-based and path-based resolution strategies.
- **FluentResults**: Extensions for Minimal APIs (`ToMinimalApiResult`) and standardized error handling.
- **Telemetry**: OpenTelemetry integration and consolidated telemetry setup.
- **Authentication**: Shared abstractions for user context and attribute providers.
- **Configuration**: Extensions for standardized configuration loading and validation across services.

#### Dependency Graph (Common)

```mermaid
graph TD
    %% Telemetry Core
    TelAbs["Telemetry.Abstractions"]
    
    %% Authentication
    AuthAbs["Authentication.Abstractions"]
    Auth0["Authentication.Auth0"]
    AuthHttp["Authentication.Http.Extensions"]
    
    Auth0 --> AuthAbs
    AuthHttp --> AuthAbs
    AuthHttp --> Auth0
    AuthHttp --> TelAbs
    
    %% MultiTenant
    MTAbs["MultiTenant.Abstractions"]
    MTHttp["MultiTenant.Http.Extensions"]
    
    MTHttp --> MTAbs
    MTHttp --> TelAbs
    
    %% MediatR
    MedAbs["MediatR.Abstractions"]
    MedExt["MediatR.Extensions"]
    
    MedExt --> MedAbs
    MedExt --> TelAbs
    
    %% FluentResults
    ResAbs["Results.Abstractions"]
    ResExt["Results.Extensions.Api"]
    
    ResExt --> ResAbs
    
    %% Configuration
    ConfigExt["Configuration.Extensions"]
    ConfigAsp["Configuration.AspNetCore"]
    
    ConfigAsp --> ConfigExt
    
    %% OpenTelemetry Aggregation
    TelOT["Telemetry.Extensions.OpenTelemetry"]
    
    TelOT --> TelAbs
    TelOT --> AuthAbs
    TelOT --> AuthHttp
    TelOT --> MTHttp
```

### WebApi
The main entry point for the platform. It is a Minimal API project that hosts the modular monolith. It aggregates APIs from all domains and provides a unified interface.
- **Documentation**: Uses [Scalar](https://github.com/scalar/scalar) (available at `/api-doc`) for verifying the OpenAPI V3 specification. 
    - Provides an interactive API reference.
    - Includes authentication support for testing secure endpoints.
- **Error Handling**: Implements standardized **Problem Details** (RFC 7807) for all API errors.
    - **Extensions**: Custom fields `traceId`, `errorCode`, and `requestTime` are included.
    - **Integration**: `FluentResults` extensions map domain errors directly to standard HTTP status codes (`VALIDATION_ERROR` -> 400, `NOT_FOUND` -> 404, etc.) with consistent `type` URIs.
    - **Compatibility**: Uses `Microsoft.AspNetCore.OpenApi` (v10) with a custom transformer to generate referenced schemas compatible with Scalar and other tools.
- **Validation**: Uses [FluentValidation](https://docs.fluentvalidation.net/) for defining strongly-typed rules.
    - **Automatic Registration**: Validators inheriting from `AbstractValidator<T>` are automatically discovered and registered.
    - **Integration**: Runs before the endpoint logic. Invalid requests return a **400 Bad Request** Problem Details response with a `DATA_VALIDATION_FAILED` error code.
    - **OpenAPI**: Validation rules (required, length, regex, ranges) are automatically reflected in the OpenAPI V3 schema.
- **Deployment**: Automated via GitHub Actions using a [Reusable Container App Workflow](.github/workflows/templates/README.md).

## Project Structure

The solution is organized into modular domains (e.g., **Tenancy**, **Identity**). Each domain follows a strict Clean Architecture layering strategy with the following projects:

### Domain Layering
Each domain (`src/<Domain>`) consists of the following libraries:

| Project | Role | Dependencies |
|---------|------|--------------|
| **`<Domain>.Domain`** | **Core Domain Models** (DDD Aggregates, Entities, Value Objects) | *None* |
| **`<Domain>.Actors.Abstractions`** | **Orleans Interfaces** (Actor Contracts) | `Domain` |
| **`<Domain>.Actors`** | **Orleans Implementations** (Stateful Logic) | `Actors.Abstractions`, `Store` |
| **`<Domain>.Store`** | **Data Persistence** (Repositories, DB Contexts) | `Core` (Interfaces), `Domain` (Entities) |
| **`<Domain>.Core`** | **Business Logic** (Services, Use Cases) | `Domain`, `Actors.Abstractions` |
| **`<Domain>.Infrastructure`** | **External Services** (Email, Bus, 3rd Party APIs) | `Core` (Interfaces) |
| **`<Domain>.WebApi`** | **Domain API Definitions** (Endpoints, Route Groups) | All Domain Projects |
| **`WebApi`** | **Platform Entry Point** (Host, Configuration) | `<Domain>.WebApi` (All Modules) |

### Dependency Graph (Domain Internal)

```mermaid
graph TD
    PlatformHost["WebApi (Host)"] --> DomainWebApi["<Domain>.WebApi"]
    DomainWebApi --> Core
    DomainWebApi --> Store
    DomainWebApi --> Infrastructure
    DomainWebApi --> Actors
    
    Infrastructure --> Core
    Store --> Core
    
    Actors --> Actors.Abstractions
    Actors --> Store
    
    Core --> Actors.Abstractions
    Core --> Domain
    
    Actors.Abstractions --> Domain
```

### System Overview

This diagram shows how the `WebApi` host aggregates multiple independent domain modules.

```mermaid
graph TD
    Host["WebApi (Entry Point)"]
    
    subgraph "Modular Monolith"
        Host --> Module1["Domain 1 Module"]
        Host --> Module2["Domain 2 Module"]
        Host -.-> ModuleN["Domain N Module..."]
    end

    subgraph "Domain 1 Module"
        Module1 --> D1API["API Definitions"]
        D1API --> D1Core["Business Logic"]
    end
```

*Note: `Store` and `Infrastructure` depend on `Core` to implement interfaces defined there (Dependency Inversion).*

## Architecture Rules
1.  **Strict Isolation**: Domains cannot reference each other directly (e.g., `Tenancy` cannot reference `Identity`).
2.  **Exception**: Domains **MAY** reference another domain's `Actors.Abstractions` to communicate via Orleans.
3.  **Circular Dependency Resolution**: `Core` depends on `Actors.Abstractions` to invoke actors; `Actors` depends on `Store` for persistence; `Store` depends on `Core` for interfaces.

## License
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
