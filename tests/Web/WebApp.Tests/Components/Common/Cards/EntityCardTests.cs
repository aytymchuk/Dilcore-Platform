using Dilcore.WebApp.Components.Common.Cards;
using MudBlazor.Services;
using Bunit;
using Shouldly;
using MudBlazor;

namespace Dilcore.WebApp.Tests.Components.Common.Cards;

public class EntityCardTests : Bunit.TestContext
{
    public EntityCardTests()
    {
        Services.AddMudServices();
    }

    [TestCase("Acme Corp", "AC")]
    [TestCase("Acme", "AC")]
    [TestCase("A", "A")]
    [TestCase("", "")]
    [TestCase("   ", "")]
    public void EntityCard_ShouldRenderInitials(string title, string expectedInitials)
    {
        // Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.Title, title)
        );

        // Assert
        if (string.IsNullOrEmpty(expectedInitials))
        {
             // If expected is empty, we check that it doesn't contain a specific class or check emptiness
             // But the component logic returns "", so we can check if it contains the text.
             // However, Markup.ShouldContain("") is always true.
             // We can check the element text content.
             var initialsEl = cut.FindAll(".rounded-lg").FirstOrDefault();
             initialsEl.ShouldNotBeNull();
             initialsEl.TextContent.Trim().ShouldBe(expectedInitials);
        }
        else
        {
             cut.Markup.ShouldContain(expectedInitials);
        }
    }

    [Test]
    public void EntityCard_ShouldRenderRole()
    {
        // Arrange
        var role = "Admin";

        // Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.Role, role)
        );

        // Assert
        cut.Markup.ShouldContain(role);
    }
    
    [Test]
    public void EntityCard_ShouldRenderTitleAndSubtitle_WhenProvided()
    {
        // Arrange
        var title = "Test Title";
        var subtitle = "Test Subtitle";

        // Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.Title, title)
            .Add(p => p.Subtitle, subtitle)
        );

        // Assert
        cut.Find("h6").TextContent.ShouldBe(title);
        cut.Find(".mud-typography-body2").TextContent.ShouldBe(subtitle);
    }

    [Test]
    public void EntityCard_ShouldRenderButton_WithCorrectText()
    {
        // Arrange
        var buttonText = "Click Me";

        // Act
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.ButtonText, buttonText)
        );

        // Assert
        cut.Find("button").TextContent.ShouldContain(buttonText);
    }
    
    [Test]
    public void EntityCard_ShouldTriggerOnClick_WhenButtonClicked()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<EntityCard>(parameters => parameters
            .Add(p => p.OnClick, () => clicked = true)
        );

        // Act
        cut.Find("button").Click();

        // Assert
        clicked.ShouldBeTrue();
    }
}
