using System.Diagnostics;
using Dilcore.MediatR.Abstractions;
using Dilcore.MediatR.Extensions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Shouldly;

namespace MediatR.Extensions.Tests;

[TestFixture]
public class TracingBehaviorTests
{
    private Mock<IHostEnvironment> _environmentMock;
    private ActivitySource _activitySource;
    private List<Activity> _activities;
    private ActivityListener _activityListener;

    [SetUp]
    public void SetUp()
    {
        _environmentMock = new Mock<IHostEnvironment>();
        _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");

        _activities = new List<Activity>();
        _activitySource = new ActivitySource("Application.Operations"); // Must match the one in TracingBehavior

        _activityListener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => _activities.Add(activity)
        };

        ActivitySource.AddActivityListener(_activityListener);
    }

    [TearDown]
    public void TearDown()
    {
        _activityListener.Dispose();
        _activitySource.Dispose();
        _activities.Clear();
    }

    [Test]
    public async Task Handle_ShouldCreateCommandActivity_WhenRequestIsCommand()
    {
        // Arrange
        var behavior = new TracingBehavior<TestCommand, Result>(_environmentMock.Object);
        var request = new TestCommand();
        RequestHandlerDelegate<Result> next = (_) => Task.FromResult(Result.Ok());

        // Act
        await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var activity = _activities.Single();
        activity.DisplayName.ShouldBe("Command: TestCommand");
        activity.Kind.ShouldBe(ActivityKind.Internal);
        activity.GetTagItem("db.system").ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldCreateQueryActivity_WithJsonPayload_WhenRequestIsQuery()
    {
        // Arrange
        var behavior = new TracingBehavior<TestQuery, Result<string>>(_environmentMock.Object);
        var request = new TestQuery { Id = 123, SearchTerm = "test" };
        RequestHandlerDelegate<Result<string>> next = (_) => Task.FromResult(Result.Ok("success"));

        // Act
        await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var activity = _activities.Single();
        activity.DisplayName.ShouldBe("Query: TestQuery");
        activity.Kind.ShouldBe(ActivityKind.Client);
        activity.GetTagItem("db.system").ShouldBe("mediatr");

        var json = activity.GetTagItem("db.statement") as string;
        json.ShouldNotBeNull();
        json.ShouldContain("\"Id\":123");
        json.ShouldContain("\"SearchTerm\":\"test\"");
    }

    [Test]
    public async Task Handle_ShouldNotLogPayload_WhenEnvironmentIsProduction()
    {
        // Arrange
        _environmentMock.Setup(x => x.EnvironmentName).Returns("Production");
        var behavior = new TracingBehavior<TestQuery, Result<string>>(_environmentMock.Object);
        var request = new TestQuery { Id = 123, SearchTerm = "test" };
        RequestHandlerDelegate<Result<string>> next = (_) => Task.FromResult(Result.Ok("success"));

        // Act
        await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var activity = _activities.Single();
        activity.GetTagItem("db.statement").ShouldBeNull();
    }

    [Test]
    public async Task Handle_ShouldCreateGenericActivity_WhenRequestIsGeneralRequest()
    {
        // Arrange
        var behavior = new TracingBehavior<TestRequest, Unit>(_environmentMock.Object);
        var request = new TestRequest();
        RequestHandlerDelegate<Unit> next = (_) => Task.FromResult(Unit.Value);

        // Act
        await behavior.Handle(request, next, CancellationToken.None);

        // Assert
        var activity = _activities.Single();
        activity.DisplayName.ShouldBe("MediatR: TestRequest");
        activity.Kind.ShouldBe(ActivityKind.Internal);
    }

    // Test helper classes
    public class TestCommand : ICommand
    {
    }

    // Checking if it handles generic IQuery<T>
    public class TestQuery : IQuery<string>
    {
        public int Id { get; set; }
        public string? SearchTerm { get; set; }
    }

    public class TestRequest : IRequest<Unit>
    {
    }
}