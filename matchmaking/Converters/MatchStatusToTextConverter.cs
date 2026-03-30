using System;
using Microsoft.UI.Xaml.Data;
using matchmaking.Domain.Enums;

namespace matchmaking.Converters;

public class MatchStatusToTextConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is MatchStatus status)
        {
            return status switch
            {
                MatchStatus.Accepted => "Accepted",
                MatchStatus.Rejected => "Rejected",
                _                    => "Applied"
            };
        }
        return "Applied";
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotImplementedException();
}
