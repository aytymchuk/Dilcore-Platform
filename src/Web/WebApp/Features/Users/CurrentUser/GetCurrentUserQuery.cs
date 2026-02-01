using Dilcore.MediatR.Abstractions;
using Dilcore.WebApp.Models.Users;

namespace Dilcore.WebApp.Features.Users.CurrentUser;

public record GetCurrentUserQuery : IQuery<UserModel?>;