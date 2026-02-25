using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;
using Dilcore.Domain.Abstractions;
using Shouldly;

namespace Dilcore.Blueprints.Domain.Tests.Entities;

public class EntityDefinitionTests
{
    [Test]
    public void Should_SetAllProperties_WhenInstantiated()
    {
        var id = Guid.CreateVersion7();
        var extendsId = Guid.CreateVersion7();
        var metadata = new EntityMetadata { Tags = ["crm", "sales"] };

        var entity = new EntityDefinition
        {
            Id = id,
            Name = "invoice_record",
            DisplayName = "Invoice",
            Description = "Tracks all invoices",
            IsAbstract = false,
            ExtendsEntityId = extendsId,
            Metadata = metadata
        };

        entity.Id.ShouldBe(id);
        entity.Name.ShouldBe("invoice_record");
        entity.DisplayName.ShouldBe("Invoice");
        entity.Description.ShouldBe("Tracks all invoices");
        entity.IsAbstract.ShouldBe(false);
        entity.ExtendsEntityId.ShouldBe(extendsId);
        entity.Metadata.ShouldBe(metadata);
    }

    [Test]
    public void Should_BeAssignableToBaseDomain()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.ShouldBeAssignableTo<BaseDomain>();
    }

    [Test]
    public void Fields_ShouldDefaultToEmptyList()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.Fields.ShouldNotBeNull();
        entity.Fields.ShouldBeEmpty();
    }

    [Test]
    public void Metadata_ShouldDefaultToEmptyInstance()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.Metadata.ShouldNotBeNull();
        entity.Metadata.Tags.ShouldBeEmpty();
    }

    [Test]
    public void Description_ShouldDefaultToNull()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.Description.ShouldBeNull();
    }

    [Test]
    public void ExtendsEntityId_ShouldDefaultToNull()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.ExtendsEntityId.ShouldBeNull();
    }

    [Test]
    public void IsAbstract_ShouldDefaultToFalse()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.IsAbstract.ShouldBe(false);
    }

    [Test]
    public void AddField_ShouldAppendField()
    {
        var entity = new EntityDefinition
        {
            Name = "test",
            DisplayName = "Test"
        };

        var field = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "amount",
            DisplayName = "Amount",
            Type = FieldType.Number
        };

        entity.AddField(field);

        entity.Fields.Count.ShouldBe(1);
        entity.Fields[0].ShouldBe(field);
    }

    [Test]
    public void AddField_ShouldPreserveExistingFields()
    {
        var field1 = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "name",
            DisplayName = "Name",
            Type = FieldType.String
        };

        var entity = new EntityDefinition([field1])
        {
            Name = "test",
            DisplayName = "Test"
        };

        var field2 = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "active",
            DisplayName = "Active",
            Type = FieldType.Boolean
        };

        entity.AddField(field2);

        entity.Fields.Count.ShouldBe(2);
        entity.Fields[0].ShouldBe(field1);
        entity.Fields[1].ShouldBe(field2);
    }

    [Test]
    public void RemoveField_ShouldRemoveMatchingField()
    {
        var fieldId = Guid.CreateVersion7();
        var field = new FieldDefinition
        {
            Id = fieldId,
            Name = "amount",
            DisplayName = "Amount",
            Type = FieldType.Number
        };

        var entity = new EntityDefinition([field])
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.RemoveField(fieldId);

        entity.Fields.ShouldBeEmpty();
    }

    [Test]
    public void RemoveField_ShouldNotAffectOtherFields()
    {
        var keepField = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "name",
            DisplayName = "Name",
            Type = FieldType.String
        };

        var removeId = Guid.CreateVersion7();
        var removeField = new FieldDefinition
        {
            Id = removeId,
            Name = "temp",
            DisplayName = "Temp",
            Type = FieldType.String
        };

        var entity = new EntityDefinition([keepField, removeField])
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.RemoveField(removeId);

        entity.Fields.Count.ShouldBe(1);
        entity.Fields[0].ShouldBe(keepField);
    }

    [Test]
    public void RemoveField_WithNonExistentId_ShouldNotChangeFields()
    {
        var field = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "name",
            DisplayName = "Name",
            Type = FieldType.String
        };

        var entity = new EntityDefinition([field])
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.RemoveField(Guid.CreateVersion7());

        entity.Fields.Count.ShouldBe(1);
    }

    [Test]
    public void Constructor_WithFields_ShouldInitializeFields()
    {
        var fields = new List<FieldDefinition>
        {
            new()
            {
                Id = Guid.CreateVersion7(),
                Name = "name",
                DisplayName = "Name",
                Type = FieldType.String
            }
        };

        var entity = new EntityDefinition(fields)
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.Fields.Count.ShouldBe(1);
        entity.Fields[0].Name.ShouldBe("name");
    }

    [Test]
    public void Constructor_WithNull_ShouldInitializeEmptyFields()
    {
        var entity = new EntityDefinition(fields: null)
        {
            Name = "test",
            DisplayName = "Test"
        };

        entity.Fields.ShouldNotBeNull();
        entity.Fields.ShouldBeEmpty();
    }
}
