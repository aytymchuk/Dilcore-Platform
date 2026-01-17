using Dilcore.CorrelationId.Abstractions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dilcore.CorrelationId.Extensions.OpenApi;

/// <summary>
/// OpenAPI operation transformer that adds the x-correlation-id header parameter
/// to all endpoints for request tracking and distributed tracing.
/// </summary>
public sealed class OpenApiCorrelationIdHeaderTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Parameters ??= [];

        // Check if parameter already exists to avoid duplicates
        var existingParam = operation.Parameters.FirstOrDefault(p =>
            p.Name == CorrelationIdConstants.HeaderName &&
            p.In == ParameterLocation.Header);

        if (existingParam != null)
        {
            return Task.CompletedTask;
        }

        var correlationIdParameter = new OpenApiParameter
        {
            Name = CorrelationIdConstants.HeaderName,
            In = ParameterLocation.Header,
            Required = false, // Optional - will be generated if not provided
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uuid"
            },
            Description = "Correlation identifier for request tracking and distributed tracing. If not provided, a new GUID will be generated."
        };

        operation.Parameters.Add(correlationIdParameter);

        return Task.CompletedTask;
    }
}
