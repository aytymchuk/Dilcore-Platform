namespace Dilcore.MultiTenant.Abstractions;

/// <summary>
/// Exception thrown when a tenant context cannot be resolved but is required.
/// </summary>
public sealed class TenantNotResolvedException : Exception
{
    public TenantNotResolvedException(string message) : base(message)
    {
    }

    public TenantNotResolvedException() : base("Tenant could not be resolved.")
    {
    }

    public TenantNotResolvedException(string message, Exception innerException) : base(message, innerException)
    {
    }
}