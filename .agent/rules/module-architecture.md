# Module Architecture & Dependency Guidelines

This document outlines the architectural rules and dependency constraints for the solution's modules (e.g., `src/Tenancy`, `src/Identity`).

## General Principles

1.  **Domain Independence**: Domains (like `Tenancy` and `Identity`) are separate. All modules are independent and not related.
2.  **Cross-Module Communication**: Communication across modules is typically handled via `Module.Actors.Abstractions`.

## Layer Responsibilities and Dependencies

### Module.Contracts
*   **Role**: Data Transfer Objects (DTOs).
*   **Rules**:
    *   Must **ONLY** be used by `Module.WebApi`.
    *   Strictly forbidden in other layers `Core`, `Domain`, `Actors` etc.

### Module.WebApi
*   **Role**: Entrypoint to the module.
*   **Responsibilities**:
    *   Describes API endpoints.
    *   Handles service registrations.
    *   **Does not contain specific logic.**
    *   Invokes feature-specific commands and queries from the modules.
*   **Allowed Dependencies**:
    *   `Module.Contracts`
    *   `Module.Core`
    *   `Module.Store`
    *   `Module.Actors`
    *   `Module.Actors.Abstractions`
    *   `Module.Infrastructure`

### Module.Core
*   **Role**: Main logical part of the module.
*   **Responsibilities**:
    *   Logic orchestration.
    *   Module features wrapped via Mediator pattern.
    *   Contains service abstractions for Store, Core, and Infrastructure levels.
    *   Processes functionality via appropriate actors or store repositories.
*   **Allowed Dependencies**:
    *   `Module.Store` (via abstractions)
    *   `Module.Actors`
    *   `Module.Actors.Abstractions`
    *   `Module.Domain`

### Module.Store
*   **Role**: Data Access Layer.
*   **Responsibilities**:
    *   Repositories.
    *   Data objects.
*   **Allowed Dependencies**:
    *   `Module.Domain`

### Module.Actors
*   **Role**: Grain-based part (Orleans Actors).
*   **Responsibilities**:
    *   Contains actors (grains) that are entity-specific.
    *   Manipulates domain logic and applies domain models to state.
    *   **Only actors can perform domain logic.**
    *   Communicates with other actors via `Module.Actors.Abstractions`.
*   **Allowed Dependencies**:
    *   `Module.Actors.Abstractions`
    *   `Module.Domain`
    *   `Module.Store`

### Module.Actors.Abstractions
*   **Role**: Grain interfaces and grain contracts.
*   **Responsibilities**:
    *   Defines the public interface for Actors.
    *   **Available to use across different modules.**

### Module.Domain
*   **Role**: Rich Domain Models.
*   **Responsibilities**:
    *   Contains specific business entity logic.
*   **Allowed Dependencies**:
    *   None (Pure POCO/Domain objects).

### Module.Infrastructure
*   **Role**: Infrastructure specific implementations.
*   **Note**: Usually implements abstractions defined in `Module.Core`.
