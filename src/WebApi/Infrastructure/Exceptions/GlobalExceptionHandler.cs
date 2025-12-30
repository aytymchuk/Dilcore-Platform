using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Dilcore.WebApi.Infrastructure.Exceptions;

/// <summary>
/// Global exception handler that converts unhandled exceptions to Problem Details responses.
/// Implements IExceptionHandler for centralized exception processing.
/// </summary>
public sealed partial class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorCode, title) = MapException(exception);

        LogExceptionOccurred(exception, errorCode, exception.Message);

        httpContext.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = GetExceptionDetail(exception),
            Type = $"{Constants.ProblemDetails.TypeBaseUri}/{errorCode.ToLowerInvariant().Replace('_', '-')}"
        };

        // Add error code extension
        problemDetails.Extensions[Constants.ProblemDetails.Fields.ErrorCode] = errorCode;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    /// <summary>
    /// Maps an exception to its corresponding HTTP status code, error code, and title.
    /// </summary>
    private static (HttpStatusCode StatusCode, string ErrorCode, string Title) MapException(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException =>
                (HttpStatusCode.BadRequest, Constants.ProblemDetails.ValidationError, "Invalid Request"),
            ArgumentException =>
                (HttpStatusCode.BadRequest, Constants.ProblemDetails.ValidationError, "Validation Error"),
            KeyNotFoundException =>
                (HttpStatusCode.NotFound, Constants.ProblemDetails.NotFound, "Resource Not Found"),
            UnauthorizedAccessException =>
                (HttpStatusCode.Unauthorized, Constants.ProblemDetails.Unauthorized, "Unauthorized"),
            InvalidOperationException =>
                (HttpStatusCode.InternalServerError, Constants.ProblemDetails.UnexpectedError, "Internal Server Error"),
            NotSupportedException =>
                (HttpStatusCode.NotImplemented, Constants.ProblemDetails.NotImplemented, "Not Implemented"),
            NotImplementedException =>
                (HttpStatusCode.NotImplemented, Constants.ProblemDetails.NotImplemented, "Not Implemented"),
            OperationCanceledException =>
                (HttpStatusCode.BadRequest, Constants.ProblemDetails.OperationCancelled, "Operation Cancelled"),
            TimeoutException =>
                (HttpStatusCode.RequestTimeout, Constants.ProblemDetails.Timeout, "Request Timeout"),
            _ =>
                (HttpStatusCode.InternalServerError, Constants.ProblemDetails.UnexpectedError, "An unexpected error occurred")
        };
    }

    /// <summary>
    /// Gets the exception detail message, sanitizing sensitive information in production.
    /// </summary>
    private string GetExceptionDetail(Exception exception)
    {
        // In development, show full exception message
        if (environment.IsDevelopment())
        {
            return exception.Message;
        }

        // In production, show generic messages for security
        return exception switch
        {
            ArgumentException or ArgumentNullException => "Invalid request parameters.",
            KeyNotFoundException => "The requested resource was not found.",
            UnauthorizedAccessException => "You are not authorized to access this resource.",
            InvalidOperationException => "The requested operation cannot be performed.",
            NotSupportedException or NotImplementedException => "This operation is not supported.",
            OperationCanceledException => "The operation was cancelled.",
            TimeoutException => "The operation timed out.",
            _ => "An unexpected error occurred. Please try again later."
        };
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Exception occurred: {ErrorCode} - {ExceptionMessage}")]
    private partial void LogExceptionOccurred(Exception exception, string errorCode, string exceptionMessage);
}