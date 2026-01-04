using Dilcore.WebApi.Extensions;
using Microsoft.AspNetCore.OpenApi;
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
        // Check if the endpoint has the RequireMultiTenant metadata
        var requiresTenant = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<RequireMultiTenantAttribute>()
            .Any();

        if (!requiresTenant)
        {
            return Task.CompletedTask;
        }

        // Add the x-tenant header parameter
        operation.Parameters ??= [];

        // Check if parameter already exists to avoid duplicates
        var existingParam = operation.Parameters.FirstOrDefault(p =>
            p.Name == Constants.Headers.Tenant &&
            p.In == ParameterLocation.Header);

        if (existingParam != null)
        {
            return Task.CompletedTask;
        }

        var tenantParameter = new OpenApiParameter
        {
            Name = Constants.Headers.Tenant,
            In = ParameterLocation.Header,
            Required = true,
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