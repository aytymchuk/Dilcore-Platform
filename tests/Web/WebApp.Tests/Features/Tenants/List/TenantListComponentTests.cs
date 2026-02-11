using Bunit;
using Dilcore.WebApp.Features.Tenants.List;
using Dilcore.WebApp.Models.Tenants;
using Dilcore.WebApp.Services;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MudBlazor;
using MudBlazor.Services;
using Shouldly;

namespace Dilcore.WebApp.Tests.Features.Tenants.List;

[TestFixture]
public class TenantListComponentTests
{
    private Bunit.TestContext _ctx = default!;
    private Mock<ISender> _mockMediator = default!;
    private Mock<IAppNavigator> _mockNavigator = default!;
    private Mock<IDialogService> _mockDialogService = default!;

    [SetUp]
    public void Setup()
    {
        _ctx = new Bunit.TestContext();
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();

        _mockMediator = new Mock<ISender>();
        _mockNavigator = new Mock<IAppNavigator>();
        _mockDialogService = new Mock<IDialogService>();

        _ctx.Services.AddSingleton(_mockMediator.Object);
        _ctx.Services.AddSingleton<IAppNavigator>(_mockNavigator.Object);
        _ctx.Services.AddSingleton(_mockDialogService.Object);

        _ctx.RenderComponent<MudPopoverProvider>();
    }

    [TearDown]
    public void TearDown()
    {
        _ctx?.Dispose();
    }

    [Test]
    public void RendersLoadingText_BeforeDataArrives()
    {
        // Arrange — mediator never completes
        var tcs = new TaskCompletionSource<Result<List<Tenant>>>();
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTenantListQuery>(), It.IsAny<CancellationToken>()))
            .Returns(tcs.Task);

        // Act
        var cut = _ctx.RenderComponent<TenantList>();

        // Assert
        cut.Find("div[data-testid='loading-tenants']").TextContent.ShouldContain("Loading tenants...");
    }

    [Test]
    public void RendersTenantCards_WhenQuerySucceeds()
    {
        // Arrange
        var tenants = CreateTestTenants();
        SetupSuccessfulQuery(tenants);

        // Act
        var cut = _ctx.RenderComponent<TenantList>();

        // Assert — one EntityCard per tenant + one CreateEntityCard
        var cards = cut.FindAll("div[data-testid='tenant-card']");
        cards.Count.ShouldBe(2);
        cards[0].TextContent.ShouldContain("Acme Corp");
        cards[1].TextContent.ShouldContain("Fin Consult");
        
        cut.FindAll("div[data-testid='loading-tenants']").ShouldBeEmpty();
    }

    [Test]
    public void RendersTenantDetails_ForEachCard()
    {
        // Arrange
        var tenants = CreateTestTenants();
        SetupSuccessfulQuery(tenants);

        // Act
        var cut = _ctx.RenderComponent<TenantList>();

        // Assert — Name, SystemName (subtitle), Description visible
        var cards = cut.FindAll("div[data-testid='tenant-card']");
        
        var card1 = cards[0];
        card1.TextContent.ShouldContain("Acme Corp");
        card1.TextContent.ShouldContain("acme-corp");
        card1.TextContent.ShouldContain("Enterprise solutions provider");

        var card2 = cards[1];
        card2.TextContent.ShouldContain("Fin Consult");
        card2.TextContent.ShouldContain("fin-consult");
        card2.TextContent.ShouldContain("Financial consulting services");
    }

    [Test]
    public void RendersCreateEntityCard_WhenQuerySucceeds()
    {
        // Arrange
        SetupSuccessfulQuery(CreateTestTenants());

        // Act
        var cut = _ctx.RenderComponent<TenantList>();

        // Assert — CreateEntityCard always renders its "Deploy New Tenant" text
        cut.Find("div[data-testid='create-tenant-card']").TextContent.ShouldContain("Deploy New Tenant");
    }

    [Test]
    public void NavigatesToWorkspace_WhenTenantCardClicked()
    {
        // Arrange
        var tenants = new List<Tenant>
        {
            new() { Id = Guid.NewGuid(), Name = "Solo Tenant", SystemName = "solo-tenant", Description = "Only tenant" }
        };
        SetupSuccessfulQuery(tenants);

        var cut = _ctx.RenderComponent<TenantList>();

        // Act — click the EntityCard button (EntityPrimaryButton renders as a MudButton)
        // Find the card, then find the button inside it
        var card = cut.Find("div[data-testid='tenant-card']");
        var selectButton = card.QuerySelector("button");
        selectButton.ShouldNotBeNull();
        selectButton.Click();

        // Assert
        _mockNavigator.Verify(n => n.ToTenantWorkspace("solo-tenant"), Times.Once);
    }

    [Test]
    public void RendersOnlyCreateCard_WhenQueryReturnsEmptyList()
    {
        // Arrange
        SetupSuccessfulQuery(new List<Tenant>());

        // Act
        var cut = _ctx.RenderComponent<TenantList>();

        // Assert — no tenant cards, but CreateEntityCard still rendered
        cut.FindAll("div[data-testid='loading-tenants']").ShouldBeEmpty();
        cut.Find("div[data-testid='create-tenant-card']").TextContent.ShouldContain("Deploy New Tenant");
        cut.FindAll("div[data-testid='tenant-card']").ShouldBeEmpty();
    }

    [Test]
    public void RendersEmptyState_WhenQueryFails()
    {
        // Arrange
        var failedResult = Result.Fail<List<Tenant>>("Something went wrong");
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTenantListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(failedResult);

        // Act
        var cut = _ctx.RenderComponent<TenantList>();

        // Assert — should show CreateEntityCard but no tenant cards
        cut.FindAll("div[data-testid='loading-tenants']").ShouldBeEmpty();
        cut.Find("div[data-testid='create-tenant-card']").TextContent.ShouldContain("Deploy New Tenant");
        cut.FindAll("div[data-testid='tenant-card']").ShouldBeEmpty();
    }

    [Test]
    public void RendersActiveLabel_OnActiveTenantCard()
    {
        // Arrange
        var tenants = CreateTestTenants();
        SetupSuccessfulQuery(tenants);

        // Make the first tenant active
        var activeTenant = tenants[0];
        var tenantState = new TenantState(activeTenant.SystemName!, activeTenant.Name!);

        // Act
        var cut = _ctx.RenderComponent<TenantList>(parameters => parameters
            .AddCascadingValue(tenantState)
        );

        // Assert
        var cards = cut.FindAll("div[data-testid='tenant-card']");
        
        // First card (Active)
        cards[0].TextContent.ShouldContain("Active");
        
        // Second card (Inactive)
        cards[1].TextContent.ShouldNotContain("Active");
    }

    private void SetupSuccessfulQuery(List<Tenant> tenants)
    {
        var result = Result.Ok(tenants);
        _mockMediator
            .Setup(m => m.Send(It.IsAny<GetTenantListQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);
    }

    private static List<Tenant> CreateTestTenants()
    {
        return
        [
            new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Acme Corp",
                SystemName = "acme-corp",
                Description = "Enterprise solutions provider"
            },
            new Tenant
            {
                Id = Guid.NewGuid(),
                Name = "Fin Consult",
                SystemName = "fin-consult",
                Description = "Financial consulting services"
            }
        ];
    }
}
