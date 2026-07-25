using Dilcore.Blueprints.Domain.Entities.Fields;
using Dilcore.Domain.Abstractions;

namespace Dilcore.Blueprints.Domain.Entities;

public record EntityDefinition : BaseDomain
{
    private readonly List<FieldDefinition> _fields = [];
    private readonly List<EntityReference> _references = [];

    public string SchemaName { get; init; } = string.Empty;
    public required string DisplayName
    {
        get;
        init
        {
            field = value;
            if (string.IsNullOrWhiteSpace(SchemaName))
                SchemaName = SchemaNameGenerator.Generate(value);
        }
    }
    public string? Description { get; init; }
    public bool IsAbstract { get; init; }
    public Guid? ExtendsEntityId { get; init; }

    public IReadOnlyList<FieldDefinition> Fields => _fields.AsReadOnly();
    public IReadOnlyList<EntityReference> References => _references.AsReadOnly();
    public EntityMetadata Metadata { get; init; } = new();

    public EntityDefinition(IEnumerable<FieldDefinition>? fields = null, IEnumerable<EntityReference>? references = null)
    {
        _fields = fields?.ToList() ?? [];
        _references = references?.ToList() ?? [];
    }

    public void AddField(FieldDefinition field)
    {
        _fields.Add(field);
    }

    public void RemoveField(string schemaName)
    {
        _fields.RemoveAll(f => f.SchemaName == schemaName);
    }

    public void AddReference(EntityReference reference)
    {
        _references.Add(reference);
    }

    public void RemoveReference(string schemaName)
    {
        _references.RemoveAll(r => r.SchemaName == schemaName);
    }
}
