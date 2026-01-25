using Dilcore.WebApp.Helpers;

namespace Dilcore.WebApp.Tests.Helpers;

public class UrlHelperTests
{
    [Test]
    [TestCase(null, ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase("/", ExpectedResult = true)]
    [TestCase("/foo", ExpectedResult = true)]
    [TestCase("/foo/bar", ExpectedResult = true)]
    [TestCase("~/", ExpectedResult = true)]
    [TestCase("~/foo", ExpectedResult = true)]
    [TestCase("//", ExpectedResult = false)]
    [TestCase("/\\", ExpectedResult = false)]
    [TestCase("//foo", ExpectedResult = false)]
    [TestCase("/\\foo", ExpectedResult = false)]
    [TestCase("~//", ExpectedResult = false)]
    [TestCase("~/\\", ExpectedResult = false)]
    [TestCase("http://example.com", ExpectedResult = false)]
    [TestCase("https://example.com", ExpectedResult = false)]
    [TestCase("javascript:alert(1)", ExpectedResult = false)]
    [TestCase("file:///etc/passwd", ExpectedResult = false)]
    public bool IsLocalUrl_ReturnsExpectedResult(string? url)
    {
        return UrlHelper.IsLocalUrl(url);
    }
}
