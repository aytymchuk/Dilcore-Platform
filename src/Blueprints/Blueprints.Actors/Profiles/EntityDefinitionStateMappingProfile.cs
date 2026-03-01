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
            .ForMember(dest => dest.Metadata, opt => opt.MapFrom(src => new EntityMetadata { Tags = src.Tags }))
            .ForMember(dest => dest.Fields, opt => opt.Ignore());

        CreateMap<EntityDefinition, EntityDefinitionState>()
            .ForMember(dest => dest.IsCreated, opt => opt.MapFrom(_ => true))
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(src => MapFields(src.Fields)))
            .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Metadata.Tags.ToList()))
            .ForMember(dest => dest.SchemaName, opt => opt.MapFrom(src => src.SchemaName));

        CreateMap<FieldDefinitionGrainDto, FieldDefinition>()
            .ConstructUsing(src =>
                IsComplexType(src.Type)
                    ? new ComplexFieldDefinition
                    {
                        SchemaName = src.SchemaName,
                        DisplayName = src.DisplayName,
                        Type = Enum.Parse<FieldType>(src.Type),
                        Fields = (src.Fields ?? new List<FieldDefinitionGrainDto>()).Select(MapFieldDto).ToList()
                    }
                    : new FieldDefinition
                    {
                        SchemaName = src.SchemaName,
                        DisplayName = src.DisplayName,
                        Type = Enum.Parse<FieldType>(src.Type)
                    })
            .ForAllMembers(opt => opt.Ignore());

        CreateMap<FieldDefinition, FieldDefinitionGrainDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Fields, opt => opt.MapFrom(new FieldDefinitionFieldsResolver()));
    }

    private static List<FieldDefinition> MapFieldDtos(List<FieldDefinitionGrainDto> dtos) =>
        dtos.Select(MapFieldDto).ToList();

    private static FieldDefinition MapFieldDto(FieldDefinitionGrainDto dto) =>
        IsComplexType(dto.Type)
            ? new ComplexFieldDefinition
            {
                SchemaName = dto.SchemaName,
                DisplayName = dto.DisplayName,
                Type = Enum.Parse<FieldType>(dto.Type),
                Fields = (dto.Fields ?? []).Select(MapFieldDto).ToList()
            }
            : new FieldDefinition
            {
                SchemaName = dto.SchemaName,
                DisplayName = dto.DisplayName,
                Type = Enum.Parse<FieldType>(dto.Type)
            };

    private static bool IsComplexType(string type) =>
        type is nameof(FieldType.Object) or nameof(FieldType.Array);

    private static List<FieldDefinitionGrainDto> MapFields(IReadOnlyList<FieldDefinition> fields) =>
        fields.Select(MapField).ToList();

    private static FieldDefinitionGrainDto MapField(FieldDefinition field) =>
        new()
        {
            SchemaName = field.SchemaName,
            DisplayName = field.DisplayName,
            Type = field.Type.ToString(),
            Fields = field is ComplexFieldDefinition complex
                ? complex.Fields.Select(MapField).ToList()
                : null
        };
}

internal class FieldDefinitionFieldsResolver
    : IValueResolver<FieldDefinition, FieldDefinitionGrainDto, List<FieldDefinitionGrainDto>?>
{
    public List<FieldDefinitionGrainDto>? Resolve(
        FieldDefinition source, FieldDefinitionGrainDto destination,
        List<FieldDefinitionGrainDto>? destMember, ResolutionContext context)
    {
        if (source is not ComplexFieldDefinition complex)
            return null;

        return complex.Fields
            .Select(f => context.Mapper.Map<FieldDefinitionGrainDto>(f))
            .ToList();
    }
}
