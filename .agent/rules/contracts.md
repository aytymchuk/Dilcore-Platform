---
trigger: glob
globs: **/*.Contracts, **/*.Contracts.Tests
---

# Contracts Project Standards

Projects ending in `.Contracts` define the API contracts for a specific domain. To ensure consistency and interoperability, all contracts must adhere to the following standards.

## Core Principles

- **POCO Only**: Contracts must contain only Plain Old CLR Objects (POCOs).
- **Not Sealed**: Classes must NOT be marked as `sealed`.
- **Suffix**: All contract classes must end with the `Dto` suffix (e.g., `CreateTenantDto`).
- **No Logic**: Contracts MUST NOT contain any logic, including methods, computed properties, or validation logic innerhalb der Klasse.
- **API Specific**: These classes describe only the API contracts of the appropriate domain.

## Project Structure

Contracts projects must follow a feature-based structure:

- **Feature**
  - **Operation**
    - **DTO Class**: The data transfer object itself.
    - **Validator** (Optional): FluentValidation validator if the class is a request body requiring validation.

Example:

```text
- Tenants
  - Create
    - CreateTenantDto.cs
    - CreateTenantDtoValidator.cs
```

## Implementation Standards

### Properties
- Every property must have a **public getter** and a **public setter**.
- `init` accessors are strictly prohibited.

### Records
- Record types should be avoided if they lead to `sealed` classes (default for records) or if they don't support the required property structure. Normal classes are preferred to ensure compliance with "Not Sealed" and "Public Getter/Setter" rules.

## Testing

Each `*.Contracts` project must have a corresponding `*.Contracts.Tests` project.
- Implementation of validation tests is mandatory for any DTO with a validator.
- Tests should follow the patterns established in `tests/Identity/Identity.Contracts.Tests`.

## Examples

### Correct DTO
```csharp
namespace Dilcore.Tenancy.Contracts.Tenants.Create;

public class CreateTenantDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}
```

### Correct Validator
```csharp
using FluentValidation;

namespace Dilcore.Tenancy.Contracts.Tenants.Create;

public class CreateTenantDtoValidator : AbstractValidator<CreateTenantDto>
{
    public CreateTenantDtoValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}
```
