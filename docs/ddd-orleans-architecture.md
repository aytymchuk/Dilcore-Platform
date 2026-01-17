# DDD + Orleans Architecture Design

## Executive Summary

This document describes the optimal architecture for combining Domain-Driven Design (DDD) with Orleans Actor Model in the Dilcore Platform, ensuring:

- **Domain models remain pure** and non-serializable
- **Business logic stays in the domain layer**
- **Orleans grains act as thin orchestrators** for concurrency and persistence
- **MediatR handlers coordinate workflows**
- **Clear separation of concerns** between domain, infrastructure, and application layers

## Current Architecture Analysis

### Issues Identified

1. **Domain Logic in Grains**: Business validation and rules live in `UserGrain.RegisterAsync()` and `TenantGrain.CreateAsync()` instead of domain models
2. **Unused Domain Models**: `Identity.Domain.User` exists but isn't used by grains
3. **Missing Domain Models**: Tenancy has no domain layer at all
4. **Tight Coupling**: State objects map directly to DTOs without domain transformation
5. **No Clear Pattern**: No established pattern for hydrating domain from state

### Current Flow

```
HTTP Request → MediatR Handler → Orleans Grain → State Mutation → Persistence
                                      ↓
                                Business Logic (WRONG PLACE!)
```

## Proposed Architecture

### Layered Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Presentation Layer (WebApi)                                 │
│  - Minimal API Endpoints                                     │
│  - DTO Validation (FluentValidation)                         │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│  Application Layer (Core)                                    │
│  - MediatR Commands/Queries                                  │
│  - Command/Query Handlers                                    │
│  - Application Services (orchestration)                      │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│  Domain Layer (Domain)                                       │
│  - Entities (User, Tenant)                                   │
│  - Value Objects                                             │
│  - Domain Services                                           │
│  - Domain Events                                             │
│  ✓ Pure business logic                                       │
│  ✓ Framework agnostic                                        │
│  ✗ NO serialization attributes                               │
└────────────────────┬────────────────────────────────────────┘
                     ↓
┌─────────────────────────────────────────────────────────────┐
│  Infrastructure Layer (Actors)                               │
│  - Orleans Grains (thin orchestrators)                       │
│  - State Objects (serializable, anemic)                      │
│  - State ↔ Domain Mappers                                    │
│  - Persistence coordination                                  │
└─────────────────────────────────────────────────────────────┘
```

### Key Principles

1. **Domain Models are NOT Serializable**
   - No `[GenerateSerializer]` attributes
   - No Orleans-specific concerns
   - Pure C# records/classes with rich behavior

2. **State Objects are Anemic**
   - Pure data structures
   - Serializable with Orleans attributes
   - No business logic
   - Located in `*.Actors` project

3. **Grains are Thin Orchestrators**
   - Load state from persistence
   - Reconstitute domain model from state
   - Execute domain methods
   - Extract updated state from domain
   - Persist state

4. **Domain Services for Complex Logic**
   - Cross-aggregate operations
   - Domain rules spanning multiple entities
   - Injected into grains when needed

## Implementation Pattern

### 1. Domain Layer Structure

```
src/Identity/Identity.Domain/
├── User.cs                    # Rich domain entity
├── UserDomainService.cs       # Domain service (if needed)
├── ValueObjects/
│   ├── Email.cs
│   └── UserName.cs
└── Events/
    ├── UserRegistered.cs
    └── UserProfileUpdated.cs
```

### 2. Actors Layer Structure

```
src/Identity/Identity.Actors/
├── UserGrain.cs               # Thin orchestrator
├── UserState.cs               # Serializable state
├── Mappers/
│   └── UserStateMapper.cs     # State ↔ Domain mapping
└── LoggerExtensions.cs
```

### 3. Example: Enhanced Domain Model

```csharp
// src/Identity/Identity.Domain/User.cs
namespace Dilcore.Identity.Domain;

/// <summary>
/// User aggregate root - contains all business logic.
/// NOT serializable - never used in Orleans contracts.
/// </summary>
public sealed record User : BaseDomain
{
    public string IdentityId { get; init; } = default!;
    public Email Email { get; init; } = default!;
    public UserName Name { get; init; } = default!;
    public DateTime RegisteredAt { get; init; }
    public bool IsActive { get; init; }

