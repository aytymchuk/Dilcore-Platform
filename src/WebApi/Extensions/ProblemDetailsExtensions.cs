using System.Diagnostics;
using Dilcore.Results.Abstractions;

namespace Dilcore.WebApi.Extensions;

/// <summary>
/// Extension methods for configuring Problem Details support.
/// </summary>
public static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds Problem Details services with custom configuration for RFC 9457 compliance.
    /// </summary>
    public static IServiceCollection AddProblemDetailsServices(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                // Add trace ID for correlation
                context.ProblemDetails.Extensions[Constants.ProblemDetails.Fields.TraceId] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                // Add timestamp for debugging
                context.ProblemDetails.Extensions[Constants.ProblemDetails.Fields.Timestamp] = DateTimeOffset.UtcNow;

                // Set instance to the request path
                context.ProblemDetails.Instance ??=
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                // Set type URI based on status code if not already set
                if (string.IsNullOrEmpty(context.ProblemDetails.Type))
                {
                    context.ProblemDetails.Type = context.ProblemDetails.Status switch
                    {
                        StatusCodes.Status400BadRequest => $"{ProblemDetailsConstants.TypeBaseUri}/bad-request",
                        StatusCodes.Status401Unauthorized => $"{ProblemDetailsConstants.TypeBaseUri}/unauthorized",
                        StatusCodes.Status403Forbidden => $"{ProblemDetailsConstants.TypeBaseUri}/forbidden",
                        StatusCodes.Status404NotFound => $"{ProblemDetailsConstants.TypeBaseUri}/not-found",
                        StatusCodes.Status409Conflict => $"{ProblemDetailsConstants.TypeBaseUri}/conflict",
                        StatusCodes.Status422UnprocessableEntity => $"{ProblemDetailsConstants.TypeBaseUri}/validation-error",
                        StatusCodes.Status500InternalServerError => $"{ProblemDetailsConstants.TypeBaseUri}/internal-error",
                        StatusCodes.Status501NotImplemented => $"{ProblemDetailsConstants.TypeBaseUri}/not-implemented",
                        _ => $"{ProblemDetailsConstants.TypeBaseUri}/error"
                    };
                }

                // Ensure error code is present for 400 Bad Request (e.g. binding errors)
                if (context.ProblemDetails.Status == StatusCodes.Status400BadRequest &&
                    !context.ProblemDetails.Extensions.ContainsKey(Constants.ProblemDetails.Fields.ErrorCode))
                {
                    context.ProblemDetails.Extensions[Constants.ProblemDetails.Fields.ErrorCode] =
                        ProblemDetailsConstants.InvalidRequest;

                    // Maintain consistency with GlobalExceptionHandler
                    if (context.ProblemDetails.Title == "Bad Request")
                    {
                        context.ProblemDetails.Title = "Invalid Request";
                    }
                }
            };
        });

        return services;
    }
}