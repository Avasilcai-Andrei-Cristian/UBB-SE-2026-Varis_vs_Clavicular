using System;
using System.IO;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;

namespace matchmaking.Converters;

internal static class ChatDisplayResolver
{
    private const int MissingSenderId = 0;
    private const string DefaultChatName = "Chat";

    public static string ResolveChatName(Chat chat)
    {
        if (!string.IsNullOrWhiteSpace(chat.OtherPartyName))
        {
            return chat.OtherPartyName;
        }

        var session = App.Session;
        if (session is null)
        {
            return DefaultChatName;
        }

        if (session.CurrentMode == AppMode.CompanyMode)
        {
            return $"User {chat.UserId}";
        }

        if (chat.CompanyId.HasValue)
        {
            var companyId = chat.CompanyId.Value;
            return $"Company {companyId}";
        }

        if (chat.SecondUserId.HasValue)
        {
            var currentUserId = session.CurrentUserId;
            var otherUserId = currentUserId.HasValue && chat.UserId == currentUserId.Value
                ? chat.SecondUserId.Value
                : chat.UserId;

            return $"User {otherUserId}";
        }

        return DefaultChatName;
    }

    public static int GetCurrentSenderId()
    {
        var session = App.Session;
        if (session is null)
        {
            return MissingSenderId;
        }

        return session.CurrentMode == AppMode.UserMode
            ? session.CurrentUserId ?? MissingSenderId
            : session.CurrentCompanyId ?? MissingSenderId;
    }
}

public class ChatNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is Chat chat)
        {
            return ChatDisplayResolver.ResolveChatName(chat);
        }

        if (value is User user)
        {
            return user.Name;
        }

        if (value is Company company)
        {
            return company.CompanyName;
        }

        var type = value.GetType();
        var nameProperty = type.GetProperty("Name", BindingFlags.Public | BindingFlags.Instance);
        if (nameProperty?.GetValue(value) is string name && !string.IsNullOrWhiteSpace(name))
        {
            return name;
        }

        var companyNameProperty = type.GetProperty("CompanyName", BindingFlags.Public | BindingFlags.Instance);
        if (companyNameProperty?.GetValue(value) is string companyName && !string.IsNullOrWhiteSpace(companyName))
        {
            return companyName;
        }

        return value.ToString() ?? string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatPartyNameConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is Chat chat
            ? ChatDisplayResolver.ResolveChatName(chat)
            : "No chat selected";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ReadReceiptConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is bool isRead ? (isRead ? "Seen" : "Delivered") : "Delivered";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageContentDisplayConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return string.Empty;
        }

        if (message.Type == MessageType.Text)
        {
            return message.Content;
        }

        var fileName = Path.GetFileName(message.Content);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return message.Content;
        }

        return message.Type == MessageType.Image
            ? $"📷 {fileName}"
            : $"📎 {fileName}";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageTextVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        return message.Type == MessageType.Text ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageAttachmentVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        return message.Type == MessageType.Text ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageFileAttachmentVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        return message.Type == MessageType.File ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageImageVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        return message.Type == MessageType.Image ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message || message.Type != MessageType.Image || string.IsNullOrWhiteSpace(message.Content))
        {
            return null;
        }

        try
        {
            if (!File.Exists(message.Content))
            {
                return null;
            }

            return new BitmapImage(new Uri(message.Content));
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IsOtherPartyMessageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        var currentSenderId = ChatDisplayResolver.GetCurrentSenderId();
        return message.SenderId != currentSenderId ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IsCurrentUserMessageConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Message message)
        {
            return Visibility.Collapsed;
        }

        var currentSenderId = ChatDisplayResolver.GetCurrentSenderId();
        return message.SenderId == currentSenderId ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatAvatarCornerRadiusConverter : IValueConverter
{
    private const double CompanyAvatarCornerRadius = 8;
    private const double CircularAvatarCornerRadius = 999;

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return new CornerRadius(CircularAvatarCornerRadius);
        }

        return chat.CompanyId.HasValue
            ? new CornerRadius(CompanyAvatarCornerRadius)
            : new CornerRadius(CircularAvatarCornerRadius);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatAvatarBgConverter : IValueConverter
{
    private const string DefaultAvatarBackgroundColor = "#FFE8EEF8";
    private const string CompanyAvatarBackgroundColor = "#FFF3F4F6";

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return DefaultAvatarBackgroundColor;
        }

        return chat.CompanyId.HasValue ? CompanyAvatarBackgroundColor : DefaultAvatarBackgroundColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatAvatarFgConverter : IValueConverter
{
    private const string DefaultAvatarForegroundColor = "#FF0F4FAD";
    private const string CompanyAvatarForegroundColor = "#FF374151";

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return DefaultAvatarForegroundColor;
        }

        return chat.CompanyId.HasValue ? CompanyAvatarForegroundColor : DefaultAvatarForegroundColor;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatInitialsConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return "?";
        }

        var name = ChatDisplayResolver.ResolveChatName(chat);
        if (string.IsNullOrWhiteSpace(name))
        {
            return "?";
        }

        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpperInvariant();
        }

        return string.Concat(parts[0][0], parts[1][0]).ToUpperInvariant();
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatSubtitleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not Chat chat)
        {
            return string.Empty;
        }

        return chat.IsBlocked ? "Blocked conversation" : string.Empty;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return value is bool booleanValue ? !booleanValue : true;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        return value is bool booleanValue ? !booleanValue : false;
    }
}
