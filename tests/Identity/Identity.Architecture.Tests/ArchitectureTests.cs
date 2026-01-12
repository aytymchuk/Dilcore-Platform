using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace Dilcore.Identity.Architecture.Tests;

public class ArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(
            Assembly.Load("Identity.Core"),
            Assembly.Load("Identity.Store"),
            Assembly.Load("Identity.Infrastructure"),
            Assembly.Load("Identity.WebApi"),
            Assembly.Load("Identity.Domain"),
            Assembly.Load("Identity.Actors"),
            Assembly.Load("Identity.Actors.Abstractions")
        ).Build();

    private readonly IObjectProvider<IType> _domainLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.Domain");

    private readonly IObjectProvider<IType> _coreLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.Core");

    private readonly IObjectProvider<IType> _storeLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.Store");

    private readonly IObjectProvider<IType> _infrastructureLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.Infrastructure");

    private readonly IObjectProvider<IType> _actorsLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.Actors");

    private readonly IObjectProvider<IType> _actorsAbstractionsLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.Actors.Abstractions");

    private readonly IObjectProvider<IType> _webApiLayer =
        Types().That().ResideInNamespace("Dilcore.Identity.WebApi");

    [Test]
    public void Domain_Should_Not_Depend_On_Any_Other_Layer()
    {
        Types().That().Are(_domainLayer)
            .Should().NotDependOnAny(_coreLayer)
            .AndShould().NotDependOnAny(_storeLayer)
            .AndShould().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_actorsLayer)
            .AndShould().NotDependOnAny(_actorsAbstractionsLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void Core_Should_Only_Depend_On_Domain_And_ActorsAbstractions()
    {
        // Core -> Domain, Actors.Abstractions
        // Forbidden: Store, Infra, Actors, WebApi
        Types().That().Are(_coreLayer)
            .Should().NotDependOnAny(_storeLayer)
            .AndShould().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_actorsLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void Store_Should_Only_Depend_On_Core_And_Domain()
    {
        // Store -> Core (and transitively Domain)
        // Forbidden: Infra, Actors, Actors.Abstractions, WebApi
        Types().That().Are(_storeLayer)
            .Should().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_actorsLayer)
            .AndShould().NotDependOnAny(_actorsAbstractionsLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void Infrastructure_Should_Only_Depend_On_Core_And_Domain()
    {
        // Infra -> Core (and transitively Domain)
        // Forbidden: Store, Actors, Actors.Abstractions, WebApi
        Types().That().Are(_infrastructureLayer)
            .Should().NotDependOnAny(_storeLayer)
            .AndShould().NotDependOnAny(_actorsLayer)
            .AndShould().NotDependOnAny(_actorsAbstractionsLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void ActorsAbstractions_Should_Only_Depend_On_Domain()
    {
        // Actors.Abstractions -> Domain
        // Forbidden: Core, Store, Infra, Actors, WebApi
        Types().That().Are(_actorsAbstractionsLayer)
            .Should().NotDependOnAny(_coreLayer)
            .AndShould().NotDependOnAny(_storeLayer)
            .AndShould().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_actorsLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void Actors_Should_Only_Depend_On_Store_And_Abstractions_And_Transitive()
    {
        // Actors -> Store, Actors.Abstractions
        // Forbidden: Infra, WebApi
        Types().That().Are(_actorsLayer)
            .Should().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }

    [Test]
    public void No_Cross_Domain_Dependencies_Except_ActorsAbstractions()
    {
        var identityTypes = Types().That().ResideInNamespace("Dilcore.Identity.*");

        // Tenancy Layers
        var tenancyCore = Types().That().ResideInNamespace("Dilcore.Tenancy.Core");
        var tenancyStore = Types().That().ResideInNamespace("Dilcore.Tenancy.Store");
        var tenancyInfra = Types().That().ResideInNamespace("Dilcore.Tenancy.Infrastructure");
        var tenancyWebApi = Types().That().ResideInNamespace("Dilcore.Tenancy.WebApi");
        var tenancyDomain = Types().That().ResideInNamespace("Dilcore.Tenancy.Domain");
        var tenancyActors = Types().That().ResideInNamespace("Dilcore.Tenancy.Actors");
        // Tenancy.Actors.Abstractions is ALLOWED

        identityTypes
            .Should().NotDependOnAny(tenancyCore)
            .AndShould().NotDependOnAny(tenancyStore)
            .AndShould().NotDependOnAny(tenancyInfra)
            .AndShould().NotDependOnAny(tenancyWebApi)
            .AndShould().NotDependOnAny(tenancyDomain)
            .AndShould().NotDependOnAny(tenancyActors)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
