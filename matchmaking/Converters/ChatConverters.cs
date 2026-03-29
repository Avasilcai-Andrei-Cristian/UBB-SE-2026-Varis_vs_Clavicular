using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;
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
