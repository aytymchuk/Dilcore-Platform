using FluentResults;
using MediatR;

namespace Dilcore.MediatR.Abstractions;

/// <summary>
/// Represents a query that returns a Result with a value.
/// </summary>
/// <typeparam name="TResponse">The type of the value.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}