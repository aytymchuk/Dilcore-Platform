using System.Reflection;
using Humanizer;
using FluentValidation;
using FluentValidation.Validators;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace Dilcore.FluentValidation.Extensions.OpenApi;

/// <summary>
/// OpenAPI schema filter that reflects FluentValidation rules into OpenAPI schema properties.
/// Adds required, minLength, maxLength, pattern, minimum, maximum constraints.
/// </summary>
public sealed class OpenApiValidationSchemaTransformer(IServiceScopeFactory serviceScopeFactory) : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        // Create a scope to resolve validators which are likely Scoped
        using var scope = serviceScopeFactory.CreateScope();

        // Get the validator for this type
        var validatorType = typeof(IValidator<>).MakeGenericType(type);
        var validator = scope.ServiceProvider.GetService(validatorType) as IValidator;

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
            var rules = descriptor.GetRulesForMember(propertyName.Pascalize());

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
            var validator = component.Validator;
            if (validator is null) continue;

            // Mark as required
            if (validator is INotEmptyValidator || validator is INotNullValidator)
            {
                parentSchema.Required ??= new HashSet<string>();
                var boundaryPropertyName = GetPropertyNameFromRule(rule);
                if (!string.IsNullOrEmpty(boundaryPropertyName))
                {
                    parentSchema.Required.Add(boundaryPropertyName.Camelize());
                }
            }

            // Length constraints
            if (validator is ILengthValidator lengthValidator)
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

            // Regex pattern
            if (validator is IRegularExpressionValidator regexValidator)
            {
                propertySchema.Pattern = regexValidator.Expression;
            }

            // Email format
            // Check for standard email validators or name-based check for compatibility
            if (validator.GetType().Name.Contains("EmailValidator"))
            {
                propertySchema.Format = "email";
                if (validator is IRegularExpressionValidator emailRegexValidator)
                {
                    propertySchema.Pattern = emailRegexValidator.Expression;
                }
                else
                {
                    // Fallback reflection
                    var expressionProp = validator.GetType().GetProperty("Expression");
                    if (expressionProp?.GetValue(validator) is string expression)
                    {
                        propertySchema.Pattern = expression;
                    }
                }
            }

            // Range constraints (Inclusive/Exclusive/Between)
            if (validator is IBetweenValidator betweenValidator)
            {
                SetRangeConstraints(validator, propertySchema);
            }
            // Comparison validators (Greater/Less)
            else if (validator is IComparisonValidator comparisonValidator)
            {
                // GreaterThan, GreaterThanOrEqual, LessThan, LessThanOrEqual
                // We need to distinguish between Min and Max based on the validator type name or direction
                var name = validator.GetType().Name;
                if (name.Contains("GreaterThan"))
                {
                    SetMinimumConstraint(validator, propertySchema);
                }
                else if (name.Contains("LessThan"))
                {
                    SetMaximumConstraint(validator, propertySchema);
                }
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
        catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException or ArgumentNullException)
        {
            result = string.Empty;
            return false;
        }
    }
}