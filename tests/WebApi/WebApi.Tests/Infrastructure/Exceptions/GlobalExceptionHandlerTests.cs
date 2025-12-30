using System.Reflection;
using Dilcore.WebApi;
using Dilcore.WebApi.Infrastructure.Exceptions;
using Shouldly;

namespace Dilcore.WebApi.Tests.Infrastructure.Exceptions;

[TestFixture]
public class GlobalExceptionHandlerTests
{
    private MethodInfo _buildProblemTypeUriMethod = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var methodInfo = typeof(GlobalExceptionHandler)
            .GetMethod("BuildProblemTypeUri", BindingFlags.NonPublic | BindingFlags.Static);

        methodInfo.ShouldNotBeNull("BuildProblemTypeUri method should be found via reflection");

        _buildProblemTypeUriMethod = methodInfo;
    }

    [Test]
    public void BuildProblemTypeUri_ValidErrorCode_ReturnsFormattedUri()
    {
        // Arrange
        var errorCode = "SOME_ERROR_CODE";
        var expectedUri = $"{Constants.ProblemDetails.TypeBaseUri}/some-error-code";

        // Act
        var result = _buildProblemTypeUriMethod.Invoke(null, [errorCode]) as string;

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildProblemTypeUri_MixedCaseWithUnderscores_ReturnsKebabCase()
    {
        // Arrange
        var errorCode = "Mixed_Case_Underscore_Code";
        var expectedUri = $"{Constants.ProblemDetails.TypeBaseUri}/mixed-case-underscore-code";

        // Act
        var result = _buildProblemTypeUriMethod.Invoke(null, [errorCode]) as string;

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildProblemTypeUri_NullErrorCode_ReturnsBaseUri()
    {
        // Arrange
        string? errorCode = null;
        var expectedUri = Constants.ProblemDetails.TypeBaseUri;

        // Act
        var result = _buildProblemTypeUriMethod.Invoke(null, [errorCode]) as string;

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildProblemTypeUri_EmptyErrorCode_ReturnsBaseUri()
    {
        // Arrange
        var errorCode = "";
        var expectedUri = Constants.ProblemDetails.TypeBaseUri;

        // Act
        var result = _buildProblemTypeUriMethod.Invoke(null, [errorCode]) as string;

        // Assert
        result.ShouldBe(expectedUri);
    }

    [Test]
    public void BuildProblemTypeUri_WhitespaceErrorCode_ReturnsBaseUri()
    {
        // Arrange
        var errorCode = "   ";
        var expectedUri = Constants.ProblemDetails.TypeBaseUri;

        // Act
        var result = _buildProblemTypeUriMethod.Invoke(null, [errorCode]) as string;

        // Assert
        result.ShouldBe(expectedUri);
    }
}
