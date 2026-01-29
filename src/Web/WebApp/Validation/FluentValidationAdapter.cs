using FluentValidation;

namespace Dilcore.WebApp.Validation;

/// <summary>
/// Adapts FluentValidation to match MudBlazor's validation signature.
/// </summary>
/// <typeparam name="T">The type of model being validated.</typeparam>
public class FluentValidationAdapter<T>
{
    private readonly IValidator<T> _validator;

    public FluentValidationAdapter(IValidator<T> validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Validates a specific property of the model.
    /// Matches MudBlazor's Func&lt;object, string, Task&lt;IEnumerable&lt;string&gt;&gt;&gt; signature.
    /// </summary>
    public async Task<IEnumerable<string>> ValidateValue(object model, string propertyName)
    {
        if (model is not T typedModel)
        {
            return [];
        }

        var result = await _validator.ValidateAsync(ValidationContext<T>.CreateWithOptions(typedModel, x => x.IncludeProperties(propertyName)));
        
        if (result.IsValid)
        {
            return [];
        }
        
        return result.Errors.Select(e => e.ErrorMessage);
    }

    /// <summary>
    /// Validates the entire model.
    /// </summary>
    public async Task<bool> ValidateAsync(T model)
    {
        var result = await _validator.ValidateAsync(model);
        return result.IsValid;
    }
}
