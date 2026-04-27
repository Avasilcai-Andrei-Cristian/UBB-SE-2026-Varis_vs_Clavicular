using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using matchmaking.Domain.Enums;
using Windows.UI;

namespace matchmaking.Converters;

public class MatchStatusToColorConverter : IValueConverter
{
    private static readonly Color AcceptedStatusColor = Color.FromArgb(255, 76, 175, 80);
    private static readonly Color RejectedStatusColor = Color.FromArgb(255, 244, 67, 54);
    private static readonly Color DefaultStatusColor = Color.FromArgb(255, 33, 150, 243);

    public static Color GetColor(MatchStatus status)
    {
        return status switch
        {
            MatchStatus.Accepted => AcceptedStatusColor,
            MatchStatus.Rejected => RejectedStatusColor,
            _ => DefaultStatusColor
        };
    }

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return new SolidColorBrush(GetColor(value is MatchStatus status ? status : MatchStatus.Applied));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();
}
