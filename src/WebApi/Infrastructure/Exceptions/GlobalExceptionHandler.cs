using System.Net;
using System.Text.Json;
using Dilcore.Authentication.Abstractions.Exceptions;
using Dilcore.MultiTenant.Abstractions.Exceptions;
using Dilcore.Results.Abstractions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Dilcore.Extensions.OpenApi.Abstractions;

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
            Type = ProblemDetailsHelper.BuildTypeUri(errorCode)
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
            ArgumentNullException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.ValidationError, "Validation Error"),
            ArgumentException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.ValidationError, "Validation Error"),
            BadHttpRequestException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.InvalidRequest, "Invalid Request"),
            JsonException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.JsonParseError, "Invalid JSON Format"),
            FormatException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.FormatError, "Invalid Format"),
            KeyNotFoundException _ =>
                (HttpStatusCode.NotFound, ProblemDetailsConstants.NotFound, "Resource Not Found"),
            UnauthorizedAccessException _ =>
                (HttpStatusCode.Unauthorized, ProblemDetailsConstants.Unauthorized, "Unauthorized"),
            InvalidOperationException _ =>
                (HttpStatusCode.InternalServerError, ProblemDetailsConstants.UnexpectedError, "Internal Server Error"),
            NotSupportedException _ =>
                (HttpStatusCode.NotImplemented, ProblemDetailsConstants.NotImplemented, "Not Implemented"),
            NotImplementedException _ =>
                (HttpStatusCode.NotImplemented, ProblemDetailsConstants.NotImplemented, "Not Implemented"),
            OperationCanceledException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.OperationCancelled, "Operation Cancelled"),
            TimeoutException _ =>
                (HttpStatusCode.RequestTimeout, ProblemDetailsConstants.Timeout, "Request Timeout"),
            TenantNotResolvedException _ =>
                (HttpStatusCode.BadRequest, ProblemDetailsConstants.TenantNotResolved, "Tenant Not Resolved"),
            UserNotResolvedException _ =>
                (HttpStatusCode.Unauthorized, ProblemDetailsConstants.UserNotResolved, "User Not Resolved"),
            _ =>
                (HttpStatusCode.InternalServerError, ProblemDetailsConstants.UnexpectedError, "An unexpected error occurred")
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
            var message = exception.Message;
            if ((exception is BadHttpRequestException or JsonException or FormatException)
                && exception.InnerException is not null)
            {
                message += $" Details: {exception.InnerException.Message}";
            }
            return message;
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