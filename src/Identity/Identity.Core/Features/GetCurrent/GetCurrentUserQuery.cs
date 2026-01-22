using Dilcore.Identity.Actors.Abstractions;
using Dilcore.MediatR.Abstractions;

namespace Dilcore.Identity.Core.Features.GetCurrent;

/// <summary>
/// Query to get the current user's profile.
/// </summary>
public record GetCurrentUserQuery : IQuery<UserResponse?>;
