using System.Diagnostics;
using Dilcore.MediatR.Abstractions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Dilcore.Tests.Common;

namespace Dilcore.MediatR.Extensions.Tests;

public class MediatRServiceCollectionExtensionsTests
{
    private ServiceProvider _serviceProvider;

    public class TestCommand : ICommand<string>
    {
        public string Input { get; set; } = string.Empty;
    }

    public class TestCommandHandler : ICommandHandler<TestCommand, string>
    {
        public Task<Result<string>> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            if (request.Input == "Throw")
            {
                throw new InvalidOperationException("Test Exception");
            }
            if (request.Input == "FailRequest")
            {
                return Task.FromResult(Result.Fail<string>("Simulated Error"));
            }
            return Task.FromResult(Result.Ok($"Processed: {request.Input}"));
        }
    }

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        var listLogger = new ListLogger();
        services.AddSingleton(listLogger);
        services.AddLogging(builder =>
        {
            builder.AddProvider(new ListLoggerProvider(listLogger));
        });

        services.AddMediatRInfrastructure(typeof(MediatRServiceCollectionExtensionsTests).Assembly);
        _serviceProvider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown()
    {
        _serviceProvider.Dispose();
    }

    [Test]
    public async Task AddMediatRInfrastructure_ShouldRegisterHandlersAndBehaviors()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Act
        var response = await mediator.Send(new TestCommand { Input = "Test" });

        // Assert
        response.IsSuccess.ShouldBeTrue();
        response.Value.ShouldBe("Processed: Test");
    }

    [Test]
    public async Task LoggingBehavior_ShouldLogRequestHandling()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var listLogger = _serviceProvider.GetRequiredService<ListLogger>();
        listLogger.Clear();

        // Act
        await mediator.Send(new TestCommand { Input = "Test" });

        // Assert
        listLogger.Logs.ShouldContain(l => l.Contains("Handling TestCommand"));
        listLogger.Logs.ShouldContain(l => l.Contains("Handled TestCommand"));
    }

    [Test]
    public async Task LoggingBehavior_ShouldLogFailure_OnResultFailed()
    {
        // Arrange
        var mediator = _serviceProvider.GetRequiredService<IMediator>();
        var listLogger = _serviceProvider.GetRequiredService<ListLogger>();
        listLogger.Clear();

        // Act
        await mediator.Send(new TestCommand { Input = "FailRequest" });

        // Assert
        listLogger.Logs.ShouldContain(l => l.Contains("Handled TestCommand with errors: Simulated Error"));
    }

    [Test]
    public async Task TracingBehavior_ShouldCreateActivity()
    {
        // Arrange
        var activities = new List<Activity>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = s => s.Name == "Application.Operations",
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activities.Add
        };
        ActivitySource.AddActivityListener(listener);

        var mediator = _serviceProvider.GetRequiredService<IMediator>();

        // Act
        await mediator.Send(new TestCommand { Input = "Test" });

        // Assert
        activities.ShouldNotBeEmpty();
        var activity = activities.Single();
        activity.OperationName.ShouldBe("MediatR: TestCommand");
        activity.GetTagItem("mediatr.request_name").ShouldBe("TestCommand");
    }
}
