namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Marker attribute to indicate that an endpoint requires multi-tenancy.
/// Used by OpenAPI transformer to conditionally add the x-tenant header parameter.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public sealed class RequireMultiTenantAttribute : Attribute
{
}