using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using Assembly = System.Reflection.Assembly;

using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Dilcore.Tenancy.Architecture.Tests;

public class ArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(
            Assembly.Load("Tenancy.Core"),
            Assembly.Load("Tenancy.Store"),
            Assembly.Load("Tenancy.Infrastructure"),
            Assembly.Load("Tenancy.WebApi"),
            Assembly.Load("Tenancy.Domain"),
            Assembly.Load("Tenancy.Actors"),
            Assembly.Load("Tenancy.Actors.Abstractions")
        ).Build();

    private readonly IObjectProvider<IType> _domainLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.Domain");

    private readonly IObjectProvider<IType> _coreLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.Core");

    private readonly IObjectProvider<IType> _storeLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.Store");

    private readonly IObjectProvider<IType> _infrastructureLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.Infrastructure");

    private readonly IObjectProvider<IType> _actorsLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.Actors");

    private readonly IObjectProvider<IType> _actorsAbstractionsLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.Actors.Abstractions");

    private readonly IObjectProvider<IType> _webApiLayer =
        Types().That().ResideInNamespace("Dilcore.Tenancy.WebApi");

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
        var tenancyTypes = Types().That().ResideInNamespace("Dilcore.Tenancy.*");

        // Identity Layers
        var identityCore = Types().That().ResideInNamespace("Dilcore.Identity.Core");
        var identityStore = Types().That().ResideInNamespace("Dilcore.Identity.Store");
        var identityInfra = Types().That().ResideInNamespace("Dilcore.Identity.Infrastructure");
        var identityWebApi = Types().That().ResideInNamespace("Dilcore.Identity.WebApi");
        var identityDomain = Types().That().ResideInNamespace("Dilcore.Identity.Domain");
        var identityActors = Types().That().ResideInNamespace("Dilcore.Identity.Actors");
        // Identity.Actors.Abstractions is ALLOWED

        tenancyTypes
            .Should().NotDependOnAny(identityCore)
            .AndShould().NotDependOnAny(identityStore)
            .AndShould().NotDependOnAny(identityInfra)
            .AndShould().NotDependOnAny(identityWebApi)
            .AndShould().NotDependOnAny(identityDomain)
            .AndShould().NotDependOnAny(identityActors)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