    // Factory method with business rules
    public static Result<User> Register(
        string identityId,
        string email,
        string firstName,
        string lastName,
        TimeProvider timeProvider)
    {
        // Domain validation
        if (string.IsNullOrWhiteSpace(identityId))
            return Result.Fail<User>(new ValidationError("Identity ID is required"));

        var emailResult = Email.Create(email);
        if (emailResult.IsFailed)
            return Result.Fail<User>(emailResult.Errors);

        var nameResult = UserName.Create(firstName, lastName);
        if (nameResult.IsFailed)
            return Result.Fail<User>(nameResult.Errors);

        var now = timeProvider.GetUtcNow().DateTime;

        return Result.Ok(new User
        {
            Id = Guid.NewGuid(),
            IdentityId = identityId,
            Email = emailResult.Value,
            Name = nameResult.Value,
            RegisteredAt = now,
            IsActive = true,
            CreatedOn = now,
            ETag = 0
        });
    }

    // Domain behavior
    public Result<User> UpdateProfile(string firstName, string lastName, TimeProvider timeProvider)
    {
        var nameResult = UserName.Create(firstName, lastName);
        if (nameResult.IsFailed)
            return Result.Fail<User>(nameResult.Errors);

        return Result.Ok(this with
        {
            Name = nameResult.Value,
            UpdatedOn = timeProvider.GetUtcNow().DateTime,
            ETag = ETag + 1
        });
    }

    // Domain behavior
    public Result<User> Deactivate(TimeProvider timeProvider)
    {
        if (!IsActive)
            return Result.Fail<User>(new ValidationError("User is already deactivated"));

        return Result.Ok(this with
        {
            IsActive = false,
            UpdatedOn = timeProvider.GetUtcNow().DateTime,
            ETag = ETag + 1
        });
    }
}
```

### 4. Example: Value Objects

```csharp
// src/Identity/Identity.Domain/ValueObjects/Email.cs
namespace Dilcore.Identity.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; init; } = default!;

    private Email() { }

    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Fail<Email>(new ValidationError("Email is required"));

        if (!IsValidEmail(email))
            return Result.Fail<Email>(new ValidationError("Email format is invalid"));

        return Result.Ok(new Email { Value = email.ToLowerInvariant() });
    }

    private static bool IsValidEmail(string email)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(email,
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }

    public override string ToString() => Value;
}

// src/Identity/Identity.Domain/ValueObjects/UserName.cs
namespace Dilcore.Identity.Domain.ValueObjects;

public sealed record UserName
{
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string FullName => $"{FirstName} {LastName}";

    private UserName() { }

    public static Result<UserName> Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Fail<UserName>(new ValidationError("First name is required"));

        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Fail<UserName>(new ValidationError("Last name is required"));

        if (firstName.Length < 2)
            return Result.Fail<UserName>(new ValidationError("First name must be at least 2 characters"));

        if (lastName.Length < 2)
            return Result.Fail<UserName>(new ValidationError("Last name must be at least 2 characters"));

        return Result.Ok(new UserName
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim()
        });
    }
}
```

### 5. Example: State Object (Unchanged)

```csharp
// src/Identity/Identity.Actors/UserState.cs
namespace Dilcore.Identity.Actors;

/// <summary>
/// Anemic, serializable state - NO business logic.
/// </summary>
[GenerateSerializer]
public sealed class UserState
{
    [Id(0)] public Guid Id { get; set; }
    [Id(1)] public string IdentityId { get; set; } = string.Empty;
    [Id(2)] public string Email { get; set; } = string.Empty;
    [Id(3)] public string FirstName { get; set; } = string.Empty;
    [Id(4)] public string LastName { get; set; } = string.Empty;
    [Id(5)] public DateTime RegisteredAt { get; set; }
    [Id(6)] public bool IsActive { get; set; }
    [Id(7)] public long ETag { get; set; }
    [Id(8)] public DateTime CreatedOn { get; set; }
    [Id(9)] public DateTime? UpdatedOn { get; set; }
    [Id(10)] public bool IsRegistered { get; set; }
}
```

### 6. Example: State Mapper

```csharp
// src/Identity/Identity.Actors/Mappers/UserStateMapper.cs
namespace Dilcore.Identity.Actors.Mappers;

