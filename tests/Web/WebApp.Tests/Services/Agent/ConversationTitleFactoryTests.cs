using Dilcore.WebApp.Http.AiAgent.Dtos;
using Dilcore.WebApp.Services.Agent;

using Shouldly;

namespace Dilcore.WebApp.Tests.Services.Agent;

public class ConversationTitleFactoryTests
{
    private readonly ConversationTitleFactory _sut = new();

    [Test]
    public void BuildTitle_Should_Prefer_Name_On_Summary_When_Present()
    {
        var summary = new ThreadResponseDto { Id = "thread-1", Name = "List title from API" };

        _sut.BuildTitle(summary).ShouldBe("List title from API");
    }

    [Test]
    public void BuildTitle_Should_Fallback_To_Short_Id_On_Summary_When_No_Name()
    {
        var summary = new ThreadResponseDto { Id = "abcdefgh-extra", Name = null };

        _sut.BuildTitle(summary).ShouldBe("Thread abcdefgh");
    }

    [Test]
    public void BuildTitle_Should_Prefer_Name_On_Full_Thread_When_Present()
    {
        var thread = new ThreadStateDto
        {
            Id = "thread-1",
            Name = "Server title",
            Messages =
            [
                new MessageDto { Type = "user", Content = "ignored when name set" }
            ]
        };

        _sut.BuildTitle(thread).ShouldBe("Server title");
    }

    [Test]
    public void BuildTitle_Should_Truncate_First_User_Message()
    {
        var longText = new string('a', 60);
        var thread = new ThreadStateDto
        {
            Id = "thread-1",
            Messages =
            [
                new MessageDto { Type = "user", Content = longText }
            ]
        };

        var title = _sut.BuildTitle(thread);

        title.Length.ShouldBe(51);
        title.ShouldEndWith("…");
    }

    [Test]
    public void BuildTitle_Should_Use_First_User_Message_When_Multiple_Messages()
    {
        var thread = new ThreadStateDto
        {
            Id = "t2",
            Messages =
            [
                new MessageDto { Type = "assistant", Content = "Hi" },
                new MessageDto { Type = "user", Content = "Hello there" }
            ]
        };

        _sut.BuildTitle(thread).ShouldBe("Hello there");
    }

    [Test]
    public void BuildTitle_Should_Fallback_To_Short_Id_When_No_User_Message()
    {
        var thread = new ThreadStateDto
        {
            Id = "abcdefgh-extra",
            Messages =
            [
                new MessageDto { Type = "assistant", Content = "Only assistant" }
            ]
        };

        _sut.BuildTitle(thread).ShouldBe("Thread abcdefgh");
    }

    [Test]
    public void BuildTitle_Should_Normalize_Line_Endings_For_Title()
    {
        var thread = new ThreadStateDto
        {
            Id = "x",
            Messages =
            [
                new MessageDto { Type = "user", Content = "Line1\r\nLine2" }
            ]
        };

        _sut.BuildTitle(thread).ShouldBe("Line1 Line2");
    }
}
