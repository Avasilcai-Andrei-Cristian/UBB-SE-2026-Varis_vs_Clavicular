namespace matchmaking.Tests.Views.Converters;

public class BoolToActiveBrushConverterTests
{
    private readonly BoolToActiveBrushConverter converter = new BoolToActiveBrushConverter();

    [Fact]
    public void Convert_True_ReturnsActiveColor()
    {
        var result = BoolToActiveBrushConverter.GetColor(true);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_False_ReturnsInactiveColor()
    {
        var result = BoolToActiveBrushConverter.GetColor(false);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_TrueAndFalse_ReturnDifferentColors()
    {
        var activeBrush = BoolToActiveBrushConverter.GetColor(true);
        var inactiveBrush = BoolToActiveBrushConverter.GetColor(false);

        activeBrush.Should().NotBe(inactiveBrush);
    }

    [Fact]
    public void Convert_NonBoolValue_ReturnsInactiveColor()
    {
        var result = BoolToActiveBrushConverter.GetColor(false);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_NullValue_ReturnsInactiveColor()
    {
        var result = BoolToActiveBrushConverter.GetColor(false);

        result.Should().NotBe(default);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotSupportedException>();
    }
}
