using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Contracts.EntityDefinitions;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Create;
using Dilcore.Blueprints.Contracts.EntityDefinitions.Update;
using Dilcore.Blueprints.Core.Features.EntityDefinitions;
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
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new EntityMetadata { Tags = src.Tags.ToList() }))
            .ForMember(dest => dest.Fields, opt => opt.Ignore());

        CreateMap<CreateEntityDefinitionCommand, CreateEntityDefinitionGrainCommand>();
        CreateMap<UpdateEntityDefinitionCommand, UpdateEntityDefinitionGrainCommand>();
        CreateMap<FieldDefinitionParameters, FieldDefinitionGrainDto>();

        CreateMap<CreateEntityDefinitionDto, CreateEntityDefinitionCommand>();
        CreateMap<UpdateEntityDefinitionDto, UpdateEntityDefinitionCommand>();
        CreateMap<ContractFieldDto, FieldDefinitionParameters>();

        CreateMap<EntityDefinition, EntityDefinitionDto>()
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Metadata.Tags));

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
