using Dilcore.Blueprints.Domain.Entities.Fields;
using Dilcore.Domain.Abstractions;

namespace Dilcore.Blueprints.Domain.Entities;

public record EntityDefinition : BaseDomain
{
    private readonly List<FieldDefinition> _fields = [];

    public string SchemaName { get; init; } = string.Empty;
    public required string DisplayName
    {
        get;
        init
        {
            field = value;
            if (string.IsNullOrEmpty(SchemaName))
                SchemaName = SchemaNameGenerator.Generate(value);
        }
    }
    public string? Description { get; init; }
    public bool IsAbstract { get; init; }
    public Guid? ExtendsEntityId { get; init; }

    public IReadOnlyList<FieldDefinition> Fields => _fields.AsReadOnly();
    public EntityMetadata Metadata { get; init; } = new();

    public EntityDefinition(IEnumerable<FieldDefinition>? fields = null)
    {
        _fields = fields?.ToList() ?? [];
    }

    public void AddField(FieldDefinition field)
    {
        _fields.Add(field);
    }

    public void RemoveField(string schemaName)
    {
        _fields.RemoveAll(f => f.SchemaName == schemaName);
    }
}
