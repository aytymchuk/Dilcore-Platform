---
description: Create a new domains module based on architecture guidelines
---
# Create New Module

This workflow scaffolds a new module in `src/[ModuleName]` and its tests in `tests/[ModuleName]` based on the architecture rules defined in `.agent/rules/module-architecture.md` and the project setup rules in `.agent/rules/project-files.md`.

## Step 1: Prompt for Module Name
Ask the user for the name of the new module (e.g., `Catalog`, `Billing`). If the user already provided it, proceed.
Ensure the module name is in PascalCase. All placeholders like `[ModuleName]` must be replaced with the provided module name. For `[module-name-lowercase]`, convert it to lowercase.

## Step 2: Create Module Directories and Projects
// turbo-all
Run the following bash script to create the main projects. Replace `[ModuleName]` with the actual module name (e.g. `Catalog`):

```bash
MODULE_NAME=[ModuleName]
SRC_DIR="src/$MODULE_NAME"
TESTS_DIR="tests/$MODULE_NAME"

mkdir -p "$SRC_DIR"
mkdir -p "$TESTS_DIR"

# Sub-modules
PROJECTS=(
  "Actors"
  "Actors.Abstractions"
  "Contracts"
  "Core"
  "Domain"
  "Infrastructure"
  "Store"
  "WebApi"
)

# Create src projects
for PROJ in "${PROJECTS[@]}"; do
    mkdir -p "$SRC_DIR/$MODULE_NAME.$PROJ"
    
    # Create minimal compliant csproj
    cat <<EOF > "$SRC_DIR/$MODULE_NAME.$PROJ/$MODULE_NAME.$PROJ.csproj"
<Project Sdk="Microsoft.NET.Sdk">
</Project>
EOF

    # Add to solution under 'src/Modules/$MODULE_NAME' solution folder
    dotnet sln add "$SRC_DIR/$MODULE_NAME.$PROJ/$MODULE_NAME.$PROJ.csproj" --solution-folder "src/$MODULE_NAME"
done

# Create tests projects
# Architecture tests
mkdir -p "$TESTS_DIR/$MODULE_NAME.Architecture.Tests"
cat <<EOF > "$TESTS_DIR/$MODULE_NAME.Architecture.Tests/$MODULE_NAME.Architecture.Tests.csproj"
<Project Sdk="Microsoft.NET.Sdk">
</Project>
EOF
dotnet sln add "$TESTS_DIR/$MODULE_NAME.Architecture.Tests/$MODULE_NAME.Architecture.Tests.csproj" --solution-folder "tests/$MODULE_NAME"

for PROJ in "${PROJECTS[@]}"; do
    mkdir -p "$TESTS_DIR/$MODULE_NAME.$PROJ.Tests"
    
    # Create minimal compliant csproj
    cat <<EOF > "$TESTS_DIR/$MODULE_NAME.$PROJ.Tests/$MODULE_NAME.$PROJ.Tests.csproj"
<Project Sdk="Microsoft.NET.Sdk">
</Project>
EOF

    # Add to solution under 'tests/$MODULE_NAME'
    dotnet sln add "$TESTS_DIR/$MODULE_NAME.$PROJ.Tests/$MODULE_NAME.$PROJ.Tests.csproj" --solution-folder "tests/$MODULE_NAME"
done
```

## Step 3: Set Up Project Dependencies
// turbo-all
Run the following script to set up correct project references based on the architecture rules:

