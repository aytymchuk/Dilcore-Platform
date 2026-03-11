using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Dilcore.Blueprints.Core.Features.EntityDefinitions;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.AddReference;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Create;
using Dilcore.Blueprints.Core.Features.EntityDefinitions.Update;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;
using ContractFieldDto = Dilcore.Blueprints.Contracts.EntityDefinitions.FieldDefinitionDto;

namespace Dilcore.Blueprints.Core.Profiles;

public class EntityDefinitionMappingProfile : Profile
{
    public EntityDefinitionMappingProfile()
    {
        CreateMap<EntityDefinitionGrainDto, EntityDefinition>()
            .ForCtorParam("fields", opt => opt.MapFrom(src => src.Fields.Select(FieldDefinitionMapper.ToFieldDefinition).ToList()))
            .ForCtorParam("references", opt => opt.MapFrom(src => src.References.Select(r => new EntityReference
            {
                SchemaName = r.SchemaName,
                ReferenceType = Enum.Parse<EntityReferenceType>(r.ReferenceType, ignoreCase: true),
                RelatedEntityDefinitionId = r.RelatedEntityDefinitionId,
                RelatedEntitySchemaName = r.RelatedEntitySchemaName
            }).ToList()))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new EntityMetadata { Tags = src.Tags.ToList() }))
            .ForMember(dest => dest.Fields, opt => opt.Ignore());

        CreateMap<CreateEntityDefinitionCommand, CreateEntityDefinitionGrainCommand>();
        CreateMap<UpdateEntityDefinitionCommand, UpdateEntityDefinitionGrainCommand>();
        CreateMap<FieldDefinitionParameters, FieldDefinitionGrainDto>();

        CreateMap<CreateEntityDefinitionDto, CreateEntityDefinitionCommand>();
        CreateMap<CreateEntityReferenceDto, EntityReferenceParameters>()
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src =>
                Enum.Parse<EntityReferenceType>(src.ReferenceType, ignoreCase: true)));
        
        CreateMap<EntityReferenceParameters, EntityReferenceGrainParameter>()
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src => src.ReferenceType.ToString()));
            
        CreateMap<UpdateEntityDefinitionDto, UpdateEntityDefinitionCommand>();
        CreateMap<ContractFieldDto, FieldDefinitionParameters>();

        CreateMap<EntityDefinition, EntityDefinitionDto>()
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
            .ForMember(dest => dest.References, opt => opt.MapFrom(src => src.References ?? new List<EntityReference>()))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Metadata.Tags));

        CreateMap<EntityReference, EntityReferenceDto>()
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src => src.ReferenceType.ToString()));

        CreateMap<CreateEntityReferenceDto, AddEntityReferenceCommand>()
            .ForMember(dest => dest.EntityDefinitionId, opt => opt.Ignore())
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src =>
                Enum.Parse<EntityReferenceType>(src.ReferenceType, ignoreCase: true)))
            .ForMember(dest => dest.RelatedEntityDefinitionId, opt => opt.MapFrom(src => src.RelatedEntityDefinitionId));

        CreateMap<FieldDefinition, ContractFieldDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(new ContractFieldsResolver()));
    }

    private class ContractFieldsResolver : IValueResolver<FieldDefinition, ContractFieldDto, List<ContractFieldDto>?>
    {
        public List<ContractFieldDto>? Resolve(
            FieldDefinition source, ContractFieldDto destination,
            List<ContractFieldDto>? destMember, ResolutionContext context)
        {
            return source is ComplexFieldDefinition complex
                ? context.Mapper.Map<List<ContractFieldDto>>(complex.Fields)
                : null;
        }
    }
}
