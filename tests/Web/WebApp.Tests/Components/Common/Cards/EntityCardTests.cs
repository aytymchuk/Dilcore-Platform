using Dilcore.WebApp.Components.Common.Cards;
using MudBlazor.Services;
using Shouldly;

namespace Dilcore.WebApp.Tests.Components.Common.Cards;

public class EntityCardTests : Bunit.TestContext
{
    public EntityCardTests()
    {
        Services.AddMudServices();
    }
    
    [Test]
    public void GradientStart_ShouldFallbackToDefault_WhenInvalidColorProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.GradientStart, "invalid-color-value")
        );

        // Assert
        // Logic in OnParametersSet checks validity. If invalid, it reverts to default "#1e3a8a".
        cut.Instance.GradientStart.ShouldBe("#1e3a8a");
    }

    [Test]
    public void GradientStart_ShouldKeepValue_WhenValidHexColorProvided()
    {
        // Arrange
        var validColor = "#ff0000";

        // Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.GradientStart, validColor)
        );

        // Assert
        cut.Instance.GradientStart.ShouldBe(validColor);
    }

    [Test]
    public void GradientStart_ShouldKeepValue_WhenValidRgbColorProvided()
    {
        // Arrange
        var validColor = "rgb(255, 0, 0)";

        // Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.GradientStart, validColor)
        );

        // Assert
        cut.Instance.GradientStart.ShouldBe(validColor);
    }

    [Test]
    public void GradientEnd_ShouldFallbackToDefault_WhenInvalidColorProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.GradientEnd, "url('http://malicious.com')")
        );

        // Assert
        // Default is "#0f172a"
        cut.Instance.GradientEnd.ShouldBe("#0f172a");
    }

    [Test]
    public void LabelColors_ShouldBeNull_WhenInvalidColorProvided()
    {
        // Arrange & Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.Label, "Test Label")
            .Add(p => p.LabelBackgroundColor, "javascript:alert(1)")
            .Add(p => p.LabelTextColor, "invalid-color")
            .Add(p => p.LabelBorderColor, "  ")
        );

        // Assert
        cut.Instance.LabelBackgroundColor.ShouldBeNull();
        cut.Instance.LabelTextColor.ShouldBeNull();
        cut.Instance.LabelBorderColor.ShouldBeNull();
    }

    [TestCase("#abc")]
    [TestCase("#AABBCC")]
    [TestCase("rgb(0,0,0)")]
    [TestCase("rgba(0, 0, 0, 0.5)")]
    [TestCase("hsl(0, 100%, 50%)")]
    [TestCase("hsla(0, 100%, 50%, 0.5)")]
    [TestCase("red")]
    [TestCase("blueviolet")]
    public void IsValidCssColor_ShouldAcceptValidColors(string validColor)
    {
         // Arrange & Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.GradientStart, validColor)
        );

        // Assert
        cut.Instance.GradientStart.ShouldBe(validColor);
    }
}
