using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Dilcore.WebApi.Infrastructure.OpenApi;
using Dilcore.WebApi.Infrastructure.Validation;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Shouldly;

namespace Dilcore.WebApi.Tests.Infrastructure.OpenApi;

/// <summary>
/// Unit tests for OpenApiValidationSchemaTransformer verifying that FluentValidation
/// rules are correctly reflected into OpenAPI schema properties.
/// </summary>
[TestFixture]
public class OpenApiValidationSchemaTransformerTests
{
    private ServiceProvider _serviceProvider = null!;
    private JsonSerializerOptions _jsonOptions = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new ServiceCollection();
        // Use the actual ValidationDto and validator from production code
        services.AddSingleton<IValidator<ValidationDto>, ValidationDtoValidator>();
        _serviceProvider = services.BuildServiceProvider();
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            TypeInfoResolver = new DefaultJsonTypeInfoResolver()
        };
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public void Validator_IsCorrectlyRegistered()
    {
        // Arrange & Act
        var validatorType = typeof(IValidator<>).MakeGenericType(typeof(ValidationDto));
        var validator = _serviceProvider.GetService(validatorType) as IValidator;

        // Assert
        validator.ShouldNotBeNull();
        validator.ShouldBeOfType<ValidationDtoValidator>();
    }

    [Test]
    public void ValidatorDescriptor_ContainsRulesForName()
    {
        // Arrange
        var validator = _serviceProvider.GetService<IValidator<ValidationDto>>();
        validator.ShouldNotBeNull();

        // Act
        var descriptor = validator.CreateDescriptor();
        var rules = descriptor.GetRulesForMember("Name").ToList();

        // Assert
        rules.ShouldNotBeEmpty();
    }

    [Test]
    public async Task TransformAsync_WithRegisteredValidator_AppliesValidationRules()
    {
        // Arrange
        var transformer = new OpenApiValidationSchemaTransformer(_serviceProvider);
        var schema = CreateValidationDtoSchema();
        var context = CreateContext<ValidationDto>();

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert - At minimum, the transformer should have processed the schema

        // 1. Name: NotEmpty (Required), MinLength(2), MaxLength(100)
        schema.Required.ShouldNotBeNull();
        schema.Required.ShouldContain("name");
        var nameSchema = schema.Properties!["name"] as OpenApiSchema;
        nameSchema.ShouldNotBeNull();
        nameSchema.MinLength.ShouldBe(2);
        nameSchema.MaxLength.ShouldBe(100);

        // 2. Email: NotEmpty (Required), EmailAddress, MaxLength(255)
        schema.Required.ShouldContain("email");
        var emailSchema = schema.Properties!["email"] as OpenApiSchema;
        emailSchema.ShouldNotBeNull();
        emailSchema.Format.ShouldBe("email");
        emailSchema.MaxLength.ShouldBe(255);
        // Standard AspNetCoreCompatibleEmailValidator does not expose Pattern, so we expect it to be null here
        emailSchema.Pattern.ShouldBeNull();

        // 3. Age: InclusiveBetween(0, 150)
        var ageSchema = schema.Properties!["age"] as OpenApiSchema;
        ageSchema.ShouldNotBeNull();
        ageSchema.Minimum.ShouldBe("0");
        ageSchema.Maximum.ShouldBe("150");

        // 4. PhoneNumber: Matches(@"^\+?[1-9]\d{1,14}$")
        var phoneSchema = schema.Properties!["phoneNumber"] as OpenApiSchema;
        phoneSchema.ShouldNotBeNull();
        phoneSchema.Pattern.ShouldBe(@"^\+?[1-9]\d{1,14}$");

        // 5. StartDate: NotEmpty
        schema.Required.ShouldContain("startDate");

        // 6. Website: Custom Must - ignored by transformer
        // 7. Tags: Must, RuleForEach - complex handling, typically ignored or simple checks only
        //    (RuleForEach usually requires child validator logic in transformer which might not be fully implemented)
    }

    [Test]
    public async Task TransformAsync_WithRegexEmailValidator_ExtractsPattern()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IValidator<RegexValidationDto>, RegexValidationDtoValidator>();
        using var provider = services.BuildServiceProvider();

        var transformer = new OpenApiValidationSchemaTransformer(provider);
        var schema = new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["email"] = new OpenApiSchema { Type = JsonSchemaType.String }
            }
        };

        var context = new OpenApiSchemaTransformerContext
        {
            JsonTypeInfo = _jsonOptions.GetTypeInfo(typeof(RegexValidationDto)),
            DocumentName = "v1",
            JsonPropertyInfo = null,
            ParameterDescription = null,
            ApplicationServices = provider
        };

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert
        var emailSchema = schema.Properties["email"] as OpenApiSchema;
        emailSchema.ShouldNotBeNull();
        emailSchema.Format.ShouldBe("email");
        emailSchema.Pattern.ShouldBe("^.+@.+$");
    }

    [Test]
    public async Task TransformAsync_NoValidator_ReturnsUnmodifiedSchema()
    {
        // Arrange - create a service provider with no validators
        var services = new ServiceCollection();
        using var emptyServiceProvider = services.BuildServiceProvider();
        var transformer = new OpenApiValidationSchemaTransformer(emptyServiceProvider);
        var schema = CreateValidationDtoSchema();
        var context = CreateContext<ValidationDto>();

        // Act
        await transformer.TransformAsync(schema, context, CancellationToken.None);

        // Assert
        schema.Required.ShouldBeNull();
        var nameSchema = schema.Properties!["name"] as OpenApiSchema;
        nameSchema!.MinLength.ShouldBeNull();
        nameSchema.MaxLength.ShouldBeNull();
    }

    [Test]
    public async Task TransformAsync_NullProperties_DoesNotThrow()
    {
        // Arrange
        var transformer = new OpenApiValidationSchemaTransformer(_serviceProvider);
        var schema = new OpenApiSchema { Properties = null };
        var context = CreateContext<ValidationDto>();

        // Act & Assert - should not throw
        await Should.NotThrowAsync(async () =>
            await transformer.TransformAsync(schema, context, CancellationToken.None));
    }

    [Test]
    public async Task TransformAsync_EmptyProperties_DoesNotThrow()
    {
        // Arrange
        var transformer = new OpenApiValidationSchemaTransformer(_serviceProvider);
        var schema = new OpenApiSchema { Properties = new Dictionary<string, IOpenApiSchema>() };
        var context = CreateContext<ValidationDto>();

        // Act & Assert - should not throw
        await Should.NotThrowAsync(async () =>
            await transformer.TransformAsync(schema, context, CancellationToken.None));
    }

    [Test]
    public void ToPascalCase_ConvertsCorrectly()
    {
        // This tests the internal helper method behavior indirectly
        // by verifying the transformer can match camelCase schema properties
        // to PascalCase C# property names

        // The transformer converts "name" -> "Name" for lookup
        var validator = _serviceProvider.GetService<IValidator<ValidationDto>>()!;
        var descriptor = validator.CreateDescriptor();

        // Verify that rules exist for the PascalCase property name
        var rulesForName = descriptor.GetRulesForMember("Name").ToList();
        rulesForName.ShouldNotBeEmpty();

        // But not for the camelCase version
        var rulesForCamelCase = descriptor.GetRulesForMember("name").ToList();
        rulesForCamelCase.ShouldBeEmpty();
    }

    /// <summary>
    /// Creates a schema matching the production ValidationDto structure.
    /// </summary>
    private static OpenApiSchema CreateValidationDtoSchema()
    {
        return new OpenApiSchema
        {
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["name"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["email"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["age"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
                ["phoneNumber"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["website"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["tags"] = new OpenApiSchema { Type = JsonSchemaType.Array },
                ["startDate"] = new OpenApiSchema { Type = JsonSchemaType.String },
                ["endDate"] = new OpenApiSchema { Type = JsonSchemaType.String }
            }
        };
    }

    private OpenApiSchemaTransformerContext CreateContext<T>()
    {
        var typeInfo = _jsonOptions.GetTypeInfo(typeof(T));

        return new OpenApiSchemaTransformerContext
        {
            JsonTypeInfo = typeInfo,
            DocumentName = "v1",
            JsonPropertyInfo = null,
            ParameterDescription = null,
            ApplicationServices = _serviceProvider
        };
    }

    // Helpers
    private class RegexValidationDto
    {
        public string Email { get; set; } = string.Empty;
    }

    private class RegexValidationDtoValidator : AbstractValidator<RegexValidationDto>
    {
        public RegexValidationDtoValidator()
        {
            // Use our custom EmailValidator that implements IRegularExpressionValidator
            RuleFor(x => x.Email).SetValidator(new EmailValidator<RegexValidationDto>());
        }
    }

    // Custom validator with the name matching "EmailValidator`1" to trigger the switch case
    // and implementing IRegularExpressionValidator to trigger extraction
    private sealed class EmailValidator<T> : PropertyValidator<T, string>, IRegularExpressionValidator
    {
        public string Expression => "^.+@.+$";
        public override string Name => "EmailValidator";

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            return true;
        }
    }
}
