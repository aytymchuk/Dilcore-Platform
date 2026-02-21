# Blueprints Module

The **Blueprints** module is a domain module within the Dilcore Platform responsible for managing blueprint definitions. It follows the platform's standard layered architecture, separating contracts, domain logic, application core, actors, persistence, and HTTP API concerns into dedicated projects.

## Project Structure

```
src/Blueprints/
├── Blueprints.Actors.Abstractions/   # Orleans grain interfaces for Blueprints
├── Blueprints.Actors/                # Orleans grain implementations
├── Blueprints.Contracts/             # Public-facing DTOs and API contracts
├── Blueprints.Domain/                # Domain entities and value objects
├── Blueprints.Core/                  # Application logic (MediatR commands/queries, AutoMapper profiles)
├── Blueprints.Infrastructure/        # Infrastructure services and integrations
├── Blueprints.Store/                 # MongoDB persistence layer
└── Blueprints.WebApi/                # HTTP endpoints and DI registration
```

## Project Responsibilities

| Project | Responsibility |
|---|---|
| `Blueprints.Contracts` | Shared request/response records consumed by clients |
| `Blueprints.Domain` | Core domain entities and business rules, no external dependencies |
| `Blueprints.Actors.Abstractions` | Orleans grain interface definitions |
| `Blueprints.Actors` | Orleans grain implementations for distributed blueprint state |
| `Blueprints.Core` | MediatR command/query handlers, AutoMapper profiles, FluentValidation validators |
| `Blueprints.Infrastructure` | External service integrations |
| `Blueprints.Store` | MongoDB document model (`BlueprintDocument`) and repository registration |
| `Blueprints.WebApi` | Minimal API endpoint mapping (`/blueprints`) and module DI wiring |

## Key Technologies

- **MediatR** — Command/Query pattern for all business logic in `Blueprints.Core`
- **AutoMapper** — Object mapping between domain models and contracts
- **FluentValidation** — Input validation on commands and queries
- **Orleans** — Distributed actor model via `Blueprints.Actors`
- **MongoDB** — Document persistence via `Blueprints.Store`

## Getting Started

### Register the Module

In your `WebApplicationBuilder` setup, call:

```csharp
builder.AddBlueprintsModule();
```

### Map Endpoints

After building the app, map the Blueprints HTTP routes:

```csharp
app.MapBlueprintsEndpoints();
```

### Configuration

The store requires a MongoDB connection string. Add the following section to your `appsettings.json`:

```json
{
  "Blueprints": {
    "MongoDb": {
      "ConnectionString": "mongodb://localhost:27017"
    }
  }
}
```

## Tests

Corresponding test projects live under `tests/Blueprints/` and mirror this structure:

```
tests/Blueprints/
├── Blueprints.Actors.Abstractions.Tests/
├── Blueprints.Actors.Tests/
├── Blueprints.Architecture.Tests/
├── Blueprints.Contracts.Tests/
├── Blueprints.Core.Tests/
├── Blueprints.Domain.Tests/
├── Blueprints.Infrastructure.Tests/
├── Blueprints.Store.Tests/
└── Blueprints.WebApi.Tests/
```

Run all Blueprints tests:

```bash
dotnet test tests/Blueprints/
```
