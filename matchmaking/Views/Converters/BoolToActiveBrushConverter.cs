using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace matchmaking.Views.Converters;

public sealed class BoolToActiveBrushConverter : IValueConverter
{
    public static Color GetColor(bool isActive)
    {
        return isActive
            ? Color.FromArgb(0xFF, 0x25, 0x63, 0xEB)
            : Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B);
    }

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return new SolidColorBrush(GetColor(value is true));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotSupportedException();
}
