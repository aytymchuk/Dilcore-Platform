using Dilcore.Identity.Contracts.Profile;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.WebApp.Features.Users.CurrentUser;

/// <summary>
/// Query to get the current authenticated user's profile.
/// </summary>
public record GetCurrentUserQuery : IQuery<UserDto?>;