/// <summary>
/// Maps between domain model (User) and serializable state (UserState).
/// </summary>
internal static class UserStateMapper
{
    /// <summary>
    /// Reconstitute domain model from persisted state.
    /// </summary>
    public static Result<User> ToDomain(UserState state)
    {
        if (!state.IsRegistered)
            return Result.Fail<User>(new ValidationError("User is not registered"));

        var emailResult = Email.Create(state.Email);
        if (emailResult.IsFailed)
            return Result.Fail<User>(emailResult.Errors);

        var nameResult = UserName.Create(state.FirstName, state.LastName);
        if (nameResult.IsFailed)
            return Result.Fail<User>(nameResult.Errors);

        return Result.Ok(new User
        {
            Id = state.Id,
            IdentityId = state.IdentityId,
            Email = emailResult.Value,
            Name = nameResult.Value,
            RegisteredAt = state.RegisteredAt,
            IsActive = state.IsActive,
            ETag = state.ETag,
            CreatedOn = state.CreatedOn,
            UpdatedOn = state.UpdatedOn
        });
    }

    /// <summary>
    /// Extract state from domain model for persistence.
    /// </summary>
    public static UserState ToState(User user)
    {
        return new UserState
        {
            Id = user.Id,
            IdentityId = user.IdentityId,
            Email = user.Email.Value,
            FirstName = user.Name.FirstName,
            LastName = user.Name.LastName,
            RegisteredAt = user.RegisteredAt,
            IsActive = user.IsActive,
            ETag = user.ETag,
            CreatedOn = user.CreatedOn,
            UpdatedOn = user.UpdatedOn,
            IsRegistered = true
        };
    }
}
```

### 7. Example: Refactored Grain (Thin Orchestrator)

```csharp
// src/Identity/Identity.Actors/UserGrain.cs
namespace Dilcore.Identity.Actors;

/// <summary>
/// Thin orchestrator - delegates business logic to domain model.
/// Responsibilities: state loading, persistence, concurrency control.
/// </summary>
public sealed class UserGrain : Grain, IUserGrain
{
    private readonly IPersistentState<UserState> _state;
    private readonly ILogger<UserGrain> _logger;
    private readonly TimeProvider _timeProvider;

    public UserGrain(
        [PersistentState("user", "UserStore")] IPersistentState<UserState> state,
        ILogger<UserGrain> logger,
        TimeProvider timeProvider)
    {
        _state = state;
        _logger = logger;
        _timeProvider = timeProvider;
    }

    public async Task<UserCreationResult> RegisterAsync(string email, string firstName, string lastName)
    {
        var userId = this.GetPrimaryKeyString();

        // Check if already registered
        if (_state.State.IsRegistered)
        {
            _logger.LogUserAlreadyRegistered(userId);
            return UserCreationResult.Failure($"User '{userId}' is already registered.");
        }

        // Call domain factory method (business logic in domain!)
        var userResult = User.Register(userId, email, firstName, lastName, _timeProvider);
        if (userResult.IsFailed)
        {
            var error = userResult.Errors.First();
            _logger.LogUserRegistrationFailed(userId, error.Message);
            return UserCreationResult.Failure(error.Message);
        }

        // Extract state from domain model
        _state.State = UserStateMapper.ToState(userResult.Value);

        // Persist
        await _state.WriteStateAsync();

        _logger.LogUserRegistered(userId, email);

        // Return DTO
        return UserCreationResult.Success(ToDto(_state.State));
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(string firstName, string lastName)
    {
        // Load domain model from state
        var userResult = UserStateMapper.ToDomain(_state.State);
        if (userResult.IsFailed)
        {
            return Result.Fail<UserDto>(userResult.Errors);
        }

        // Call domain method (business logic in domain!)
        var updatedUserResult = userResult.Value.UpdateProfile(firstName, lastName, _timeProvider);
        if (updatedUserResult.IsFailed)
        {
            return Result.Fail<UserDto>(updatedUserResult.Errors);
        }

        // Extract and persist state
        _state.State = UserStateMapper.ToState(updatedUserResult.Value);
        await _state.WriteStateAsync();

        return Result.Ok(ToDto(_state.State));
    }

    public Task<UserDto?> GetProfileAsync()
    {
        if (!_state.State.IsRegistered)
        {
            _logger.LogUserNotFound(this.GetPrimaryKeyString());
            return Task.FromResult<UserDto?>(null);
        }

        return Task.FromResult<UserDto?>(ToDto(_state.State));
    }

    private static UserDto ToDto(UserState state) => new(
        state.Id.ToString(),
        state.Email,
        $"{state.FirstName} {state.LastName}",
        state.RegisteredAt);
}
```

### 8. Example: MediatR Handler (Unchanged Pattern)

```csharp
// src/Identity/Identity.Core/Features/Register/RegisterUserHandler.cs
namespace Dilcore.Identity.Core.Features.Register;

public sealed class RegisterUserHandler : ICommandHandler<RegisterUserCommand, UserDto>
{
    private readonly IUserContextResolver _userContextResolver;
    private readonly IGrainFactory _grainFactory;

