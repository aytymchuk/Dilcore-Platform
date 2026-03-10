---
name: dilcore-architecture
description: Describes the Dilcore Platform project structure, layer dependencies, and architectural rules enforced by ArchUnitNET tests. Use when adding new code, creating modules, resolving dependency issues, or making architectural decisions in the Dilcore Platform codebase.
---

# Dilcore Platform Architecture

## Project Structure

```text
src/
├── Common/           # Shared infrastructure (Auth, Domain, MediatR, MultiTenant, etc.)
├── Identity/         # Identity module
├── Tenancy/          # Tenancy module
├── Blueprints/       # Blueprints module
├── WebApi/           # API host
├── WebApi.Client/    # Typed Refit API client
└── Web/              # Blazor WebApp host
```

## Module Layer Pattern

Each domain module (Identity, Tenancy, Blueprints) follows the same 7-layer structure. Architecture tests (ArchUnitNET) enforce the dependency rules below.

## Layer Dependency Matrix

Each cell shows whether the **row** layer may depend on the **column** layer.

| From ↓ / To → | Domain | Contracts | Actors.Abs | Core | Store | Infrastructure | Actors | WebApi |
|---|---|---|---|---|---|---|---|---|
| **Domain** | - | NO | NO | NO | NO | NO | NO | NO |
| **Contracts** | NO | - | NO | NO | NO | NO | NO | NO |
| **Actors.Abstractions** | YES | NO | - | NO | NO | NO | NO | NO |
| **Core** | YES | YES | YES | - | NO | NO | NO | NO |
| **Store** | YES | NO | NO | YES | - | NO | NO | NO |
| **Infrastructure** | YES | NO | NO | YES | NO | - | NO | NO |
| **Actors** | YES | NO | YES | NO* | YES | NO | - | NO |
| **WebApi** | YES | YES | YES | YES | YES | YES | YES | - |

\* Blueprints forbids Actors→Core; Identity allows it transitively

### Per-Layer Rules (enforced by architecture tests)

**Domain** — Innermost layer. Zero internal dependencies.
- Must NOT depend on: Core, Store, Infrastructure, Actors, Actors.Abstractions, WebApi
- May reference: Common/Domain.Abstractions, NuGet packages only

**Contracts** — API boundary. DTOs, FluentValidation validators. Shared with clients.
- Must NOT depend on: Domain, Core, Store, Infrastructure, Actors, Actors.Abstractions, WebApi
- May reference: FluentValidation, NuGet packages only
- If validation needs to mirror Domain logic, duplicate it in a self-contained helper (e.g. `SchemaNameHelper`)

**Actors.Abstractions** — Orleans grain interfaces and serializable DTOs.
- Must NOT depend on: Core, Store, Infrastructure, Actors, WebApi
- May reference: Domain

**Core** — MediatR command/query handlers, AutoMapper profiles, pipeline behaviors.
- Must NOT depend on: Store, Infrastructure, Actors, WebApi
- May reference: Domain, Contracts, Actors.Abstractions, Common

**Store** — MongoDB persistence, repository implementations.
- Must NOT depend on: Infrastructure, Actors, Actors.Abstractions, WebApi
- May reference: Core, Domain

**Infrastructure** — External service integrations.
- Must NOT depend on: Store, Actors, Actors.Abstractions, WebApi
- May reference: Core, Domain

**Actors** — Orleans grain implementations, grain storage.
- Must NOT depend on: Infrastructure, WebApi
- Must NOT depend on: Core *(Blueprints rule; Identity currently allows it)*
- May reference: Domain, Store, Actors.Abstractions

**WebApi** — HTTP endpoint mapping, DI wiring, module registration.
- May reference: All module layers

## Cross-Module Rules

Modules must not depend on each other's internal layers. The only allowed cross-module dependency is **Actors.Abstractions**.

| Allowed | Forbidden |
|---|---|
| Identity → Tenancy.Actors.Abstractions | Identity → Tenancy.Core |
| Tenancy → Identity.Actors.Abstractions | Tenancy → Identity.Domain |
| Blueprints → Identity.Actors.Abstractions | Blueprints → Identity.Store |

This is enforced by `No_Cross_Domain_Dependencies_Except_ActorsAbstractions` tests.

## WebApi.Client Rules

The typed API client (`WebApi.Client`) has its own constraints:

| Rule | Enforced By |
|---|---|
| Must NOT depend on WebApi (server) | `ClientProject_ShouldNotDependOnWebApiProject` |
| Must NOT depend on Infrastructure | `ClientProject_ShouldNotDependOnInfrastructureProjects` |
| Must NOT depend on Core, Domain, or Actors | `ClientProject_ShouldOnlyDependOnContractsProjects` |
| May only depend on **Contracts** projects | (above tests combined) |
| Client interfaces must be public | `Clients_Should_Only_Expose_Interfaces_Publicly` |

## Common Projects

Shared across modules — not subject to the per-module layer rules:

| Project | Purpose |
|---|---|
| Domain.Abstractions | Base entities (`BaseDomain`), shared interfaces |
| MediatR.Abstractions, MediatR.Extensions | CQRS command/query infrastructure |
| Results.Abstractions, Results.Extensions.Api | FluentResults-based result types |
| MultiTenant.Abstractions, MultiTenant.Http.Extensions | Tenant resolution |
| Authentication.Abstractions, Authentication.Http.Extensions | Auth |
| FluentValidation.Extensions.MinimalApi | Endpoint validation filters |
| Configuration.Extensions, Configuration.AspNetCore | Config binding |
| Telemetry.Abstractions, Telemetry.Extensions.OpenTelemetry | Observability |
| CorrelationId.Abstractions, CorrelationId.Http.Extensions | Request correlation |

## Request Flow

```text
HTTP → WebApi → Core (MediatR) → Actors (Orleans Grain) → Store (MongoDB)
```

## When Adding New Code

1. **New module**: Create all 7 layers, including Contracts. Add architecture tests mirroring the existing pattern.
2. **New validation in Contracts**: Use self-contained helpers. Never add `ProjectReference` to Domain.
3. **New domain logic**: Put it in Domain. Core/Actors/Store reference Domain — not the other way.
4. **New API endpoint**: WebApi maps to Core command/query; Contracts define request/response DTOs.
5. **Cross-module dependency**: Only reference the other module's Actors.Abstractions. Never Core, Domain, Store, etc.
6. **WebApi.Client**: Only depend on Contracts projects. Expose only public interfaces.
