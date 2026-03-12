using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Results.Abstractions;
using FluentResults;

namespace Dilcore.Blueprints.Core.Features.EntityDefinitions;

internal static class EntityDefinitionGrainResultExtensions
{
    public static IError ToFluentError(
        this EntityDefinitionGrainResult result,
        string notFoundDefaultMessage,
        string validationDefaultMessage,
        string unexpectedDefaultMessage)
    {
        return result.ErrorCode switch
        {
            EntityDefinitionGrainResult.NotFoundCode =>
                new NotFoundError(result.ErrorMessage ?? notFoundDefaultMessage),
            EntityDefinitionGrainResult.ValidationErrorCode =>
                new ValidationError(result.ErrorMessage ?? validationDefaultMessage),
            _ => new UnexpectedError(result.ErrorMessage ?? unexpectedDefaultMessage)
        };
    }
}