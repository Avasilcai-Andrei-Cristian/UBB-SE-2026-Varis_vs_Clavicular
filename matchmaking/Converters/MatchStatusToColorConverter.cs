using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using matchmaking.Domain.Enums;

namespace matchmaking.Converters;

public class MatchStatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Accepted => new SolidColorBrush(Color.FromArgb(255,  76, 175,  80)),
                MatchStatus.Rejected => new SolidColorBrush(Color.FromArgb(255, 244,  67,  54)),
                _                    => new SolidColorBrush(Color.FromArgb(255,  33, 150, 243))
            };
        }
        return new SolidColorBrush(Color.FromArgb(255, 33, 150, 243));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
