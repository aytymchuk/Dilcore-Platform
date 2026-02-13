using AutoMapper;
using Dilcore.Tenancy.Contracts.Tenants;
using Dilcore.Tenancy.Domain;

namespace Dilcore.Tenancy.Core;

/// <summary>
/// AutoMapper profile for mapping TenantDto to Tenant domain model.
/// </summary>
public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<TenantDto, Tenant>();
        CreateMap<Tenant, TenantDto>();
        CreateMap<Dilcore.Tenancy.Actors.Abstractions.TenantDto, Tenant>();
    }
}