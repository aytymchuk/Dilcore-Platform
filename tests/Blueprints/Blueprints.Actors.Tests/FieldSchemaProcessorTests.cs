using Dilcore.Blueprints.Actors;
using Dilcore.Blueprints.Actors.Abstractions;
using Shouldly;

namespace Dilcore.Blueprints.Actors.Tests;

public class FieldSchemaProcessorTests
{
    [Test]
    public void GenerateSchemaNames_WhenFieldsEmpty_ShouldReturnEmptyList()
    {
        var result = FieldSchemaProcessor.GenerateSchemaNames([]);

        result.ShouldBeEmpty();
    }

    [Test]
    public void GenerateSchemaNames_WhenDisplayNameProvided_ShouldGenerateCamelCaseSchemaName()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "",
                DisplayName = "First Name",
                Type = "String"
            }
        };

        var result = FieldSchemaProcessor.GenerateSchemaNames(fields);

        result.Count.ShouldBe(1);
        result[0].SchemaName.ShouldBe("firstName");
    }

    [Test]
    public void GenerateSchemaNames_WhenExplicitSchemaNameProvided_ShouldPreserveIt()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "customName",
                DisplayName = "Display Name",
                Type = "String"
            }
        };

        var result = FieldSchemaProcessor.GenerateSchemaNames(fields);

        result.Count.ShouldBe(1);
        result[0].SchemaName.ShouldBe("customName");
    }

    [Test]
    public void GenerateSchemaNames_WhenNestedFields_ShouldRecurse()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "",
                DisplayName = "Address",
                Type = "Object",
                Fields =
                [
                    new FieldDefinitionGrainDto
                    {
                        SchemaName = "",
                        DisplayName = "Street Name",
                        Type = "String"
                    }
                ]
            }
        };

        var result = FieldSchemaProcessor.GenerateSchemaNames(fields);

        result.Count.ShouldBe(1);
        result[0].SchemaName.ShouldBe("address");
        result[0].Fields!.Length.ShouldBe(1);
        result[0].Fields![0].SchemaName.ShouldBe("streetName");
    }

    [Test]
    public void MergeWithExisting_WhenIncomingMatchesExisting_ShouldPreserveSchemaName()
    {
        var existing = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "existingField",
                DisplayName = "Existing",
                Type = "String"
            }
        };
        var incoming = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "existingField",
                DisplayName = "Updated Display",
                Type = "String"
            }
        };

        var result = FieldSchemaProcessor.MergeWithExisting(incoming, existing);

        result.Count.ShouldBe(1);
        result[0].SchemaName.ShouldBe("existingField");
    }

    [Test]
    public void MergeWithExisting_WhenIncomingNew_ShouldGenerateSchemaName()
    {
        var existing = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "oldField",
                DisplayName = "Old",
                Type = "String"
            }
        };
        var incoming = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "",
                DisplayName = "New Field",
                Type = "String"
            }
        };

        var result = FieldSchemaProcessor.MergeWithExisting(incoming, existing);

        result.Count.ShouldBe(1);
        result[0].SchemaName.ShouldBe("newField");
    }

    [Test]
    public void MergeWithExisting_WhenCaseInsensitiveMatch_ShouldPreserveExistingSchemaName()
    {
        var existing = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "myField",
                DisplayName = "My Field",
                Type = "String"
            }
        };
        var incoming = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "MYFIELD",
                DisplayName = "My Field",
                Type = "String"
            }
        };

        var result = FieldSchemaProcessor.MergeWithExisting(incoming, existing);

        result.Count.ShouldBe(1);
        result[0].SchemaName.ShouldBe("myField");
    }

    [Test]
    public void FindDuplicate_WhenNoDuplicates_ShouldReturnNull()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A", Type = "String" },
            new FieldDefinitionGrainDto { SchemaName = "b", DisplayName = "B", Type = "String" }
        };

        var result = FieldSchemaProcessor.FindDuplicate(fields);

        result.ShouldBeNull();
    }

    [Test]
    public void FindDuplicate_WhenDuplicateAtSameLevel_ShouldReturnDuplicateName()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A", Type = "String" },
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A2", Type = "String" }
        };

        var result = FieldSchemaProcessor.FindDuplicate(fields);

        result.ShouldBe("a");
    }

    [Test]
    public void FindDuplicate_WhenCaseInsensitiveDuplicate_ShouldReturnDuplicateName()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "myField", DisplayName = "A", Type = "String" },
            new FieldDefinitionGrainDto { SchemaName = "myfield", DisplayName = "B", Type = "String" }
        };

        var result = FieldSchemaProcessor.FindDuplicate(fields);

        result.ShouldBe("myfield");
    }

    [Test]
    public void ValidateSchemaNames_WhenReservedName_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "id", DisplayName = "Id", Type = "String" }
        };

        var result = FieldSchemaProcessor.ValidateSchemaNames(fields);

        result.ShouldContain("reserved");
    }

    [Test]
    public void ValidateSchemaNames_WhenCollisionWithEntitySchemaName_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "myEntity", DisplayName = "My Entity", Type = "String" }
        };

        var result = FieldSchemaProcessor.ValidateSchemaNames(fields, "myEntity");

        result.ShouldContain("collides with entity schema name");
    }

    [Test]
    public void ValidateSchemaNames_WhenValid_ShouldReturnNull()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "validField", DisplayName = "Valid", Type = "String" }
        };

        var result = FieldSchemaProcessor.ValidateSchemaNames(fields, "myEntity");

        result.ShouldBeNull();
    }

    [Test]
    public void ComputeChanges_WhenFieldAdded_ShouldIncludeInAdded()
    {
        var oldFields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A", Type = "String" }
        };
        var newFields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A", Type = "String" },
            new FieldDefinitionGrainDto { SchemaName = "b", DisplayName = "B", Type = "String" }
        };

        var result = FieldSchemaProcessor.ComputeChanges(oldFields, newFields);

        result.Added.ShouldContain("b");
        result.Removed.ShouldBeEmpty();
    }

    [Test]
    public void ComputeChanges_WhenFieldRemoved_ShouldIncludeInRemoved()
    {
        var oldFields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A", Type = "String" },
            new FieldDefinitionGrainDto { SchemaName = "b", DisplayName = "B", Type = "String" }
        };
        var newFields = new[]
        {
            new FieldDefinitionGrainDto { SchemaName = "a", DisplayName = "A", Type = "String" }
        };

        var result = FieldSchemaProcessor.ComputeChanges(oldFields, newFields);

        result.Added.ShouldBeEmpty();
        result.Removed.ShouldContain("b");
    }

    [Test]
    public void ComputeChanges_WhenNestedFields_ShouldCollectAllSchemaNames()
    {
        var oldFields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "parent",
                DisplayName = "Parent",
                Type = "Object",
                Fields =
                [
                    new FieldDefinitionGrainDto { SchemaName = "child", DisplayName = "Child", Type = "String" }
                ]
            }
        };
        var newFields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "parent",
                DisplayName = "Parent",
                Type = "Object",
                Fields = []
            }
        };

        var result = FieldSchemaProcessor.ComputeChanges(oldFields, newFields);

        result.Removed.ShouldContain("child");
    }
}
