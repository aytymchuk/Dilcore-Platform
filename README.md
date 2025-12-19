# Dilcore Platform

[![CodeRabbit Reviews](https://img.shields.io/coderabbit/prs/github/aytymchuk/Dilcore-Platform?utm_source=oss&utm_medium=github&utm_campaign=aytymchuk%2FDilcore-Platform&labelColor=171717&color=FF570A&link=https%3A%2F%2Fcoderabbit.ai&label=CodeRabbit+Reviews)](https://coderabbit.ai)

A new platform based on .NET 10.

## Domains

### Tenancy
Enables multitenancy capabilities within the platform. This domain manages tenants and controls user access to specific tenant environments.

### Identity
Handles user management at the platform level. It manages users and their relationships to tenants, including defining access levels and permissions.

### WebApi
The main entry point for the platform. It is a Minimal API project that hosts the modular monolith. It aggregates APIs from all domains and provides a unified interface.
- **Documentation**: Uses Scalar (available at `/api-doc` in Development).
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
