using AutoMapper;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Actors.Profiles;

public class TenantStateMappingProfile : Profile
{
    public TenantStateMappingProfile()
    {
        CreateMap<TenantState, Tenant>()
            .ReverseMap()
            .ForMember(dest => dest.IsCreated, opt => opt.MapFrom(src => true));
    }
}