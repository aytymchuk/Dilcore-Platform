using Dilcore.Blueprints.Domain;
using Dilcore.Results.Abstractions;
using FluentAssertions;

namespace Dilcore.Blueprints.Core.Tests.Features.EntityDefinitions;

public class SchemaNameResolverTests
{
    [Test]
    public void SafeResolve_WhenDisplayNameHasNoAlphanumeric_ShouldReturnValidationError()
    {
        var result = SchemaNameGenerator.SafeResolve(null, "!!!");

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>();
        result.Errors[0].Message.Should().Contain("alphanumeric");
    }

    [Test]
    public void SafeResolve_WhenNameHasNoAlphanumeric_ShouldReturnValidationError()
    {
        var result = SchemaNameGenerator.SafeResolve("---", "Fallback");

        result.IsFailed.Should().BeTrue();
        result.Errors.Should().ContainSingle()
            .Which.Should().BeOfType<ValidationError>();
    }

    [Test]
    public void SafeResolve_WhenInputsAreValid_ShouldReturnResolvedSchemaName()
    {
        var result = SchemaNameGenerator.SafeResolve(null, "Customer Order");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("customerOrder");
    }
}
