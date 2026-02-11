using AutoMapper;
using Dilcore.Tenancy.Actors.Abstractions;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Core;

/// <summary>
/// AutoMapper profile for mapping TenantDto to Tenant domain model.
/// </summary>
public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<TenantDto, Tenant>()
            .ForMember(dest => dest.StoragePrefix, opt => opt.MapFrom(src => src.StorageIdentifier));

        CreateMap<Tenant, TenantDto>()
            .ForMember(dest => dest.StorageIdentifier, opt => opt.MapFrom(src => src.StoragePrefix));
    }
}
