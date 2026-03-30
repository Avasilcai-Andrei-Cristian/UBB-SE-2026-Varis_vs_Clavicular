using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using matchmaking.Domain.Enums;

namespace matchmaking.Views.Converters;

public sealed class MatchStatusDisplayConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        var status = value is MatchStatus matchStatus ? matchStatus : MatchStatus.Applied;
        var mode = parameter?.ToString();

        return mode switch
        {
            "Label" => GetLabel(status),
            "Background" => GetBackgroundBrush(status),
            "Foreground" => GetForegroundBrush(status),
            _ => string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }

    private static string GetLabel(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => "Accepted",
            MatchStatus.Rejected => "Rejected",
            _ => "Pending Review"
        };
    }

    private static SolidColorBrush GetBackgroundBrush(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xDC, 0xFC, 0xE7)),
            MatchStatus.Rejected => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xFE, 0xE2, 0xE2)),
            _ => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0xFE, 0xF3, 0xC7))
        };
    }

    private static SolidColorBrush GetForegroundBrush(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x16, 0x65, 0x34)),
            MatchStatus.Rejected => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x99, 0x1B, 0x1B)),
            _ => new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x92, 0x40, 0x0E))
        };
    }
}
