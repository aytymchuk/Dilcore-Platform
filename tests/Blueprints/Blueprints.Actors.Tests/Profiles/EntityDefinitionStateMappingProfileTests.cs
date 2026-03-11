using AutoMapper;
using Dilcore.Blueprints.Actors.Abstractions;
using Dilcore.Blueprints.Actors.Profiles;
using Dilcore.Blueprints.Domain.Entities;
using Dilcore.Blueprints.Domain.Entities.Fields;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace Dilcore.Blueprints.Actors.Tests.Profiles;

[TestFixture]
public class EntityDefinitionStateMappingProfileTests
{
    private IMapper _mapper = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var loggerFactory = new LoggerFactory([NullLoggerProvider.Instance]);
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile<EntityDefinitionStateMappingProfile>(), loggerFactory);

        config.AssertConfigurationIsValid();
        _mapper = config.CreateMapper();
    }

    #region State → Domain

    [Test]
    public void StateToDomain_ShouldMapScalarProperties()
    {
        var id = Guid.CreateVersion7();
        var extendsId = Guid.CreateVersion7();
        var now = DateTime.UtcNow;
        var state = new EntityDefinitionState
        {
            Id = id,
            ETag = 42,
            SchemaName = "customer",
            DisplayName = "Customer",
            Description = "Customer entity",
            IsAbstract = true,
            ExtendsEntityId = extendsId,
            Fields = [],
            Tags = ["crm"],
            CreatedAt = now,
            UpdatedAt = now,
            IsCreated = true
        };

        var domain = _mapper.Map<EntityDefinition>(state);

        domain.Id.ShouldBe(id);
        domain.ETag.ShouldBe(42);
        domain.SchemaName.ShouldBe("customer");
        domain.DisplayName.ShouldBe("Customer");
        domain.Description.ShouldBe("Customer entity");
        domain.IsAbstract.ShouldBeTrue();
        domain.ExtendsEntityId.ShouldBe(extendsId);
        domain.CreatedAt.ShouldBe(now);
        domain.UpdatedAt.ShouldBe(now);
        domain.Metadata.Tags.ShouldContain("crm");
    }

    [Test]
    public void StateToDomain_ShouldMapSimpleFields()
    {
        var state = new EntityDefinitionState
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Test",
            Fields =
            [
                new FieldDefinitionGrainDto
                {
                    SchemaName = "email",
                    DisplayName = "Email",
                    Type = "String"
                }
            ],
            IsCreated = true
        };

        var domain = _mapper.Map<EntityDefinition>(state);

        domain.Fields.Count.ShouldBe(1);
        var field = domain.Fields[0];
        field.ShouldBeOfType<FieldDefinition>();
        field.ShouldNotBeOfType<ComplexFieldDefinition>();
        field.SchemaName.ShouldBe("email");
        field.DisplayName.ShouldBe("Email");
        field.Type.ShouldBe(FieldType.String);
    }

    [Test]
    public void StateToDomain_ShouldMapComplexFields()
    {
        var state = new EntityDefinitionState
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Test",
            Fields =
            [
                new FieldDefinitionGrainDto
                {
                    SchemaName = "address",
                    DisplayName = "Address",
                    Type = "Object",
                    Fields =
                    [
                        new FieldDefinitionGrainDto
                        {
                            SchemaName = "city",
                            DisplayName = "City",
                            Type = "String"
                        }
                    ]
                }
            ],
            IsCreated = true
        };

        var domain = _mapper.Map<EntityDefinition>(state);

        domain.Fields.Count.ShouldBe(1);
        var complex = domain.Fields[0].ShouldBeOfType<ComplexFieldDefinition>();
        complex.SchemaName.ShouldBe("address");
        complex.Type.ShouldBe(FieldType.Object);
        complex.Fields.Count.ShouldBe(1);
        complex.Fields[0].SchemaName.ShouldBe("city");
        complex.Fields[0].Type.ShouldBe(FieldType.String);
    }

    #endregion

    #region Domain → State

    [Test]
    public void DomainToState_ShouldMapScalarProperties()
    {
        var id = Guid.CreateVersion7();
        var extendsId = Guid.CreateVersion7();
        var now = DateTime.UtcNow;
        var domain = new EntityDefinition(fields: null)
        {
            Id = id,
            ETag = 7,
            SchemaName = "order",
            DisplayName = "Order",
            Description = "Order entity",
            IsAbstract = false,
            ExtendsEntityId = extendsId,
            CreatedAt = now,
            UpdatedAt = now,
            Metadata = new EntityMetadata { Tags = ["sales", "core"] }
        };

        var state = _mapper.Map<EntityDefinitionState>(domain);

        state.Id.ShouldBe(id);
        state.ETag.ShouldBe(7);
        state.SchemaName.ShouldBe("order");
        state.DisplayName.ShouldBe("Order");
        state.Description.ShouldBe("Order entity");
        state.IsAbstract.ShouldBeFalse();
        state.ExtendsEntityId.ShouldBe(extendsId);
        state.CreatedAt.ShouldBe(now);
        state.UpdatedAt.ShouldBe(now);
        state.Tags.Count.ShouldBe(2);
        state.Tags.ShouldContain("sales");
        state.Tags.ShouldContain("core");
        state.IsCreated.ShouldBeTrue();
    }

    [Test]
    public void DomainToState_ShouldMapSimpleFields()
    {
        var domain = new EntityDefinition(
        [
            new FieldDefinition
            {
                SchemaName = "age",
                DisplayName = "Age",
                Type = FieldType.Number
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Person"
        };

        var state = _mapper.Map<EntityDefinitionState>(domain);

        state.Fields.Count.ShouldBe(1);
        state.Fields[0].SchemaName.ShouldBe("age");
        state.Fields[0].Type.ShouldBe("Number");
        state.Fields[0].Fields.ShouldBeNull();
    }

    [Test]
    public void DomainToState_ShouldMapComplexFields()
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
                    new FieldDefinition
                    {
                        SchemaName = "sku",
                        DisplayName = "SKU",
                        Type = FieldType.String
                    }
                ]
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Order"
        };

        var state = _mapper.Map<EntityDefinitionState>(domain);

        state.Fields.Count.ShouldBe(1);
        var dto = state.Fields[0];
        dto.SchemaName.ShouldBe("items");
        dto.Type.ShouldBe("Array");
        dto.Fields.ShouldNotBeNull();
        dto.Fields!.Length.ShouldBe(1);
        dto.Fields[0].SchemaName.ShouldBe("sku");
        dto.Fields[0].Type.ShouldBe("String");
    }

    #endregion

    #region Round-trip

    [Test]
    public void RoundTrip_StateToDomainAndBack_ShouldPreserveData()
    {
        var original = new EntityDefinitionState
        {
            Id = Guid.CreateVersion7(),
            ETag = 5,
            SchemaName = "account",
            DisplayName = "Account",
            Description = "An account",
            IsAbstract = false,
            ExtendsEntityId = null,
            Fields =
            [
                new FieldDefinitionGrainDto
                {
                    SchemaName = "profile",
                    DisplayName = "Profile",
                    Type = "Object",
                    Fields =
                    [
                        new FieldDefinitionGrainDto
                        {
                            SchemaName = "bio",
                            DisplayName = "Bio",
                            Type = "String"
                        }
                    ]
                }
            ],
            Tags = ["account"],
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsCreated = true
        };

        var domain = _mapper.Map<EntityDefinition>(original);
        var roundTripped = _mapper.Map<EntityDefinitionState>(domain);

        roundTripped.Id.ShouldBe(original.Id);
        roundTripped.ETag.ShouldBe(original.ETag);
        roundTripped.SchemaName.ShouldBe(original.SchemaName);
        roundTripped.DisplayName.ShouldBe(original.DisplayName);
        roundTripped.Description.ShouldBe(original.Description);
        roundTripped.Fields.Count.ShouldBe(1);
        roundTripped.Fields[0].SchemaName.ShouldBe("profile");
        roundTripped.Fields[0].Fields.ShouldNotBeNull();
        roundTripped.Fields[0].Fields!.Length.ShouldBe(1);
        roundTripped.Fields[0].Fields![0].SchemaName.ShouldBe("bio");
        roundTripped.Tags.ShouldContain("account");
        roundTripped.IsCreated.ShouldBeTrue();
    }

    [Test]
    public void StateToDomain_ShouldMapReferences()
    {
        var relatedId = Guid.CreateVersion7();
        var state = new EntityDefinitionState
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "Order",
            References =
            [
                new EntityReferenceGrainDto
                {
                    SchemaName = "customer",
                    ReferenceType = "OneToMany",
                    RelatedEntityDefinitionId = relatedId,
                    RelatedEntitySchemaName = "customer"
                }
            ],
            IsCreated = true
        };

        var domain = _mapper.Map<EntityDefinition>(state);

        domain.References.Count.ShouldBe(1);
        domain.References[0].SchemaName.ShouldBe("customer");
        domain.References[0].ReferenceType.ShouldBe(EntityReferenceType.OneToMany);
        domain.References[0].RelatedEntityDefinitionId.ShouldBe(relatedId);
        domain.References[0].RelatedEntitySchemaName.ShouldBe("customer");
    }

    [Test]
    public void DomainToState_ShouldMapReferences()
    {
        var relatedId = Guid.CreateVersion7();
        var domain = new EntityDefinition(fields: null, references:
        [
            new EntityReference
            {
                SchemaName = "order",
                ReferenceType = EntityReferenceType.ManyToOne,
                RelatedEntityDefinitionId = relatedId,
                RelatedEntitySchemaName = "order"
            }
        ])
        {
            Id = Guid.CreateVersion7(),
            DisplayName = "LineItem"
        };

        var state = _mapper.Map<EntityDefinitionState>(domain);

        state.References.Count.ShouldBe(1);
        state.References[0].SchemaName.ShouldBe("order");
        state.References[0].ReferenceType.ShouldBe("ManyToOne");
        state.References[0].RelatedEntityDefinitionId.ShouldBe(relatedId);
        state.References[0].RelatedEntitySchemaName.ShouldBe("order");
    }

    #endregion
}
