using Dilcore.MediatR.Abstractions;
using Dilcore.Identity.Actors.Abstractions;

namespace Dilcore.Identity.Core.Features.GetCurrent;

/// <summary>
/// Query to get the current user's profile.
/// </summary>
public record GetCurrentUserQuery : IQuery<UserDto?>;
