using AutoMapper;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;
using Dilcore.Blueprints.Store.Entities;
using Dilcore.Blueprints.Store.Entities.Fields;
using Dilcore.Blueprints.Store.Profiles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Dilcore.Blueprints.Store.Tests.Profiles;

[TestFixture]
public class EntityDefinitionStoreMappingProfileTests
{
    private IMapper _mapper = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var loggerFactory = new LoggerFactory([NullLoggerProvider.Instance]);
        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<EntityDefinitionStoreMappingProfile>(), loggerFactory);

        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    #region Domain → Document

    [Test]
    public void DomainToDocument_ShouldMapScalarProperties()
    {
        var id = Guid.CreateVersion7();
        var extendsId = Guid.CreateVersion7();
        var now = DateTime.UtcNow;
        var domain = new EntityDefinition(fields: null)
        {
            Id = id,
            ETag = 3,
            SchemaName = "customer",
            DisplayName = "Customer",
            Description = "A customer",
            IsAbstract = true,
            ExtendsEntityId = extendsId,
            CreatedAt = now,
            UpdatedAt = now,
            Metadata = new EntityMetadata { Tags = ["crm", "core"] }
        };

        var doc = _mapper.Map<EntityDefinitionDocument>(domain);

        doc.Id.ShouldBe(id);
        doc.ETag.ShouldBe(3);
        doc.SchemaName.ShouldBe("customer");
        doc.DisplayName.ShouldBe("Customer");
        doc.Description.ShouldBe("A customer");
        doc.IsAbstract.ShouldBeTrue();
        doc.ExtendsEntityId.ShouldBe(extendsId);
        doc.CreatedAt.ShouldBe(now);
        doc.UpdatedAt.ShouldBe(now);
        doc.Metadata.Tags.ShouldContain("crm");
        doc.Metadata.Tags.ShouldContain("core");
    }

    [Test]
    public void DomainToDocument_ShouldMapSimpleFieldsAsBaseType()
    {
        var domain = new EntityDefinition(
        [
            new FieldDefinition
            {
                SchemaName = "email",
                DisplayName = "Email",
                Type = FieldType.String
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Contact"
        };

        var doc = _mapper.Map<EntityDefinitionDocument>(domain);

        doc.Fields.Count.ShouldBe(1);
        var field = doc.Fields[0];
        field.ShouldBeOfType<FieldDefinitionDocument>();
        field.ShouldNotBeOfType<ComplexFieldDefinitionDocument>();
        field.SchemaName.ShouldBe("email");
        field.DisplayName.ShouldBe("Email");
        field.Type.ShouldBe("String");
    }

    [Test]
    public void DomainToDocument_ShouldMapComplexFieldsAsDerivedType()
    {
        var domain = new EntityDefinition(
        [
            new ComplexFieldDefinition
            {
                SchemaName = "address",
                DisplayName = "Address",
                Type = FieldType.Object,
                Fields =
                [
                    new FieldDefinition
                    {
                        SchemaName = "city",
                        DisplayName = "City",
                        Type = FieldType.String
                    }
                ]
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Customer"
        };

        var doc = _mapper.Map<EntityDefinitionDocument>(domain);

        doc.Fields.Count.ShouldBe(1);
        var complexDoc = doc.Fields[0].ShouldBeOfType<ComplexFieldDefinitionDocument>();
        complexDoc.SchemaName.ShouldBe("address");
        complexDoc.DisplayName.ShouldBe("Address");
        complexDoc.Type.ShouldBe("Object");
        complexDoc.Fields.Count.ShouldBe(1);
        complexDoc.Fields[0].ShouldBeOfType<FieldDefinitionDocument>();
        complexDoc.Fields[0].SchemaName.ShouldBe("city");
        complexDoc.Fields[0].Type.ShouldBe("String");
    }

    [Test]
    public void DomainToDocument_ShouldMapNestedComplexFields()
    {
        var domain = new EntityDefinition(
        [
            new ComplexFieldDefinition
            {
                SchemaName = "items",
                DisplayName = "Items",
                Type = FieldType.Array,
                Fields =
                [
                    new ComplexFieldDefinition
                    {
                        SchemaName = "details",
                        DisplayName = "Details",
                        Type = FieldType.Object,
                        Fields =
                        [
                            new FieldDefinition
                            {
                                SchemaName = "sku",
                                DisplayName = "SKU",
                                Type = FieldType.String
                            }
                        ]
                    }
                ]
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Order"
        };

        var doc = _mapper.Map<EntityDefinitionDocument>(domain);

        var outerComplex = doc.Fields[0].ShouldBeOfType<ComplexFieldDefinitionDocument>();
        outerComplex.Type.ShouldBe("Array");
        var innerComplex = outerComplex.Fields[0].ShouldBeOfType<ComplexFieldDefinitionDocument>();
        innerComplex.SchemaName.ShouldBe("details");
        innerComplex.Type.ShouldBe("Object");
        innerComplex.Fields[0].ShouldBeOfType<FieldDefinitionDocument>();
        innerComplex.Fields[0].SchemaName.ShouldBe("sku");
    }

    #endregion

    #region Document → Domain

    [Test]
    public void DocumentToDomain_ShouldMapScalarProperties()
    {
        var id = Guid.CreateVersion7();
        var extendsId = Guid.CreateVersion7();
        var now = DateTime.UtcNow;
        var doc = new EntityDefinitionDocument
        {
            Id = id,
            ETag = 5,
            SchemaName = "product",
            DisplayName = "Product",
            Description = "A product",
            IsAbstract = false,
            ExtendsEntityId = extendsId,
            Fields = [],
            Metadata = new EntityMetadataDocument { Tags = ["catalog"] },
            CreatedAt = now,
            UpdatedAt = now
        };

        var domain = _mapper.Map<EntityDefinition>(doc);

        domain.Id.ShouldBe(id);
        domain.ETag.ShouldBe(5);
        domain.SchemaName.ShouldBe("product");
        domain.DisplayName.ShouldBe("Product");
        domain.Description.ShouldBe("A product");
        domain.IsAbstract.ShouldBeFalse();
        domain.ExtendsEntityId.ShouldBe(extendsId);
        domain.CreatedAt.ShouldBe(now);
        domain.UpdatedAt.ShouldBe(now);
        domain.Metadata.Tags.ShouldContain("catalog");
    }

    [Test]
    public void DocumentToDomain_ShouldMapSimpleFieldsAsBaseType()
    {
        var doc = new EntityDefinitionDocument
        {
            Id = Guid.CreateVersion7(),
            SchemaName = "contact",
            DisplayName = "Contact",
            Fields =
            [
                new FieldDefinitionDocument
                {
                    SchemaName = "phone",
                    DisplayName = "Phone",
                    Type = "String"
                }
            ]
        };

        var domain = _mapper.Map<EntityDefinition>(doc);

        domain.Fields.Count.ShouldBe(1);
        var field = domain.Fields[0];
        field.ShouldBeOfType<FieldDefinition>();
        field.ShouldNotBeOfType<ComplexFieldDefinition>();
        field.SchemaName.ShouldBe("phone");
        field.DisplayName.ShouldBe("Phone");
        field.Type.ShouldBe(FieldType.String);
    }

    [Test]
    public void DocumentToDomain_ShouldMapComplexFieldsAsDerivedType()
    {
        var doc = new EntityDefinitionDocument
        {
            Id = Guid.CreateVersion7(),
            SchemaName = "customer",
            DisplayName = "Customer",
            Fields =
            [
                new ComplexFieldDefinitionDocument
                {
                    SchemaName = "address",
                    DisplayName = "Address",
                    Type = "Object",
                    Fields =
                    [
                        new FieldDefinitionDocument
                        {
                            SchemaName = "street",
                            DisplayName = "Street",
                            Type = "String"
                        }
                    ]
                }
            ]
        };

        var domain = _mapper.Map<EntityDefinition>(doc);

        domain.Fields.Count.ShouldBe(1);
        var complex = domain.Fields[0].ShouldBeOfType<ComplexFieldDefinition>();
        complex.SchemaName.ShouldBe("address");
        complex.Type.ShouldBe(FieldType.Object);
        complex.Fields.Count.ShouldBe(1);
        complex.Fields[0].ShouldBeOfType<FieldDefinition>();
        complex.Fields[0].SchemaName.ShouldBe("street");
        complex.Fields[0].Type.ShouldBe(FieldType.String);
    }

    [Test]
    public void DocumentToDomain_ShouldMapNestedComplexFields()
    {
        var doc = new EntityDefinitionDocument
        {
            Id = Guid.CreateVersion7(),
            SchemaName = "order",
            DisplayName = "Order",
            Fields =
            [
                new ComplexFieldDefinitionDocument
                {
                    SchemaName = "items",
                    DisplayName = "Items",
                    Type = "Array",
                    Fields =
                    [
                        new ComplexFieldDefinitionDocument
                        {
                            SchemaName = "meta",
                            DisplayName = "Meta",
                            Type = "Object",
                            Fields =
                            [
                                new FieldDefinitionDocument
                                {
                                    SchemaName = "label",
                                    DisplayName = "Label",
                                    Type = "String"
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var domain = _mapper.Map<EntityDefinition>(doc);

        var outer = domain.Fields[0].ShouldBeOfType<ComplexFieldDefinition>();
        outer.Type.ShouldBe(FieldType.Array);
        var inner = outer.Fields[0].ShouldBeOfType<ComplexFieldDefinition>();
        inner.SchemaName.ShouldBe("meta");
        inner.Type.ShouldBe(FieldType.Object);
        inner.Fields[0].ShouldBeOfType<FieldDefinition>();
        inner.Fields[0].SchemaName.ShouldBe("label");
    }

    #endregion

    #region Round-trip

    [Test]
    public void RoundTrip_DomainToDocumentAndBack_ShouldPreserveData()
    {
        var original = new EntityDefinition(
        [
            new FieldDefinition
            {
                SchemaName = "name",
                DisplayName = "Name",
                Type = FieldType.String
            },
            new ComplexFieldDefinition
            {
                SchemaName = "profile",
                DisplayName = "Profile",
                Type = FieldType.Object,
                Fields =
                [
                    new FieldDefinition
                    {
                        SchemaName = "bio",
                        DisplayName = "Bio",
                        Type = FieldType.String
                    }
                ]
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            ETag = 10,
            SchemaName = "user",
            DisplayName = "User",
            Description = "A user",
            Metadata = new EntityMetadata { Tags = ["identity"] },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var doc = _mapper.Map<EntityDefinitionDocument>(original);
        var roundTripped = _mapper.Map<EntityDefinition>(doc);

        roundTripped.Id.ShouldBe(original.Id);
        roundTripped.ETag.ShouldBe(original.ETag);
        roundTripped.SchemaName.ShouldBe("user");
        roundTripped.DisplayName.ShouldBe("User");
        roundTripped.Description.ShouldBe("A user");
        roundTripped.Fields.Count.ShouldBe(2);

        var simple = roundTripped.Fields[0];
        simple.ShouldBeOfType<FieldDefinition>();
        simple.SchemaName.ShouldBe("name");

        var complex = roundTripped.Fields[1].ShouldBeOfType<ComplexFieldDefinition>();
        complex.SchemaName.ShouldBe("profile");
        complex.Type.ShouldBe(FieldType.Object);
        complex.Fields.Count.ShouldBe(1);
        complex.Fields[0].SchemaName.ShouldBe("bio");

        roundTripped.Metadata.Tags.ShouldContain("identity");
    }

    [Test]
    public void DomainToDocument_ShouldMapReferences()
    {
        var relatedId = Guid.CreateVersion7();
        var domain = new EntityDefinition(fields: null, references:
        [
            new EntityReference
            {
                SchemaName = "customer",
                ReferenceType = EntityReferenceType.OneToMany,
                RelatedEntityDefinitionId = relatedId,
                RelatedEntitySchemaName = "customer"
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Order"
        };

        var doc = _mapper.Map<EntityDefinitionDocument>(domain);

        doc.References.Count.ShouldBe(1);
        doc.References[0].SchemaName.ShouldBe("customer");
        doc.References[0].ReferenceType.ShouldBe("OneToMany");
        doc.References[0].RelatedEntityDefinitionId.ShouldBe(relatedId);
        doc.References[0].RelatedEntitySchemaName.ShouldBe("customer");
    }

    [Test]
    public void DocumentToDomain_ShouldMapReferences()
    {
        var relatedId = Guid.CreateVersion7();
        var doc = new EntityDefinitionDocument
        {
            Id = Guid.CreateVersion7(),
            SchemaName = "lineItem",
            DisplayName = "Line Item",
            References =
            [
                new EntityReferenceDocument
                {
                    SchemaName = "order",
                    ReferenceType = "ManyToOne",
                    RelatedEntityDefinitionId = relatedId,
                    RelatedEntitySchemaName = "order"
                }
            ]
        };

        var domain = _mapper.Map<EntityDefinition>(doc);

        domain.References.Count.ShouldBe(1);
        domain.References[0].SchemaName.ShouldBe("order");
        domain.References[0].ReferenceType.ShouldBe(EntityReferenceType.ManyToOne);
        domain.References[0].RelatedEntityDefinitionId.ShouldBe(relatedId);
        domain.References[0].RelatedEntitySchemaName.ShouldBe("order");
    }

    #endregion
}
