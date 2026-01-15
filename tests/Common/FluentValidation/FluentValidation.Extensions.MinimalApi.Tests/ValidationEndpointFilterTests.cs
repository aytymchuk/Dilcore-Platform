using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;

namespace Dilcore.FluentValidation.Extensions.MinimalApi.Tests;

[TestFixture]
public class ValidationEndpointFilterTests
{
    private Mock<IServiceProvider> _serviceProviderMock;
    private Mock<IValidator<TestDto>> _validatorMock;
    private Mock<ILogger<ValidationEndpointFilter<TestDto>>> _loggerMock;
    private DefaultHttpContext _httpContext;
    private EndpointFilterInvocationContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _serviceProviderMock = new Mock<IServiceProvider>();
        _validatorMock = new Mock<IValidator<TestDto>>();
        _loggerMock = new Mock<ILogger<ValidationEndpointFilter<TestDto>>>();
        _httpContext = new DefaultHttpContext { RequestServices = _serviceProviderMock.Object };

        // Mock logging
        _serviceProviderMock.Setup(x => x.GetService(typeof(ILogger<ValidationEndpointFilter<TestDto>>)))
            .Returns(_loggerMock.Object);
    }

    [Test]
    public async Task InvokeAsync_NoValidatorRegistered_InvokesNext()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IValidator<TestDto>))).Returns((IValidator<TestDto>?)null);
        var filter = new ValidationEndpointFilter<TestDto>();
        _context = CreateContext(new TestDto());
        bool nextInvoked = false;

        // Act
        await filter.InvokeAsync(_context, (ctx) =>
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(Microsoft.AspNetCore.Http.Results.Ok());
        });

        // Assert
        nextInvoked.ShouldBeTrue();
    }

    [Test]
    public async Task InvokeAsync_ArgumentNotFound_InvokesNext()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IValidator<TestDto>))).Returns(_validatorMock.Object);
        var filter = new ValidationEndpointFilter<TestDto>();
        _context = CreateContext("something else"); // No TestDto here
        bool nextInvoked = false;

        // Act
        await filter.InvokeAsync(_context, (ctx) =>
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(Microsoft.AspNetCore.Http.Results.Ok());
        });

        // Assert
        nextInvoked.ShouldBeTrue();
    }

    [Test]
    public async Task InvokeAsync_ValidationPasses_InvokesNext()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IValidator<TestDto>))).Returns(_validatorMock.Object);
        var validDto = new TestDto { Name = "Valid" };
        _validatorMock.Setup(v => v.ValidateAsync(validDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult()); // Valid result

        var filter = new ValidationEndpointFilter<TestDto>();
        _context = CreateContext(validDto);
        bool nextInvoked = false;

        // Act
        await filter.InvokeAsync(_context, (ctx) =>
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(Microsoft.AspNetCore.Http.Results.Ok());
        });

        // Assert
        nextInvoked.ShouldBeTrue();
    }

    [Test]
    public async Task InvokeAsync_ValidationFails_ReturnsValidationProblem()
    {
        // Arrange
        _serviceProviderMock.Setup(x => x.GetService(typeof(IValidator<TestDto>))).Returns(_validatorMock.Object);
        var invalidDto = new TestDto { Name = "" };
        var validationResult = new ValidationResult(new[] { new ValidationFailure("Name", "Name is required") });

        _validatorMock.Setup(v => v.ValidateAsync(It.IsAny<TestDto>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var filter = new ValidationEndpointFilter<TestDto>();
        _context = CreateContext(invalidDto);
        bool nextInvoked = false;

        // Act
        var result = await filter.InvokeAsync(_context, (ctx) =>
        {
            nextInvoked = true;
            return ValueTask.FromResult<object?>(Microsoft.AspNetCore.Http.Results.Ok());
        });

        // Assert
        nextInvoked.ShouldBeFalse();
        result.ShouldBeOfType<ValidationProblem>();
        var problem = (ValidationProblem)result!;
        problem.ProblemDetails.Errors.ShouldContainKey("name");
        problem.ProblemDetails.Errors["name"].ShouldContain("Name is required");
    }

    private EndpointFilterInvocationContext CreateContext(object argument)
    {
        return EndpointFilterInvocationContext.Create(_httpContext, argument);
    }

    public class TestDto
    {
        public string Name { get; set; } = string.Empty;
    }
}
