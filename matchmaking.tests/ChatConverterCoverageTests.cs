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
    public void ChatNameConverter_WhenCompanyMode_ReturnsUserName()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsCompany(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatNameConverter();
            var chat = new Chat { ChatId = 1, UserId = 2 };

            var result = converter.Convert(chat, typeof(string), null, string.Empty);

            result.Should().Be("Bogdan Ionescu");
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    [Fact]
    public void ChatNameConverter_WhenUserChatHasCompany_ReturnsCompanyName()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
            var converter = new ChatNameConverter();
            var chat = new Chat { ChatId = 1, UserId = 1, CompanyId = 1 };

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
            var result = converter.Convert(new Chat { UserId = 1, CompanyId = 1 }, typeof(string), null, string.Empty);

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

    private static SessionContext? GetAppSession()
    {
        return (SessionContext?)typeof(App).GetProperty(nameof(App.Session), BindingFlags.Static | BindingFlags.Public)!.GetValue(null);
    }

    private static void SetAppSession(SessionContext? session)
    {
        typeof(App).GetProperty(nameof(App.Session), BindingFlags.Static | BindingFlags.Public)!.SetValue(null, session);
    }
}
