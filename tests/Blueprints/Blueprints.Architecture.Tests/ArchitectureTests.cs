using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.NUnit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Assembly = System.Reflection.Assembly;

namespace Dilcore.Blueprints.Architecture.Tests;

public class ArchitectureTests
{
    private static readonly ArchUnitNET.Domain.Architecture Architecture =
        new ArchLoader().LoadAssemblies(
            Assembly.Load("Blueprints.Core"),
            Assembly.Load("Blueprints.Store"),
            Assembly.Load("Blueprints.Infrastructure"),
            Assembly.Load("Blueprints.WebApi"),
            Assembly.Load("Blueprints.Domain"),
            Assembly.Load("Blueprints.Actors"),
            Assembly.Load("Blueprints.Actors.Abstractions")
        ).Build();

    private readonly IObjectProvider<IType> _domainLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.Domain");

    private readonly IObjectProvider<IType> _coreLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.Core");

    private readonly IObjectProvider<IType> _storeLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.Store");

    private readonly IObjectProvider<IType> _infrastructureLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.Infrastructure");

    private readonly IObjectProvider<IType> _actorsLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.Actors");

    private readonly IObjectProvider<IType> _actorsAbstractionsLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.Actors.Abstractions");

    private readonly IObjectProvider<IType> _webApiLayer =
        Types().That().ResideInNamespace("Dilcore.Blueprints.WebApi");

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
        Types().That().Are(_actorsLayer)
            .Should().NotDependOnAny(_infrastructureLayer)
            .AndShould().NotDependOnAny(_webApiLayer)
            .WithoutRequiringPositiveResults()
            .Check(Architecture);
    }
}
