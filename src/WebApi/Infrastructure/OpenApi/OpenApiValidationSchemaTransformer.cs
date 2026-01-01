using System.Reflection;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Dilcore.WebApi.Infrastructure.OpenApi;

/// <summary>
/// OpenAPI schema filter that reflects FluentValidation rules into OpenAPI schema properties.
/// Adds required, minLength, maxLength, pattern, minimum, maximum constraints.
/// </summary>
internal sealed class OpenApiValidationSchemaTransformer(IServiceProvider serviceProvider) : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        // Get the validator for this type
        var validatorType = typeof(IValidator<>).MakeGenericType(type);
        var validator = serviceProvider.GetService(validatorType) as IValidator;

        if (validator is null)
        {
            return Task.CompletedTask;
        }

        // Create empty instance to get the descriptor
        var descriptor = validator.CreateDescriptor();

        // Process each property
        if (schema.Properties is null)
        {
            return Task.CompletedTask;
        }

        foreach (var property in schema.Properties)
        {
            var propertyName = property.Key;
            var propertySchema = property.Value as OpenApiSchema;

            if (propertySchema is null)
            {
                continue;
            }

            // Find validators for this property (case-insensitive match)
            var rules = descriptor.GetRulesForMember(ToPascalCase(propertyName));

            foreach (var rule in rules)
            {
                ProcessValidationRule(rule, propertySchema, schema);
            }
        }

        return Task.CompletedTask;
    }

    private static void ProcessValidationRule(IValidationRule rule, OpenApiSchema propertySchema, OpenApiSchema parentSchema)
    {
        foreach (var component in rule.Components)
        {
            var validatorType = component.Validator?.GetType();

            if (validatorType is null)
            {
                continue;
            }

            var validatorName = validatorType.Name;

            switch (validatorName)
            {
                case nameof(NotEmptyValidator<object, object>):
                case "NotNullValidator`2":
                    // Mark as required
                    parentSchema.Required ??= new HashSet<string>();
                    var propertyName = GetPropertyNameFromRule(rule);
                    if (!string.IsNullOrEmpty(propertyName))
                    {
                        parentSchema.Required.Add(ToCamelCase(propertyName));
                    }
                    break;

                case nameof(LengthValidator<object>):
                    if (component.Validator is ILengthValidator lengthValidator)
                    {
                        if (lengthValidator.Min > 0)
                        {
                            propertySchema.MinLength = lengthValidator.Min;
                        }
                        if (lengthValidator.Max > 0 && lengthValidator.Max < int.MaxValue)
                        {
                            propertySchema.MaxLength = lengthValidator.Max;
                        }
                    }
                    break;

                case nameof(MinimumLengthValidator<object>):
                    if (component.Validator is ILengthValidator minLengthValidator && minLengthValidator.Min > 0)
                    {
                        propertySchema.MinLength = minLengthValidator.Min;
                    }
                    break;

                case nameof(MaximumLengthValidator<object>):
                    if (component.Validator is ILengthValidator maxLengthValidator && maxLengthValidator.Max > 0)
                    {
                        propertySchema.MaxLength = maxLengthValidator.Max;
                    }
                    break;

                case nameof(RegularExpressionValidator<object>):
                    if (component.Validator is IRegularExpressionValidator regexValidator)
                    {
                        propertySchema.Pattern = regexValidator.Expression;
                    }
                    break;

                case "EmailValidator`1":
                    propertySchema.Format = "email";
                    break;

                case "InclusiveBetweenValidator`2":
                case "ExclusiveBetweenValidator`2":
                    SetRangeConstraints(component.Validator, propertySchema);
                    break;

                case "GreaterThanValidator`2":
                case "GreaterThanOrEqualValidator`2":
                    SetMinimumConstraint(component.Validator, propertySchema);
                    break;

                case "LessThanValidator`2":
                case "LessThanOrEqualValidator`2":
                    SetMaximumConstraint(component.Validator, propertySchema);
                    break;
            }
        }
    }

    private static string? GetPropertyNameFromRule(IValidationRule rule)
    {
        // Use reflection to get the property name from the rule
        var propertyNameProp = rule.GetType().GetProperty("PropertyName", BindingFlags.Public | BindingFlags.Instance);
        return propertyNameProp?.GetValue(rule) as string;
    }

    private static void SetRangeConstraints(IPropertyValidator? validator, OpenApiSchema schema)
    {
        if (validator is null) return;

        var validatorType = validator.GetType();
        var fromProperty = validatorType.GetProperty("From");
        var toProperty = validatorType.GetProperty("To");

        if (fromProperty?.GetValue(validator) is IComparable from && TryConvertToString(from, out var min))
        {
            schema.Minimum = min;
        }

        if (toProperty?.GetValue(validator) is IComparable to && TryConvertToString(to, out var max))
        {
            schema.Maximum = max;
        }
    }

    private static void SetMinimumConstraint(IPropertyValidator? validator, OpenApiSchema schema)
    {
        if (validator is null) return;

        var valueProperty = validator.GetType().GetProperty("ValueToCompare");
        if (valueProperty?.GetValue(validator) is IComparable value && TryConvertToString(value, out var min))
        {
            schema.Minimum = min;
        }
    }

    private static void SetMaximumConstraint(IPropertyValidator? validator, OpenApiSchema schema)
    {
        if (validator is null) return;

        var valueProperty = validator.GetType().GetProperty("ValueToCompare");
        if (valueProperty?.GetValue(validator) is IComparable value && TryConvertToString(value, out var max))
        {
            schema.Maximum = max;
        }
    }

    private static bool TryConvertToString(object value, out string result)
    {
        try
        {
            var decimalValue = Convert.ToDecimal(value);
            result = decimalValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            result = string.Empty;
            return false;
        }
    }

    private static string ToPascalCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToUpperInvariant();
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        if (name.Length == 1) return name.ToLowerInvariant();
        return char.ToLowerInvariant(name[0]) + name[1..];
    }
}