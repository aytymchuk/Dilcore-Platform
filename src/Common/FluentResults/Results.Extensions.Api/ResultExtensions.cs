using Dilcore.Results.Abstractions;
using FluentResults;
using Microsoft.AspNetCore.Http;

namespace Dilcore.Results.Extensions.Api;

public static class ResultExtensions
{
    public static IResult ToMinimalApiResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok(result.Value);
        }

        return result.ToProblemDetails();
    }

    public static IResult ToMinimalApiResult<T>(this Result<T> result, Func<T, IResult> onSuccess)
    {
        if (result.IsSuccess)
        {
            return onSuccess(result.Value);
        }

        return result.ToProblemDetails();
    }

    public static IResult ToMinimalApiResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return Microsoft.AspNetCore.Http.Results.Ok();
        }

        return result.ToProblemDetails();
    }

    private static IResult ToProblemDetails(this ResultBase result)
    {
        if (result.Errors.Count == 0)
        {
            return Microsoft.AspNetCore.Http.Results.Problem(statusCode: StatusCodes.Status500InternalServerError, title: "An unexpected error occurred.");
        }

        var error = result.Errors.First(); // Use primary error for main status

        // Default to 400 Bad Request
        var statusCode = StatusCodes.Status400BadRequest;
        var title = "Bad Request";
        var (type, code) = error is AppError appError
            ? (appError.Type, appError.Code)
            : (ErrorType.Failure, ProblemDetailsConstants.UnexpectedError);

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
            { ProblemDetailsFields.Errors, result.Errors.Select(e => new { e.Message }).ToArray() }
        };

        return Microsoft.AspNetCore.Http.Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Message,
            type: ProblemDetailsHelper.BuildTypeUri(code),
            extensions: extensions
        );
    }
}