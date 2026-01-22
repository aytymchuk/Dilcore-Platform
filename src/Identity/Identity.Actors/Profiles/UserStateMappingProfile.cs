using AutoMapper;
using Dilcore.Identity.Domain;

namespace Dilcore.Identity.Actors.Profiles;

/// <summary>
/// AutoMapper profile for mapping between UserState and User domain entity.
/// </summary>
public class UserStateMappingProfile : Profile
{
    public UserStateMappingProfile()
    {
        // Map from User domain entity to UserState (for reading from repository)
        CreateMap<User, UserState>()
            .ForMember(dest => dest.RegisteredAt, opt => opt.MapFrom(src => src.CreatedAt))
            .ForMember(dest => dest.IsRegistered, opt => opt.MapFrom(_ => true));

        // Map from UserState to User domain entity (for writing to repository)
        CreateMap<UserState, User>()
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.RegisteredAt));
    }
}
