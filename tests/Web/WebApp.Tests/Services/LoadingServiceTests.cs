using Dilcore.WebApp.Services.Loading;
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
        _sut = new LoadingService();
    }

    [Test]
    public void IsLoading_ShouldBeFalse_WhenNoMessages()
    {
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
}
