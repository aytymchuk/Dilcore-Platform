using Dilcore.Results.Abstractions;
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
        // Check extensions has errors list with correct messages
        var errors = problem.ProblemDetails.Extensions["errors"];
        errors.ShouldNotBeNull();

        // Serialize to JSON and deserialize to access properties
        var json = System.Text.Json.JsonSerializer.Serialize(errors);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var errorArray = doc.RootElement.EnumerateArray().ToList();

        errorArray.Count.ShouldBe(2);

        // Validate first error (ValidationError)
        errorArray[0].GetProperty("Message").GetString().ShouldBe("Field 1 invalid");

        // Validate second error (NotFoundError)
        errorArray[1].GetProperty("Message").GetString().ShouldBe("Related resource not found");
    }
}