using Dilcore.WebApp.Services.Agent;
using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Agent;

[TestFixture]
public class AgentReasoningPayloadTests
{
    [Test]
    public void Parse_Should_Return_Structured_Narrative_And_NextRoute()
    {
        const string json = """
            {
              "reasoning": "The user confirmed the design.",
              "decision": { "next_route": "generate" }
            }
            """;

        var r = AgentReasoningPayload.Parse(json);

        r.IsStructured.ShouldBeTrue();
        r.Narrative.ShouldBe("The user confirmed the design.");
        r.NextRoute.ShouldBe("generate");
    }

    [Test]
    public void Parse_Should_Return_Structured_When_Only_NextRoute()
    {
        const string json = """{"decision":{"next_route":"design"}}""";

        var r = AgentReasoningPayload.Parse(json);

        r.IsStructured.ShouldBeTrue();
        r.Narrative.ShouldBeNull();
        r.NextRoute.ShouldBe("design");
    }

    [Test]
    public void Parse_Should_Fallback_When_Incomplete_Json()
    {
        const string partial = """{"reasoning":"still typing""";

        var r = AgentReasoningPayload.Parse(partial);

        r.IsStructured.ShouldBeFalse();
        r.RawText.ShouldBe(partial);
    }

    [Test]
    public void Parse_Should_Fallback_When_Not_Json()
    {
        const string plain = "Plain reasoning line";

        var r = AgentReasoningPayload.Parse(plain);

        r.IsStructured.ShouldBeFalse();
        r.RawText.ShouldBe(plain);
    }
}
