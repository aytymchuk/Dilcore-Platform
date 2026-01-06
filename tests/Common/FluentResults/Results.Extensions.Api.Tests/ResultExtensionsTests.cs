using Dilcore.FluentResults;
using Dilcore.FluentResults.Extensions.Api;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Shouldly;

namespace Dilcore.Results.Extensions.Api.Tests;

public class ResultExtensionsTests
{
    [Test]
    public void ToMinimalApiResult_Success_ReturnsOk()
    {
        // Arrange
        var result = Result.Ok("Success");

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        apiResult.ShouldBeOfType<Ok<string>>();
        ((Ok<string>)apiResult).Value.ShouldBe("Success");
    }

    [Test]
    public void ToMinimalApiResult_VoidSuccess_ReturnsOk()
    {
        // Arrange
        var result = Result.Ok();

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        apiResult.ShouldBeOfType<Ok>();
    }

    [Test]
    public void ToMinimalApiResult_ValidationError_ReturnsBadRequest()
    {
        // Arrange
        var result = Result.Fail(new ValidationError("Invalid input"));

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problem = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Title.ShouldBe("Validation Error");
        problem.ProblemDetails.Detail.ShouldBe("Invalid input");
        problem.ProblemDetails.Extensions["errorCode"].ShouldBe("VALIDATION_ERROR");
    }

    [Test]
    public void ToMinimalApiResult_NotFoundError_ReturnsNotFound()
    {
        // Arrange
        var result = Result.Fail(new NotFoundError("User not found"));

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problem = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status404NotFound);
        problem.ProblemDetails.Title.ShouldBe("Resource Not Found");
        problem.ProblemDetails.Extensions["errorCode"].ShouldBe("NOT_FOUND");
    }

    [Test]
    public void ToMinimalApiResult_MultipleErrors_ReturnsFirstErrorStatus()
    {
        // Arrange
        var result = Result.Fail(new Error[]
        {
            new ValidationError("Field 1 invalid"),
            new NotFoundError("Related resource not found")
        });

        // Act
        var apiResult = result.ToMinimalApiResult();

        // Assert
        var problem = apiResult.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(StatusCodes.Status400BadRequest);
        problem.ProblemDetails.Title.ShouldBe("Validation Error");
        // Check extensions has errors list
        var errors = problem.ProblemDetails.Extensions["errors"] as IEnumerable<object>;
        errors.ShouldNotBeNull();
        errors.Count().ShouldBe(2);
    }
}