    public RegisterUserHandler(IUserContextResolver userContextResolver, IGrainFactory grainFactory)
    {
        _userContextResolver = userContextResolver;
        _grainFactory = grainFactory;
    }

    public async Task<Result<UserDto>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userContext = _userContextResolver.Resolve();

        if (userContext.Id is null)
            return Result.Fail<UserDto>(new ValidationError("User ID is required for registration"));

        // Grain handles orchestration, domain handles business logic
        var grain = _grainFactory.GetGrain<IUserGrain>(userContext.Id);
        var result = await grain.RegisterAsync(request.Email, request.FirstName, request.LastName);

        if (!result.IsSuccess)
            return Result.Fail(new ConflictError(result.ErrorMessage ?? "Failed to register user."));

        if (result.User is null)
            return Result.Fail<UserDto>("User registration succeeded but returned null.");

        return Result.Ok(result.User);
    }
}
```

### 9. Example: Updated Grain Interface

```csharp
// src/Identity/Identity.Actors.Abstractions/IUserGrain.cs
namespace Dilcore.Identity.Actors.Abstractions;

public interface IUserGrain : IGrainWithStringKey
{
    /// <summary>
    /// Registers a new user (now accepts firstName/lastName separately).
    /// </summary>
    Task<UserCreationResult> RegisterAsync(string email, string firstName, string lastName);

    /// <summary>
    /// Updates user profile.
    /// </summary>
    Task<Result<UserDto>> UpdateProfileAsync(string firstName, string lastName);

    /// <summary>
    /// Gets the user's profile information.
    /// </summary>
    Task<UserDto?> GetProfileAsync();

    /// <summary>
    /// Deactivates the user account.
    /// </summary>
    Task<Result> DeactivateAsync();
}
```

## Advanced Patterns

### Domain Services

When business logic spans multiple aggregates or requires external dependencies:

```csharp
// src/Identity/Identity.Domain/Services/IUserDomainService.cs
namespace Dilcore.Identity.Domain.Services;

/// <summary>
/// Domain service for complex user operations.
/// </summary>
public interface IUserDomainService
{
    /// <summary>
    /// Checks if user can be promoted to admin (complex business rule).
    /// </summary>
    Result<bool> CanPromoteToAdmin(User user, Tenant tenant);
}

// Implementation in Actors layer (injected into grains)
// src/Identity/Identity.Actors/Services/UserDomainService.cs
namespace Dilcore.Identity.Actors.Services;

internal sealed class UserDomainService : IUserDomainService
{
    private readonly IGrainFactory _grainFactory;

    public UserDomainService(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public Result<bool> CanPromoteToAdmin(User user, Tenant tenant)
    {
        // Complex domain logic spanning User and Tenant aggregates
        if (!user.IsActive)
            return Result.Fail<bool>(new ValidationError("Inactive users cannot be promoted"));

        if (tenant.UserCount >= tenant.MaxAdmins)
            return Result.Fail<bool>(new ValidationError("Tenant has reached max admin limit"));

        return Result.Ok(true);
    }
}
```

### Domain Events (Future Enhancement)

```csharp
// src/Identity/Identity.Domain/Events/UserRegistered.cs
namespace Dilcore.Identity.Domain.Events;

public sealed record UserRegistered : IDomainEvent
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = default!;
    public DateTime OccurredAt { get; init; }
}

// Domain model raises events
public sealed record User : BaseDomain
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Result<User> Register(...)
    {
        var user = new User { /* ... */ };

        // Raise domain event
        user._domainEvents.Add(new UserRegistered
        {
            UserId = user.Id,
            Email = user.Email.Value,
            OccurredAt = timeProvider.GetUtcNow().DateTime
        });

        return Result.Ok(user);
    }

    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Grain publishes events after persistence
