using AutoMapper;
using Dilcore.Identity.Domain;
using Dilcore.Identity.Store.Entities;

namespace Dilcore.Identity.Store.Profiles;

/// <summary>
/// AutoMapper profile for mapping between User domain entity and UserDocument storage entity.
/// </summary>
public class UserMappingProfile : Profile
{
    public UserMappingProfile()
    {
        // Map from User (domain) to UserDocument (storage)
        CreateMap<User, UserDocument>()
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        // Map from UserDocument (storage) to User (domain)
        CreateMap<UserDocument, User>();
    }
}
