# Identity & Tenancy Architecture Review
**Dilcore Platform - Domain Modeling and Relationship Management**

**Version**: 1.0
**Date**: 2026-01-07
**Status**: Architecture Review & Proposal

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Current State Analysis](#current-state-analysis)
3. [Business Requirements](#business-requirements)
4. [Bounded Context Definitions](#bounded-context-definitions)
5. [Relationship Modeling Options](#relationship-modeling-options)
6. [Recommended Architecture](#recommended-architecture)
7. [Domain Models](#domain-models)
8. [Process Flows](#process-flows)
9. [Integration Patterns](#integration-patterns)
10. [Orleans Grain Design](#orleans-grain-design)
11. [Implementation Structure](#implementation-structure)
12. [Migration Strategy](#migration-strategy)
13. [Security & Authorization](#security--authorization)
14. [References](#references)

---

## 1. Executive Summary

### The Challenge

The Dilcore Platform requires clear separation between **Identity** (user accounts) and **Tenancy** (organizations) domains, while supporting many-to-many relationships between users and tenants. The current README states that Identity "manages users and their relationships to tenants," which creates tight coupling between these bounded contexts and violates Domain-Driven Design principles.

### Business Requirements

- ✅ One user account per person (platform-level identity)
- ✅ Users can create multiple tenants (organizations)
- ✅ Users can be invited to existing tenants
- ✅ Users can switch between tenants they belong to
- ✅ Clean separation between Identity and Tenancy domains

### Recommended Solution

**Introduce a third bounded context: Access Control Domain**

```
┌─────────────────┐         ┌─────────────────┐
│     Identity    │         │     Tenancy     │
│     Context     │         │     Context     │
│                 │         │                 │
│  - User         │         │  - Tenant       │
│  - Profile      │         │  - Settings     │
│  - Credentials  │         │  - Subscription │
└────────┬────────┘         └────────┬────────┘
         │                           │
         │    No Direct Coupling     │
         │                           │
         └───────────┬───────────────┘
                     │
         ┌───────────▼────────────┐
         │   Access Control       │
         │   Context              │
         │                        │
         │  - Membership          │
         │  - Role                │
         │  - Invitation          │
         │  - Permission          │
         └────────────────────────┘
```

**Key Benefits**:
- Clean separation of concerns
- No coupling between Identity and Tenancy
- Explicit ownership of the relationship concept
- Clear authorization boundaries
- Supports future multi-tenancy patterns (teams, groups, etc.)

---

## 2. Current State Analysis

### 2.1 Project Structure

Both domains follow the same clean architecture pattern:

```
Identity/
  ├── Identity.Domain              ← DDD models (empty)
  ├── Identity.Core                ← Use cases (empty)
  ├── Identity.Store               ← Repositories (empty)
  ├── Identity.Infrastructure      ← External services (empty)
  ├── Identity.WebApi              ← API endpoints (empty)
  ├── Identity.Actors.Abstractions ← Grain interfaces (empty)
  └── Identity.Actors              ← Grain implementations (empty)

Tenancy/
  ├── Tenancy.Domain               ← DDD models (empty)
  ├── Tenancy.Core                 ← Use cases (empty)
  ├── Tenancy.Store                ← Repositories (empty)
  ├── Tenancy.Infrastructure       ← External services (empty)
  ├── Tenancy.WebApi               ← API endpoints (empty)
  ├── Tenancy.Actors.Abstractions  ← Grain interfaces (empty)
  └── Tenancy.Actors               ← Grain implementations (empty)
```

### 2.2 Current Documentation Issues

**Identity README** (src/Identity/README.md, line 3):
> "Handles user management at the platform level. It manages users and their **relationships to tenants**, including defining **access levels and permissions**."

**Problems**:
1. ❌ Identity domain knows about tenants (coupling)
2. ❌ Mixing authentication concerns with authorization concerns
3. ❌ Unclear ownership: Does Identity own the relationship, or does Tenancy?
4. ❌ Violates Single Responsibility Principle

**Tenancy README** (src/Tenancy/README.md, line 2):
> "Enables multitenancy capabilities within the platform. This domain manages tenants and **controls user access** to specific tenant environments."

**Problems**:
1. ❌ Tenancy domain knows about users (coupling)
2. ❌ "Controls user access" suggests authorization logic in Tenancy

### 2.3 Assessment

**Status**: ✅ Good news - modules are empty scaffolds, so we can implement optimal architecture from scratch.

**Recommendation**: Revise READMEs and introduce a third domain for relationships.

---

## 3. Business Requirements

### 3.1 User Account Management

**Requirement**: One user account per person in the system.

**Characteristics**:
- Unique email address across the platform
- Single set of credentials (password, MFA, etc.)
- Profile information (name, avatar, preferences)
- Platform-level identity (not tenant-specific)

**Examples**:
- john@example.com registers once
- John can belong to multiple tenants using the same account
- John has one profile, one password, regardless of tenant membership

### 3.2 Tenant Management

**Requirement**: Users can create multiple tenants.

**Characteristics**:
- Tenant = Organization, Workspace, Company, etc.
- Tenant has settings, subscription plan, billing info
- Tenant lifecycle independent of user lifecycle
- Tenant can exist with no members (edge case: last member leaves)

**Examples**:
- Sarah creates "Acme Corp" tenant (becomes owner)
- Sarah creates "Freelance Projects" tenant (becomes owner)
- Sarah owns 2 tenants simultaneously

### 3.3 Membership & Invitations

**Requirement**: Users can be invited to existing tenants.

**Characteristics**:
- Invitation sent to email address
- User must accept invitation to join
- User assigned a role when joining (Owner, Admin, Member, Viewer)
- User can belong to multiple tenants
- User can leave a tenant

**Examples**:
- Sarah invites bob@example.com to "Acme Corp" as Admin
- Bob receives email, clicks link, joins "Acme Corp"
- Bob now belongs to "Acme Corp" (Admin) and his own tenant "Bob's Startup" (Owner)

### 3.4 Tenant Context Switching

**Requirement**: Users can switch between tenants they belong to.

**Characteristics**:
- API calls include `x-tenant` header
- User must be a member of the tenant to access it
- Different permissions per tenant (Admin in one, Viewer in another)

**Examples**:
- Sarah makes API call with `x-tenant: acme-corp` → sees Acme Corp data
- Sarah makes API call with `x-tenant: freelance-projects` → sees Freelance Projects data
- Sarah cannot use `x-tenant: bobs-startup` → unauthorized (not a member)

---

## 4. Bounded Context Definitions

### 4.1 Identity Context

**Responsibility**: Manage platform-level user accounts and authentication.

**Ubiquitous Language**:
- **User**: A person with an account on the platform
- **Credentials**: Authentication information (email, password hash, MFA)
- **Profile**: User's personal information (name, avatar, bio)
- **Authentication**: Process of verifying user identity

**What Identity DOES**:
- User registration
- User login/logout
- Password management (reset, change)
- MFA enrollment and verification
- Profile updates (name, avatar, preferences)
- Account deactivation/deletion

**What Identity DOES NOT**:
- ❌ Know about tenants
- ❌ Manage tenant memberships
- ❌ Handle authorization (roles, permissions)
- ❌ Control access to tenant resources

**External Integration Points**:
- Auth0 (authentication provider)
- Email service (verification emails)

### 4.2 Tenancy Context

**Responsibility**: Manage tenant organizations and their lifecycle.

**Ubiquitous Language**:
- **Tenant**: An organization, workspace, or company
- **Subscription**: Tenant's plan and billing information
- **Settings**: Tenant-specific configuration
- **Provisioning**: Creating and configuring a new tenant

**What Tenancy DOES**:
- Tenant creation
- Tenant settings management (name, logo, theme)
- Subscription management (plan, billing)
- Tenant deactivation/deletion
- Feature flags per tenant

**What Tenancy DOES NOT**:
- ❌ Know about users
- ❌ Manage user memberships
- ❌ Handle user authentication
- ❌ Control who can access the tenant

**External Integration Points**:
- Billing provider (Stripe, etc.)
- Feature flag service

### 4.3 Access Control Context (NEW)

**Responsibility**: Manage relationships between users and tenants, including roles and permissions.

**Ubiquitous Language**:
- **Membership**: A user's association with a tenant, including their role
- **Role**: A named set of permissions (Owner, Admin, Member, Viewer)
- **Invitation**: A request for a user to join a tenant
- **Permission**: A specific action a user can perform (e.g., "tenants.settings.update")
- **Authorization**: Process of determining if a user can perform an action in a tenant

**What Access Control DOES**:
- Create memberships when tenant is created (owner)
- Send invitations to users
- Accept/reject invitations
- Assign roles to members
- Verify user has permission to access tenant
- Remove members from tenants
- List tenants a user belongs to
- List members of a tenant

**What Access Control DOES NOT**:
- ❌ Authenticate users (Identity's job)
- ❌ Manage tenant settings (Tenancy's job)
- ❌ Store user profiles (Identity's job)
- ❌ Handle billing (Tenancy's job)

**External Integration Points**:
- Email service (invitation emails)
- Notification service (membership changes)

### 4.4 Context Map

**Relationship Type**: Customer-Supplier (Identity/Tenancy supply IDs to Access Control)

```
┌──────────────────────────────────────────────────────────────┐
│                                                              │
│  Platform                                                    │
│                                                              │
│  ┌───────────────┐                     ┌──────────────────┐ │
│  │   Identity    │                     │     Tenancy      │ │
│  │   Context     │                     │     Context      │ │
│  │               │                     │                  │ │
│  │ Published:    │                     │ Published:       │ │
│  │ - UserId      │                     │ - TenantId       │ │
│  │ - Email       │                     │ - TenantName     │ │
│  └───────┬───────┘                     └────────┬─────────┘ │
│          │                                      │           │
│          │ Supplies                Supplies     │           │
│          │ UserId                  TenantId     │           │
│          │                                      │           │
│          └───────────┬──────────────────────────┘           │
│                      │                                      │
│          ┌───────────▼────────────┐                         │
│          │   Access Control       │                         │
│          │   Context              │                         │
│          │                        │                         │
│          │ Consumes:              │                         │
│          │ - UserId (from Identity)                         │
│          │ - TenantId (from Tenancy)                        │
│          │                        │                         │
│          │ Manages:               │                         │
│          │ - Membership           │                         │
│          │ - Invitation           │                         │
│          │ - Role                 │                         │
│          │ - Permission           │                         │
│          └────────────────────────┘                         │
│                                                              │
└──────────────────────────────────────────────────────────────┘
```

**Integration Pattern**: **Conformist**
- Access Control conforms to IDs provided by Identity and Tenancy
- Access Control does not dictate structure of User or Tenant
- Access Control uses correlation IDs (UserId, TenantId) not entity references

---

## 5. Relationship Modeling Options

### Option 1: In Identity Domain ❌

**Model**: Identity.Domain contains User aggregate with TenantMemberships collection

```csharp
// Identity.Domain/User.cs
public class User : AggregateRoot
{
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public UserProfile Profile { get; private set; }

    // ❌ Coupling to Tenancy
    private List<TenantMembership> _memberships = new();
    public IReadOnlyList<TenantMembership> Memberships => _memberships;

    public void JoinTenant(TenantId tenantId, Role role) { ... }
}
```

**Pros**:
- User is the root for "my memberships" queries
- Single transaction when creating user with initial membership

**Cons**:
- ❌ Identity domain knows about Tenancy (tight coupling)
- ❌ User aggregate becomes large and complex
- ❌ Violates Single Responsibility Principle
- ❌ Cannot query "members of tenant" efficiently
- ❌ Authorization logic mixed with identity logic

**Verdict**: ❌ **NOT RECOMMENDED**

---

### Option 2: In Tenancy Domain ❌

**Model**: Tenancy.Domain contains Tenant aggregate with Members collection

```csharp
// Tenancy.Domain/Tenant.cs
public class Tenant : AggregateRoot
{
    public TenantId Id { get; private set; }
    public TenantName Name { get; private set; }
    public TenantSettings Settings { get; private set; }

    // ❌ Coupling to Identity
    private List<TenantMember> _members = new();
    public IReadOnlyList<TenantMember> Members => _members;

    public void AddMember(UserId userId, Role role) { ... }
}
```

**Pros**:
- Tenant is the root for "tenant members" queries
- Single transaction when adding member

**Cons**:
- ❌ Tenancy domain knows about Identity (tight coupling)
- ❌ Tenant aggregate becomes large with many members
- ❌ Cannot query "user's tenants" efficiently
- ❌ Authorization logic mixed with tenancy logic
- ❌ Tenant lifecycle coupled to member management

**Verdict**: ❌ **NOT RECOMMENDED**

---

### Option 3: Separate Access Control Domain ✅

**Model**: New domain with Membership aggregate

```csharp
// AccessControl.Domain/Membership.cs
public class Membership : AggregateRoot
{
    public MembershipId Id { get; private set; }
    public UserId UserId { get; private set; }        // Correlation ID
    public TenantId TenantId { get; private set; }    // Correlation ID
    public Role Role { get; private set; }
    public MembershipStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }

    public void ChangeRole(Role newRole) { ... }
    public void Deactivate() { ... }
}
```

**Pros**:
- ✅ **Clean separation**: No coupling between Identity and Tenancy
- ✅ **Single Responsibility**: Access Control owns authorization
- ✅ **Explicit concept**: Membership is a first-class entity
- ✅ **Flexible queries**: Can query by UserId OR TenantId
- ✅ **Scalable**: Membership aggregate is lightweight
- ✅ **Future-proof**: Easy to add teams, groups, hierarchies

**Cons**:
- ⚠️ Additional domain (more complexity)
- ⚠️ Distributed transactions (user creation + initial membership)

**Mitigation**:
- Use domain events for eventual consistency
- Use sagas/process managers for complex workflows

**Verdict**: ✅ **RECOMMENDED**

---

### Option 4: Shared Kernel ⚠️

**Model**: Shared domain with UserTenantRelationship

```csharp
// Shared.Domain/UserTenantRelationship.cs
public class UserTenantRelationship : Entity
{
    public UserId UserId { get; private set; }
    public TenantId TenantId { get; private set; }
    public Role Role { get; private set; }
}
```

**Pros**:
- Both domains can reference the relationship
- No new bounded context

**Cons**:
- ❌ Shared kernel requires coordination between teams
- ❌ Changes affect both Identity and Tenancy
- ❌ Unclear ownership of relationship logic
- ❌ Still creates coupling (via shared kernel)

**Verdict**: ⚠️ **NOT RECOMMENDED** (shared kernels should be minimal)

---

## 6. Recommended Architecture

### 6.1 Three-Domain Model

**Architecture**: Introduce **Access Control** as a separate bounded context.

```
┌──────────────────────────────────────────────────────────────────────┐
│                         Platform                                     │
│                                                                      │
│  ┌─────────────────────┐    ┌─────────────────────┐                 │
│  │  Identity Context   │    │  Tenancy Context    │                 │
│  ├─────────────────────┤    ├─────────────────────┤                 │
│  │ Aggregates:         │    │ Aggregates:         │                 │
│  │ - User              │    │ - Tenant            │                 │
│  │                     │    │ - Subscription      │                 │
│  │ Value Objects:      │    │                     │                 │
│  │ - Email             │    │ Value Objects:      │                 │
│  │ - UserProfile       │    │ - TenantName        │                 │
│  │                     │    │ - TenantSettings    │                 │
│  │ Domain Events:      │    │                     │                 │
│  │ - UserRegistered    │    │ Domain Events:      │                 │
│  │ - UserProfileUpdated│    │ - TenantCreated     │                 │
│  └──────────┬──────────┘    └──────────┬──────────┘                 │
│             │                          │                            │
│             │ Publishes events         │ Publishes events           │
│             │                          │                            │
│             └───────────┬──────────────┘                            │
│                         │                                           │
│             ┌───────────▼──────────────────────────┐                │
│             │   Access Control Context             │                │
│             ├──────────────────────────────────────┤                │
│             │ Aggregates:                          │                │
│             │ - Membership                         │                │
│             │ - Invitation                         │                │
│             │                                      │                │
│             │ Value Objects:                       │                │
│             │ - Role (Owner, Admin, Member, etc.)  │                │
│             │ - Permission                         │                │
│             │                                      │                │
│             │ Domain Events:                       │                │
│             │ - MembershipCreated                  │                │
│             │ - InvitationSent                     │                │
│             │ - MembershipRoleChanged              │                │
│             │                                      │                │
│             │ Subscribes to:                       │                │
│             │ - UserRegistered (from Identity)     │                │
│             │ - TenantCreated (from Tenancy)       │                │
│             └──────────────────────────────────────┘                │
│                                                                      │
└──────────────────────────────────────────────────────────────────────┘
```

### 6.2 Dependency Flow

**No Circular Dependencies**:

```
┌──────────────┐       ┌──────────────┐
│   Identity   │       │   Tenancy    │
│              │       │              │
│  Depends on: │       │  Depends on: │
│  - None      │       │  - None      │
└──────────────┘       └──────────────┘
       │                      │
       │                      │
       └──────┬───────────────┘
              │
              ▼
       ┌──────────────────┐
       │  Access Control  │
       │                  │
       │  Depends on:     │
       │  - Identity IDs  │
       │  - Tenancy IDs   │
       │  - Domain Events │
       └──────────────────┘
```

**Rules**:
1. ✅ Identity NEVER references Tenancy
2. ✅ Tenancy NEVER references Identity
3. ✅ Access Control references Identity and Tenancy IDs (value objects only)
4. ✅ Communication via domain events (decoupled)

### 6.3 Communication Patterns

**Domain Events** (Pub/Sub):

```csharp
// Identity publishes
public class UserRegistered : DomainEvent
{
    public UserId UserId { get; }
    public Email Email { get; }
    public string Name { get; }
}

// Tenancy publishes
public class TenantCreated : DomainEvent
{
    public TenantId TenantId { get; }
    public UserId CreatedByUserId { get; }  // Correlation ID
    public string TenantName { get; }
}

// Access Control subscribes
public class TenantCreatedHandler : IHandleEvent<TenantCreated>
{
    public async Task Handle(TenantCreated evt, CancellationToken ct)
    {
        // Create owner membership for the user who created the tenant
        var membership = Membership.CreateOwner(
            evt.CreatedByUserId,
            evt.TenantId);

        await _membershipRepository.AddAsync(membership);
    }
}
```

---

## 7. Domain Models

### 7.1 Identity Domain

#### User Aggregate

```csharp
namespace Dilcore.Identity.Domain;

public class User : AggregateRoot<UserId>
{
    // Properties
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public UserProfile Profile { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // Factory method
    public static User Register(Email email, string name)
    {
        var user = new User
        {
            Id = UserId.New(),
            Email = email,
            Profile = UserProfile.Create(name),
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        user.RaiseDomainEvent(new UserRegistered(user.Id, user.Email, name));

        return user;
    }

    // Commands
    public void UpdateProfile(string name, string bio, string avatarUrl)
    {
        Profile = Profile.Update(name, bio, avatarUrl);
        RaiseDomainEvent(new UserProfileUpdated(Id, name, bio, avatarUrl));
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        if (Status == UserStatus.Deactivated)
            throw new InvalidOperationException("User already deactivated");

        Status = UserStatus.Deactivated;
        RaiseDomainEvent(new UserDeactivated(Id));
    }
}
```

#### Value Objects

```csharp
public record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public static UserId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}

public record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail("Email is required");

        if (!IsValidEmail(value))
            return Result.Fail("Email format is invalid");

        return Result.Ok(new Email(value.ToLowerInvariant()));
    }

    private static bool IsValidEmail(string email)
    {
        // Use regex or EmailAddressAttribute
        return new EmailAddressAttribute().IsValid(email);
    }

    public override string ToString() => Value;
}

public record UserProfile
{
    public string Name { get; init; }
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }

    public static UserProfile Create(string name) => new()
    {
        Name = name,
        Bio = null,
        AvatarUrl = null
    };

    public UserProfile Update(string name, string? bio, string? avatarUrl) => this with
    {
        Name = name,
        Bio = bio,
        AvatarUrl = avatarUrl
    };
}

public enum UserStatus
{
    Active,
    Deactivated
}
```

#### Domain Events

```csharp
public record UserRegistered(
    UserId UserId,
    Email Email,
    string Name) : DomainEvent;

public record UserProfileUpdated(
    UserId UserId,
    string Name,
    string? Bio,
    string? AvatarUrl) : DomainEvent;

public record UserDeactivated(UserId UserId) : DomainEvent;
```

---

### 7.2 Tenancy Domain

#### Tenant Aggregate

```csharp
namespace Dilcore.Tenancy.Domain;

public class Tenant : AggregateRoot<TenantId>
{
    // Properties
    public TenantId Id { get; private set; }
    public TenantName Name { get; private set; }
    public TenantSlug Slug { get; private set; }
    public TenantSettings Settings { get; private set; }
    public TenantStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Factory method
    public static Tenant Create(string name, UserId createdByUserId)
    {
        var slug = TenantSlug.FromName(name);

        var tenant = new Tenant
        {
            Id = TenantId.New(),
            Name = TenantName.Create(name).Value,
            Slug = slug,
            Settings = TenantSettings.Default(),
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        tenant.RaiseDomainEvent(new TenantCreated(
            tenant.Id,
            createdByUserId,
            tenant.Name.Value,
            tenant.Slug.Value));

        return tenant;
    }

    // Commands
    public void UpdateSettings(string name, string? logoUrl, string? theme)
    {
        Name = TenantName.Create(name).Value;
        Settings = Settings.Update(logoUrl, theme);

        RaiseDomainEvent(new TenantSettingsUpdated(Id, name, logoUrl, theme));
    }

    public void Deactivate()
    {
        if (Status == TenantStatus.Deactivated)
            throw new InvalidOperationException("Tenant already deactivated");

        Status = TenantStatus.Deactivated;
        RaiseDomainEvent(new TenantDeactivated(Id));
    }
}
```

#### Value Objects

```csharp
public record TenantId(Guid Value)
{
    public static TenantId New() => new(Guid.NewGuid());
    public static TenantId From(Guid value) => new(value);
    public static TenantId From(string value) => new(Guid.Parse(value));

    public override string ToString() => Value.ToString();
}

public record TenantName
{
    public string Value { get; }

    private TenantName(string value)
    {
        Value = value;
    }

    public static Result<TenantName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Fail("Tenant name is required");

        if (value.Length > 100)
            return Result.Fail("Tenant name must be 100 characters or less");

        return Result.Ok(new TenantName(value));
    }

    public override string ToString() => Value;
}

public record TenantSlug
{
    public string Value { get; }

    private TenantSlug(string value)
    {
        Value = value;
    }

    public static TenantSlug FromName(string name)
    {
        // Convert "Acme Corp" → "acme-corp"
        var slug = name
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");

        // Remove special characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        return new TenantSlug(slug);
    }

    public override string ToString() => Value;
}

public record TenantSettings
{
    public string? LogoUrl { get; init; }
    public string? Theme { get; init; }

    public static TenantSettings Default() => new()
    {
        LogoUrl = null,
        Theme = "light"
    };

    public TenantSettings Update(string? logoUrl, string? theme) => this with
    {
        LogoUrl = logoUrl,
        Theme = theme ?? Theme
    };
}

public enum TenantStatus
{
    Active,
    Deactivated
}
```

#### Domain Events

```csharp
public record TenantCreated(
    TenantId TenantId,
    UserId CreatedByUserId,  // Correlation ID from Identity
    string TenantName,
    string TenantSlug) : DomainEvent;

public record TenantSettingsUpdated(
    TenantId TenantId,
    string Name,
    string? LogoUrl,
    string? Theme) : DomainEvent;

public record TenantDeactivated(TenantId TenantId) : DomainEvent;
```

---

### 7.3 Access Control Domain (NEW)

#### Membership Aggregate

```csharp
namespace Dilcore.AccessControl.Domain;

public class Membership : AggregateRoot<MembershipId>
{
    // Properties
    public MembershipId Id { get; private set; }
    public UserId UserId { get; private set; }       // From Identity
    public TenantId TenantId { get; private set; }   // From Tenancy
    public Role Role { get; private set; }
    public MembershipStatus Status { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    // Factory methods
    public static Membership CreateOwner(UserId userId, TenantId tenantId)
    {
        var membership = new Membership
        {
            Id = MembershipId.New(),
            UserId = userId,
            TenantId = tenantId,
            Role = Role.Owner,
            Status = MembershipStatus.Active,
            JoinedAt = DateTime.UtcNow
        };

        membership.RaiseDomainEvent(new MembershipCreated(
            membership.Id,
            membership.UserId,
            membership.TenantId,
            membership.Role));

        return membership;
    }

    public static Membership FromInvitation(
        UserId userId,
        TenantId tenantId,
        Role role)
    {
        var membership = new Membership
        {
            Id = MembershipId.New(),
            UserId = userId,
            TenantId = tenantId,
            Role = role,
            Status = MembershipStatus.Active,
            JoinedAt = DateTime.UtcNow
        };

        membership.RaiseDomainEvent(new MembershipCreated(
            membership.Id,
            membership.UserId,
            membership.TenantId,
            membership.Role));

        return membership;
    }

    // Commands
    public void ChangeRole(Role newRole)
    {
        if (Role == Role.Owner && newRole != Role.Owner)
        {
            throw new InvalidOperationException(
                "Cannot change owner role. Transfer ownership first.");
        }

        var oldRole = Role;
        Role = newRole;

        RaiseDomainEvent(new MembershipRoleChanged(
            Id,
            UserId,
            TenantId,
            oldRole,
            newRole));
    }

    public void Leave()
    {
        if (Status == MembershipStatus.Left)
            throw new InvalidOperationException("Member already left");

        if (Role == Role.Owner)
            throw new InvalidOperationException("Owner cannot leave. Transfer ownership first.");

        Status = MembershipStatus.Left;
        LeftAt = DateTime.UtcNow;

        RaiseDomainEvent(new MemberLeft(Id, UserId, TenantId));
    }

    public void Remove()
    {
        if (Role == Role.Owner)
            throw new InvalidOperationException("Cannot remove owner");

        Status = MembershipStatus.Removed;
        LeftAt = DateTime.UtcNow;

        RaiseDomainEvent(new MemberRemoved(Id, UserId, TenantId));
    }

    // Queries
    public bool IsActive => Status == MembershipStatus.Active;
    public bool CanManageMembers => Role == Role.Owner || Role == Role.Admin;
    public bool CanUpdateSettings => Role == Role.Owner || Role == Role.Admin;
}
```

#### Invitation Aggregate

```csharp
public class Invitation : AggregateRoot<InvitationId>
{
    // Properties
    public InvitationId Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public Email InviteeEmail { get; private set; }
    public Role Role { get; private set; }
    public UserId InvitedByUserId { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime InvitedAt { get; private set; }
    public DateTime? RespondedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    // Factory method
    public static Invitation Create(
        TenantId tenantId,
        Email inviteeEmail,
        Role role,
        UserId invitedByUserId)
    {
        var invitation = new Invitation
        {
            Id = InvitationId.New(),
            TenantId = tenantId,
            InviteeEmail = inviteeEmail,
            Role = role,
            InvitedByUserId = invitedByUserId,
            Status = InvitationStatus.Pending,
            InvitedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        invitation.RaiseDomainEvent(new InvitationSent(
            invitation.Id,
            invitation.TenantId,
            invitation.InviteeEmail,
            invitation.Role,
            invitation.InvitedByUserId));

        return invitation;
    }

    // Commands
    public Result<UserId> Accept(UserId userId)
    {
        if (Status != InvitationStatus.Pending)
            return Result.Fail("Invitation already responded to");

        if (DateTime.UtcNow > ExpiresAt)
            return Result.Fail("Invitation has expired");

        Status = InvitationStatus.Accepted;
        RespondedAt = DateTime.UtcNow;

        RaiseDomainEvent(new InvitationAccepted(
            Id,
            TenantId,
            userId,
            Role));

        return Result.Ok(userId);
    }

    public Result Reject()
    {
        if (Status != InvitationStatus.Pending)
            return Result.Fail("Invitation already responded to");

        Status = InvitationStatus.Rejected;
        RespondedAt = DateTime.UtcNow;

        RaiseDomainEvent(new InvitationRejected(Id, TenantId, InviteeEmail));

        return Result.Ok();
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
```

#### Value Objects

```csharp
public record MembershipId(Guid Value)
{
    public static MembershipId New() => new(Guid.NewGuid());
    public static MembershipId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

public record InvitationId(Guid Value)
{
    public static InvitationId New() => new(Guid.NewGuid());
    public static InvitationId From(Guid value) => new(value);

    public override string ToString() => Value.ToString();
}

public enum Role
{
    Owner = 1,
    Admin = 2,
    Member = 3,
    Viewer = 4
}

public enum MembershipStatus
{
    Active,
    Left,
    Removed
}

public enum InvitationStatus
{
    Pending,
    Accepted,
    Rejected
}
```

#### Domain Events

```csharp
public record MembershipCreated(
    MembershipId MembershipId,
    UserId UserId,
    TenantId TenantId,
    Role Role) : DomainEvent;

public record MembershipRoleChanged(
    MembershipId MembershipId,
    UserId UserId,
    TenantId TenantId,
    Role OldRole,
    Role NewRole) : DomainEvent;

public record MemberLeft(
    MembershipId MembershipId,
    UserId UserId,
    TenantId TenantId) : DomainEvent;

public record MemberRemoved(
    MembershipId MembershipId,
    UserId UserId,
    TenantId TenantId) : DomainEvent;

public record InvitationSent(
    InvitationId InvitationId,
    TenantId TenantId,
    Email InviteeEmail,
    Role Role,
    UserId InvitedByUserId) : DomainEvent;

public record InvitationAccepted(
    InvitationId InvitationId,
    TenantId TenantId,
    UserId UserId,
    Role Role) : DomainEvent;

public record InvitationRejected(
    InvitationId InvitationId,
    TenantId TenantId,
    Email InviteeEmail) : DomainEvent;
```

---

## 8. Process Flows

### 8.1 User Registration

**Scenario**: New user creates an account.

**Actors**: User, Identity Context

```
┌──────┐                  ┌──────────────────┐
│ User │                  │ Identity Context │
└───┬──┘                  └────────┬─────────┘
    │                              │
    │ POST /auth/register          │
    │ { email, password, name }    │
    │─────────────────────────────>│
    │                              │
    │                              │ Validate email unique
    │                              │ Hash password
    │                              │ Create User aggregate
    │                              │
    │                              │ Publish: UserRegistered
    │                              │ ─────────────────────>
    │                              │
    │ 201 Created                  │
    │ { userId, email, name }      │
    │<─────────────────────────────│
    │                              │
```

**Key Points**:
- No tenant involved at this stage
- User exists at platform level
- UserRegistered event published (for integration)

---

### 8.2 Create First Tenant

**Scenario**: User creates their first tenant (becomes owner).

**Actors**: User, Tenancy Context, Access Control Context

```
┌──────┐       ┌──────────────────┐       ┌────────────────────┐
│ User │       │ Tenancy Context  │       │ Access Control     │
└───┬──┘       └────────┬─────────┘       └─────────┬──────────┘
    │                   │                           │
    │ POST /tenants                                 │
    │ { name: "Acme" }                              │
    │──────────────────>│                           │
    │                   │                           │
    │                   │ Create Tenant aggregate   │
    │                   │ (Id, Name, Slug, Status)  │
    │                   │                           │
    │                   │ Publish: TenantCreated    │
    │                   │ (TenantId, UserId)        │
    │                   │──────────────────────────>│
    │                   │                           │
    │                   │                           │ Subscribe to TenantCreated
    │                   │                           │
    │                   │                           │ Create Membership
    │                   │                           │ (UserId, TenantId, Owner)
    │                   │                           │
    │                   │                           │ Publish: MembershipCreated
    │                   │                           │ ───────────────────────>
    │                   │                           │
    │ 201 Created       │                           │
    │ { tenantId, name }│                           │
    │<──────────────────│                           │
    │                   │                           │
```

**Key Points**:
- Tenancy creates Tenant aggregate
- TenantCreated event triggers membership creation
- Access Control creates owner membership automatically
- Eventually consistent (event-driven)

---

### 8.3 Invite User to Tenant

**Scenario**: Tenant owner invites another user.

**Actors**: Owner, Invitee, Access Control Context, Identity Context

```
┌───────┐    ┌────────────────────┐    ┌──────────────────┐    ┌─────────┐
│ Owner │    │ Access Control     │    │ Identity Context │    │ Invitee │
└───┬───┘    └─────────┬──────────┘    └────────┬─────────┘    └────┬────┘
    │                  │                        │                   │
    │ POST /tenants/{id}/invitations            │                   │
    │ { email, role }                           │                   │
    │─────────────────>│                        │                   │
    │                  │                        │                   │
    │                  │ Verify owner has       │                   │
    │                  │ permission to invite   │                   │
    │                  │                        │                   │
    │                  │ Lookup user by email   │                   │
    │                  │───────────────────────>│                   │
    │                  │                        │                   │
    │                  │ User exists (UserId)   │                   │
    │                  │<───────────────────────│                   │
    │                  │                        │                   │
    │                  │ Create Invitation      │                   │
    │                  │ (TenantId, Email, Role)│                   │
    │                  │                        │                   │
    │                  │ Publish: InvitationSent│                   │
    │                  │ ─────────────────────────────────────────>│
    │                  │                        │                   │
    │                  │                        │  Email: Join Acme │
    │                  │                        │  as Admin         │
    │                  │                        │<──────────────────│
    │                  │                        │                   │
    │ 201 Created      │                        │                   │
    │ { invitationId } │                        │                   │
    │<─────────────────│                        │                   │
    │                  │                        │                   │
```

**Key Points**:
- Access Control verifies inviter has permission
- Identity is queried to check if email exists
- Invitation aggregate created
- Email sent asynchronously via event

---

### 8.4 Accept Invitation

**Scenario**: Invited user accepts invitation and joins tenant.

**Actors**: Invitee, Access Control Context

```
┌─────────┐                  ┌────────────────────┐
│ Invitee │                  │ Access Control     │
└────┬────┘                  └─────────┬──────────┘
     │                                 │
     │ POST /invitations/{id}/accept   │
     │────────────────────────────────>│
     │                                 │
     │                                 │ Load Invitation aggregate
     │                                 │ (verify not expired, pending)
     │                                 │
     │                                 │ Accept(UserId)
     │                                 │   Status = Accepted
     │                                 │
     │                                 │ Publish: InvitationAccepted
     │                                 │ (InvitationId, UserId, TenantId, Role)
     │                                 │
     │                                 │ Create Membership
     │                                 │ (UserId, TenantId, Role)
     │                                 │
     │                                 │ Publish: MembershipCreated
     │                                 │ ───────────────────────>
     │                                 │
     │ 200 OK                          │
     │ { membershipId, role }          │
     │<────────────────────────────────│
     │                                 │
```

**Key Points**:
- Invitation.Accept() validates and changes status
- Membership created as a result of acceptance
- User now has access to tenant

---

### 8.5 Switch Tenant Context

**Scenario**: User makes API call with different tenant.

**Actors**: User, Middleware, Access Control Context

```
┌──────┐       ┌────────────────┐       ┌────────────────────┐
│ User │       │ Middleware     │       │ Access Control     │
└───┬──┘       └────────┬───────┘       └─────────┬──────────┘
    │                   │                          │
    │ GET /projects     │                          │
    │ x-tenant: acme    │                          │
    │──────────────────>│                          │
    │                   │                          │
    │                   │ Parse x-tenant header    │
    │                   │ (tenantId = "acme")      │
    │                   │                          │
    │                   │ Parse auth token         │
    │                   │ (userId = "user123")     │
    │                   │                          │
    │                   │ Verify membership        │
    │                   │ (userId, tenantId)       │
    │                   │─────────────────────────>│
    │                   │                          │
    │                   │                          │ Query: GetMembership
    │                   │                          │   by UserId + TenantId
    │                   │                          │
    │                   │ Membership exists        │
    │                   │ { role: Admin }          │
    │                   │<─────────────────────────│
    │                   │                          │
    │                   │ Set ITenantContext       │
    │                   │ (Name: acme, Role: Admin)│
    │                   │                          │
    │                   │ Continue pipeline        │
    │                   │   ─────────>             │
    │                   │                          │
    │ 200 OK            │                          │
    │ [...projects...]  │                          │
    │<──────────────────│                          │
    │                   │                          │
```

**Key Points**:
- Middleware parses x-tenant header
- Access Control verifies user is member of tenant
- ITenantContext populated with tenant info + role
- Request proceeds if authorized, 403 if not a member

---

### 8.6 List My Tenants

**Scenario**: User queries which tenants they belong to.

**Actors**: User, Access Control Context

```
┌──────┐                  ┌────────────────────┐
│ User │                  │ Access Control     │
└───┬──┘                  └─────────┬──────────┘
    │                              │
    │ GET /users/me/tenants        │
    │─────────────────────────────>│
    │                              │
    │                              │ Query: GetMembershipsByUserId
    │                              │   where UserId = user123
    │                              │   and Status = Active
    │                              │
    │                              │ Result:
    │                              │ - (TenantId: acme, Role: Admin)
    │                              │ - (TenantId: startup, Role: Owner)
    │                              │
    │ 200 OK                       │
    │ [                            │
    │   { tenantId, role },        │
    │   { tenantId, role }         │
    │ ]                            │
    │<─────────────────────────────│
    │                              │
```

**Key Points**:
- Access Control queries memberships by UserId
- Returns TenantId + Role pairs
- Client can then query Tenancy context for tenant details (name, logo, etc.)

---

## 9. Integration Patterns

### 9.1 Domain Events with MediatR

**Implementation**: Use MediatR notifications for domain events.

**AccessControl.Core/EventHandlers/TenantCreatedHandler.cs**:

```csharp
public class TenantCreatedHandler : INotificationHandler<TenantCreated>
{
    private readonly IMembershipRepository _membershipRepository;

    public async Task Handle(TenantCreated notification, CancellationToken ct)
    {
        // Create owner membership automatically
        var membership = Membership.CreateOwner(
            notification.CreatedByUserId,
            notification.TenantId);

        await _membershipRepository.AddAsync(membership, ct);
    }
}
```

**Tenancy.Core/Commands/CreateTenantHandler.cs**:

```csharp
public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, Result<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPublisher _publisher;

    public async Task<Result<TenantDto>> Handle(
        CreateTenantCommand request,
        CancellationToken ct)
    {
        var tenant = Tenant.Create(request.Name, request.CreatedByUserId);

        await _tenantRepository.AddAsync(tenant, ct);

        // Publish domain events
        foreach (var domainEvent in tenant.DomainEvents)
        {
            await _publisher.Publish(domainEvent, ct);
        }

        tenant.ClearDomainEvents();

        return Result.Ok(TenantDto.FromAggregate(tenant));
    }
}
```

### 9.2 Cross-Context Queries

**Problem**: Access Control has TenantId, but needs tenant name/logo for display.

**Solution**: Use read models or query services (not aggregate references).

**AccessControl.Core/QueryServices/IMembershipEnrichmentService.cs**:

```csharp
public interface IMembershipEnrichmentService
{
    Task<MembershipDetailsDto> EnrichAsync(
        Membership membership,
        CancellationToken ct);
}

public class MembershipEnrichmentService : IMembershipEnrichmentService
{
    private readonly IUserQueryService _userQueryService;     // Queries Identity
    private readonly ITenantQueryService _tenantQueryService; // Queries Tenancy

    public async Task<MembershipDetailsDto> EnrichAsync(
        Membership membership,
        CancellationToken ct)
    {
        // Parallel queries to Identity and Tenancy contexts
        var userTask = _userQueryService.GetUserBasicInfoAsync(
            membership.UserId, ct);

        var tenantTask = _tenantQueryService.GetTenantBasicInfoAsync(
            membership.TenantId, ct);

        await Task.WhenAll(userTask, tenantTask);

        return new MembershipDetailsDto
        {
            MembershipId = membership.Id,
            UserId = membership.UserId,
            UserName = userTask.Result.Name,
            UserEmail = userTask.Result.Email,
            TenantId = membership.TenantId,
            TenantName = tenantTask.Result.Name,
            TenantSlug = tenantTask.Result.Slug,
            Role = membership.Role,
            JoinedAt = membership.JoinedAt
        };
    }
}
```

**Query Services** (in each context's Core layer):

```csharp
// Identity.Core/QueryServices/IUserQueryService.cs
public interface IUserQueryService
{
    Task<UserBasicInfo> GetUserBasicInfoAsync(UserId userId, CancellationToken ct);
    Task<User?> FindByEmailAsync(Email email, CancellationToken ct);
}

// Tenancy.Core/QueryServices/ITenantQueryService.cs
public interface ITenantQueryService
{
    Task<TenantBasicInfo> GetTenantBasicInfoAsync(TenantId tenantId, CancellationToken ct);
    Task<Tenant?> FindBySlugAsync(string slug, CancellationToken ct);
}
```

**Benefits**:
- Read-only queries across contexts
- No aggregate references (just IDs)
- Loosely coupled via interfaces
- Can optimize with read models/caching

### 9.3 Saga/Process Manager (Optional)

For complex workflows requiring coordination.

**Example**: Tenant Deactivation Saga

```
User Deactivates Tenant
  ↓
Tenancy: Tenant.Deactivate()
  ↓ Publishes: TenantDeactivated
  ↓
Saga Coordinator:
  1. Deactivate all memberships (AccessControl)
  2. Cancel subscription (Billing)
  3. Archive data (DataArchival)
  4. Send notifications (Notifications)
  ↓
Complete or Compensate on failure
```

**Implementation** (if needed later):

```csharp
public class TenantDeactivationSaga :
    IHandleEvent<TenantDeactivated>,
    IHandleEvent<MembershipDeactivated>,
    IHandleEvent<SubscriptionCancelled>
{
    // Tracks state of multi-step process
    // Coordinates compensation if steps fail
}
```

---

## 10. Orleans Grain Design

### 10.1 Grain Mapping to Aggregates

**Principle**: One grain per aggregate root.

| Aggregate | Grain Interface | Grain Key | Domain |
|-----------|----------------|-----------|--------|
| User | IUserGrain | UserId (Guid) | Identity |
| Tenant | ITenantGrain | TenantId (Guid) | Tenancy |
| Membership | IMembershipGrain | MembershipId (Guid) | AccessControl |
| Invitation | IInvitationGrain | InvitationId (Guid) | AccessControl |

### 10.2 Grain Interfaces

**Identity.Actors.Abstractions/IUserGrain.cs**:

```csharp
public interface IUserGrain : IGrainWithGuidKey
{
    Task<UserDto> GetProfileAsync();
    Task<Result> UpdateProfileAsync(string name, string? bio, string? avatarUrl);
    Task<Result> DeactivateAsync();
}
```

**Tenancy.Actors.Abstractions/ITenantGrain.cs**:

```csharp
public interface ITenantGrain : IGrainWithGuidKey
{
    Task<TenantDto> GetDetailsAsync();
    Task<Result> UpdateSettingsAsync(string name, string? logoUrl, string? theme);
    Task<Result> DeactivateAsync();
}
```

**AccessControl.Actors.Abstractions/IMembershipGrain.cs**:

```csharp
public interface IMembershipGrain : IGrainWithGuidKey
{
    Task<MembershipDto> GetDetailsAsync();
    Task<Result> ChangeRoleAsync(Role newRole);
    Task<Result> LeaveAsync();
    Task<Result> RemoveAsync();
}
```

**AccessControl.Actors.Abstractions/IInvitationGrain.cs**:

```csharp
public interface IInvitationGrain : IGrainWithGuidKey
{
    Task<InvitationDto> GetDetailsAsync();
    Task<Result<MembershipId>> AcceptAsync(UserId userId);
    Task<Result> RejectAsync();
}
```

### 10.3 Query Grains (Stateless Workers)

For cross-aggregate queries.

**AccessControl.Actors.Abstractions/IMembershipQueryGrain.cs**:

```csharp
[StatelessWorker]
public interface IMembershipQueryGrain : IGrainWithIntegerKey
{
    Task<List<MembershipDto>> GetMembershipsByUserIdAsync(UserId userId);
    Task<List<MembershipDto>> GetMembershipsByTenantIdAsync(TenantId tenantId);
    Task<MembershipDto?> GetMembershipAsync(UserId userId, TenantId tenantId);
}
```

**Implementation** (AccessControl.Actors/MembershipQueryGrain.cs):

```csharp
public class MembershipQueryGrain : Grain, IMembershipQueryGrain
{
    private readonly IMembershipRepository _repository;

    public async Task<List<MembershipDto>> GetMembershipsByUserIdAsync(UserId userId)
    {
        var memberships = await _repository.GetByUserIdAsync(userId);
        return memberships.Select(MembershipDto.FromAggregate).ToList();
    }

    public async Task<MembershipDto?> GetMembershipAsync(
        UserId userId,
        TenantId tenantId)
    {
        var membership = await _repository.GetByUserAndTenantAsync(userId, tenantId);
        return membership != null
            ? MembershipDto.FromAggregate(membership)
            : null;
    }
}
```

**Usage from MediatR Handler**:

```csharp
public class GetMyTenantsHandler : IRequestHandler<GetMyTenantsQuery, Result<List<TenantMembershipDto>>>
{
    private readonly IGrainFactory _grainFactory;
    private readonly IUserContext _userContext;

    public async Task<Result<List<TenantMembershipDto>>> Handle(
        GetMyTenantsQuery request,
        CancellationToken ct)
    {
        // Get stateless worker grain (any instance)
        var queryGrain = _grainFactory.GetGrain<IMembershipQueryGrain>(0);

        // Query memberships
        var memberships = await queryGrain.GetMembershipsByUserIdAsync(
            _userContext.UserId);

        return Result.Ok(memberships);
    }
}
```

### 10.4 Grain-to-Grain Communication

**Scenario**: Deactivate tenant → deactivate all memberships.

**TenantGrain.cs**:

```csharp
public class TenantGrain : Grain, ITenantGrain
{
    private readonly IPersistentState<TenantState> _state;
    private readonly IGrainFactory _grainFactory;

    public async Task<Result> DeactivateAsync()
    {
        // 1. Deactivate tenant aggregate
        _state.State.Status = TenantStatus.Deactivated;
        await _state.WriteStateAsync();

        // 2. Publish domain event (for eventual consistency)
        // Event handler will deactivate memberships asynchronously

        // 3. OR: Direct grain-to-grain calls (synchronous)
        var tenantId = TenantId.From(this.GetPrimaryKey());
        var queryGrain = _grainFactory.GetGrain<IMembershipQueryGrain>(0);

        var memberships = await queryGrain.GetMembershipsByTenantIdAsync(tenantId);

        foreach (var membership in memberships)
        {
            var membershipGrain = _grainFactory.GetGrain<IMembershipGrain>(
                membership.MembershipId.Value);

            await membershipGrain.RemoveAsync();
        }

        return Result.Ok();
    }
}
```

**Trade-offs**:
- **Option A**: Publish event, let event handler deactivate memberships (eventual consistency, decoupled)
- **Option B**: Direct grain calls (immediate consistency, coupled)

**Recommendation**: Option A (event-driven) for loose coupling.

---

## 11. Implementation Structure

### 11.1 Project Structure (Updated)

**Add AccessControl domain**:

```
src/
  AccessControl/                      ← NEW
    AccessControl.Domain/
      Aggregates/
        Membership.cs
        Invitation.cs
      ValueObjects/
        MembershipId.cs
        InvitationId.cs
        Role.cs
      Events/
        MembershipCreated.cs
        InvitationSent.cs
        InvitationAccepted.cs

    AccessControl.Core/
      Commands/
        CreateInvitationCommand.cs
        AcceptInvitationCommand.cs
        ChangeMemberRoleCommand.cs
      Queries/
        GetMyTenantsQuery.cs
        GetTenantMembersQuery.cs
      Handlers/
        CreateInvitationHandler.cs
        AcceptInvitationHandler.cs
      EventHandlers/
        TenantCreatedHandler.cs       ← Subscribes to Tenancy events
        UserRegisteredHandler.cs      ← Subscribes to Identity events

    AccessControl.Store/
      Repositories/
        MembershipRepository.cs
        InvitationRepository.cs
      EF/
        AccessControlDbContext.cs
        MembershipConfiguration.cs

    AccessControl.Infrastructure/
      Services/
        EmailInvitationService.cs

    AccessControl.WebApi/
      Endpoints/
        MembershipEndpoints.cs
        InvitationEndpoints.cs

    AccessControl.Actors.Abstractions/
      IMembershipGrain.cs
      IInvitationGrain.cs
      IMembershipQueryGrain.cs

    AccessControl.Actors/
      MembershipGrain.cs
      InvitationGrain.cs
      MembershipQueryGrain.cs

  Identity/
    Identity.Domain/
      Aggregates/
        User.cs                       ← NO tenant references
      ValueObjects/
        UserId.cs
        Email.cs
        UserProfile.cs
      Events/
        UserRegistered.cs

    Identity.Core/
      Commands/
        RegisterUserCommand.cs
      Queries/
        GetUserProfileQuery.cs
      Handlers/
        RegisterUserHandler.cs
      QueryServices/
        IUserQueryService.cs          ← For cross-context queries

    Identity.Store/
      Repositories/
        UserRepository.cs
      EF/
        IdentityDbContext.cs

    Identity.WebApi/
      Endpoints/
        AuthEndpoints.cs
        UserEndpoints.cs

    Identity.Actors.Abstractions/
      IUserGrain.cs

    Identity.Actors/
      UserGrain.cs

  Tenancy/
    Tenancy.Domain/
      Aggregates/
        Tenant.cs                     ← NO user references
      ValueObjects/
        TenantId.cs
        TenantName.cs
        TenantSlug.cs
      Events/
        TenantCreated.cs

    Tenancy.Core/
      Commands/
        CreateTenantCommand.cs
      Queries/
        GetTenantDetailsQuery.cs
      Handlers/
        CreateTenantHandler.cs
      QueryServices/
        ITenantQueryService.cs        ← For cross-context queries

    Tenancy.Store/
      Repositories/
        TenantRepository.cs
      EF/
        TenancyDbContext.cs

    Tenancy.WebApi/
      Endpoints/
        TenantEndpoints.cs

    Tenancy.Actors.Abstractions/
      ITenantGrain.cs

    Tenancy.Actors/
      TenantGrain.cs
```

### 11.2 Database Schema

**Three separate databases** (or schemas):

```sql
-- Identity Database
CREATE TABLE Users (
    Id UUID PRIMARY KEY,
    Email VARCHAR(255) UNIQUE NOT NULL,
    PasswordHash VARCHAR(255) NOT NULL,
    Name VARCHAR(255) NOT NULL,
    Bio TEXT,
    AvatarUrl VARCHAR(500),
    Status VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL,
    LastLoginAt TIMESTAMP
);

-- Tenancy Database
CREATE TABLE Tenants (
    Id UUID PRIMARY KEY,
    Name VARCHAR(255) NOT NULL,
    Slug VARCHAR(255) UNIQUE NOT NULL,
    LogoUrl VARCHAR(500),
    Theme VARCHAR(50),
    Status VARCHAR(50) NOT NULL,
    CreatedAt TIMESTAMP NOT NULL
);

-- AccessControl Database
CREATE TABLE Memberships (
    Id UUID PRIMARY KEY,
    UserId UUID NOT NULL,        -- Correlation ID (from Identity)
    TenantId UUID NOT NULL,      -- Correlation ID (from Tenancy)
    Role VARCHAR(50) NOT NULL,
    Status VARCHAR(50) NOT NULL,
    JoinedAt TIMESTAMP NOT NULL,
    LeftAt TIMESTAMP,
    UNIQUE(UserId, TenantId)     -- One membership per user-tenant pair
);

CREATE INDEX IX_Memberships_UserId ON Memberships(UserId);
CREATE INDEX IX_Memberships_TenantId ON Memberships(TenantId);

CREATE TABLE Invitations (
    Id UUID PRIMARY KEY,
    TenantId UUID NOT NULL,
    InviteeEmail VARCHAR(255) NOT NULL,
    Role VARCHAR(50) NOT NULL,
    InvitedByUserId UUID NOT NULL,
    Status VARCHAR(50) NOT NULL,
    InvitedAt TIMESTAMP NOT NULL,
    RespondedAt TIMESTAMP,
    ExpiresAt TIMESTAMP NOT NULL
);

CREATE INDEX IX_Invitations_TenantId ON Invitations(TenantId);
CREATE INDEX IX_Invitations_InviteeEmail ON Invitations(InviteeEmail);
```

**Benefits**:
- Physical separation of contexts
- Independent scaling and backups
- Clear ownership boundaries

**Alternative**: Single database with separate schemas (Identity, Tenancy, AccessControl)

### 11.3 API Endpoint Structure

**Identity.WebApi/Endpoints/AuthEndpoints.cs**:

```csharp
public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
{
    group.MapPost("/register", RegisterUser)
        .AllowAnonymous()
        .ExcludeFromMultiTenantResolution();

    group.MapPost("/login", Login)
        .AllowAnonymous()
        .ExcludeFromMultiTenantResolution();

    return group;
}
```

**Tenancy.WebApi/Endpoints/TenantEndpoints.cs**:

```csharp
public static RouteGroupBuilder MapTenantEndpoints(this RouteGroupBuilder group)
{
    group.MapPost("/", CreateTenant);           // POST /tenants
    group.MapGet("/{tenantId}", GetTenant);     // GET /tenants/{id}
    group.MapPut("/{tenantId}", UpdateTenant);  // PUT /tenants/{id}

    return group;
}
```

**AccessControl.WebApi/Endpoints/MembershipEndpoints.cs**:

```csharp
public static RouteGroupBuilder MapMembershipEndpoints(this RouteGroupBuilder group)
{
    group.MapGet("/users/me/tenants", GetMyTenants);
        // GET /users/me/tenants → List tenants I belong to

    group.MapGet("/tenants/{tenantId}/members", GetTenantMembers);
        // GET /tenants/{id}/members → List members of tenant

    group.MapPost("/tenants/{tenantId}/invitations", InviteUser);
        // POST /tenants/{id}/invitations → Invite user

    group.MapPost("/invitations/{invitationId}/accept", AcceptInvitation);
        // POST /invitations/{id}/accept

    group.MapDelete("/tenants/{tenantId}/members/{userId}", RemoveMember);
        // DELETE /tenants/{id}/members/{userId}

    return group;
}
```

**Program.cs** (WebApi):

```csharp
// Map all module endpoints
app.MapGroup("/auth").MapAuthEndpoints();
app.MapGroup("/tenants").MapTenantEndpoints();
app.MapGroup("/").MapMembershipEndpoints();  // Multiple groups
```

---

## 12. Migration Strategy

### 12.1 Current State

- ✅ Projects exist (empty scaffolds)
- ❌ README suggests Identity owns relationships (incorrect)

### 12.2 Implementation Order

**Phase 1: Identity Context** (Week 1)
1. Implement User aggregate
2. Implement registration/login use cases
3. Set up IdentityDbContext
4. Create UserGrain
5. Add API endpoints

**Phase 2: Tenancy Context** (Week 2)
1. Implement Tenant aggregate
2. Implement create/update tenant use cases
3. Set up TenancyDbContext
4. Create TenantGrain
5. Add API endpoints

**Phase 3: Access Control Context** (Week 3-4)
1. Create AccessControl projects
2. Implement Membership aggregate
3. Implement Invitation aggregate
4. Set up AccessControlDbContext
5. Implement event handlers (subscribe to TenantCreated, etc.)
6. Create Membership/Invitation grains
7. Add API endpoints
8. Build middleware for membership verification

**Phase 4: Integration** (Week 5)
1. Wire up domain events (MediatR notifications)
2. Implement query services for cross-context queries
3. Build membership enrichment services
4. End-to-end testing

### 12.3 Documentation Updates

**Update READMEs**:

**Identity/README.md**:
```markdown
# Identity Domain

Handles platform-level user authentication and profile management.

**Responsibilities**:
- User registration and authentication
- Password management (reset, change)
- User profile updates
- Account lifecycle (activate, deactivate)

**What Identity does NOT do**:
- Manage tenant memberships (see AccessControl domain)
- Handle authorization (see AccessControl domain)
```

**Tenancy/README.md**:
```markdown
# Tenancy Domain

Manages tenant organizations and their settings.

**Responsibilities**:
- Tenant creation and configuration
- Tenant settings management
- Subscription management
- Tenant lifecycle

**What Tenancy does NOT do**:
- Manage user memberships (see AccessControl domain)
- Authenticate users (see Identity domain)
```

**AccessControl/README.md** (NEW):
```markdown
# Access Control Domain

Manages relationships between users and tenants, including roles and permissions.

**Responsibilities**:
- User-tenant memberships
- Role assignment (Owner, Admin, Member, Viewer)
- Invitations (send, accept, reject)
- Authorization (verify user can access tenant)

**Dependencies**:
- Consumes UserId from Identity (correlation ID)
- Consumes TenantId from Tenancy (correlation ID)
```

---

## 13. Security & Authorization

### 13.1 Authorization Middleware

**TenantAuthorizationMiddleware.cs** (in AccessControl.Infrastructure):

```csharp
public class TenantAuthorizationMiddleware : IMiddleware
{
    private readonly IMembershipQueryGrain _queryGrain;
    private readonly IUserContext _userContext;
    private readonly ITenantContextResolver _tenantResolver;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Skip for excluded endpoints
        if (context.IsExcludedFromMultiTenantResolution())
        {
            await next(context);
            return;
        }

        // Resolve tenant from x-tenant header
        if (!_tenantResolver.TryResolve(out var tenantContext))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsJsonAsync(new { error = "Missing x-tenant header" });
            return;
        }

        // Verify user is member of tenant
        var membership = await _queryGrain.GetMembershipAsync(
            _userContext.UserId,
            TenantId.From(tenantContext.Name));

        if (membership == null || !membership.IsActive)
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsJsonAsync(new { error = "Not a member of this tenant" });
            return;
        }

        // Store membership in context (for authorization checks)
        context.Items["Membership"] = membership;

        await next(context);
    }
}
```

**Register in Program.cs**:

```csharp
// After UseMultiTenant()
app.UseMiddleware<TenantAuthorizationMiddleware>();
```

### 13.2 Permission-Based Authorization

**Define Permissions**:

```csharp
public static class Permissions
{
    // Tenant management
    public const string TenantSettingsUpdate = "tenants.settings.update";
    public const string TenantDelete = "tenants.delete";

    // Member management
    public const string MembersInvite = "members.invite";
    public const string MembersRemove = "members.remove";
    public const string MembersRoleChange = "members.role.change";

    // Projects (example)
    public const string ProjectsCreate = "projects.create";
    public const string ProjectsDelete = "projects.delete";
}
```

**Role-Permission Mapping**:

```csharp
public static class RolePermissions
{
    public static IReadOnlySet<string> GetPermissions(Role role) => role switch
    {
        Role.Owner => new HashSet<string>
        {
            Permissions.TenantSettingsUpdate,
            Permissions.TenantDelete,
            Permissions.MembersInvite,
            Permissions.MembersRemove,
            Permissions.MembersRoleChange,
            Permissions.ProjectsCreate,
            Permissions.ProjectsDelete
        },
        Role.Admin => new HashSet<string>
        {
            Permissions.TenantSettingsUpdate,
            Permissions.MembersInvite,
            Permissions.ProjectsCreate,
            Permissions.ProjectsDelete
        },
        Role.Member => new HashSet<string>
        {
            Permissions.ProjectsCreate
        },
        Role.Viewer => new HashSet<string>(),
        _ => new HashSet<string>()
    };
}
```

**Authorization Attribute**:

```csharp
public class RequirePermissionAttribute : Attribute, IAuthorizationFilter
{
    private readonly string _permission;

    public RequirePermissionAttribute(string permission)
    {
        _permission = permission;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var membership = context.HttpContext.Items["Membership"] as MembershipDto;

        if (membership == null)
        {
            context.Result = new ForbidResult();
            return;
        }

        var permissions = RolePermissions.GetPermissions(membership.Role);

        if (!permissions.Contains(_permission))
        {
            context.Result = new ForbidResult();
        }
    }
}
```

**Usage**:

```csharp
group.MapDelete("/tenants/{tenantId}/members/{userId}", RemoveMember)
    .RequirePermission(Permissions.MembersRemove);
```

---

## 14. References

### 14.1 Domain-Driven Design

- **Bounded Contexts**: Vernon, Vaughn. "Implementing Domain-Driven Design"
- **Context Mapping**: [DDD Reference by Eric Evans](http://domainlanguage.com/ddd/reference/)
- **Aggregates**: Martin Fowler, [bliki: DDD_Aggregate](https://martinfowler.com/bliki/DDD_Aggregate.html)

### 14.2 Multi-Tenancy Patterns

- [Multi-Tenant SaaS Database Tenancy Patterns](https://learn.microsoft.com/en-us/azure/azure-sql/database/saas-tenancy-app-design-patterns)
- [AWS Multi-Tenant Best Practices](https://docs.aws.amazon.com/whitepapers/latest/saas-architecture-fundamentals/multi-tenant-best-practices.html)

### 14.3 Orleans

- [Orleans Documentation](https://learn.microsoft.com/en-us/dotnet/orleans/)
- [Orleans Design Patterns](https://github.com/OrleansContrib/DesignPatterns)

---

## Appendix A: Comparison Matrix

| Aspect | Option 1: In Identity | Option 2: In Tenancy | Option 3: Access Control (Recommended) |
|--------|----------------------|---------------------|---------------------------------------|
| **Coupling** | Identity → Tenancy ❌ | Tenancy → Identity ❌ | No coupling ✅ |
| **Complexity** | Low (2 domains) | Low (2 domains) | Medium (3 domains) |
| **Query Performance** | Fast for "my tenants" | Fast for "tenant members" | Fast for both ✅ |
| **Scalability** | User aggregate grows ❌ | Tenant aggregate grows ❌ | Lightweight aggregates ✅ |
| **SRP** | Violated ❌ | Violated ❌ | Maintained ✅ |
| **Authorization** | Mixed with identity ❌ | Mixed with tenancy ❌ | Dedicated domain ✅ |
| **Future Extensibility** | Hard to add teams/groups | Hard to add teams/groups | Easy to extend ✅ |

---

## Appendix B: Key Decisions

| Decision | Rationale | Date |
|----------|-----------|------|
| Introduce Access Control domain | Clean separation, explicit ownership of relationships | 2026-01-07 |
| Use correlation IDs (not references) | Loose coupling between contexts | 2026-01-07 |
| Domain events for communication | Eventual consistency, decoupled | 2026-01-07 |
| One grain per aggregate | Aligns Orleans grains with DDD aggregates | 2026-01-07 |
| Stateless worker grains for queries | Read optimization without state overhead | 2026-01-07 |
| Three separate databases | Physical separation of contexts | 2026-01-07 |

---

**End of Document**

**Next Steps**:
1. Review and approve this architecture
2. Update README files in Identity and Tenancy modules
3. Create AccessControl module projects
4. Begin Phase 1 implementation (Identity Context)
5. Schedule architecture review meetings

**Questions/Feedback**: Contact architecture team or create GitHub issue.