public async Task<UserCreationResult> RegisterAsync(...)
{
    var userResult = User.Register(...);
    _state.State = UserStateMapper.ToState(userResult.Value);
    await _state.WriteStateAsync();

    // Publish domain events (e.g., to Orleans Streams or event bus)
    foreach (var domainEvent in userResult.Value.DomainEvents)
    {
        await PublishEventAsync(domainEvent);
    }

    return UserCreationResult.Success(ToDto(_state.State));
}
```

### Repository Pattern (Optional)

For complex grain interactions or testing:

```csharp
// src/Identity/Identity.Core/Repositories/IUserRepository.cs
namespace Dilcore.Identity.Core.Repositories;

/// <summary>
/// Repository abstraction over UserGrain.
/// Useful for testing and abstracting Orleans details from handlers.
/// </summary>
public interface IUserRepository
{
    Task<Result<User>> GetByIdAsync(string userId);
    Task<Result<User>> RegisterAsync(string userId, string email, string firstName, string lastName);
    Task<Result<User>> UpdateAsync(User user);
}

// src/Identity/Identity.Actors/Repositories/UserRepository.cs
namespace Dilcore.Identity.Actors.Repositories;

internal sealed class UserRepository : IUserRepository
{
    private readonly IGrainFactory _grainFactory;

    public UserRepository(IGrainFactory grainFactory)
    {
        _grainFactory = grainFactory;
    }

    public async Task<Result<User>> GetByIdAsync(string userId)
    {
        var grain = _grainFactory.GetGrain<IUserGrain>(userId);
        var dto = await grain.GetProfileAsync();

        if (dto is null)
            return Result.Fail<User>(new NotFoundError("User", userId));

        // Map DTO back to domain (if needed)
        // Or store full domain state in UserState
        return Result.Ok(/* user */);
    }

    public async Task<Result<User>> RegisterAsync(string userId, string email, string firstName, string lastName)
    {
        var grain = _grainFactory.GetGrain<IUserGrain>(userId);
        var result = await grain.RegisterAsync(email, firstName, lastName);

        if (!result.IsSuccess)
            return Result.Fail<User>(new ValidationError(result.ErrorMessage!));

        return Result.Ok(/* user */);
    }
}
```

## Testing Strategy

### Domain Model Tests (Pure Unit Tests)

```csharp
// tests/Identity/Identity.Domain.Tests/UserTests.cs
[TestFixture]
public class UserTests
{
    private TimeProvider _timeProvider = null!;

    [SetUp]
    public void Setup()
    {
        _timeProvider = TimeProvider.System;
    }

    [Test]
    public void Register_ValidData_CreatesUser()
    {
        // Arrange
        var identityId = "auth0|123";
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var result = User.Register(identityId, email, firstName, lastName, _timeProvider);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var user = result.Value;
        user.IdentityId.ShouldBe(identityId);
        user.Email.Value.ShouldBe(email);
        user.Name.FirstName.ShouldBe(firstName);
        user.Name.LastName.ShouldBe(lastName);
        user.IsActive.ShouldBeTrue();
    }

    [Test]
    public void Register_InvalidEmail_ReturnsFailed()
    {
        // Act
        var result = User.Register("auth0|123", "invalid-email", "John", "Doe", _timeProvider);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("Email format is invalid"));
    }

    [Test]
    public void UpdateProfile_ValidData_UpdatesUser()
    {
        // Arrange
        var user = User.Register("auth0|123", "test@example.com", "John", "Doe", _timeProvider).Value;

        // Act
        var result = user.UpdateProfile("Jane", "Smith", _timeProvider);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        var updated = result.Value;
        updated.Name.FirstName.ShouldBe("Jane");
        updated.Name.LastName.ShouldBe("Smith");
        updated.ETag.ShouldBe(user.ETag + 1);
    }

    [Test]
    public void Deactivate_ActiveUser_DeactivatesUser()
    {
        // Arrange
        var user = User.Register("auth0|123", "test@example.com", "John", "Doe", _timeProvider).Value;

        // Act
        var result = user.Deactivate(_timeProvider);

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.Value.IsActive.ShouldBeFalse();
    }

    [Test]
    public void Deactivate_InactiveUser_ReturnsFailed()
    {
        // Arrange
        var user = User.Register("auth0|123", "test@example.com", "John", "Doe", _timeProvider).Value;
        var deactivated = user.Deactivate(_timeProvider).Value;

        // Act
        var result = deactivated.Deactivate(_timeProvider);

        // Assert
        result.IsFailed.ShouldBeTrue();
        result.Errors.ShouldContain(e => e.Message.Contains("already deactivated"));
    }
}
```

### Grain Tests (Integration Tests)

```csharp
// tests/Identity/Identity.Actors.Tests/UserGrainTests.cs
[TestFixture]
public class UserGrainTests : IClassFixture<ClusterFixture>
{
    private readonly ClusterFixture _fixture;

