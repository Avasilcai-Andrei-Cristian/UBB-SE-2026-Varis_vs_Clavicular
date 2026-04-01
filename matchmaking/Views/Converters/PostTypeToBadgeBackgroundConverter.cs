using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace matchmaking.Views.Converters;

public sealed class PostTypeToBadgeBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true
            ? new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x16, 0xA3, 0x4A))  // keyword — green
            : new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x25, 0x63, 0xEB)); // parameter — blue
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
