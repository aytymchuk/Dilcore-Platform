using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Identity.Core.Features.Register;

/// <summary>
/// Request to register the current user.
/// </summary>
/// <param name="Email">The user's email address.</param>
/// <param name="FirstName">The user's first name.</param>
/// <param name="LastName">The user's last name.</param>
public record RegisterUserCommand(string Email, string FirstName, string LastName) : ICommand<UserResponse>;