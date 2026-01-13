using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dilcore.Extensions.OpenApi;

/// <summary>
///     OpenAPI transformer that adds standard Problem Details responses and schema.
///     Implements both Document and Operation transformers to handle components and operation responses respectively.
/// </summary>
internal sealed class OpenApiProblemDetailsTransformer : IOpenApiDocumentTransformer, IOpenApiOperationTransformer
{
    private const string ProblemDetailsSchemaId = "ProblemDetails";

    // Document transformer runs first
    public async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        // 1. Ensure Components and Schemas are initialized
        document.Components ??= new OpenApiComponents();
        document.Components.Schemas ??= new Dictionary<string, IOpenApiSchema>();

        // 2. Ensure "ProblemDetails" schema is generated and added to Components
        var schema = await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), null, cancellationToken);
        document.Components.Schemas[ProblemDetailsSchemaId] = schema;

        // 3. Customize the schema to match our custom Problem Details (adding traceId, errorCode, timestamp)
        schema.Properties ??= new Dictionary<string, IOpenApiSchema>();

        if (!schema.Properties.ContainsKey(Constants.ProblemDetails.Fields.TraceId))
        {
            schema.Properties.Add(Constants.ProblemDetails.Fields.TraceId, new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "The trace ID for the request."
            });
        }

        if (!schema.Properties.ContainsKey(Constants.ProblemDetails.Fields.ErrorCode))
        {
            schema.Properties.Add(Constants.ProblemDetails.Fields.ErrorCode, new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Description = "A specific application error code."
            });
        }

        if (!schema.Properties.ContainsKey(Constants.ProblemDetails.Fields.Timestamp))
        {
            schema.Properties.Add(Constants.ProblemDetails.Fields.Timestamp, new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "date-time",
                Description = "The timestamp when the problem occurred in ISO 8601 format."
            });
        }
    }

    public async Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        // Ensure Responses is initialized to avoid null-forgiving usage
        operation.Responses ??= new OpenApiResponses();

        // Retrieve the reference schema again to use in responses (or just get the reference object)
        var problemDetailsSchema = await context.GetOrCreateSchemaAsync(typeof(ProblemDetails), null, cancellationToken);

        // 2. Add responses to Operations (Operation scope)
        // Add "default" error response
        if (!operation.Responses.ContainsKey("default"))
        {
            operation.Responses.Add("default", CreateProblemDetailsResponse("Unexpected error occurred.", problemDetailsSchema));
        }

        // Check for AllowAnonymous attribute
        var isAnonymous = context.Description.ActionDescriptor.EndpointMetadata
            .OfType<AllowAnonymousAttribute>()
            .Any();

        if (!isAnonymous)
        {
            if (!operation.Responses.ContainsKey("401"))
            {
                operation.Responses.Add("401", CreateProblemDetailsResponse("Unauthorized - Authentication is required.", problemDetailsSchema));
            }

            if (!operation.Responses.ContainsKey("403"))
            {
                operation.Responses.Add("403", CreateProblemDetailsResponse("Forbidden - User does not have necessary permissions.", problemDetailsSchema));
            }
        }
    }

    private static OpenApiResponse CreateProblemDetailsResponse(string description, IOpenApiSchema schema)
    {
        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [Constants.ProblemDetails.ContentType] = new()
                {
                    Schema = (OpenApiSchema)schema
                }
            }
        };
    }
}