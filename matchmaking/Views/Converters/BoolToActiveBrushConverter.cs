using System;
using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace matchmaking.Views.Converters;

public sealed class BoolToActiveBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        return value is true
            ? new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x25, 0x63, 0xEB))
            : new SolidColorBrush(ColorHelper.FromArgb(0xFF, 0x6B, 0x6B, 0x6B));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
        => throw new NotSupportedException();
}
