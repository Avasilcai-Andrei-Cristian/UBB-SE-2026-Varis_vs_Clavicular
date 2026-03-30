using System;
using System.IO;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;

namespace matchmaking.Converters;

internal static class ChatDisplayResolver
{
    private static readonly UserRepository UserRepository = new();
    private static readonly CompanyRepository CompanyRepository = new();

    public static string ResolveChatName(Chat chat)
    {
        var session = App.Session;
        if (session is null)
            return "Chat";

        if (session.CurrentMode == AppMode.CompanyMode)
        {
            return UserRepository.GetById(chat.UserId)?.Name ?? $"User {chat.UserId}";
        }

        if (chat.CompanyId.HasValue)
        {
            var companyId = chat.CompanyId.Value;
            return CompanyRepository.GetById(companyId)?.CompanyName ?? $"Company {companyId}";
        }

        if (chat.SecondUserId.HasValue)
        {
            var currentUserId = session.CurrentUserId;
            var otherUserId = currentUserId.HasValue && chat.UserId == currentUserId.Value
                ? chat.SecondUserId.Value
                : chat.UserId;

            return UserRepository.GetById(otherUserId)?.Name ?? $"User {otherUserId}";
        }

        return "Chat";
    }

    public static int GetCurrentSenderId()
    {
        var session = App.Session;
        if (session is null)
            return 0;

        return session.CurrentMode == AppMode.UserMode
            ? session.CurrentUserId ?? 0
            : session.CurrentCompanyId ?? 0;
    }
}

public class ChatNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
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
            return user.Name;

        if (value is Company company)
            return company.CompanyName;

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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatPartyNameConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is Chat chat
            ? ChatDisplayResolver.ResolveChatName(chat)
            : "No chat selected";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ReadReceiptConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool isRead ? (isRead ? "Seen" : "Delivered") : "Delivered";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageContentDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return string.Empty;

        if (message.Type == MessageType.Text)
            return message.Content;

        var fileName = Path.GetFileName(message.Content);
        if (string.IsNullOrWhiteSpace(fileName))
            return message.Content;

        return message.Type == MessageType.Image
            ? $"📷 {fileName}"
            : $"📎 {fileName}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageTextVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return Visibility.Collapsed;

        return message.Type == MessageType.Text ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageAttachmentVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return Visibility.Collapsed;

        return message.Type == MessageType.Text ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageFileAttachmentVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return Visibility.Collapsed;

        return message.Type == MessageType.File ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageImageVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return Visibility.Collapsed;

        return message.Type == MessageType.Image ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class MessageImageSourceConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message || message.Type != MessageType.Image || string.IsNullOrWhiteSpace(message.Content))
            return null;

        try
        {
            if (!File.Exists(message.Content))
                return null;

            return new BitmapImage(new Uri(message.Content));
        }
        catch
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IsOtherPartyMessageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return Visibility.Collapsed;

        var currentSenderId = ChatDisplayResolver.GetCurrentSenderId();
        return message.SenderId != currentSenderId ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IsCurrentUserMessageConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Message message)
            return Visibility.Collapsed;

        var currentSenderId = ChatDisplayResolver.GetCurrentSenderId();
        return message.SenderId == currentSenderId ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b && b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is Visibility v && v == Visibility.Visible;
    }
}

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ObjectToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value != null ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class IntToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            return intValue > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatAvatarCornerRadiusConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Chat chat)
        {
            return new CornerRadius(999);
        }

        return chat.CompanyId.HasValue ? new CornerRadius(8) : new CornerRadius(999);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatAvatarBgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Chat chat)
        {
            return "#FFE8EEF8";
        }

        return chat.CompanyId.HasValue ? "#FFF3F4F6" : "#FFE8EEF8";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatAvatarFgConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Chat chat)
        {
            return "#FF0F4FAD";
        }

        return chat.CompanyId.HasValue ? "#FF374151" : "#FF0F4FAD";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatInitialsConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
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

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class ChatSubtitleConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is not Chat chat)
        {
            return string.Empty;
        }

        return chat.IsBlocked ? "Blocked conversation" : string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is bool b ? !b : true;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        return value is bool b ? !b : false;
    }
}
