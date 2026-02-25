using Dilcore.Blueprints.Domain.Entities.Fields;
using Shouldly;

namespace Dilcore.Blueprints.Domain.Tests.Entities;

public class FieldDefinitionTests
{
    [Test]
    public void Should_SetAllProperties_WhenInstantiated()
    {
        var id = Guid.CreateVersion7();

        var field = new FieldDefinition
        {
            Id = id,
            Name = "total_amount",
            DisplayName = "Total Amount",
            Type = FieldType.Number
        };

        field.Id.ShouldBe(id);
        field.Name.ShouldBe("total_amount");
        field.DisplayName.ShouldBe("Total Amount");
        field.Type.ShouldBe(FieldType.Number);
    }

    [TestCase(FieldType.String)]
    [TestCase(FieldType.Number)]
    [TestCase(FieldType.Boolean)]
    [TestCase(FieldType.DateTime)]
    [TestCase(FieldType.File)]
    public void Should_SupportPrimitiveFieldTypes(FieldType type)
    {
        var field = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "test_field",
            DisplayName = "Test",
            Type = type
        };

        field.Type.ShouldBe(type);
    }

    [Test]
    public void ComplexField_ShouldBeAssignableToFieldDefinition()
    {
        var complex = new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "address",
            DisplayName = "Address",
            Type = FieldType.Object
        };

        complex.ShouldBeAssignableTo<FieldDefinition>();
    }

    [TestCase(FieldType.String)]
    [TestCase(FieldType.Number)]
    [TestCase(FieldType.Boolean)]
    [TestCase(FieldType.DateTime)]
    [TestCase(FieldType.File)]
    public void ComplexField_ShouldRejectNonComplexTypes(FieldType type)
    {
        Should.Throw<ArgumentException>(() => new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "test",
            DisplayName = "Test",
            Type = type
        });
    }

    [TestCase(FieldType.Object)]
    [TestCase(FieldType.Array)]
    public void ComplexField_ShouldAcceptComplexTypes(FieldType type)
    {
        Should.NotThrow(() => new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "test",
            DisplayName = "Test",
            Type = type
        });
    }

    [Test]
    public void ComplexField_Fields_ShouldDefaultToEmpty()
    {
        var complex = new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "address",
            DisplayName = "Address",
            Type = FieldType.Object
        };

        complex.Fields.ShouldNotBeNull();
        complex.Fields.ShouldBeEmpty();
    }

    [Test]
    public void ComplexField_ShouldSupportNestedFields()
    {
        var streetField = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "street",
            DisplayName = "Street",
            Type = FieldType.String
        };

        var objectField = new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "address",
            DisplayName = "Address",
            Type = FieldType.Object,
            Fields = [streetField]
        };

        objectField.Fields.Count.ShouldBe(1);
        objectField.Fields[0].Name.ShouldBe("street");
        objectField.Fields[0].Type.ShouldBe(FieldType.String);
    }

    [Test]
    public void ComplexField_ShouldSupportDeeplyNestedStructures()
    {
        var cityField = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "city",
            DisplayName = "City",
            Type = FieldType.String
        };

        var locationField = new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "location",
            DisplayName = "Location",
            Type = FieldType.Object,
            Fields = [cityField]
        };

        var addressField = new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "address",
            DisplayName = "Address",
            Type = FieldType.Object,
            Fields = [locationField]
        };

        var nested = addressField.Fields[0].ShouldBeOfType<ComplexFieldDefinition>();
        nested.Fields[0].Name.ShouldBe("city");
    }

    [Test]
    public void ArrayField_ShouldSupportNestedItemSchema()
    {
        var nameField = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "product_name",
            DisplayName = "Product Name",
            Type = FieldType.String
        };

        var quantityField = new FieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "quantity",
            DisplayName = "Quantity",
            Type = FieldType.Number
        };

        var lineItemsField = new ComplexFieldDefinition
        {
            Id = Guid.CreateVersion7(),
            Name = "line_items",
            DisplayName = "Line Items",
            Type = FieldType.Array,
            Fields = [nameField, quantityField]
        };

        lineItemsField.Fields.Count.ShouldBe(2);
    }

    [Test]
    public void ComplexField_CanBeMixedWithPrimitiveInSameList()
    {
        var fields = new List<FieldDefinition>
        {
            new FieldDefinition
            {
                Id = Guid.CreateVersion7(),
                Name = "title",
                DisplayName = "Title",
                Type = FieldType.String
            },
            new ComplexFieldDefinition
            {
                Id = Guid.CreateVersion7(),
                Name = "address",
                DisplayName = "Address",
                Type = FieldType.Object,
                Fields =
                [
                    new FieldDefinition
                    {
                        Id = Guid.CreateVersion7(),
                        Name = "city",
                        DisplayName = "City",
                        Type = FieldType.String
                    }
                ]
            }
        };

        fields.Count.ShouldBe(2);
        fields[0].ShouldBeOfType<FieldDefinition>();
        fields[1].ShouldBeOfType<ComplexFieldDefinition>();
    }
}