    public UserGrainTests(ClusterFixture fixture)
    {
        _fixture = fixture;
    }

    [Test]
    public async Task RegisterAsync_ValidData_PersistsUser()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(userId);

        // Act
        var result = await grain.RegisterAsync("test@example.com", "John", "Doe");

        // Assert
        result.IsSuccess.ShouldBeTrue();
        result.User.ShouldNotBeNull();
        result.User.Email.ShouldBe("test@example.com");

        // Verify persistence
        var profile = await grain.GetProfileAsync();
        profile.ShouldNotBeNull();
        profile.Email.ShouldBe("test@example.com");
    }

    [Test]
    public async Task RegisterAsync_InvalidEmail_ReturnsFailed()
    {
        // Arrange
        var grain = _fixture.Cluster.GrainFactory.GetGrain<IUserGrain>(Guid.NewGuid().ToString());

        // Act
        var result = await grain.RegisterAsync("invalid-email", "John", "Doe");

        // Assert
        result.IsSuccess.ShouldBeFalse();
        result.ErrorMessage.ShouldContain("Email format is invalid");
    }
}
```

## Migration Strategy

### Phase 1: Add Domain Models (Current Sprint)
1. Create value objects (Email, UserName)
2. Enhance User domain model with business methods
3. Add domain model tests
4. **No changes to grains yet** - parallel development

### Phase 2: Add Mappers (Next Sprint)
1. Create UserStateMapper
2. Add mapper tests
3. Update UserState to include all domain properties

### Phase 3: Refactor Grains (Following Sprint)
1. Update UserGrain to use domain model
2. Update grain tests
3. Update handlers if interfaces change
4. Deploy and monitor

### Phase 4: Apply to Tenancy (Future)
1. Create Tenant domain model
2. Create TenantState mapper
3. Refactor TenantGrain
4. Follow same pattern

### Phase 5: Advanced Features (Future)
1. Add domain events
2. Add domain services
3. Consider repository pattern
4. Add aggregate root base class with event handling

## Benefits

1. **Clear Separation of Concerns**
   - Domain logic in domain layer (testable, framework-agnostic)
   - Infrastructure concerns in actors layer (Orleans-specific)

2. **Rich Domain Models**
   - Business rules enforced by domain
   - Immutable value objects
   - Factory methods for creation

3. **Testability**
   - Pure domain tests (no mocking, no infrastructure)
   - Grain tests focus on orchestration
   - Clear boundaries

4. **Flexibility**
   - Can change persistence without touching domain
   - Can add new behaviors to domain without changing infrastructure
   - Easy to add new features (events, specifications, etc.)

5. **Maintainability**
   - Business rules in one place (domain)
   - Grains are thin and simple
   - Clear patterns to follow

## Anti-Patterns to Avoid

1. ❌ **Don't add business logic to grains**
   - Grains should orchestrate, not implement business rules

2. ❌ **Don't serialize domain models**
   - Keep domain pure, use state objects for serialization

3. ❌ **Don't leak Orleans into domain**
   - No Orleans references in domain projects

4. ❌ **Don't make state objects smart**
   - State is anemic data, not active records

5. ❌ **Don't skip mappers**
   - Always map between state and domain explicitly

## References

- [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- [Implementing Domain-Driven Design by Vaughn Vernon](https://vaughnvernon.com/)
- [Orleans Documentation](https://learn.microsoft.com/en-us/dotnet/orleans/)
- [Clean Architecture by Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## Conclusion

This architecture provides the best of both worlds:
- **DDD**: Rich domain models with business logic in the right place
- **Orleans**: Concurrency control, distribution, and persistence
- **MediatR**: Clean application orchestration

By keeping domain models separate from Orleans state and using grains as thin orchestrators, we maintain clean architecture principles while leveraging Orleans' powerful actor model capabilities.
