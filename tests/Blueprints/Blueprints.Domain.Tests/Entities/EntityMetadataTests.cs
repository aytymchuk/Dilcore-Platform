using Dilcore.Blueprints.Domain.Entities;
using Shouldly;

namespace Dilcore.Blueprints.Domain.Tests.Entities;

public class EntityMetadataTests
{
    [Test]
    public void Tags_ShouldDefaultToEmptyList()
    {
        var metadata = new EntityMetadata();

        metadata.Tags.ShouldNotBeNull();
        metadata.Tags.ShouldBeEmpty();
    }

    [Test]
    public void Tags_ShouldRetainAssignedValues()
    {
        var metadata = new EntityMetadata
        {
            Tags = ["crm", "sales", "billing"]
        };

        metadata.Tags.Count.ShouldBe(3);
        metadata.Tags.ShouldContain("crm");
        metadata.Tags.ShouldContain("sales");
        metadata.Tags.ShouldContain("billing");
    }

    [Test]
    public void Should_SupportValueEquality_WhenSameTagsInstance()
    {
        IReadOnlyList<string> tags = ["crm", "sales"];
        var a = new EntityMetadata { Tags = tags };
        var b = new EntityMetadata { Tags = tags };

        a.ShouldBe(b);
    }
}
