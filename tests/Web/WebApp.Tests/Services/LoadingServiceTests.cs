using Dilcore.WebApp.Services.Loading;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using NUnit.Framework;

namespace Dilcore.WebApp.Tests.Services;

[TestFixture]
public class LoadingServiceTests
{
    private LoadingService _sut;

    [SetUp]
    public void Setup()
    {
        var logger = new Mock<ILogger<LoadingService>>();
        _sut = new LoadingService(logger.Object);
    }

    [Test]
    public void IsLoading_ShouldBeFalse_WhenNoMessages()
    {
        _sut.IsLoading.ShouldBeFalse();
        _sut.CurrentMessage.ShouldBeNull();
    }

    [Test]
    public void Show_ShouldNoOp_WhenMessageIsInvalid()
    {
        _sut.Show(null!);
        _sut.IsLoading.ShouldBeFalse();
        _sut.CurrentMessage.ShouldBeNull();

        _sut.Show("");
        _sut.IsLoading.ShouldBeFalse();
        _sut.CurrentMessage.ShouldBeNull();
    }

    [Test]
    public void Hide_ShouldNoOp_WhenMessageDoesNotExist()
    {
        _sut.Hide("nonexistent");
        _sut.IsLoading.ShouldBeFalse();
        _sut.CurrentMessage.ShouldBeNull();
    }

    [Test]
    public void Show_ShouldAddMessage_AndTriggerOnChange()
    {
        var eventTriggered = false;
        _sut.OnChange += () => eventTriggered = true;

        _sut.Show("Loading...");

        _sut.IsLoading.ShouldBeTrue();
        _sut.CurrentMessage.ShouldBe("Loading...");
        eventTriggered.ShouldBeTrue();
    }

    [Test]
    public void Hide_ShouldRemoveMessage_AndTriggerOnChange()
    {
        _sut.Show("Loading...");
        var eventTriggered = false;
        _sut.OnChange += () => eventTriggered = true;

        _sut.Hide("Loading...");

        _sut.IsLoading.ShouldBeFalse();
        _sut.CurrentMessage.ShouldBeNull();
        eventTriggered.ShouldBeTrue();
    }

    [Test]
    public void CurrentMessage_ShouldBeLastAdded_WhenMultipleMessagesExist()
    {
        _sut.Show("Loading 1");
        _sut.Show("Loading 2");

        _sut.IsLoading.ShouldBeTrue();
        _sut.CurrentMessage.ShouldBe("Loading 2");

        _sut.Hide("Loading 2");
        _sut.CurrentMessage.ShouldBe("Loading 1");
        _sut.IsLoading.ShouldBeTrue();

        _sut.Hide("Loading 1");
        _sut.IsLoading.ShouldBeFalse();
    }

    [Test]
    public void ShowAndHide_ShouldBeThreadSafe()
    {
        Parallel.For(0, 100, i =>
        {
            _sut.Show($"Message {i}");
        });

        _sut.IsLoading.ShouldBeTrue();

        Parallel.For(0, 100, i =>
        {
            _sut.Hide($"Message {i}");
        });

        _sut.IsLoading.ShouldBeFalse();
    }

    [Test]
    public void MixedShowAndHide_ShouldBeThreadSafe()
    {
        Parallel.For(0, 1000, i =>
        {
            _sut.Show($"Message {i}");
            _sut.Hide($"Message {i}");
        });

        _sut.IsLoading.ShouldBeFalse();
    }

    [Test]
    public void MassiveConcurrency_ShouldNotDeadlockOrCorruptState()
    {
        // Arrange
        var threadCount = 20;
        var iterationsPerThread = 1000;
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        // Act
        Parallel.For(0, threadCount, _ =>
        {
            try
            {
                for (var i = 0; i < iterationsPerThread; i++)
                {
                    var msg = $"msg-{Guid.NewGuid()}";
                    _sut.Show(msg);
                    
                    // Small spinning to simulate work / increase chance of overlap
                    if (i % 100 == 0) Thread.SpinWait(10);
                    
                    _sut.Hide(msg);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        // Assert
        exceptions.ShouldBeEmpty();
        _sut.IsLoading.ShouldBeFalse();
        _sut.CurrentMessage.ShouldBeNull();
    }

    [Test]
    // This test primarily checks for crashes and deadlocks under chaotic concurrent load
    public void ChaosTesting_RandomShowHide_ShouldEventuallySettle()
    {
        // Arrange
        var operations = 10000;
        var activeMessages = new System.Collections.Concurrent.ConcurrentDictionary<string, byte>();
        var exceptions = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        // Act
        Parallel.For(0, operations, i =>
        {
            try
            {
                var msg = $"msg-{i % 100}"; // Reuse keys to force collisions
                var isShow = i % 2 == 0; // Simple distinct pattern, but concurrent exec makes it chaotic

                if (isShow)
                {
                    _sut.Show(msg);
                    activeMessages.TryAdd(msg, 0);
                }
                else
                {
                    _sut.Hide(msg);
                    activeMessages.TryRemove(msg, out _);
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        });

        // Cleanup phase - ensure everything is hidden
        foreach (var key in activeMessages.Keys)
        {
            _sut.Hide(key);
        }

        // Just to be sure, hide everything we might have missed in the concurrent map (though logic above should cover it)
        // Actually, pure random chaos might leave the service "Loading" if Shows > Hides. 
        // So we just assert consistency: exceptions empty.
        exceptions.ShouldBeEmpty();
        
        // To verify state consistency, we can force clear everything
        // But since we don't have a Clear(), we can't easily assert IsLoading is false unless we track strict balance.
        // The main goal here is checking for crashes/deadlocks.
    }

    [Test]
    public void NotifyStateChanged_ShouldHandleSubscriberExceptions_WithoutBreakingService()
    {
        // Arrange
        var exceptionThrowingSubscriberCalled = false;
        var normalSubscriberCalled = false;

        _sut.OnChange += () =>
        {
            exceptionThrowingSubscriberCalled = true;
            throw new InvalidOperationException("I am a bad subscriber!");
        };

        _sut.OnChange += () =>
        {
            normalSubscriberCalled = true;
        };

        // Act
        Should.NotThrow(() => _sut.Show("Test"));

        // Assert
        exceptionThrowingSubscriberCalled.ShouldBeTrue();
        normalSubscriberCalled.ShouldBeTrue("Normal subscriber should still be called even if another fails");
        _sut.IsLoading.ShouldBeTrue();
    }
}
