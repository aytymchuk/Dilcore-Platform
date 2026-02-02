using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Users;

namespace Dilcore.WebApp.Features.Users.CurrentUser;

/// <summary>
/// Query to retrieve the currently authenticated user.
/// </summary>
/// <returns>A <see cref="UserModel"/> representing the current user, or null if not authenticated.</returns>
public record GetCurrentUserQuery : IQuery<UserModel?>;