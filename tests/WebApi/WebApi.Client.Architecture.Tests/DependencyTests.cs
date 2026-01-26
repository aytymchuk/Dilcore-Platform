using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using Dilcore.WebApi.Client.Clients;
using Shouldly;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Dilcore.WebApi.Client.Architecture.Tests;

[TestFixture]
public class DependencyTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture = new ArchLoader()
        .LoadAssemblies(typeof(IIdentityClient).Assembly)
        .Build();

    [Test]
    public void ClientProject_ShouldNotDependOnWebApiProject()
    {
        // Arrange & Act & Assert
        Types()
            .That().ResideInAssembly(typeof(IIdentityClient).Assembly)
            .Should().NotDependOnAny(Types()
                .That().ResideInNamespace("Dilcore.WebApi"))
            .Because("the client library should not depend on the WebApi project")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void ClientProject_ShouldNotDependOnInfrastructureProjects()
    {
        // Arrange & Act & Assert
        Types()
            .That().ResideInAssembly(typeof(IIdentityClient).Assembly)
            .Should().NotDependOnAny(Types()
                .That().ResideInNamespace("Dilcore.Identity.Infrastructure")
                .Or().ResideInNamespace("Dilcore.Tenancy.Infrastructure"))
            .Because("the client library should not depend on infrastructure projects")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void ClientProject_ShouldOnlyDependOnContractsProjects()
    {
        // Arrange & Act & Assert
        Types()
            .That().ResideInAssembly(typeof(IIdentityClient).Assembly)
            .Should().NotDependOnAny(Types()
                .That().ResideInNamespace("Dilcore.Identity.Core")
                .Or().ResideInNamespace("Dilcore.Identity.Domain")
                .Or().ResideInNamespace("Dilcore.Identity.Actors")
                .Or().ResideInNamespace("Dilcore.Tenancy.Core")
                .Or().ResideInNamespace("Dilcore.Tenancy.Domain")
                .Or().ResideInNamespace("Dilcore.Tenancy.Actors"))
            .Because("the client library should only depend on *.Contracts projects, not Core, Domain, or Actors")
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void ClientProject_ShouldOnlyHavePublicInterfaces()
    {
        // Arrange - Verify that main API interfaces are public
        var identityClientType = typeof(IIdentityClient);
        var tenancyClientType = typeof(ITenancyClient);

        // Assert
        identityClientType.IsPublic.ShouldBeTrue("IIdentityClient should be public");
        tenancyClientType.IsPublic.ShouldBeTrue("ITenancyClient should be public");
    }

    [Test]
    public void Clients_Should_Only_Expose_Interfaces_Publicly()
    {
        Types().That().ResideInNamespace("Dilcore.WebApi.Client.Clients")
            .Should().BePublic()
            .AndShould().Be(Interfaces())
            .Check(Architecture);
    }

    [Test]
    public void Internal_Namespaces_Should_Not_Be_Public()
    {
        Types().That().ResideInNamespace("Dilcore.WebApi.Client.Errors")
            .Or().ResideInNamespace("Dilcore.WebApi.Client.Extensions")
            .Should().NotBePublic()
            .Check(Architecture);
    }
}
