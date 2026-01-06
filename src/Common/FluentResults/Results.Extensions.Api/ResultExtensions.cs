using FluentResults;
using Microsoft.AspNetCore.Http;
using System.Net;

namespace Dilcore.FluentResults.Extensions.Api;

public static class ResultExtensions
{
    public static IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return result.ToProblemDetails();
    }

    public static IResult ToMinimalApiResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return result.ToProblemDetails();
    }

    private static IResult ToProblemDetails(this ResultBase result)
    {
        if (result.Errors.Count == 0)
        {
            return Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
        }

        var error = result.Errors.First(); // Use primary error for main status

        // Default to 400 Bad Request
        var statusCode = StatusCodes.Status400BadRequest;
        var title = "Bad Request";
        var type = error is AppError appError ? appError.Type : ErrorType.Failure;
        var code = error is AppError appErrorCode ? appErrorCode.Code : ProblemDetailsConstants.UnexpectedError;

        switch (type)
        {
            case ErrorType.Validation:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Validation Error";
                break;
            case ErrorType.NotFound:
                statusCode = StatusCodes.Status404NotFound;
                title = "Resource Not Found";
                break;
            case ErrorType.Conflict:
                statusCode = StatusCodes.Status409Conflict;
                title = "Conflict";
                break;
            case ErrorType.Unauthorized:
                statusCode = StatusCodes.Status401Unauthorized;
                title = "Unauthorized";
                break;
            case ErrorType.Forbidden:
                statusCode = StatusCodes.Status403Forbidden;
                title = "Forbidden";
                break;
            case ErrorType.Unexpected:
                statusCode = StatusCodes.Status500InternalServerError;
                title = "Internal Server Error";
                break;
            case ErrorType.Failure:
            default:
                statusCode = StatusCodes.Status400BadRequest;
                title = "Bad Request";
                break;
        }

        var extensions = new Dictionary<string, object?>
        {
            { ProblemDetailsFields.ErrorCode, code },
            { ProblemDetailsFields.Errors, result.Errors.Select(e => new { e.Message, Metadata = e.Metadata }).ToArray() }
        };

        return Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Message,
            type: BuildProblemTypeUri(code),
            extensions: extensions
        );
    }

    private static string BuildProblemTypeUri(string errorCode)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            return ProblemDetailsConstants.TypeBaseUri;
        }

        return $"{ProblemDetailsConstants.TypeBaseUri}/{errorCode.ToLowerInvariant().Replace('_', '-')}";
    }
}
