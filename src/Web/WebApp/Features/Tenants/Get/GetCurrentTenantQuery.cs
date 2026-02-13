using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Tenants;

namespace Dilcore.WebApp.Features.Tenants.Get;

/// <summary>
/// Query to get the current tenant based on the context.
/// </summary>
public record GetCurrentTenantQuery() : IQuery<Tenant>;
