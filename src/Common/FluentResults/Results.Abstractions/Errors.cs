namespace Dilcore.Results.Abstractions;

public class ValidationError : AppError
{
    public ValidationError(string message, string code = ProblemDetailsConstants.ValidationError)
        : base(message, code, ErrorType.Validation)
    {
    }
}

public class NotFoundError : AppError
{
    public NotFoundError(string message, string code = ProblemDetailsConstants.NotFound)
        : base(message, code, ErrorType.NotFound)
    {
    }
}

public class ConflictError : AppError
{
    public ConflictError(string message, string code = ProblemDetailsConstants.Conflict)
        : base(message, code, ErrorType.Conflict)
    {
    }
}

public class UnauthorizedError : AppError
{
    public UnauthorizedError(string message, string code = ProblemDetailsConstants.Unauthorized)
        : base(message, code, ErrorType.Unauthorized)
    {
    }
}

public class ForbiddenError : AppError
{
    public ForbiddenError(string message, string code = ProblemDetailsConstants.Forbidden)
        : base(message, code, ErrorType.Forbidden)
    {
    }
}

public class UnexpectedError : AppError
{
    public UnexpectedError(string message, string code = ProblemDetailsConstants.UnexpectedError)
        : base(message, code, ErrorType.Unexpected)
    {
    }
}
