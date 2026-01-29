using FluentValidation;

namespace Dilcore.WebApp.Validation;

public class BaseValidator<T> : AbstractValidator<T>
{
    private FluentValidationAdapter<T>? _adapter;

    private FluentValidationAdapter<T> Adapter => _adapter ??= new FluentValidationAdapter<T>(this);

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => 
        (model, propertyName) => Adapter.ValidateValue(model, propertyName);
}