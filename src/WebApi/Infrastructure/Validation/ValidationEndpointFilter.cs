using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dilcore.WebApi.Infrastructure.Validation;

/// <summary>
/// Endpoint filter that automatically validates request parameters using FluentValidation.
/// Returns Problem Details response with validation errors on failure.
/// </summary>
public sealed class ValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is null)
        {
            return await next(context);
        }

        // Find the parameter of type T in the endpoint arguments
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
        {
            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(argument);

        if (validationResult.IsValid)
        {
            return await next(context);
        }

        return CreateValidationProblem(validationResult);
    }

    private static ValidationProblem CreateValidationProblem(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => ToCamelCase(g.Key),
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return TypedResults.ValidationProblem(
            errors,
            title: Constants.ProblemDetails.DataValidationTitle,
            detail: Constants.ProblemDetails.DataValidationDetail,
            type: $"{Constants.ProblemDetails.TypeBaseUri}/data-validation-failed",
            extensions: new Dictionary<string, object?>
            {
                [Constants.ProblemDetails.Fields.ErrorCode] = Constants.ProblemDetails.DataValidationFailed
            }
        );
    }

    /// <summary>
    /// Converts property name to camelCase for consistent JSON output.
    /// </summary>
    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        if (propertyName.Length == 1)
            return propertyName.ToLowerInvariant();

        return char.ToLowerInvariant(propertyName[0]) + propertyName[1..];
    }
}