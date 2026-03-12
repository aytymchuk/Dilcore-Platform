using Dilcore.Blueprints.Actors;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Domain;
using Shouldly;

namespace Dilcore.Blueprints.Actors.Tests;

public class EntityDefinitionValidatorTests
{
    [Test]
    public void ValidateFields_WhenNull_ShouldReturnError()
    {
        var result = EntityDefinitionValidator.ValidateFields(null);

        result.ShouldBe("Fields must not be null.");
    }

    [Test]
    public void ValidateFields_WhenEmpty_ShouldReturnNull()
    {
        var result = EntityDefinitionValidator.ValidateFields([]);

        result.ShouldBeNull();
    }

    [Test]
    public void ValidateFields_WhenInvalidFieldType_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "bad",
                DisplayName = "Bad",
                Type = "InvalidType"
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain("is not valid");
    }

    [Test]
    public void ValidateFields_WhenObjectFieldHasNoNestedFields_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "address",
                DisplayName = "Address",
                Type = "Object",
                Fields = null
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain("must contain at least one nested field");
    }

    [Test]
    public void ValidateFields_WhenObjectFieldHasEmptyNestedFields_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "address",
                DisplayName = "Address",
                Type = "Object",
                Fields = []
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain("must contain at least one nested field");
    }

    [Test]
    public void ValidateFields_WhenArrayFieldHasNoNestedFields_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "items",
                DisplayName = "Items",
                Type = "Array",
                Fields = []
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain("must contain at least one nested field");
    }

    [Test]
    public void ValidateFields_WhenPrimitiveFieldHasNestedFields_ShouldReturnError()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "name",
                DisplayName = "Name",
                Type = "String",
                Fields =
                [
                    new FieldDefinitionGrainDto { SchemaName = "nested", DisplayName = "Nested", Type = "String" }
                ]
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain("must not have nested fields");
    }

    [Test]
    public void ValidateFields_WhenTooManyTopLevelFields_ShouldReturnError()
    {
        var fields = Enumerable.Range(0, EntityDefinitionLimits.MaxTopLevelFields + 1)
            .Select(i => new FieldDefinitionGrainDto
            {
                SchemaName = $"field{i}",
                DisplayName = $"Field {i}",
                Type = "String"
            })
            .ToArray();

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain($"Must not have more than {EntityDefinitionLimits.MaxTopLevelFields} fields");
    }

    [Test]
    public void ValidateFields_WhenTooManyNestedFields_ShouldReturnError()
    {
        var nestedFields = Enumerable.Range(0, EntityDefinitionLimits.MaxFieldsPerLevel + 1)
            .Select(i => new FieldDefinitionGrainDto
            {
                SchemaName = $"nested{i}",
                DisplayName = $"Nested {i}",
                Type = "String"
            })
            .ToArray();

        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "parent",
                DisplayName = "Parent",
                Type = "Object",
                Fields = nestedFields
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain($"Must not have more than {EntityDefinitionLimits.MaxFieldsPerLevel} fields");
    }

    [Test]
    public void ValidateFields_WhenNestingDepthExceeded_ShouldReturnError()
    {
        FieldDefinitionGrainDto BuildNested(int depth)
        {
            if (depth >= EntityDefinitionLimits.MaxNestingDepth + 1)
            {
                return new FieldDefinitionGrainDto
                {
                    SchemaName = "leaf",
                    DisplayName = "Leaf",
                    Type = "String"
                };
            }

            return new FieldDefinitionGrainDto
            {
                SchemaName = $"level{depth}",
                DisplayName = $"Level {depth}",
                Type = "Object",
                Fields =
                [
                    BuildNested(depth + 1)
                ]
            };
        }

        var fields = new[] { BuildNested(1) };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldContain($"Field nesting depth must not exceed {EntityDefinitionLimits.MaxNestingDepth} levels");
    }

    [Test]
    public void ValidateFields_WhenValidStructure_ShouldReturnNull()
    {
        var fields = new[]
        {
            new FieldDefinitionGrainDto
            {
                SchemaName = "address",
                DisplayName = "Address",
                Type = "Object",
                Fields =
                [
                    new FieldDefinitionGrainDto
                    {
                        SchemaName = "street",
                        DisplayName = "Street",
                        Type = "String"
                    }
                ]
            }
        };

        var result = EntityDefinitionValidator.ValidateFields(fields);

        result.ShouldBeNull();
    }
}
