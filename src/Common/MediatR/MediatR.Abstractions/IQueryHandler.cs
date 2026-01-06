using FluentResults;
using MediatR;

namespace Dilcore.MediatR.Abstractions;

/// <summary>
/// Defines a handler for a query that returns a Result with a value.
/// </summary>
/// <typeparam name="TQuery">The type of query being handled.</typeparam>
/// <typeparam name="TResponse">The type of the value returned by the query.</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}