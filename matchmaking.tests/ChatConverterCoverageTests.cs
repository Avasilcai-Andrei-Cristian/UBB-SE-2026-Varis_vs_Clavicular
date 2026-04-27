using System.Reflection;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;

namespace matchmaking.Tests.Converters;

[Collection("AppState")]
public sealed class ChatConverterCoverageTests
{
    [Fact]
    public void ChatNameConverter_WhenSessionIsNull_ReturnsFallbackChatName()
    {
        var previousSession = GetAppSession();
        SetAppSession(null);

        try
        {
            var converter = new ChatNameConverter();
            var result = converter.Convert(new Chat { ChatId = 1, UserId = 1 }, typeof(string), null, string.Empty);

            result.Should().Be("Chat");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void ChatNameConverter_WhenChatHasResolvedOtherPartyName_ReturnsThatName()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsCompany(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatNameConverter();
            var chat = new Chat { ChatId = 1, UserId = 2, OtherPartyName = "Bogdan Ionescu" };

            var result = converter.Convert(chat, typeof(string), null, string.Empty);

            result.Should().Be("Bogdan Ionescu");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void ChatNameConverter_WhenCompanyChatHasResolvedOtherPartyName_ReturnsCompanyName()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatNameConverter();
            var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 1, OtherPartyName = "TechNova" };

            var result = converter.Convert(chat, typeof(string), null, string.Empty);

            result.Should().Be("TechNova");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void IsOtherPartyMessageConverter_WhenMessageIsFromOtherUser_ReturnsVisible()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new IsOtherPartyMessageConverter();
            var result = converter.Convert(new Message { SenderId = 2 }, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Visible);
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void IsCurrentUserMessageConverter_WhenMessageIsFromCurrentUser_ReturnsVisible()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new IsCurrentUserMessageConverter();
            var result = converter.Convert(new Message { SenderId = 1 }, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Visible);
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void ChatInitialsConverter_WhenChatHasCompany_ReturnsFirstTwoLetters()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatInitialsConverter();
            var result = converter.Convert(new Chat { UserId = 1, CompanyId = 1, OtherPartyName = "TechNova" }, typeof(string), null, string.Empty);

            result.Should().Be("T");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void MessageImageSourceConverter_WhenFileIsMissing_ReturnsNull()
    {
        var converter = new MessageImageSourceConverter();

        var result = converter.Convert(new Message { Type = MessageType.Image, Content = @"C:\missing\image.png" }, typeof(object), null, string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void ChatNameConverter_WhenValueIsNull_ReturnsEmpty()
    {
        var converter = new ChatNameConverter();

        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ChatNameConverter_WhenValueIsUser_ReturnsUserName()
    {
        var converter = new ChatNameConverter();

        var result = converter.Convert(new User { Name = "Alice" }, typeof(string), null, string.Empty);

        result.Should().Be("Alice");
    }

    [Fact]
    public void ChatNameConverter_WhenValueIsCompany_ReturnsCompanyName()
    {
        var converter = new ChatNameConverter();

        var result = converter.Convert(new Company { CompanyName = "TechCorp" }, typeof(string), null, string.Empty);

        result.Should().Be("TechCorp");
    }

    [Fact]
    public void ChatNameConverter_WhenUserModeAndChatHasSecondUser_ReturnsOtherUserName()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatNameConverter();
            var chat = new Chat { ChatId = 1, UserId = 1, SecondUserId = 2, OtherPartyName = "Bogdan Ionescu" };

            var result = converter.Convert(chat, typeof(string), null, string.Empty);

            result.Should().Be("Bogdan Ionescu");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void ChatPartyNameConverter_WhenValueIsNotChat_ReturnsNoChatSelected()
    {
        var converter = new ChatPartyNameConverter();

        var result = converter.Convert("not a chat", typeof(string), null, string.Empty);

        result.Should().Be("No chat selected");
    }

    [Fact]
    public void IsOtherPartyMessageConverter_WhenMessageIsFromCurrentUser_ReturnsCollapsed()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new IsOtherPartyMessageConverter();
            var result = converter.Convert(new Message { SenderId = 1 }, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Collapsed);
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void IsCurrentUserMessageConverter_WhenMessageIsFromOtherUser_ReturnsCollapsed()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new IsCurrentUserMessageConverter();
            var result = converter.Convert(new Message { SenderId = 2 }, typeof(Visibility), null, string.Empty);

            result.Should().Be(Visibility.Collapsed);
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void IsOtherPartyMessageConverter_WhenValueIsNotMessage_ReturnsCollapsed()
    {
        var converter = new IsOtherPartyMessageConverter();

        var result = converter.Convert("not a message", typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void MessageImageSourceConverter_WhenMessageIsNotImage_ReturnsNull()
    {
        var converter = new MessageImageSourceConverter();

        var result = converter.Convert(new Message { Type = MessageType.Text, Content = "hello" }, typeof(object), null, string.Empty);

        result.Should().BeNull();
    }

    [Fact]
    public void ChatInitialsConverter_WhenChatNameHasMultipleWords_ReturnsTwoInitials()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsCompany(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatInitialsConverter();
            var result = converter.Convert(new Chat { UserId = 2, OtherPartyName = "Bogdan Ionescu" }, typeof(string), null, string.Empty);

            result.Should().Be("BI");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void ChatInitialsConverter_WhenValueIsNotChat_ReturnsQuestionMark()
    {
        var converter = new ChatInitialsConverter();

        var result = converter.Convert("not a chat", typeof(string), null, string.Empty);

        result.Should().Be("?");
    }

    private static SessionContext? GetAppSession()
    {
        return (SessionContext?)typeof(App).GetProperty(nameof(App.Session), BindingFlags.Static | BindingFlags.Public)!.GetValue(null);
    }

    private static void SetAppSession(SessionContext? session)
    {
        typeof(App).GetProperty(nameof(App.Session), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, session);
    }
}