```bash
MODULE_NAME=[ModuleName]
SRC_DIR="src/$MODULE_NAME"
TESTS_DIR="tests/$MODULE_NAME"

# WebApi
dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" reference "$SRC_DIR/$MODULE_NAME.Contracts/$MODULE_NAME.Contracts.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" reference "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" reference "$SRC_DIR/$MODULE_NAME.Store/$MODULE_NAME.Store.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" reference "$SRC_DIR/$MODULE_NAME.Actors/$MODULE_NAME.Actors.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" reference "$SRC_DIR/$MODULE_NAME.Actors.Abstractions/$MODULE_NAME.Actors.Abstractions.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" reference "$SRC_DIR/$MODULE_NAME.Infrastructure/$MODULE_NAME.Infrastructure.csproj"

# Core
dotnet add "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj" reference "$SRC_DIR/$MODULE_NAME.Domain/$MODULE_NAME.Domain.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj" reference "$SRC_DIR/$MODULE_NAME.Actors.Abstractions/$MODULE_NAME.Actors.Abstractions.csproj"

# Store
dotnet add "$SRC_DIR/$MODULE_NAME.Store/$MODULE_NAME.Store.csproj" reference "$SRC_DIR/$MODULE_NAME.Domain/$MODULE_NAME.Domain.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.Store/$MODULE_NAME.Store.csproj" reference "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj"

# Actors
dotnet add "$SRC_DIR/$MODULE_NAME.Actors/$MODULE_NAME.Actors.csproj" reference "$SRC_DIR/$MODULE_NAME.Actors.Abstractions/$MODULE_NAME.Actors.Abstractions.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.Actors/$MODULE_NAME.Actors.csproj" reference "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj"
dotnet add "$SRC_DIR/$MODULE_NAME.Actors/$MODULE_NAME.Actors.csproj" reference "$SRC_DIR/$MODULE_NAME.Domain/$MODULE_NAME.Domain.csproj"

# Infrastructure
dotnet add "$SRC_DIR/$MODULE_NAME.Infrastructure/$MODULE_NAME.Infrastructure.csproj" reference "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj"

# Test dependencies
# For Architecture tests
dotnet add "$TESTS_DIR/$MODULE_NAME.Architecture.Tests/$MODULE_NAME.Architecture.Tests.csproj" reference "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj"
dotnet add "$TESTS_DIR/$MODULE_NAME.Architecture.Tests/$MODULE_NAME.Architecture.Tests.csproj" package NetArchTest.Rules

for PROJ in "Actors" "Actors.Abstractions" "Contracts" "Core" "Domain" "Infrastructure" "Store" "WebApi"; do
    dotnet add "$TESTS_DIR/$MODULE_NAME.$PROJ.Tests/$MODULE_NAME.$PROJ.Tests.csproj" reference "$SRC_DIR/$MODULE_NAME.$PROJ/$MODULE_NAME.$PROJ.csproj"
done
```

## Step 4: Add Common External Dependencies
// turbo-all
For standard modules, `Core` usually requires `MediatR`, `FluentResults`, and `AutoMapper`. `WebApi` may require `FluentValidation`. Run the following to add common packages (versions are managed centrally, so omit versions):

```bash
MODULE_NAME=[ModuleName]
SRC_DIR="src/$MODULE_NAME"

dotnet add "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj" package MediatR
dotnet add "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj" package FluentResults
dotnet add "$SRC_DIR/$MODULE_NAME.Core/$MODULE_NAME.Core.csproj" package AutoMapper

dotnet add "$SRC_DIR/$MODULE_NAME.WebApi/$MODULE_NAME.WebApi.csproj" package FluentValidation
```

## Step 5: Add Basic Package and Project Reference Setup
Compare with a similar module (e.g. `src/Tenancy`) and add the same `PackageReference`s and Common/Identity `ProjectReference`s to each `[ModuleName]` project. Edit the `.csproj` files so they match the reference module layout.

**WebApi** (`[ModuleName].WebApi.csproj`):
- Add `FrameworkReference Include="Microsoft.AspNetCore.App"` (if not already present).
- Packages: `Finbuckle.MultiTenant.AspNetCore`, `FluentValidation.DependencyInjectionExtensions`. Remove standalone `FluentValidation` if present.
- Project refs: `Common/FluentValidation/FluentValidation.Extensions.MinimalApi`, `Common/FluentResults/Results.Extensions.Api`, plus existing module refs (Contracts, Core, Store, Actors, Actors.Abstractions, Infrastructure).

**Store** (`[ModuleName].Store.csproj`):
- Packages: `Dilcore.DocumentDb.MongoDb`, `Dilcore.DocumentDb.MongoDb.Repositories`.
- Project refs: `Common/Configuration/Configuration.Extensions`, `[ModuleName].Core`, `[ModuleName].Domain`.

**Domain** (`[ModuleName].Domain.csproj`):
- Project refs: `Common/Domain/Domain.Abstractions`.

