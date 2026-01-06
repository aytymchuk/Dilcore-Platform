using FluentResults;

namespace Dilcore.Results.Abstractions;

/// <summary>
/// Base class for application errors.
/// </summary>
public abstract class AppError : Error
{
    public string Code { get; }
    public ErrorType Type { get; }

    protected AppError(string message, string code, ErrorType type)
        : base(message)
    {
        Code = code;
        Type = type;
        Metadata.Add("Code", code);
        Metadata.Add("Type", type.ToString());
    }
}

public enum ErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Unexpected = 6
}
