namespace matchmaking.Tests.Converters;

public class BoolToVisibilityConverterTests
{
    private readonly BoolToVisibilityConverter converter = new BoolToVisibilityConverter();

    [Fact]
    public void Convert_True_ReturnsVisible()
    {
        var result = converter.Convert(true, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_False_ReturnsCollapsed()
    {
        var result = converter.Convert(false, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_TrueWithInverseParameter_ReturnsCollapsed()
    {
        var result = converter.Convert(true, typeof(Visibility), "Inverse", string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_FalseWithInverseParameter_ReturnsVisible()
    {
        var result = converter.Convert(false, typeof(Visibility), "Inverse", string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void Convert_NullValue_ReturnsCollapsed()
    {
        var result = converter.Convert(null, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NonBoolValue_ReturnsCollapsed()
    {
        var result = converter.Convert("not a bool", typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void Convert_NullWithInverseParameter_ReturnsVisible()
    {
        var result = converter.Convert(null, typeof(Visibility), "Inverse", string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void ConvertBack_Visible_ReturnsTrue()
    {
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null, string.Empty);

        result.Should().Be(true);
    }

    [Fact]
    public void ConvertBack_Collapsed_ReturnsFalse()
    {
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, string.Empty);

        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_VisibleWithInverseParameter_ReturnsFalse()
    {
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), "Inverse", string.Empty);

        result.Should().Be(false);
    }

    [Fact]
    public void ConvertBack_CollapsedWithInverseParameter_ReturnsTrue()
    {
        var result = converter.ConvertBack(Visibility.Collapsed, typeof(bool), "Inverse", string.Empty);

        result.Should().Be(true);
    }
}
