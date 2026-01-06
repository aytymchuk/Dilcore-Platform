using System.Diagnostics;
using Dilcore.MediatR.Abstractions;
using Dilcore.MediatR.Extensions;
using FluentResults;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Shouldly;

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
            ShouldListenTo = s => s.Name == "Dilcore.MediatR",
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

public class ListLogger : ILogger
{
    public List<string> Logs { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Logs.Add(formatter(state, exception));
    }

    public void Clear() => Logs.Clear();
}

public class ListLoggerProvider : ILoggerProvider
{
    private readonly ListLogger _logger;

    public ListLoggerProvider(ListLogger logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose() { }
}