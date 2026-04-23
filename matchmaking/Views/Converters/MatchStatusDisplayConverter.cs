using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using matchmaking.Domain.Enums;
using Windows.UI;

namespace matchmaking.Views.Converters;

public sealed class MatchStatusDisplayConverter : IValueConverter
{
    public static string GetLabel(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => "Accepted",
            MatchStatus.Rejected => "Rejected",
            MatchStatus.Advanced => "In Review",
            _ => "Pending Review"
        };
    }

    public static Color GetBackgroundColor(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => Color.FromArgb(0xFF, 0xDC, 0xFC, 0xE7),
            MatchStatus.Rejected => Color.FromArgb(0xFF, 0xFE, 0xE2, 0xE2),
            _ => Color.FromArgb(0xFF, 0xFE, 0xF3, 0xC7)
        };
    }

    public static Color GetForegroundColor(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => Color.FromArgb(0xFF, 0x16, 0x65, 0x34),
            MatchStatus.Rejected => Color.FromArgb(0xFF, 0x99, 0x1B, 0x1B),
            _ => Color.FromArgb(0xFF, 0x92, 0x40, 0x0E)
        };
    }

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        var status = value is MatchStatus matchStatus ? matchStatus : MatchStatus.Applied;
        var mode = parameter?.ToString();

        return mode switch
        {
            "Label" => GetLabel(status),
            "Background" => new SolidColorBrush(GetBackgroundColor(status)),
            "Foreground" => new SolidColorBrush(GetForegroundColor(status)),
            _ => string.Empty
        };
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        throw new NotSupportedException();
    }
}
