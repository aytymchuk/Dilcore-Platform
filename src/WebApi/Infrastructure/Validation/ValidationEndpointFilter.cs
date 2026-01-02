using Dilcore.WebApi.Extensions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dilcore.WebApi.Infrastructure.Validation;

/// <summary>
/// Endpoint filter that automatically validates request parameters using FluentValidation.
/// Returns Problem Details response with validation errors on failure.
/// </summary>
public sealed partial class ValidationEndpointFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var services = context.HttpContext.RequestServices;
        var validator = services.GetService<IValidator<T>>();

        if (validator is null)
        {
            var logger = services.GetService<ILogger<ValidationEndpointFilter<T>>>();

            if (logger is not null)
            {
                LogValidationSkippedNoValidatorRegisteredForTypeTypeNameRequestMethodPath(logger, typeof(T).Name,
                    context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            }

            return await next(context);
        }

        // Find the parameter of type T in the endpoint arguments
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
        {
            var logger = services.GetService<ILogger<ValidationEndpointFilter<T>>>();

            if (logger is not null)
            {
                LogValidationSkippedArgumentOfTypeTypeNameNotFoundRequestMethodPath(logger, typeof(T).Name,
                    context.HttpContext.Request.Method, context.HttpContext.Request.Path);
            }

            return await next(context);
        }

        var validationResult = await validator.ValidateAsync(argument);

        if (validationResult.IsValid)
        {
            return await next(context);
        }

        var loggerFail = services.GetService<ILogger<ValidationEndpointFilter<T>>>();

        if (loggerFail is not null)
        {
            LogValidationFailedForTypeTypenameRequestMethodPathErrorsErrors(loggerFail, typeof(T).Name,
                context.HttpContext.Request.Method, context.HttpContext.Request.Path,
                FormatValidationErrors(validationResult));
        }

        return CreateValidationProblem(validationResult);
    }

    private static string FormatValidationErrors(ValidationResult validationResult)
    {
        return string.Join("; ", validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .Select(g => $"{g.Key}: {string.Join(", ", g.Select(e => e.ErrorMessage))}"));
    }

    private static ValidationProblem CreateValidationProblem(ValidationResult validationResult)
    {
        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key.ToCamelCase(),
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

    [LoggerMessage(LogLevel.Error, "Validation failed for type {typeName}. Request: {method} {path}. Errors: {errors}")]
    static partial void LogValidationFailedForTypeTypenameRequestMethodPathErrorsErrors(ILogger<ValidationEndpointFilter<T>> logger, string typeName, string method, PathString path, string errors);

    [LoggerMessage(LogLevel.Debug, "Validation skipped: Argument of type {typeName} not found. Request: {method} {path}")]
    static partial void LogValidationSkippedArgumentOfTypeTypeNameNotFoundRequestMethodPath(ILogger<ValidationEndpointFilter<T>> logger, string typeName, string method, PathString path);

    [LoggerMessage(LogLevel.Debug, "Validation skipped: No validator registered for type {typeName}. Request: {method} {path}")]
    static partial void LogValidationSkippedNoValidatorRegisteredForTypeTypeNameRequestMethodPath(ILogger<ValidationEndpointFilter<T>> logger, string typeName, string method, PathString path);
}