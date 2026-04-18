using Dilcore.WebApp.Services.Agent;

using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Agent;

public class MarkdownRendererTests
{
    private readonly MarkdownRenderer _sut = new();

    [Test]
    public void ToHtml_Should_Render_Emphasis_And_Code()
    {
        var html = _sut.ToHtml("This is **bold** and `code`.").Value;

        html.ShouldContain("<strong>bold</strong>");
        html.ShouldContain("<code>code</code>");
    }

    [Test]
    public void ToHtml_Should_Not_Pass_Through_Raw_Html()
    {
        var html = _sut.ToHtml("<script>alert(1)</script>").Value;

        html.ShouldNotContain("<script>");
        html.ShouldContain("&lt;script&gt;");
    }

    [Test]
    public void ToHtml_Should_Return_Empty_For_Null_Or_Empty()
    {
        _sut.ToHtml(null).Value.ShouldBe(string.Empty);
        _sut.ToHtml("").Value.ShouldBe(string.Empty);
    }
}
