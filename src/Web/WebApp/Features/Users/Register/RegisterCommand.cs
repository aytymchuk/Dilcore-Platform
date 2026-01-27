using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Users;

namespace Dilcore.WebApp.Features.Users.Register;

/// <summary>
/// Command to register the current user.
/// </summary>
/// <param name="Parameters">The registration parameters.</param>
public record RegisterCommand(RegisterUserParameters Parameters) : ICommand<UserModel>;
