using AutoMapper;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.Blueprints.Store.Entities.Fields;

namespace Dilcore.Blueprints.Store.Profiles;

public class EntityDefinitionStoreMappingProfile : Profile
{
    public EntityDefinitionStoreMappingProfile()
    {
        CreateMap<EntityDefinition, EntityDefinitionDocument>()
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => src.Fields))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                new EntityMetadataDocument { Tags = src.Metadata.Tags.ToList() }))
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore());

        CreateMap<EntityDefinitionDocument, EntityDefinition>()
            .ForCtorParam("fields", opt => opt.MapFrom(src => src.Fields))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src =>
                new EntityMetadata { Tags = src.Metadata.Tags }))
            .ForMember(dest => dest.Fields, opt => opt.Ignore());

        CreateMap<FieldDefinition, FieldDefinitionDocument>()
            .Include<ComplexFieldDefinition, ComplexFieldDefinitionDocument>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<ComplexFieldDefinition, ComplexFieldDefinitionDocument>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));

        CreateMap<FieldDefinitionDocument, FieldDefinition>()
            .Include<ComplexFieldDefinitionDocument, ComplexFieldDefinition>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ParseFieldType(src.Type)));

        CreateMap<ComplexFieldDefinitionDocument, ComplexFieldDefinition>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ParseFieldType(src.Type)));
    }

    private static FieldType ParseFieldType(string type) =>
        Enum.TryParse<FieldType>(type, ignoreCase: true, out var parsed)
            ? parsed
            : throw new AutoMapperMappingException($"Unknown field type '{type}'.");
}
