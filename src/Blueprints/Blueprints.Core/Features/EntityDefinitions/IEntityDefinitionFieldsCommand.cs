namespace Dilcore.Blueprints.Core.Features.EntityDefinitions;

public interface IEntityDefinitionFieldsCommand
{
    IReadOnlyList<FieldDefinitionParameters> Fields { get; }
}
