# Orleans Integration Architecture Plan
**Dilcore Platform - Distributed Actor Model Implementation**

**Version**: 1.0
**Date**: 2026-01-07
**Target Orleans Version**: 9.2.1 (Latest Stable)
**Status**: Planning Phase

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current Architecture Analysis](#current-architecture-analysis)
3. [Orleans Architecture Overview](#orleans-architecture-overview)
4. [Integration Strategy](#integration-strategy)
5. [Multi-Tenancy Design](#multi-tenancy-design)
6. [MediatR and Orleans Interaction Patterns](#mediatr-and-orleans-interaction-patterns)
7. [OpenTelemetry Integration](#opentelemetry-integration)
8. [Implementation Phases](#implementation-phases)
9. [Detailed Implementation Steps](#detailed-implementation-steps)
10. [Performance Considerations](#performance-considerations)
11. [Security & Best Practices](#security--best-practices)
12. [Testing Strategy](#testing-strategy)
13. [Operational Considerations](#operational-considerations)
14. [References & Resources](#references--resources)

---

## 1. Executive Summary

This document outlines the comprehensive plan for integrating Microsoft Orleans (v9.2.1) into the Dilcore Platform to enable distributed, high-performance, actor-based processing while maintaining the existing clean architecture principles with MediatR, multi-tenancy, and OpenTelemetry.

### Key Objectives

- ✅ Enable distributed actor model processing using Orleans
- ✅ Maintain multi-tenant isolation at the grain level
- ✅ Preserve MediatR CQRS patterns for API entry points
- ✅ Ensure end-to-end distributed tracing with OpenTelemetry
- ✅ Achieve horizontal scalability and high availability
- ✅ Implement tenant-aware grain activation and state management

### Architecture Principles

1. **Co-hosted Deployment**: Orleans silo co-hosted within WebApi for serverless-style deployment
2. **Hybrid Processing**: WebApi → MediatR → Orleans Grains (not replacing MediatR)
3. **Tenant Isolation**: Tenant ID embedded in grain keys for complete isolation
4. **Distributed Tracing**: Activity propagation from HTTP → MediatR → Grains
5. **Resilient State**: In-memory state backed by repository pattern for durable storage

---

## 2. Current Architecture Analysis

### 2.1 Existing Architecture Strengths

The Dilcore Platform is built with modern .NET practices:

**Modular Monolith Structure**:
- Domain modules: Identity, Tenancy
- Shared infrastructure: Authentication, Configuration, MediatR, MultiTenant, Telemetry
- WebApi: Entry point orchestrating features

**Clean Architecture Patterns**:
- **CQRS via MediatR**: Commands and Queries with pipeline behaviors
- **Multi-Tenancy via Finbuckle**: Header-based tenant resolution with provider pattern
- **Observability via OpenTelemetry**: Unified processors enriching traces/logs with tenant/user context
- **Resilience via Polly**: Retry, timeout, circuit breaker policies on HTTP clients
- **Result Pattern via FluentResults**: Typed error handling

### 2.2 Current Request Flow

```
HTTP Request
  ↓ [x-tenant header]
  ↓ UseMultiTenant() → Finbuckle resolves tenant
  ↓ TenantEnforcementMiddleware → Validates tenant present
  ↓ UseAuthentication/UseAuthorization
  ↓ Endpoint (Minimal API)
  ↓ MediatR.Send(Query/Command)
  ↓ TracingBehavior → Creates Activity "MediatR: {RequestName}"
  ↓ LoggingBehavior → Logs request
  ↓ Handler → Business logic (HttpClient, Repository, etc.)
  ↓ UnifiedActivityProcessor.OnEnd → Enriches with tenant/user attributes
  ↓ Response
```

### 2.3 Integration Points for Orleans

Orleans will integrate at the **Handler layer**:

```
HTTP Request
  ↓ [x-tenant header]
  ↓ Tenant Resolution
  ↓ Authentication
  ↓ Endpoint
  ↓ MediatR.Send(Query/Command)
  ↓ TracingBehavior
  ↓ Handler
      ↓ Resolve IGrainFactory
      ↓ Get Grain with Tenant-Scoped Key
      ↓ Call Grain Method ← NEW
      ↓ Activity propagates to Grain
      ↓ Grain executes business logic
      ↓ Returns Result
  ↓ UnifiedActivityProcessor.OnEnd
  ↓ Response
```

**Key Insight**: Orleans grains become the **execution layer** for stateful, distributed operations, invoked from MediatR handlers.

---

## 3. Orleans Architecture Overview

### 3.1 Core Concepts

**Grain**: Virtual actor that represents a single entity (e.g., User, Order, Session)
- Uniquely identified by type + key (string/GUID/integer/compound)
- Single-threaded execution guarantees (no race conditions)
- Automatically activated on first access, deactivated when idle
- Location-transparent (Orleans routes calls across cluster)

**Silo**: Orleans server process hosting grains
- Multiple silos form a cluster
- Grains distributed across silos for load balancing
- Silos use clustering provider (Azure Storage) to discover each other

**Grain State**: Persistent or in-memory state managed by `IPersistentState<T>`
- Loaded on activation (OnActivateAsync)
- Saved on demand (state.WriteStateAsync)
- Cleared on deactivation

**Activity Propagation**: Orleans supports distributed tracing via DiagnosticListener
- Traces flow from caller → grain → nested grain calls
- Requires enabling sources: "Microsoft.Orleans.Runtime" and "Microsoft.Orleans.Application"

### 3.2 Co-Hosted Silo Architecture

**Deployment Model**: Single WebApi process hosts both ASP.NET Core and Orleans silo

```
┌─────────────────────────────────────────────────┐
│  Azure App Service / Container / VM             │
│  ┌───────────────────────────────────────────┐  │
│  │  WebApi Process                           │  │
│  │  ┌─────────────────┐  ┌─────────────────┐ │  │
│  │  │  ASP.NET Core   │  │  Orleans Silo   │ │  │
│  │  │  - Controllers  │  │  - Grains       │ │  │
│  │  │  - Endpoints    │  │  - State        │ │  │
│  │  │  - MediatR      │  │  - Timers       │ │  │
│  │  │  - Middleware   │◄─┼─ IGrainFactory │ │  │
│  │  └─────────────────┘  └─────────────────┘ │  │
│  └───────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
          ▲                        ▲
          │                        │
    HTTP Requests            Grain-to-Grain Calls
                             Cluster Membership
                             (via Azure Storage)
```

**Benefits**:
- Simplified deployment (single artifact)
- Direct in-process grain calls (lower latency)
- Shared dependency injection container
- Shared telemetry pipeline

**Trade-offs**:
- Vertical scaling only within process (horizontal scaling via multiple instances)
- Restarts affect both API and grains (mitigated by Orleans cluster redistribution)

### 3.3 Clustering with Azure Storage

**Mechanism**: Silos use Azure Table Storage for cluster membership

```
Azure Storage Account: $(PLATFORM_GRAIN_STORAGE_ACCOUNT_NAME)
  Table: OrleansClusterMembership
    PartitionKey: DeploymentId (e.g., "dilcore-prod")
    RowKey: SiloIdentity (ip:port:epoch)

Authentication: Managed Identity (DefaultAzureCredential)
  - Eliminates connection strings
  - Follows Azure security best practices
```

**Silo Discovery Flow**:
1. Silo starts, generates identity (IP:Port:Epoch)
2. Writes entry to membership table
3. Reads table to discover other silos
4. Establishes connections to cluster
5. Begins accepting grain activations

**Health Monitoring**:
- Silos send heartbeats to table
- Missed heartbeats → silo marked dead
- Grains redistributed to healthy silos

---

## 4. Integration Strategy

### 4.1 Package Architecture

**New NuGet Packages** (Orleans 9.2.1):

```xml
<!-- Core Orleans -->
<PackageVersion Include="Microsoft.Orleans.Server" Version="9.2.1" />
<PackageVersion Include="Microsoft.Orleans.Sdk" Version="9.2.1" />

<!-- Azure Clustering -->
<PackageVersion Include="Microsoft.Orleans.Clustering.AzureStorage" Version="9.2.1" />

<!-- Observability -->
<PackageVersion Include="OrleansDashboard" Version="8.2.0" />
```

**Note**: Issue #36 requested Orleans 10.0.0-rc.2, but Orleans 10.0 does not exist. Latest stable is 9.2.1 (July 2025). No preview releases for .NET 10 are available as of Jan 2026.

### 4.2 Project Structure

**Add New Projects** (following existing module pattern):

```
src/
  Common/
    Orleans/
      Orleans.Abstractions/             ← Grain interfaces (IUserGrain, etc.)
      Orleans.Extensions/               ← DI extensions, telemetry integration
      Orleans.MultiTenant/              ← Tenant-aware grain base classes

  Identity/
    Identity.Actors.Abstractions/       ← Already exists
    Identity.Actors/                    ← Already exists (UserGrain implementation)

  Tenancy/
    Tenancy.Actors.Abstractions/        ← Already exists
    Tenancy.Actors/                     ← Already exists (TenantGrain implementation)
```

**Rationale**:
- `Orleans.Abstractions`: Shared grain interfaces (consumed by API and implementations)
- `Orleans.Extensions`: Infrastructure plumbing (DI, telemetry, tenant context)
- `Orleans.MultiTenant`: Reusable base classes for tenant-scoped grains
- Domain modules (Identity, Tenancy): Domain-specific grain implementations

### 4.3 Configuration Model

**appsettings.json** (new section):

```json
{
  "GrainsSettings": {
    "StorageAccountName": "$(PLATFORM_GRAIN_STORAGE_ACCOUNT_NAME)",
    "ClusterId": "dilcore-cluster",
    "ServiceId": "dilcore-platform"
  },
  "TelemetrySettings": {
    "ApplicationInsightsConnectionString": "...",
    "EnableOrleansTracing": true
  }
}
```

**WebApi.csproj** (GC settings):

```xml
<PropertyGroup>
  <ServerGarbageCollection>true</ServerGarbageCollection>
  <ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
</PropertyGroup>
```

**Benefits**:
- Server GC: Optimized for multi-core throughput
- Concurrent GC: Reduces pause times during collections

---

## 5. Multi-Tenancy Design

### 5.1 Tenant Isolation Strategy

**Principle**: Tenant ID is **embedded in grain keys** to ensure complete isolation.

**Grain Key Pattern**:
```
Primary Key: {tenantId}_{entityId}
  Example: "t1_user123"
  Example: "t2_order456"

Compound Key: (tenantId, entityId)
  Example: ("t1", "user123")
  Example: ("t2", Guid)
```

**Isolation Guarantees**:
- Grains from different tenants **cannot** accidentally collide (different keys)
- Tenant A cannot access Tenant B's grain (no shared state)
- State storage can be partitioned by tenant (e.g., different table partitions)

**Alternative Considered (Rejected)**: Separate ClusterId per tenant
- **Problem**: Requires separate silo clusters per tenant (operational complexity)
- **Problem**: Does not scale to hundreds/thousands of tenants
- **Conclusion**: Tenant-scoped grain keys are the standard Orleans multi-tenancy pattern

### 5.2 Tenant-Aware Grain Base Class

**Purpose**: Encapsulate tenant context access and validation

**Implementation** (Orleans.MultiTenant project):

```csharp
// Base class for tenant-scoped grains
public abstract class TenantGrainBase : Grain
{
    protected string TenantId { get; private set; } = null!;

    protected abstract string ExtractTenantIdFromKey();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantId = ExtractTenantIdFromKey();

        if (string.IsNullOrEmpty(TenantId))
        {
            throw new InvalidOperationException(
                "Tenant ID could not be extracted from grain key");
        }

        return base.OnActivateAsync(cancellationToken);
    }
}

// Example usage (string primary key pattern: "t1_user123")
public class UserGrain : TenantGrainBase, IUserGrain
{
    protected override string ExtractTenantIdFromKey()
    {
        var key = this.GetPrimaryKeyString();
        var parts = key.Split('_', 2);
        return parts.Length == 2 ? parts[0] : string.Empty;
    }

    public Task<string> GetUserNameAsync()
    {
        // TenantId is available here
        _logger.LogInformation("Getting user for tenant {TenantId}", TenantId);
        return Task.FromResult(_state.State.Name);
    }
}
```

**Benefits**:
- Centralized tenant validation
- Grain implementers don't repeat tenant extraction logic
- Compile-time enforcement of tenant awareness

### 5.3 Grain Activation with Tenant Context

**Pattern**: MediatR handler resolves tenant, creates grain key, invokes grain

**Example Handler**:

```csharp
public class GetUserHandler : IRequestHandler<GetUserQuery, Result<UserDto>>
{
    private readonly ITenantContext _tenantContext;
    private readonly IGrainFactory _grainFactory;

    public async Task<Result<UserDto>> Handle(
        GetUserQuery request,
        CancellationToken ct)
    {
        // 1. Tenant already resolved by middleware (injected via ITenantContext)
        var tenantId = _tenantContext.Name; // e.g., "t1"

        // 2. Create tenant-scoped grain key
        var grainKey = $"{tenantId}_{request.UserId}";

        // 3. Get grain (activates if not already active)
        var userGrain = _grainFactory.GetGrain<IUserGrain>(grainKey);

        // 4. Call grain method (traced by Orleans)
        var userName = await userGrain.GetUserNameAsync();

        return Result.Ok(new UserDto(request.UserId, userName));
    }
}
```

**Trace Flow**:
```
HTTP Request [x-tenant: t1]
  ↓ Activity: "HTTP GET /users/123" [tenant.name=t1]
  ↓ MediatR Handler
      ↓ Activity: "MediatR: GetUserQuery" [tenant.name=t1]
      ↓ Grain Call
          ↓ Activity: "Orleans: IUserGrain.GetUserNameAsync" [tenant.name=t1]
          ↓ Grain execution
```

### 5.4 Tenant-Scoped State Storage

**In-Memory Storage** (Issue #36 requirement):
```csharp
// Program.cs
builder.Host.UseOrleans((context, siloBuilder) =>
{
    siloBuilder.AddMemoryGrainStorage("UserStore");
    siloBuilder.AddMemoryGrainStorage("TenantStore");
});
```

**Future: Tenant-Specific Storage Providers** (for durable state):

```csharp
// Option 1: Tenant-partitioned table storage
siloBuilder.AddAzureTableGrainStorage("UserStore", options =>
{
    options.ConfigureTableServiceClient(new Uri($"https://{storageAccount}.table.core.windows.net/"),
        new DefaultAzureCredential());
    // Partition key could be tenantId for data isolation
});

// Option 2: Separate storage per tenant (via factory pattern)
siloBuilder.AddGrainStorageAsDefault(sp =>
{
    var tenantResolver = sp.GetRequiredService<ITenantContextResolver>();
    if (tenantResolver.TryResolve(out var tenant))
    {
        // Return tenant-specific storage provider
        return GetTenantStorage(tenant.Name);
    }
    return GetDefaultStorage();
});
```

**Considerations**:
- In-memory storage is **ephemeral** (lost on silo restart)
- For production, use repository pattern to hydrate state from DB
- State acts as cache while grain is active

### 5.5 Cross-Tenant Access Control

**Security Layer**: Validate calling context matches grain tenant

**Implementation**:

```csharp
public abstract class TenantGrainBase : Grain
{
    protected void ValidateTenantAccess(string callerTenantId)
    {
        if (callerTenantId != TenantId)
        {
            throw new UnauthorizedAccessException(
                $"Tenant {callerTenantId} cannot access tenant {TenantId} resources");
        }
    }
}

// Usage in grain
public Task<string> GetSensitiveDataAsync(string callerTenantId)
{
    ValidateTenantAccess(callerTenantId);
    return Task.FromResult(_state.State.SensitiveData);
}
```

**Alternative**: Use [Orleans.Multitenant library](https://github.com/Applicita/Orleans.Multitenant)
- Automatically guards grain calls with tenant validation
- Tenant ID embedded in grain key
- UnauthorizedException thrown on unauthorized access

**Recommendation**: Start with manual validation, evaluate Orleans.Multitenant if complexity grows.

---

## 6. MediatR and Orleans Interaction Patterns

### 6.1 Architectural Decision: MediatR as API Gateway

**Pattern**: Keep MediatR at the API boundary, use Orleans for stateful processing

```
┌──────────────────┐
│  HTTP Endpoint   │
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  MediatR Handler │ ← Validation, authorization, orchestration
└────────┬─────────┘
         │
         ▼
┌──────────────────┐
│  Orleans Grain   │ ← Stateful business logic
└──────────────────┘
```

**Rationale**:
- MediatR excels at: Request validation, cross-cutting concerns, simple workflows
- Orleans excels at: Stateful entities, concurrency control, distributed coordination

**NOT Recommended**: Replacing MediatR entirely with Orleans
- Loses existing pipeline behaviors (tracing, logging, validation)
- Tightly couples API contracts to grain interfaces
- Reduces testability (grains require silo infrastructure)

### 6.2 Command Pattern Integration

**Scenario**: User updates profile

**MediatR Command**:
```csharp
public record UpdateUserProfileCommand(
    string UserId,
    string NewName,
    string NewBio) : ICommand<UserProfileDto>;
```

**Handler Implementation**:
```csharp
public class UpdateUserProfileHandler
    : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    private readonly ITenantContext _tenantContext;
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<UpdateUserProfileHandler> _logger;

    public async Task<Result<UserProfileDto>> Handle(
        UpdateUserProfileCommand cmd,
        CancellationToken ct)
    {
        try
        {
            // 1. Resolve tenant-scoped grain
            var grainKey = $"{_tenantContext.Name}_{cmd.UserId}";
            var userGrain = _grainFactory.GetGrain<IUserGrain>(grainKey);

            // 2. Delegate to grain (stateful update)
            var result = await userGrain.UpdateProfileAsync(
                cmd.NewName,
                cmd.NewBio);

            if (result.IsFailed)
                return Result.Fail(result.Errors);

            // 3. Return result
            return Result.Ok(result.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile");
            return Result.Fail("Profile update failed");
        }
    }
}
```

**Grain Implementation**:
```csharp
public interface IUserGrain : IGrainWithStringKey
{
    Task<Result<UserProfileDto>> UpdateProfileAsync(string name, string bio);
    Task<UserProfileDto> GetProfileAsync();
}

public class UserGrain : TenantGrainBase, IUserGrain
{
    private readonly IPersistentState<UserState> _state;
    private readonly IUserRepository _repository;

    public UserGrain(
        [PersistentState("user", "UserStore")] IPersistentState<UserState> state,
        IUserRepository repository)
    {
        _state = state;
        _repository = repository;
    }

    public override async Task OnActivateAsync(CancellationToken ct)
    {
        await base.OnActivateAsync(ct);

        // Load state from repository (if not in memory)
        if (_state.State.UserId == null)
        {
            var userId = ExtractUserIdFromKey();
            var user = await _repository.GetUserByIdAsync(TenantId, userId);

            if (user != null)
            {
                _state.State.UserId = user.Id;
                _state.State.Name = user.Name;
                _state.State.Bio = user.Bio;
                await _state.WriteStateAsync(); // Cache in memory store
            }
        }
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(
        string name,
        string bio)
    {
        // Update in-memory state
        _state.State.Name = name;
        _state.State.Bio = bio;
        _state.State.UpdatedAt = DateTime.UtcNow;

        // Persist to repository
        await _repository.UpdateUserAsync(
            TenantId,
            _state.State.UserId,
            name,
            bio);

        // Update memory store
        await _state.WriteStateAsync();

        return Result.Ok(new UserProfileDto(
            _state.State.UserId,
            _state.State.Name,
            _state.State.Bio));
    }

    private string ExtractUserIdFromKey()
    {
        var key = this.GetPrimaryKeyString();
        return key.Split('_', 2)[1]; // "t1_user123" → "user123"
    }
}
```

### 6.3 Query Pattern Integration

**Scenario**: Get user profile (read-only)

**MediatR Query**:
```csharp
public record GetUserProfileQuery(string UserId) : IQuery<UserProfileDto>;
```

**Handler**:
```csharp
public class GetUserProfileHandler
    : IRequestHandler<GetUserProfileQuery, Result<UserProfileDto>>
{
    public async Task<Result<UserProfileDto>> Handle(
        GetUserProfileQuery query,
        CancellationToken ct)
    {
        var grainKey = $"{_tenantContext.Name}_{query.UserId}";
        var userGrain = _grainFactory.GetGrain<IUserGrain>(grainKey);

        var profile = await userGrain.GetProfileAsync();
        return Result.Ok(profile);
    }
}
```

**Optimization**: For read-heavy workloads, use **stateless worker grains**

```csharp
// Stateless grain for read queries (no state, load-balanced)
[StatelessWorker]
public class UserQueryGrain : Grain, IUserQueryGrain
{
    private readonly IUserRepository _repository;

    public async Task<UserProfileDto> GetProfileAsync(string tenantId, string userId)
    {
        // Direct repository read (no state)
        var user = await _repository.GetUserByIdAsync(tenantId, userId);
        return new UserProfileDto(user.Id, user.Name, user.Bio);
    }
}
```

**When to use stateless vs stateful**:
- **Stateful Grain**: Entity with mutable state (User, Order, Session)
- **Stateless Worker**: Read-only queries, transformations, external API calls

### 6.4 Grain-to-Grain Communication

**Scenario**: User creates order (User grain → Order grain)

```csharp
// In UserGrain
public async Task<Result<OrderDto>> CreateOrderAsync(OrderRequest request)
{
    // Validate user can create order
    if (_state.State.OrderLimit <= _state.State.OrderCount)
        return Result.Fail("Order limit exceeded");

    // Create order grain (tenant-scoped)
    var orderKey = $"{TenantId}_{Guid.NewGuid()}";
    var orderGrain = GrainFactory.GetGrain<IOrderGrain>(orderKey);

    // Initialize order
    var result = await orderGrain.InitializeAsync(
        _state.State.UserId,
        request.Items);

    // Update user state
    _state.State.OrderCount++;
    await _state.WriteStateAsync();

    return result;
}
```

**Trace Propagation**: Orleans automatically propagates activity context across grain calls

```
Activity: "MediatR: CreateOrderCommand"
  ├─ Activity: "Orleans: IUserGrain.CreateOrderAsync"
  │   └─ Activity: "Orleans: IOrderGrain.InitializeAsync"
  │       └─ Activity: "Repository: InsertOrder"
```

---

## 7. OpenTelemetry Integration

### 7.1 Orleans Tracing Configuration

**Enable Orleans Activity Sources** (Program.cs):

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing =>
    {
        tracing.AddSource("Application.Operations");    // Existing MediatR tracing
        tracing.AddSource("Microsoft.Orleans.Runtime");  // Orleans runtime events
        tracing.AddSource("Microsoft.Orleans.Application"); // Grain method calls
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();

        if (isDevelopment)
        {
            tracing.AddConsoleExporter();
        }
        else
        {
            // Azure Monitor includes Orleans sources automatically
        }
    });
```

**Orleans Dashboard Configuration**:

```csharp
builder.Host.UseOrleans((context, siloBuilder) =>
{
    // ... clustering config ...

    siloBuilder.UseDashboard(options =>
    {
        options.HostSelf = true;
        options.Port = 8080;
        options.BasePath = "/orleans-dashboard";
    });
});
```

**Dashboard Access**: `http://localhost:8080/orleans-dashboard`

### 7.2 Activity Propagation Enhancement

**Requirement**: Ensure tenant/user context flows to grain activities

**Solution**: Extend UnifiedActivityProcessor to handle Orleans activities

**Implementation** (Telemetry.Extensions.OpenTelemetry):

```csharp
public class UnifiedActivityProcessor : BaseProcessor<Activity>
{
    private readonly IEnumerable<ITelemetryAttributeProvider> _attributeProviders;

    public override void OnEnd(Activity data)
    {
        try
        {
            // Collect attributes from providers (TenantAttributeProvider, UserAttributeProvider)
            foreach (var provider in _attributeProviders)
            {
                var attributes = provider.GetAttributes();
                foreach (var attribute in attributes)
                {
                    data.SetTag(attribute.Key, attribute.Value);
                }
            }

            // Add grain-specific attributes if available
            if (data.Source.Name.StartsWith("Microsoft.Orleans"))
            {
                // Orleans sets tags like "orleans.grain_type", "orleans.grain_key"
                // TenantAttributeProvider will add "tenant.name"
            }
        }
        finally
        {
            base.OnEnd(data);
        }
    }
}
```

**Grain-Aware Tenant Provider** (Orleans.Extensions):

```csharp
public class GrainTenantAttributeProvider : ITelemetryAttributeProvider
{
    private readonly ITenantContextResolver _tenantResolver;
    private readonly IGrainActivationContext _grainContext; // Orleans-specific

    public IEnumerable<KeyValuePair<string, object?>> GetAttributes()
    {
        // Try HTTP context first (for API calls)
        if (_tenantResolver.TryResolve(out var tenantContext))
        {
            yield return new KeyValuePair<string, object?>(
                "tenant.name",
                tenantContext.Name);
        }
        // Fallback: Extract from grain key (for grain-to-grain calls)
        else if (_grainContext != null)
        {
            var grainId = _grainContext.GrainId.ToString();
            var tenantId = ExtractTenantFromGrainId(grainId);

            if (!string.IsNullOrEmpty(tenantId))
            {
                yield return new KeyValuePair<string, object?>(
                    "tenant.name",
                    tenantId);
            }
        }
    }

    private string? ExtractTenantFromGrainId(string grainId)
    {
        // Parse "t1_user123" → "t1"
        var match = Regex.Match(grainId, @"^([^_]+)_");
        return match.Success ? match.Groups[1].Value : null;
    }
}
```

**Registration**:

```csharp
// Orleans.Extensions
services.AddTelemetryAttributeProvider<GrainTenantAttributeProvider>();
```

### 7.3 Dashboard Metrics

**Orleans Dashboard Provides**:

- **Cluster Health**: Active silos, total activations, memory usage
- **Grain Metrics**: Activations per type, throughput, latency, error rates
- **Performance**: Request rates, grain method call histograms
- **Trends**: Historical charts for capacity planning

**Custom Metrics** (via ITelemetryConsumer):

```csharp
public class CustomTelemetryConsumer : IMetricTelemetryConsumer
{
    public void TrackMetric(string name, double value, IDictionary<string, string> properties)
    {
        // Send to Application Insights, Prometheus, etc.
        _telemetryClient.TrackMetric(name, value, properties);
    }
}

// Register
siloBuilder.AddTelemetryConsumer<CustomTelemetryConsumer>();
```

**Example Metrics**:
- `grain.activations.count{tenant=t1, grain_type=UserGrain}`
- `grain.method.duration{method=UpdateProfileAsync, tenant=t1}`

---

## 8. Implementation Phases

### Phase 1: Foundation (Week 1-2)

**Goal**: Basic Orleans infrastructure and prototype UserGrain

**Tasks**:
1. Add Orleans NuGet packages to solution
2. Create Orleans.Abstractions, Orleans.Extensions, Orleans.MultiTenant projects
3. Configure co-hosted silo in Program.cs
4. Set up Azure Storage clustering (development: in-memory, production: Azure)
5. Implement TenantGrainBase base class
6. Configure GC settings in WebApi.csproj
7. Enable Orleans Dashboard
8. Create IUserGrain interface and stub implementation
9. Add integration tests for grain activation

**Deliverables**:
- ✅ Silo starts successfully in development
- ✅ Dashboard accessible at /orleans-dashboard
- ✅ UserGrain activates and deactivates
- ✅ Basic telemetry visible in dashboard

**Risks**:
- Azure Storage authentication issues → Mitigate with local development emulator
- Silo startup failures → Implement comprehensive logging

### Phase 2: Multi-Tenancy Integration (Week 3)

**Goal**: Tenant-scoped grain activation and state management

**Tasks**:
1. Implement tenant extraction logic in TenantGrainBase
2. Add tenant validation in grain activation
3. Configure in-memory grain storage
4. Create GrainTenantAttributeProvider for telemetry
5. Build tenant-scoped grain key helper utilities
6. Add unit tests for tenant isolation
7. Implement grain-level tenant access validation

**Deliverables**:
- ✅ Grains activated with tenant-scoped keys (e.g., "t1_user123")
- ✅ Tenant context flows to grain activities in traces
- ✅ Unit tests prove tenant A cannot access tenant B grains

**Risks**:
- Tenant context not available in grain → Use grain key as fallback
- Activity propagation failures → Enable Orleans sources explicitly

### Phase 3: MediatR Integration (Week 4)

**Goal**: Connect MediatR handlers to Orleans grains

**Tasks**:
1. Refactor UserGrain to implement full CRUD operations
2. Create UpdateUserProfileCommand handler invoking UserGrain
3. Create GetUserProfileQuery handler invoking UserGrain
4. Add FluentResults integration in grain methods
5. Test activity propagation (HTTP → MediatR → Grain)
6. Document MediatR → Orleans patterns
7. Create example of stateless worker grain for queries

**Deliverables**:
- ✅ HTTP endpoint → MediatR → UserGrain flow working
- ✅ End-to-end trace visible in Application Insights
- ✅ Result pattern consistently used across handlers and grains

**Risks**:
- Serialization issues with FluentResults → Use custom grain exceptions
- Performance overhead of handler → grain hop → Benchmark and optimize

### Phase 4: State Persistence (Week 5)

**Goal**: Repository pattern for durable state

**Tasks**:
1. Implement IUserRepository with EF Core/Dapper
2. Inject repository into UserGrain
3. Load state from DB in OnActivateAsync
4. Save state to DB in grain methods
5. Configure write-through caching strategy
6. Add error handling for DB failures
7. Test grain reactivation after deactivation (state reload)

**Deliverables**:
- ✅ Grain state persisted to database
- ✅ Grain reactivation loads state from DB
- ✅ In-memory state acts as cache for active grains

**Risks**:
- DB latency on activation → Implement async activation patterns
- Stale state after DB updates → Define state refresh policies

### Phase 5: Advanced Patterns (Week 6-7)

**Goal**: Timers, reminders, streams, grain-to-grain calls

**Tasks**:
1. Implement grain timers for background tasks (e.g., session expiry)
2. Add grain reminders for durable scheduling
3. Explore Orleans streams for event publishing
4. Implement grain-to-grain communication patterns
5. Add stateless worker grains for read-heavy queries
6. Performance testing and optimization
7. Implement circuit breakers for grain calls

**Deliverables**:
- ✅ Timers/reminders working for scheduled tasks
- ✅ Event-driven patterns using streams
- ✅ Performance benchmarks meet SLAs

**Risks**:
- Reminder storage failures → Monitor and alert
- Stream backpressure → Configure backoff strategies

### Phase 6: Production Readiness (Week 8)

**Goal**: Monitoring, scaling, deployment

**Tasks**:
1. Configure Azure Storage clustering with Managed Identity
2. Set up production telemetry (Application Insights)
3. Implement health checks for silo
4. Document deployment topology
5. Create runbooks for common issues
6. Load testing with tenant-scoped workloads
7. Security review of tenant isolation

**Deliverables**:
- ✅ Production deployment successful
- ✅ Monitoring dashboards operational
- ✅ Load tests pass at target scale
- ✅ Security audit passed

**Risks**:
- Cluster instability under load → Gradual rollout
- Managed Identity misconfiguration → Test in staging first

---

## 9. Detailed Implementation Steps

### 9.1 Package Installation

**Step 1**: Add to `Directory.Packages.props`

```xml
<PackageVersion Include="Microsoft.Orleans.Server" Version="9.2.1" />
<PackageVersion Include="Microsoft.Orleans.Sdk" Version="9.2.1" />
<PackageVersion Include="Microsoft.Orleans.Clustering.AzureStorage" Version="9.2.1" />
<PackageVersion Include="OrleansDashboard" Version="8.2.0" />
```

**Step 2**: Add to WebApi.csproj

```xml
<PackageReference Include="Microsoft.Orleans.Server" />
<PackageReference Include="Microsoft.Orleans.Clustering.AzureStorage" />
<PackageReference Include="OrleansDashboard" />
```

**Step 3**: Add to Identity.Actors.Abstractions.csproj

```xml
<PackageReference Include="Microsoft.Orleans.Sdk" />
```

**Step 4**: Add to Identity.Actors.csproj

```xml
<PackageReference Include="Microsoft.Orleans.Sdk" />
<PackageReference Include="Microsoft.Orleans.Runtime" />
```

### 9.2 GC Configuration

**WebApi.csproj**:

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <ServerGarbageCollection>true</ServerGarbageCollection>
  <ConcurrentGarbageCollection>true</ConcurrentGarbageCollection>
</PropertyGroup>
```

### 9.3 Configuration Settings

**appsettings.json**:

```json
{
  "GrainsSettings": {
    "StorageAccountName": "$(PLATFORM_GRAIN_STORAGE_ACCOUNT_NAME)",
    "ClusterId": "dilcore-cluster",
    "ServiceId": "dilcore-platform",
    "DashboardPort": 8080
  }
}
```

**appsettings.Development.json**:

```json
{
  "GrainsSettings": {
    "StorageAccountName": "",  // Empty = use localhost clustering
    "ClusterId": "dilcore-dev",
    "ServiceId": "dilcore-platform"
  }
}
```

### 9.4 Silo Configuration (Program.cs)

```csharp
// Add Orleans
builder.Host.UseOrleans((context, siloBuilder) =>
{
    var grainsSettings = context.Configuration
        .GetSection(nameof(GrainsSettings))
        .Get<GrainsSettings>() ?? new GrainsSettings();

    var environment = context.HostingEnvironment;

    // Clustering
    if (environment.IsDevelopment())
    {
        // Local development: in-memory clustering
        siloBuilder.UseLocalhostClustering();
    }
    else
    {
        // Production: Azure Storage with Managed Identity
        siloBuilder.UseAzureStorageClustering(options =>
        {
            var serviceUri = new Uri(
                $"https://{grainsSettings.StorageAccountName}.table.core.windows.net/");

            options.ConfigureTableServiceClient(
                serviceUri,
                new DefaultAzureCredential());
        });
    }

    siloBuilder.Configure<ClusterOptions>(options =>
    {
        options.ClusterId = grainsSettings.ClusterId;
        options.ServiceId = grainsSettings.ServiceId;
    });

    // State storage (in-memory for now)
    siloBuilder.AddMemoryGrainStorage("UserStore");
    siloBuilder.AddMemoryGrainStorage("TenantStore");

    // Dashboard
    siloBuilder.UseDashboard(options =>
    {
        options.HostSelf = true;
        options.Port = grainsSettings.DashboardPort;
        options.BasePath = "/orleans-dashboard";
    });

    // Telemetry
    siloBuilder.AddActivityPropagation();

    // Logging
    siloBuilder.ConfigureLogging(logging =>
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Information);
    });
});

// Ensure silo starts before handling requests
var app = builder.Build();

// Log silo status
var logger = app.Services.GetRequiredService<ILogger<Program>>();
logger.LogInformation("Orleans silo configured. Dashboard: http://localhost:{Port}/orleans-dashboard",
    grainsSettings.DashboardPort);

// ... existing middleware ...
```

### 9.5 Grain Interface (Identity.Actors.Abstractions)

**IUserGrain.cs**:

```csharp
using FluentResults;
using Orleans;

namespace Dilcore.Identity.Actors.Abstractions;

/// <summary>
/// Represents a user entity in the system.
/// Grain key format: "{tenantId}_{userId}"
/// </summary>
public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Gets the user's profile information.
    /// </summary>
    Task<UserProfileDto> GetProfileAsync();

    /// <summary>
    /// Updates the user's profile.
    /// </summary>
    Task<Result<UserProfileDto>> UpdateProfileAsync(string name, string bio);

    /// <summary>
    /// Activates or deactivates the user account.
    /// </summary>
    Task<Result> SetActiveStatusAsync(bool isActive);
}

public record UserProfileDto(
    string UserId,
    string Name,
    string Bio,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
```

### 9.6 Grain Implementation (Identity.Actors)

**UserState.cs**:

```csharp
namespace Dilcore.Identity.Actors;

[Serializable]
public class UserState
{
    public string? UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
```

**UserGrain.cs**:

```csharp
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.Orleans.MultiTenant;
using FluentResults;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

namespace Dilcore.Identity.Actors;

public class UserGrain : TenantGrainBase, IUserGrain
{
    private readonly IPersistentState<UserState> _state;
    private readonly IUserRepository _repository;
    private readonly ILogger<UserGrain> _logger;

    public UserGrain(
        [PersistentState("user", "UserStore")] IPersistentState<UserState> state,
        IUserRepository repository,
        ILogger<UserGrain> logger)
    {
        _state = state;
        _repository = repository;
        _logger = logger;
    }

    protected override string ExtractTenantIdFromKey()
    {
        var key = this.GetPrimaryKeyString();
        var parts = key.Split('_', 2);
        return parts.Length == 2 ? parts[0] : string.Empty;
    }

    public override async Task OnActivateAsync(CancellationToken cancellationToken)
    {
        await base.OnActivateAsync(cancellationToken);

        _logger.LogInformation(
            "UserGrain activating: {GrainKey}, Tenant: {TenantId}",
            this.GetPrimaryKeyString(),
            TenantId);

        // Load state from repository if not already in memory
        if (string.IsNullOrEmpty(_state.State.UserId))
        {
            var userId = ExtractUserIdFromKey();
            var user = await _repository.GetUserByIdAsync(TenantId, userId);

            if (user != null)
            {
                _state.State.UserId = user.Id;
                _state.State.Name = user.Name;
                _state.State.Bio = user.Bio;
                _state.State.IsActive = user.IsActive;
                _state.State.CreatedAt = user.CreatedAt;
                _state.State.UpdatedAt = user.UpdatedAt;

                await _state.WriteStateAsync();

                _logger.LogInformation(
                    "Loaded user state from repository: {UserId}",
                    userId);
            }
            else
            {
                _logger.LogWarning(
                    "User not found in repository: {UserId}",
                    userId);
            }
        }
    }

    public Task<UserProfileDto> GetProfileAsync()
    {
        return Task.FromResult(new UserProfileDto(
            _state.State.UserId ?? string.Empty,
            _state.State.Name,
            _state.State.Bio,
            _state.State.IsActive,
            _state.State.CreatedAt,
            _state.State.UpdatedAt));
    }

    public async Task<Result<UserProfileDto>> UpdateProfileAsync(
        string name,
        string bio)
    {
        try
        {
            // Update in-memory state
            _state.State.Name = name;
            _state.State.Bio = bio;
            _state.State.UpdatedAt = DateTime.UtcNow;

            // Persist to database
            await _repository.UpdateUserAsync(
                TenantId,
                _state.State.UserId!,
                name,
                bio);

            // Update memory store
            await _state.WriteStateAsync();

            _logger.LogInformation(
                "Updated user profile: {UserId}",
                _state.State.UserId);

            return Result.Ok(await GetProfileAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile");
            return Result.Fail("Failed to update profile");
        }
    }

    public async Task<Result> SetActiveStatusAsync(bool isActive)
    {
        try
        {
            _state.State.IsActive = isActive;
            _state.State.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateUserStatusAsync(
                TenantId,
                _state.State.UserId!,
                isActive);

            await _state.WriteStateAsync();

            return Result.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user status");
            return Result.Fail("Failed to update status");
        }
    }

    private string ExtractUserIdFromKey()
    {
        var key = this.GetPrimaryKeyString();
        return key.Split('_', 2)[1]; // "t1_user123" → "user123"
    }
}
```

### 9.7 TenantGrainBase (Orleans.MultiTenant)

**TenantGrainBase.cs**:

```csharp
using Orleans;

namespace Dilcore.Orleans.MultiTenant;

/// <summary>
/// Base class for tenant-scoped grains.
/// Automatically extracts and validates tenant ID from grain key.
/// </summary>
public abstract class TenantGrainBase : Grain
{
    protected string TenantId { get; private set; } = null!;

    /// <summary>
    /// Implement this to extract tenant ID from grain key.
    /// For string keys like "t1_entity123", split and return "t1".
    /// </summary>
    protected abstract string ExtractTenantIdFromKey();

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        TenantId = ExtractTenantIdFromKey();

        if (string.IsNullOrEmpty(TenantId))
        {
            throw new InvalidOperationException(
                $"Tenant ID could not be extracted from grain key: {this.GetPrimaryKeyString()}");
        }

        return base.OnActivateAsync(cancellationToken);
    }

    /// <summary>
    /// Validates that the caller's tenant matches this grain's tenant.
    /// </summary>
    protected void ValidateTenantAccess(string callerTenantId)
    {
        if (callerTenantId != TenantId)
        {
            throw new UnauthorizedAccessException(
                $"Tenant {callerTenantId} cannot access tenant {TenantId} resources");
        }
    }
}
```

### 9.8 MediatR Handler Integration

**UpdateUserProfileCommand.cs** (WebApi/Features/Users):

```csharp
using Dilcore.MediatR.Abstractions;
using FluentResults;

namespace Dilcore.WebApi.Features.Users;

public record UpdateUserProfileCommand(
    string UserId,
    string Name,
    string Bio) : ICommand<UserProfileDto>;
```

**UpdateUserProfileHandler.cs**:

```csharp
using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MultiTenant.Abstractions;
using FluentResults;
using MediatR;
using Orleans;

namespace Dilcore.WebApi.Features.Users;

public class UpdateUserProfileHandler
    : IRequestHandler<UpdateUserProfileCommand, Result<UserProfileDto>>
{
    private readonly ITenantContext _tenantContext;
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<UpdateUserProfileHandler> _logger;

    public UpdateUserProfileHandler(
        ITenantContext tenantContext,
        IGrainFactory grainFactory,
        ILogger<UpdateUserProfileHandler> logger)
    {
        _tenantContext = tenantContext;
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public async Task<Result<UserProfileDto>> Handle(
        UpdateUserProfileCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            // 1. Create tenant-scoped grain key
            var grainKey = $"{_tenantContext.Name}_{request.UserId}";

            _logger.LogInformation(
                "Updating user profile via grain: {GrainKey}",
                grainKey);

            // 2. Get grain (activates if needed)
            var userGrain = _grainFactory.GetGrain<IUserGrain>(grainKey);

            // 3. Call grain method (traced by Orleans)
            var result = await userGrain.UpdateProfileAsync(
                request.Name,
                request.Bio);

            if (result.IsFailed)
            {
                _logger.LogWarning(
                    "Grain returned failure: {Errors}",
                    string.Join(", ", result.Errors));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Handler failed");
            return Result.Fail("Failed to update user profile");
        }
    }
}
```

**Endpoint Registration** (UserEndpoints.cs):

```csharp
public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
{
    group.MapPut("/{userId}/profile", UpdateUserProfile)
        .WithName("UpdateUserProfile")
        .WithSummary("Update user profile")
        .Produces<UserProfileDto>()
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError);

    return group;
}

private static async Task<IResult> UpdateUserProfile(
    string userId,
    UpdateUserProfileRequest request,
    ISender sender,
    CancellationToken ct)
{
    var command = new UpdateUserProfileCommand(
        userId,
        request.Name,
        request.Bio);

    var result = await sender.Send(command, ct);
    return result.ToMinimalApiResult();
}

public record UpdateUserProfileRequest(string Name, string Bio);
```

### 9.9 Telemetry Configuration

**Enable Orleans Sources** (TelemetryExtensions.cs):

```csharp
public static IServiceCollection AddTelemetry(
    this IServiceCollection services,
    IConfiguration configuration,
    IWebHostEnvironment env)
{
    // ... existing config ...

    otel.WithTracing(tracing =>
    {
        tracing.AddSource("Application.Operations");       // MediatR
        tracing.AddSource("Microsoft.Orleans.Runtime");     // Orleans runtime
        tracing.AddSource("Microsoft.Orleans.Application"); // Grain methods
        tracing.AddAspNetCoreInstrumentation();
        tracing.AddHttpClientInstrumentation();

        // ... exporters ...
    });

    return services;
}
```

**Register Grain Tenant Provider** (Orleans.Extensions):

```csharp
// OrleansExtensions.cs
public static IServiceCollection AddOrleansTelemetry(
    this IServiceCollection services)
{
    services.AddTelemetryAttributeProvider<GrainTenantAttributeProvider>();
    return services;
}

// Program.cs
builder.Services.AddOrleansTelemetry();
```

---

## 10. Performance Considerations

### 10.1 Grain Activation Overhead

**Challenge**: Grain activation involves:
1. Create grain instance
2. Inject dependencies
3. Load state from storage/DB
4. Call OnActivateAsync

**Mitigation Strategies**:

1. **Keep Grains Active**: Configure deactivation timeout
```csharp
siloBuilder.Configure<GrainCollectionOptions>(options =>
{
    options.CollectionAge = TimeSpan.FromMinutes(30); // Keep grains active for 30 min
    options.CollectionQuantum = TimeSpan.FromMinutes(5);
});
```

2. **Lazy State Loading**: Load state on first method call, not activation
```csharp
private bool _stateLoaded = false;

private async Task EnsureStateLoadedAsync()
{
    if (!_stateLoaded)
    {
        await LoadFromRepositoryAsync();
        _stateLoaded = true;
    }
}

public async Task<UserProfileDto> GetProfileAsync()
{
    await EnsureStateLoadedAsync();
    return CreateDto();
}
```

3. **Stateless Workers for Reads**: Use `[StatelessWorker]` for read-only queries
```csharp
[StatelessWorker]
public class UserQueryGrain : Grain, IUserQueryGrain
{
    // No state, no activation cost
}
```

### 10.2 State Persistence Strategy

**Options**:

1. **Write-Through** (Immediate consistency)
   - Every update writes to DB immediately
   - Pros: Strong consistency, simple
   - Cons: Slow (DB latency on every write)

2. **Write-Behind** (Eventual consistency)
   - Updates batched and written periodically
   - Pros: Fast, reduced DB load
   - Cons: Potential data loss on crash

3. **Hybrid** (Recommended)
   - Critical updates: Write-through
   - Non-critical updates: Write-behind with timers

**Implementation**:

```csharp
public class UserGrain : TenantGrainBase, IUserGrain
{
    private Timer? _persistenceTimer;
    private bool _isDirty = false;

    public override Task OnActivateAsync(CancellationToken ct)
    {
        // Schedule periodic persistence
        _persistenceTimer = RegisterTimer(
            PersistStateAsync,
            null,
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(30));

        return base.OnActivateAsync(ct);
    }

    public async Task<Result> UpdateProfileAsync(string name, string bio)
    {
        _state.State.Name = name;
        _state.State.Bio = bio;
        _isDirty = true;

        // Memory state updated immediately
        await _state.WriteStateAsync();

        // DB write happens on timer (write-behind)
        return Result.Ok();
    }

    private async Task PersistStateAsync(object? _)
    {
        if (_isDirty)
        {
            await _repository.UpdateUserAsync(TenantId, _state.State);
            _isDirty = false;
        }
    }
}
```

### 10.3 Horizontal Scaling

**Azure App Service / Container Apps**:
- Run multiple instances of WebApi
- Each instance hosts a silo
- Orleans automatically load-balances grains across silos

**Example Topology** (3 instances):

```
Instance 1 (Silo A): Grains [t1_user1, t2_user5, t1_user9]
Instance 2 (Silo B): Grains [t1_user2, t2_user6, t1_user10]
Instance 3 (Silo C): Grains [t1_user3, t2_user7, t1_user11]
```

**Grain Placement**:
- Orleans uses consistent hashing
- Grains stick to silos (until rebalance)
- Silo failure → grains reactivate on healthy silos

**Health Check**:

```csharp
// Program.cs
builder.Services.AddHealthChecks()
    .AddCheck("orleans_silo", () =>
    {
        var grainFactory = app.Services.GetService<IGrainFactory>();
        return grainFactory != null
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy();
    });

app.MapHealthChecks("/health");
```

### 10.4 Benchmarking

**Load Test Scenario**:
- 1000 concurrent users
- 10 tenants (100 users per tenant)
- Operations: 70% reads, 30% writes
- Target: <100ms p95 latency

**Tools**:
- K6, NBomber, or Apache JMeter
- Metrics: Throughput (req/s), latency (p50, p95, p99), error rate

**Sample K6 Script**:

```javascript
import http from 'k6/http';
import { check } from 'k6';

export let options = {
    stages: [
        { duration: '2m', target: 100 },
        { duration: '5m', target: 1000 },
        { duration: '2m', target: 0 },
    ],
};

export default function () {
    const tenantId = `t${Math.floor(Math.random() * 10) + 1}`;
    const userId = `user${Math.floor(Math.random() * 100) + 1}`;

    const res = http.get(`https://api.dilcore.com/users/${userId}/profile`, {
        headers: { 'x-tenant': tenantId },
    });

    check(res, {
        'status is 200': (r) => r.status === 200,
        'latency < 100ms': (r) => r.timings.duration < 100,
    });
}
```

---

## 11. Security & Best Practices

### 11.1 Tenant Isolation Checklist

- ✅ Tenant ID embedded in grain keys
- ✅ TenantGrainBase validates tenant on activation
- ✅ No shared state between tenant grains
- ✅ Grain-to-grain calls validate tenant context
- ✅ Database queries filter by tenant ID
- ✅ Telemetry attributes include tenant for auditing

### 11.2 Grain Security

**Input Validation**:
```csharp
public async Task<Result> UpdateProfileAsync(string name, string bio)
{
    if (string.IsNullOrWhiteSpace(name))
        return Result.Fail("Name is required");

    if (bio.Length > 500)
        return Result.Fail("Bio exceeds 500 characters");

    // ... proceed ...
}
```

**Avoid Grain Serialization Issues**:
- Use `[Serializable]` on state classes
- Prefer immutable records for DTOs
- Avoid circular references

**Grain Reentrancy**:
- By default, grains are single-threaded (requests queued)
- Use `[Reentrant]` only if safe (read-only methods)
- Use `[AlwaysInterleave]` for specific methods

```csharp
public class UserGrain : TenantGrainBase, IUserGrain
{
    [AlwaysInterleave]
    public Task<UserProfileDto> GetProfileAsync()
    {
        // Read-only, safe to interleave with writes
        return Task.FromResult(CreateDto());
    }
}
```

### 11.3 Managed Identity Best Practices

**Azure Storage Authentication**:

```csharp
siloBuilder.UseAzureStorageClustering(options =>
{
    var serviceUri = new Uri(
        $"https://{storageAccountName}.table.core.windows.net/");

    // DefaultAzureCredential tries:
    // 1. Environment variables (local dev)
    // 2. Managed Identity (Azure production)
    // 3. Azure CLI (local dev fallback)
    options.ConfigureTableServiceClient(
        serviceUri,
        new DefaultAzureCredential());
});
```

**Required Azure RBAC Roles**:
- Storage Table Data Contributor (read/write cluster membership)

**Local Development**:
- Use Azure Storage Emulator (Azurite)
- Or authenticate via Azure CLI: `az login`

### 11.4 Error Handling

**Grain Exceptions**:
- Exceptions thrown in grains propagate to caller
- Use FluentResults for expected failures
- Log exceptions before throwing

**Retry Policies** (at handler level, not grain level):

```csharp
public async Task<Result<UserProfileDto>> Handle(
    UpdateUserProfileCommand request,
    CancellationToken ct)
{
    var retryPolicy = Policy
        .Handle<OrleansException>()
        .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)));

    return await retryPolicy.ExecuteAsync(async () =>
    {
        var userGrain = _grainFactory.GetGrain<IUserGrain>(grainKey);
        return await userGrain.UpdateProfileAsync(request.Name, request.Bio);
    });
}
```

---

## 12. Testing Strategy

### 12.1 Unit Testing Grains

**Use Orleans TestCluster** (Microsoft.Orleans.TestingHost):

```csharp
public class UserGrainTests : IClassFixture<ClusterFixture>
{
    private readonly TestCluster _cluster;

    public UserGrainTests(ClusterFixture fixture)
    {
        _cluster = fixture.Cluster;
    }

    [Fact]
    public async Task UpdateProfile_ShouldPersistChanges()
    {
        // Arrange
        var grainKey = "t1_user123";
        var userGrain = _cluster.GrainFactory.GetGrain<IUserGrain>(grainKey);

        // Act
        var result = await userGrain.UpdateProfileAsync("John Doe", "Software Engineer");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var profile = await userGrain.GetProfileAsync();
        profile.Name.ShouldBe("John Doe");
    }
}

public class ClusterFixture : IDisposable
{
    public TestCluster Cluster { get; }

    public ClusterFixture()
    {
        var builder = new TestClusterBuilder();
        builder.AddSiloBuilderConfigurator<SiloConfigurator>();
        Cluster = builder.Build();
        Cluster.Deploy();
    }

    public void Dispose() => Cluster?.StopAllSilos();
}

public class SiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder)
    {
        siloBuilder.AddMemoryGrainStorage("UserStore");
        siloBuilder.AddMemoryGrainStorage("TenantStore");
    }
}
```

### 12.2 Integration Testing

**Test MediatR → Grain Flow**:

```csharp
public class UpdateUserProfileIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UpdateUserProfileIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task UpdateUserProfile_ShouldReturnSuccess()
    {
        // Arrange
        var request = new UpdateUserProfileRequest("Jane Doe", "Product Manager");

        // Act
        var response = await _client.PutAsJsonAsync(
            "/users/user123/profile",
            request,
            headers: new { { "x-tenant", "t1" } });

        // Assert
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<UserProfileDto>();
        result!.Name.ShouldBe("Jane Doe");
    }
}
```

### 12.3 Performance Testing

**Grain Throughput Test**:

```csharp
[Fact]
public async Task Grain_ShouldHandle1000ConcurrentRequests()
{
    var grainKey = "t1_user123";
    var userGrain = _cluster.GrainFactory.GetGrain<IUserGrain>(grainKey);

    var tasks = Enumerable.Range(0, 1000)
        .Select(_ => userGrain.GetProfileAsync())
        .ToArray();

    var stopwatch = Stopwatch.StartNew();
    await Task.WhenAll(tasks);
    stopwatch.Stop();

    _output.WriteLine($"1000 requests completed in {stopwatch.ElapsedMilliseconds}ms");
    stopwatch.ElapsedMilliseconds.ShouldBeLessThan(5000); // <5s
}
```

### 12.4 Multi-Tenancy Testing

**Tenant Isolation Test**:

```csharp
[Fact]
public async Task DifferentTenants_ShouldHaveIsolatedGrains()
{
    var tenant1Grain = _cluster.GrainFactory.GetGrain<IUserGrain>("t1_user123");
    var tenant2Grain = _cluster.GrainFactory.GetGrain<IUserGrain>("t2_user123");

    await tenant1Grain.UpdateProfileAsync("Tenant 1 User", "Bio 1");
    await tenant2Grain.UpdateProfileAsync("Tenant 2 User", "Bio 2");

    var profile1 = await tenant1Grain.GetProfileAsync();
    var profile2 = await tenant2Grain.GetProfileAsync();

    profile1.Name.ShouldBe("Tenant 1 User");
    profile2.Name.ShouldBe("Tenant 2 User");
}
```

---

## 13. Operational Considerations

### 13.1 Monitoring & Alerts

**Key Metrics**:

| Metric | Threshold | Alert |
|--------|-----------|-------|
| Silo CPU Usage | >80% | Warning |
| Grain Activation Rate | >1000/s | Info |
| Grain Activation Failures | >10/min | Critical |
| Average Activation Time | >500ms | Warning |
| Memory Usage | >85% | Warning |
| Failed Grain Calls | >1% | Critical |

**Application Insights Queries**:

```kusto
// Grain activation latency
traces
| where customDimensions.ActivityName startswith "Orleans"
| summarize avg(duration), p95=percentile(duration, 95) by bin(timestamp, 5m)

// Tenant-specific grain activity
dependencies
| where type == "Orleans"
| extend tenantId = tostring(customDimensions["tenant.name"])
| summarize count() by tenantId, bin(timestamp, 1h)
```

### 13.2 Deployment Strategy

**Blue-Green Deployment**:
1. Deploy new version to "green" slot
2. Orleans cluster forms (green silos)
3. Gradually shift traffic from blue → green
4. Old grains deactivate, reactivate on green silos
5. Monitor errors/latency
6. Rollback if issues detected

**Zero-Downtime Updates**:
- Orleans cluster continues during rolling restarts
- Grains migrate to new silos automatically
- Configure grace period for draining

```yaml
# Azure App Service configuration
appSettings:
  - name: WEBSITE_SWAP_WARMUP_PING_PATH
    value: /health
  - name: WEBSITE_SWAP_WARMUP_PING_STATUSES
    value: 200
```

### 13.3 Troubleshooting Guide

**Issue**: Silo won't start

**Diagnosis**:
```bash
# Check logs
az webapp log tail --name dilcore-api --resource-group dilcore-rg

# Common causes:
# - Azure Storage auth failure (Managed Identity not assigned)
# - Port conflicts (Silo/Gateway ports)
# - Missing Orleans packages
```

**Fix**:
- Verify Managed Identity has "Storage Table Data Contributor" role
- Check port configuration (11111, 30000)
- Ensure all Orleans packages referenced

---

**Issue**: Grains not activating

**Diagnosis**:
```csharp
// Add logging in OnActivateAsync
_logger.LogInformation("Grain activating: {GrainKey}", this.GetPrimaryKeyString());
```

**Fix**:
- Check grain interface registered in DI
- Verify grain key format matches extraction logic
- Check state storage provider configured

---

**Issue**: Tenant context missing in grain

**Diagnosis**:
```csharp
// Log tenant ID in grain
_logger.LogInformation("Grain tenant: {TenantId}", TenantId);
```

**Fix**:
- Verify grain key includes tenant ID
- Check ExtractTenantIdFromKey implementation
- Use GrainTenantAttributeProvider for telemetry fallback

---

**Issue**: State not persisting

**Diagnosis**:
- Check `_state.WriteStateAsync()` called after updates
- Verify repository method executed
- Check database logs for errors

**Fix**:
- Wrap repository calls in try/catch
- Add retry policies for transient DB errors
- Log state before/after writes

---

### 13.4 Disaster Recovery

**Backup Strategy**:
- Orleans in-memory state: Ephemeral (rebuilt from DB)
- Database: Standard backup/restore (Azure SQL Geo-Replication)
- Cluster membership table: Recreated on silo startup

**Recovery Procedure**:
1. Restore database from backup
2. Start silos (cluster reforms)
3. Grains reactivate and load state from DB
4. Traffic resumes

**RTO/RPO**:
- RTO: <15 minutes (silo startup time)
- RPO: Last DB backup (configure based on SLA)

---

## 14. References & Resources

### 14.1 Official Documentation

- [Microsoft Orleans Documentation](https://learn.microsoft.com/en-us/dotnet/orleans/)
- [Orleans GitHub Repository](https://github.com/dotnet/orleans)
- [Orleans Best Practices](https://learn.microsoft.com/en-us/dotnet/orleans/resources/best-practices)
- [Grain Persistence](https://learn.microsoft.com/en-us/dotnet/orleans/grains/grain-persistence/)
- [Orleans Observability](https://learn.microsoft.com/en-us/dotnet/orleans/host/monitoring/)

### 14.2 Community Resources

- [Orleans Dashboard GitHub](https://github.com/OrleansContrib/OrleansDashboard)
- [Orleans Multi-Tenant Library](https://github.com/Applicita/Orleans.Multitenant)
- [Orleans Design Patterns](https://github.com/OrleansContrib/DesignPatterns)

### 14.3 Azure Integration

- [Deploy Orleans on Azure](https://learn.microsoft.com/en-us/dotnet/orleans/quickstarts/deploy-scale-orleans-on-azure)
- [Azure Storage Clustering](https://www.nuget.org/packages/Microsoft.Orleans.Clustering.AzureStorage)
- [Managed Identity Documentation](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview)

### 14.4 OpenTelemetry & Orleans

- [Orleans OpenTelemetry PR #6853](https://github.com/dotnet/orleans/pull/6853)
- [Context Propagation](https://opentelemetry.io/docs/concepts/context-propagation/)
- [Orleans Tracing Issue #7504](https://github.com/dotnet/orleans/issues/7504)

### 14.5 Multi-Tenancy Patterns

- [Orleans Multi-Tenancy Discussion #6736](https://github.com/dotnet/orleans/issues/6736)
- [Tenant Isolation Best Practices](https://workos.com/blog/tenant-isolation-in-multi-tenant-systems)

---

## Appendix A: Glossary

- **Grain**: Virtual actor representing a single entity
- **Silo**: Orleans server process hosting grains
- **Grain Key**: Unique identifier for a grain (string/GUID/int)
- **Activation**: Process of creating a grain instance in memory
- **Deactivation**: Process of removing inactive grain from memory
- **Stateless Worker**: Grain without state, used for stateless operations
- **IPersistentState**: Orleans state management abstraction
- **Activity**: OpenTelemetry distributed trace span
- **IGrainFactory**: Factory for obtaining grain references
- **Cluster**: Group of silos working together

---

## Appendix B: Decision Log

| Decision | Rationale | Alternatives Considered | Date |
|----------|-----------|-------------------------|------|
| Use Orleans 9.2.1 (not 10.0-rc.2) | 10.0 does not exist; 9.2.1 is latest stable | Wait for Orleans 10 (no timeline) | 2026-01-07 |
| Co-host silo in WebApi | Simplified deployment, shared DI | Separate silo cluster (more complex) | 2026-01-07 |
| Tenant ID in grain keys | Standard Orleans multi-tenancy pattern | Separate ClusterId per tenant (doesn't scale) | 2026-01-07 |
| Keep MediatR at API boundary | Preserve existing patterns, loose coupling | Replace MediatR with grain interfaces (tight coupling) | 2026-01-07 |
| In-memory state + repository | Fast reads, durable writes | Direct DB access (slower, no cache) | 2026-01-07 |
| Azure Storage clustering | Managed Identity support, native Azure | Redis, Consul, ZooKeeper (more complex) | 2026-01-07 |

---

**End of Document**

**Next Steps**:
1. Review and approve this plan
2. Create Azure resources (Storage Account)
3. Assign team members to implementation phases
4. Begin Phase 1 implementation
5. Schedule weekly checkpoints

**Questions/Feedback**: Contact architecture team or create GitHub issue.
