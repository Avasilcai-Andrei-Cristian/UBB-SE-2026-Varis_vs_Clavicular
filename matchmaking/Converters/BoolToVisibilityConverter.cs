using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace matchmaking.Converters;

public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        bool flag = value is true;
        if (parameter is string p && p == "Inverse")
        {
            flag = !flag;
        }

        return flag ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
    {
        bool flag = value is Visibility v && v == Visibility.Visible;
        if (parameter is string p && p == "Inverse")
        {
            flag = !flag;
        }

        return flag;
    }
}
