using FluentResults;
using MediatR;

namespace Dilcore.MediatR.Abstractions;

/// <summary>
/// Represents a command that returns a Result.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Represents a command that returns a Result with a value.
/// </summary>
/// <typeparam name="TResponse">The type of the value.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}