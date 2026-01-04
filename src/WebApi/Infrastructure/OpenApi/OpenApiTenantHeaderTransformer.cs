using Dilcore.MultiTenant.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Finbuckle.MultiTenant.AspNetCore.Routing;
using Microsoft.OpenApi;

namespace Dilcore.WebApi.Infrastructure.OpenApi;

/// <summary>
/// OpenAPI operation transformer that conditionally adds the x-tenant header parameter
/// only to endpoints that have the RequireMultiTenantAttribute metadata.
/// </summary>
internal sealed class OpenApiTenantHeaderTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Check if the endpoint has the IExcludeFromMultiTenantResolutionMetadata to skip adding the header
        var excludeTenant = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<IExcludeFromMultiTenantResolutionMetadata>()
            .Any();

        if (excludeTenant)
        {
            return Task.CompletedTask;
        }

        // Add the x-tenant header parameter
        operation.Parameters ??= [];

        // Check if parameter already exists to avoid duplicates
        var existingParam = operation.Parameters.FirstOrDefault(p =>
            p.Name == TenantConstants.HeaderName &&
            p.In == ParameterLocation.Header);

        if (existingParam != null)
        {
            return Task.CompletedTask;
        }

        var tenantParameter = new OpenApiParameter
        {
            Name = TenantConstants.HeaderName,
            In = ParameterLocation.Header,
            Required = true, // Implicitly required
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            },
            Description = "Tenant identifier for multi-tenant operations"
        };

        operation.Parameters.Add(tenantParameter);

        return Task.CompletedTask;
    }
}