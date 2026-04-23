using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace matchmaking.Views.Converters;

public sealed class PostTypeToBadgeBackgroundConverter : IValueConverter
{
    public static Color GetColor(bool isJobPost)
    {
        return isJobPost
            ? Color.FromArgb(0xFF, 0x16, 0xA3, 0x4A)
            : Color.FromArgb(0xFF, 0x25, 0x63, 0xEB);
    }

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return new SolidColorBrush(GetColor(value is true));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotSupportedException();
}
