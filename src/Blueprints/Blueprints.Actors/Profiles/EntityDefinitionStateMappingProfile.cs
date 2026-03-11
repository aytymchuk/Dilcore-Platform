using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;

namespace Dilcore.Blueprints.Actors.Profiles;

public class EntityDefinitionStateMappingProfile : Profile
{
    public EntityDefinitionStateMappingProfile()
    {
        CreateMap<EntityDefinitionState, EntityDefinition>()
            .ForCtorParam("fields", opt => opt.MapFrom(src => MapFieldDtos(src.Fields)))
            .ForCtorParam("references", opt => opt.MapFrom(src => MapReferenceDtos(src.References ?? new List<EntityReferenceGrainDto>())))
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new EntityMetadata { Tags = src.Tags.ToList() }))
            .ForMember(dest => dest.Fields, opt => opt.Ignore());

        CreateMap<EntityDefinition, EntityDefinitionState>()
            .ForMember(dest => dest.IsCreated, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => MapFields(src.Fields)))
            .ForMember(dest => dest.References, opt => opt.MapFrom(src => MapReferences(src.References ?? new List<EntityReference>())))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Metadata.Tags.ToList()))
            .ForMember(dest => dest.SchemaName, opt => opt.MapFrom(src => src.SchemaName));

        CreateMap<FieldDefinitionGrainDto, FieldDefinition>()
            .ConstructUsing(src => FieldDefinitionMapper.ToFieldDefinition(src))
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<FieldDefinition, FieldDefinitionGrainDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(new FieldDefinitionFieldsResolver()));

        CreateMap<EntityReferenceGrainDto, EntityReference>()
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src =>
                Enum.Parse<EntityReferenceType>(src.ReferenceType, ignoreCase: true)));

        CreateMap<EntityReference, EntityReferenceGrainDto>()
            .ForMember(dest => dest.ReferenceType, opt => opt.MapFrom(src => src.ReferenceType.ToString()));
    }

    private static List<EntityReference> MapReferenceDtos(List<EntityReferenceGrainDto> dtos) =>
        dtos.Select(dto => new EntityReference
        {
            SchemaName = dto.SchemaName,
            ReferenceType = Enum.Parse<EntityReferenceType>(dto.ReferenceType, ignoreCase: true),
            RelatedEntityDefinitionId = dto.RelatedEntityDefinitionId,
            RelatedEntitySchemaName = dto.RelatedEntitySchemaName
        }).ToList();

    private static List<EntityReferenceGrainDto> MapReferences(IReadOnlyList<EntityReference> references) =>
        references.Select(r => new EntityReferenceGrainDto
        {
            SchemaName = r.SchemaName,
            ReferenceType = r.ReferenceType.ToString(),
            RelatedEntityDefinitionId = r.RelatedEntityDefinitionId,
            RelatedEntitySchemaName = r.RelatedEntitySchemaName
        }).ToList();

    private static List<FieldDefinition> MapFieldDtos(List<FieldDefinitionGrainDto> dtos) =>
        dtos.Select(FieldDefinitionMapper.ToFieldDefinition).ToList();

    private static List<FieldDefinitionGrainDto> MapFields(IReadOnlyList<FieldDefinition> fields) =>
        fields.Select(MapField).ToList();

    private static FieldDefinitionGrainDto MapField(FieldDefinition field) =>
        new()
        {
            SchemaName = field.SchemaName,
            DisplayName = field.DisplayName,
            Type = field.Type.ToString(),
            Fields = field is ComplexFieldDefinition complex
                ? complex.Fields.Select(MapField).ToArray()
                : null
        };
}

internal class FieldDefinitionFieldsResolver
    : IValueResolver<FieldDefinition, FieldDefinitionGrainDto, FieldDefinitionGrainDto[]?>
{
    public FieldDefinitionGrainDto[]? Resolve(
        FieldDefinition source, FieldDefinitionGrainDto destination,
        FieldDefinitionGrainDto[]? destMember, ResolutionContext context)
    {
        if (source is not ComplexFieldDefinition complex)
            return null;

        return complex.Fields
            .Select(f => context.Mapper.Map<FieldDefinitionGrainDto>(f))
            .ToArray();
    }
}
