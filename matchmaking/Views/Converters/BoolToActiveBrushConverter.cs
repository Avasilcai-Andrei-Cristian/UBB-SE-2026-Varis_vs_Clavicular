using System;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace matchmaking.Views.Converters;

public sealed class BoolToActiveBrushConverter : IValueConverter
{
    private static readonly Color ActiveColor = Color.FromArgb(0xFF, 0x25, 0x63, 0xEB);
    private static readonly Color InactiveColor = Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B);

    public static Color GetColor(bool isActive)
    {
        return isActive
            ? ActiveColor
            : InactiveColor;
    }

    public object Convert(object? value, Type targetType, object? parameter, string language)
    {
        return new SolidColorBrush(GetColor(value is true));
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotSupportedException();
}
