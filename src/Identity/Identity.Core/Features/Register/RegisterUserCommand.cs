using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Identity.Core.Features.Register;

/// <summary>
/// Request to register the current user.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="FullName">The user's full name.</param>
public record RegisterUserCommand(string Email, string FullName) : ICommand<UserDto>;
