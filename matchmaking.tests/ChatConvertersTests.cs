namespace matchmaking.Tests.Converters;

public class ReadReceiptConverterTests
{
    private readonly ReadReceiptConverter converter = new ();

    [Fact]
    public void Convert_True_ReturnsSeen()
    {
        var result = converter.Convert(true, typeof(string), null, string.Empty);

        result.Should().Be("Seen");
    }

    [Fact]
    public void Convert_False_ReturnsDelivered()
    {
        var result = converter.Convert(false, typeof(string), null, string.Empty);

        result.Should().Be("Delivered");
    }

    [Fact]
    public void Convert_NonBool_ReturnsDelivered()
    {
        var result = converter.Convert("not a bool", typeof(string), null, string.Empty);

        result.Should().Be("Delivered");
    }

    [Fact]
    public void Convert_Null_ReturnsDelivered()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be("Delivered");
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class MessageContentDisplayConverterTests
{
    private readonly MessageContentDisplayConverter converter = new ();

    [Fact]
    public void Convert_TextMessage_ReturnsContent()
    {
        var message = new Message { Type = MessageType.Text, Content = "Hello World" };

        var result = converter.Convert(message, typeof(string), null, string.Empty);

        result.Should().Be("Hello World");
    }

    [Fact]
    public void Convert_ImageMessage_ReturnsEmojiWithFilename()
    {
        var message = new Message { Type = MessageType.Image, Content = @"C:\photos\cat.png" };

        var result = converter.Convert(message, typeof(string), null, string.Empty);

        result.Should().Be("📷 cat.png");
    }

    [Fact]
    public void Convert_FileMessage_ReturnsEmojiWithFilename()
    {
        var message = new Message { Type = MessageType.File, Content = @"C:\docs\report.pdf" };

        var result = converter.Convert(message, typeof(string), null, string.Empty);

        result.Should().Be("📎 report.pdf");
    }

    [Fact]
    public void Convert_ImageMessageWithEmptyContent_ReturnsEmptyContent()
    {
        var message = new Message { Type = MessageType.Image, Content = string.Empty };

        var result = converter.Convert(message, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_NonMessage_ReturnsEmptyString()
    {
        var result = converter.Convert("not a message", typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_Null_ReturnsEmptyString()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class MessageTextVisibilityConverterTests
{
    private readonly MessageTextVisibilityConverter converter = new ();

    [Fact]
    public void Convert_TextMessage_ReturnsVisible()
    {
        var message = new Message { Type = MessageType.Text };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_ImageMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.Image };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_FileMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.File };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonMessage_ReturnsCollapsed()
    {
        var result = converter.Convert("not a message", typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class MessageAttachmentVisibilityConverterTests
{
    private readonly MessageAttachmentVisibilityConverter converter = new ();

    [Fact]
    public void Convert_TextMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.Text };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_ImageMessage_ReturnsVisible()
    {
        var message = new Message { Type = MessageType.Image };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_FileMessage_ReturnsVisible()
    {
        var message = new Message { Type = MessageType.File };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_NonMessage_ReturnsCollapsed()
    {
        var result = converter.Convert(null, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class MessageFileAttachmentVisibilityConverterTests
{
    private readonly MessageFileAttachmentVisibilityConverter converter = new ();

    [Fact]
    public void Convert_FileMessage_ReturnsVisible()
    {
        var message = new Message { Type = MessageType.File };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_ImageMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.Image };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_TextMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.Text };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonMessage_ReturnsCollapsed()
    {
        var result = converter.Convert("not a message", typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class MessageImageVisibilityConverterTests
{
    private readonly MessageImageVisibilityConverter converter = new ();

    [Fact]
    public void Convert_ImageMessage_ReturnsVisible()
    {
        var message = new Message { Type = MessageType.Image };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_FileMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.File };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_TextMessage_ReturnsCollapsed()
    {
        var message = new Message { Type = MessageType.Text };

        var result = converter.Convert(message, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonMessage_ReturnsCollapsed()
    {
        var result = converter.Convert(null, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class StringToVisibilityConverterTests
{
    private readonly StringToVisibilityConverter converter = new ();

    [Theory]
    [InlineData("Hello")]
    [InlineData("x")]
    [InlineData("  text  ")]
    public void Convert_NonEmptyString_ReturnsVisible(string input)
    {
        var result = converter.Convert(input, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Convert_EmptyOrWhitespace_ReturnsCollapsed(string input)
    {
        var result = converter.Convert(input, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NullValue_ReturnsCollapsed()
    {
        var result = converter.Convert(null, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonStringValue_ReturnsCollapsed()
    {
        var result = converter.Convert(42, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class ObjectToVisibilityConverterTests
{
    private readonly ObjectToVisibilityConverter converter = new ();

    [Fact]
    public void Convert_NonNullObject_ReturnsVisible()
    {
        var result = converter.Convert(new object(), typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_NonNullString_ReturnsVisible()
    {
        var result = converter.Convert("value", typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_NullValue_ReturnsCollapsed()
    {
        var result = converter.Convert(null, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class IntToVisibilityConverterTests
{
    private readonly IntToVisibilityConverter converter = new ();

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(100)]
    public void Convert_PositiveInt_ReturnsVisible(int value)
    {
        var result = converter.Convert(value, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_Zero_ReturnsCollapsed()
    {
        var result = converter.Convert(0, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Convert_NegativeInt_ReturnsCollapsed(int value)
    {
        var result = converter.Convert(value, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonInt_ReturnsCollapsed()
    {
        var result = converter.Convert("not an int", typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_Null_ReturnsCollapsed()
    {
        var result = converter.Convert(null, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class ChatAvatarBgConverterTests
{
    private readonly ChatAvatarBgConverter converter = new ();

    [Fact]
    public void Convert_ChatWithoutCompany_ReturnsUserColor()
    {
        var chat = new Chat { UserId = 1, CompanyId = null };

        var result = converter.Convert(chat, typeof(string), null, string.Empty);

        result.Should().Be("#FFE8EEF8");
    }

    [Fact]
    public void Convert_ChatWithCompany_ReturnsCompanyColor()
    {
        var chat = new Chat { UserId = 1, CompanyId = 42 };

        var result = converter.Convert(chat, typeof(string), null, string.Empty);

        result.Should().Be("#FFF3F4F6");
    }

    [Fact]
    public void Convert_NonChat_ReturnsFallbackColor()
    {
        var result = converter.Convert("not a chat", typeof(string), null, string.Empty);

        result.Should().Be("#FFE8EEF8");
    }

    [Fact]
    public void Convert_Null_ReturnsFallbackColor()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be("#FFE8EEF8");
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class ChatAvatarFgConverterTests
{
    private readonly ChatAvatarFgConverter converter = new ();

    [Fact]
    public void Convert_ChatWithoutCompany_ReturnsUserForeground()
    {
        var chat = new Chat { UserId = 1, CompanyId = null };

        var result = converter.Convert(chat, typeof(string), null, string.Empty);

        result.Should().Be("#FF0F4FAD");
    }

    [Fact]
    public void Convert_ChatWithCompany_ReturnsCompanyForeground()
    {
        var chat = new Chat { UserId = 1, CompanyId = 42 };

        var result = converter.Convert(chat, typeof(string), null, string.Empty);

        result.Should().Be("#FF374151");
    }

    [Fact]
    public void Convert_NonChat_ReturnsFallbackForeground()
    {
        var result = converter.Convert("not a chat", typeof(string), null, string.Empty);

        result.Should().Be("#FF0F4FAD");
    }

    [Fact]
    public void Convert_Null_ReturnsFallbackForeground()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be("#FF0F4FAD");
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class ChatSubtitleConverterTests
{
    private readonly ChatSubtitleConverter converter = new ();

    [Fact]
    public void Convert_BlockedChat_ReturnsBlockedMessage()
    {
        var chat = new Chat { IsBlocked = true };

        var result = converter.Convert(chat, typeof(string), null, string.Empty);

        result.Should().Be("Blocked conversation");
    }

    [Fact]
    public void Convert_NonBlockedChat_ReturnsEmptyString()
    {
        var chat = new Chat { IsBlocked = false };

        var result = converter.Convert(chat, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_NonChat_ReturnsEmptyString()
    {
        var result = converter.Convert("not a chat", typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_Null_ReturnsEmptyString()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class ChatAvatarCornerRadiusConverterTests
{
    private readonly ChatAvatarCornerRadiusConverter converter = new ();

    [Fact]
    public void Convert_ChatWithoutCompany_ReturnsCircularRadius()
    {
        var chat = new Chat { UserId = 1, CompanyId = null };

        var result = converter.Convert(chat, typeof(CornerRadius), null, string.Empty);

        result.Should().Be(new CornerRadius(999));
    }

    [Fact]
    public void Convert_ChatWithCompany_ReturnsSquaredRadius()
    {
        var chat = new Chat { UserId = 1, CompanyId = 5 };

        var result = converter.Convert(chat, typeof(CornerRadius), null, string.Empty);

        result.Should().Be(new CornerRadius(8));
    }

    [Fact]
    public void Convert_NonChat_ReturnsCircularRadius()
    {
        var result = converter.Convert("not a chat", typeof(CornerRadius), null, string.Empty);

        result.Should().Be(new CornerRadius(999));
    }

    [Fact]
    public void Convert_Null_ReturnsCircularRadius()
    {
        var result = converter.Convert(null, typeof(CornerRadius), null, string.Empty);

        result.Should().Be(new CornerRadius(999));
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class InverseBoolConverterTests
{
    private readonly InverseBoolConverter converter = new ();

    [Fact]
    public void Convert_True_ReturnsFalse()
    {
        var result = converter.Convert(true, typeof(bool), null, string.Empty);

        result.Should().Be(false);
    }

    [Fact]
    public void Convert_False_ReturnsTrue()
    {
        var result = converter.Convert(false, typeof(bool), null, string.Empty);

        result.Should().Be(true);
    }

    [Fact]
    public void Convert_NonBool_ReturnsTrue()
    {
        var result = converter.Convert("not a bool", typeof(bool), null, string.Empty);

        result.Should().Be(true);
    }

    [Fact]
    public void Convert_Null_ReturnsTrue()
    {
        var result = converter.Convert(null, typeof(bool), null, string.Empty);

        result.Should().Be(true);
    }

    [Fact]
    public void ConvertBack_True_ReturnsFalse()
    {
        var result = converter.ConvertBack(true, typeof(bool), null, string.Empty);

        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_False_ReturnsTrue()
    {
        var result = converter.ConvertBack(false, typeof(bool), null, string.Empty);

        result.Should().Be(true);
    }

    [Fact]
    public void ConvertBack_NonBool_ReturnsFalse()
    {
        var result = converter.ConvertBack("not a bool", typeof(bool), null, string.Empty);

        result.Should().Be(false);
    }

}

public class ChatNameConverterTests
{
    private readonly ChatNameConverter converter = new ();

    [Fact]
    public void Convert_NullValue_ReturnsEmptyString()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_UserObject_ReturnsUserName()
    {
        var user = new User { Name = "Alice Smith" };

        var result = converter.Convert(user, typeof(string), null, string.Empty);

        result.Should().Be("Alice Smith");
    }

    [Fact]
    public void Convert_CompanyObject_ReturnsCompanyName()
    {
        var company = new Company { CompanyName = "Acme Corp" };

        var result = converter.Convert(company, typeof(string), null, string.Empty);

        result.Should().Be("Acme Corp");
    }

    [Fact]
    public void Convert_ObjectWithNameProperty_ReturnsNameValue()
    {
        var obj = new { Name = "Test Name" };

        var result = converter.Convert(obj, typeof(string), null, string.Empty);

        result.Should().Be("Test Name");
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}

public class ChatPartyNameConverterTests
{
    private readonly ChatPartyNameConverter converter = new ();

    [Fact]
    public void Convert_NonChat_ReturnsNoChatSelected()
    {
        var result = converter.Convert("not a chat", typeof(string), null, string.Empty);

        result.Should().Be("No chat selected");
    }

    [Fact]
    public void Convert_Null_ReturnsNoChatSelected()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be("No chat selected");
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}