**Core** (`[ModuleName].Core.csproj`):
- Packages: `MediatR`, `FluentResults`, `AutoMapper` (Step 4).
- Project refs: `[ModuleName].Domain`, `[ModuleName].Actors.Abstractions`, `Common/MediatR/MediatR.Abstractions`, `Common/MediatR/MediatR.Extensions`, `Common/MultiTenant/MultiTenant.Abstractions`, `Common/FluentResults/Results.Abstractions`, `Identity/Identity.Actors.Abstractions`, `Common/Authentication/Authentication.Abstractions`, `[ModuleName].Contracts`.

**Contracts** (`[ModuleName].Contracts.csproj`):
- Packages: `FluentValidation`.

**Actors** (`[ModuleName].Actors.csproj`):
- Packages: `Microsoft.Orleans.Server`, `Microsoft.Orleans.Reminders`, `Microsoft.Extensions.Logging.Abstractions`, `AutoMapper`.
- Project refs: `[ModuleName].Actors.Abstractions`, `[ModuleName].Core`, `Identity/Identity.Actors.Abstractions`, `Common/Authentication/Authentication.Abstractions`.

**Actors.Abstractions** (`[ModuleName].Actors.Abstractions.csproj`):
- Packages: `Microsoft.Orleans.Core.Abstractions`, `Microsoft.Orleans.CodeGenerator` (with `PrivateAssets`/`IncludeAssets` as in the reference module).

**Infrastructure** (`[ModuleName].Infrastructure.csproj`):
- No extra packages; project ref to `[ModuleName].Core` only (already set in Step 3).

Use `src/Tenancy` as the reference for exact `ItemGroup` layout and any `PrivateAssets`/`IncludeAssets` on package references.

## Step 6: Create Basic Registration Files
Create the essential files for module registration.

1. Create `src/[ModuleName]/[ModuleName].WebApi/WebApplicationBuilderExtensions.cs` containing:
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Dilcore.[ModuleName].WebApi;

/// <summary>
/// Service collection extensions for [ModuleName].WebApi dependency injection.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds all [ModuleName] module services including Core and WebApi components.
    /// </summary>
    public static WebApplicationBuilder Add[ModuleName]Module(this WebApplicationBuilder builder)
    {
        // Add Core services (MediatR handlers and behaviors)
        // builder.Services.Add[ModuleName]Application();

        // Add Store services
        // builder.Services.Add[ModuleName]Store(builder.Configuration);

        return builder;
    }
}
```

2. Create `src/[ModuleName]/[ModuleName].WebApi/EndpointExtensions.cs` containing:
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Dilcore.[ModuleName].WebApi;

/// <summary>
/// HTTP endpoints for the [ModuleName] module.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all [ModuleName] module endpoints.
    /// </summary>
    public static void Map[ModuleName]Endpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/[module-name-lowercase]")
            .WithTags("[ModuleName]")
            .RequireAuthorization();
            
        // Map endpoints here
    }
}
```

## Step 7: Register Module in WebApi Host
1. Open `src/WebApi/Extensions/ServiceCollectionExtensions.cs` and add `builder.Add[ModuleName]Module();` inside the `AddDomainModules` extension method. You will need to add a `using Dilcore.[ModuleName].WebApi;` at the top of the file as well.
2. Open `src/WebApi/Extensions/EndpointExtensions.cs` and add `app.Map[ModuleName]Endpoints();` inside the `MapApplicationEndpoints` method. You will need to add a `using Dilcore.[ModuleName].WebApi;` at the top of the file as well.

## Step 8: Create Architecture Tests
Look at existing Architecture Tests such as `tests/Tenancy/Tenancy.Architecture.Tests/ArchitectureTests.cs` and create a similar file in `tests/[ModuleName]/[ModuleName].Architecture.Tests/ArchitectureTests.cs` to enforce that layer dependencies are clean, particularly verifying that Domain does not depend on anything, and other layers conform strictly to the module architecture rules.

## Step 9: Clean and Verify
// turbo-all
Build the solution using `dotnet build` to ensure all projects compile and references are correct.

```bash
dotnet clean
dotnet build
```
