using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;
using Dilcore.Domain.Abstractions;
using Shouldly;

namespace Dilcore.Blueprints.Domain.Tests.Entities;

[TestFixture]
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
            DisplayName = "Invoice Record",
            Description = "Tracks all invoices",
            IsAbstract = false,
            ExtendsEntityId = extendsId,
            Metadata = metadata
        };

        entity.Id.ShouldBe(id);
        entity.SchemaName.ShouldBe("invoiceRecord");
        entity.DisplayName.ShouldBe("Invoice Record");
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
            DisplayName = "Test"
        };

        entity.ShouldBeAssignableTo<BaseDomain>();
    }

    [Test]
    public void Fields_ShouldDefaultToEmptyList()
    {
        var entity = new EntityDefinition
        {
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
            DisplayName = "Test"
        };

        entity.Description.ShouldBeNull();
    }

    [Test]
    public void ExtendsEntityId_ShouldDefaultToNull()
    {
        var entity = new EntityDefinition
        {
            DisplayName = "Test"
        };

        entity.ExtendsEntityId.ShouldBeNull();
    }

    [Test]
    public void IsAbstract_ShouldDefaultToFalse()
    {
        var entity = new EntityDefinition
        {
            DisplayName = "Test"
        };

        entity.IsAbstract.ShouldBe(false);
    }

    [Test]
    public void AddField_ShouldAppendField()
    {
        var entity = new EntityDefinition
        {
            DisplayName = "Test"
        };

        var field = new FieldDefinition
        {
            SchemaName = "amount",
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
            SchemaName = "name",
            DisplayName = "Name",
            Type = FieldType.String
        };

        var entity = new EntityDefinition([field1])
        {
            DisplayName = "Test"
        };

        var field2 = new FieldDefinition
        {
            SchemaName = "active",
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
        var field = new FieldDefinition
        {
            SchemaName = "amount",
            DisplayName = "Amount",
            Type = FieldType.Number
        };

        var entity = new EntityDefinition([field])
        {
            DisplayName = "Test"
        };

        entity.RemoveField("amount");

        entity.Fields.ShouldBeEmpty();
    }

    [Test]
    public void RemoveField_ShouldNotAffectOtherFields()
    {
        var keepField = new FieldDefinition
        {
            SchemaName = "name",
            DisplayName = "Name",
            Type = FieldType.String
        };

        var removeField = new FieldDefinition
        {
            SchemaName = "temp",
            DisplayName = "Temp",
            Type = FieldType.String
        };

        var entity = new EntityDefinition([keepField, removeField])
        {
            DisplayName = "Test"
        };

        entity.RemoveField("temp");

        entity.Fields.Count.ShouldBe(1);
        entity.Fields[0].ShouldBe(keepField);
    }

    [Test]
    public void RemoveField_WithNonExistentSchemaName_ShouldNotChangeFields()
    {
        var field = new FieldDefinition
        {
            SchemaName = "name",
            DisplayName = "Name",
            Type = FieldType.String
        };

        var entity = new EntityDefinition([field])
        {
            DisplayName = "Test"
        };

        entity.RemoveField("nonexistent");

        entity.Fields.Count.ShouldBe(1);
    }

    [Test]
    public void Constructor_WithFields_ShouldInitializeFields()
    {
        var fields = new List<FieldDefinition>
        {
            new()
            {
                SchemaName = "name",
                DisplayName = "Name",
                Type = FieldType.String
            }
        };

        var entity = new EntityDefinition(fields)
        {
            DisplayName = "Test"
        };

        entity.Fields.Count.ShouldBe(1);
        entity.Fields[0].SchemaName.ShouldBe("name");
    }

    [Test]
    public void Constructor_WithNull_ShouldInitializeEmptyFields()
    {
        var entity = new EntityDefinition(fields: null)
        {
            DisplayName = "Test"
        };

        entity.Fields.ShouldNotBeNull();
        entity.Fields.ShouldBeEmpty();
    }

    [Test]
    public void SchemaName_ShouldBeGeneratedFromDisplayName()
    {
        var entity = new EntityDefinition { DisplayName = "Invoice Record" };

        entity.SchemaName.ShouldBe("invoiceRecord");
    }

    [TestCase("My Entity", "myEntity")]
    [TestCase("  Leading Spaces  ", "leadingSpaces")]
    [TestCase("Special!@#Chars$%^", "specialChars")]
    [TestCase("Multiple   Spaces", "multipleSpaces")]
    [TestCase("Already-Hyphenated", "alreadyHyphenated")]
    [TestCase("Mixed CASE Name", "mixedCaseName")]
    [TestCase("dots.and_underscores", "dotsAndUnderscores")]
    public void SchemaName_ShouldNormalizeDisplayName(string displayName, string expected)
    {
        var entity = new EntityDefinition { DisplayName = displayName };

        entity.SchemaName.ShouldBe(expected);
    }

    [Test]
    public void SchemaName_CanBeExplicitlySet()
    {
        var entity = new EntityDefinition
        {
            SchemaName = "custom-name",
            DisplayName = "Invoice Record"
        };

        entity.SchemaName.ShouldBe("custom-name");
    }
}
