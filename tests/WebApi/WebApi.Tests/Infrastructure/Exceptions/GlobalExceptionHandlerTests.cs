using Dilcore.Results.Abstractions;
using Shouldly;

namespace Dilcore.WebApi.Tests.Infrastructure.Exceptions;

/// <summary>
/// Tests for BuildTypeUri method in ProblemDetailsHelper.
/// </summary>
[TestFixture]
public class GlobalExceptionHandlerTests
{
    [Test]
    public void BuildTypeUri_ValidErrorCode_ReturnsFormattedUri()
    {
        // Arrange
        var errorCode = "SOME_ERROR_CODE";
        var expectedUri = $"{ProblemDetailsConstants.TypeBaseUri}/some-error-code";

        // Act
        var result = ProblemDetailsHelper.BuildTypeUri(errorCode);

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildTypeUri_MixedCaseWithUnderscores_ReturnsKebabCase()
    {
        // Arrange
        var errorCode = "Mixed_Case_Underscore_Code";
        var expectedUri = $"{ProblemDetailsConstants.TypeBaseUri}/mixed-case-underscore-code";

        // Act
        var result = ProblemDetailsHelper.BuildTypeUri(errorCode);

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildTypeUri_NullErrorCode_ReturnsBaseUri()
    {
        // Arrange
        string? errorCode = null;
        var expectedUri = ProblemDetailsConstants.TypeBaseUri;

        // Act
        var result = ProblemDetailsHelper.BuildTypeUri(errorCode!);

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildTypeUri_EmptyErrorCode_ReturnsBaseUri()
    {
        // Arrange
        var errorCode = "";
        var expectedUri = ProblemDetailsConstants.TypeBaseUri;

        // Act
        var result = ProblemDetailsHelper.BuildTypeUri(errorCode);

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildTypeUri_WhitespaceErrorCode_ReturnsBaseUri()
    {
        // Arrange
        var errorCode = "   ";
        var expectedUri = ProblemDetailsConstants.TypeBaseUri;

        // Act
        var result = ProblemDetailsHelper.BuildTypeUri(errorCode);

        // Assert
        result.ShouldBe(expectedUri);
    }
}