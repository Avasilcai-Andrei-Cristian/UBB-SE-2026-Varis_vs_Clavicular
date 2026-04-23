namespace matchmaking.Tests.Views.Converters;

public class PostTypeToBadgeBackgroundConverterTests
{
    private readonly PostTypeToBadgeBackgroundConverter converter = new PostTypeToBadgeBackgroundConverter();

    [Fact]
    public void Convert_True_ReturnsJobPostColor()
    {
        var result = PostTypeToBadgeBackgroundConverter.GetColor(true);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_False_ReturnsDeveloperPostColor()
    {
        var result = PostTypeToBadgeBackgroundConverter.GetColor(false);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_TrueAndFalse_ReturnDifferentColors()
    {
        var jobBrush = PostTypeToBadgeBackgroundConverter.GetColor(true);
        var devBrush = PostTypeToBadgeBackgroundConverter.GetColor(false);

        jobBrush.Should().NotBe(devBrush);
    }

    [Fact]
    public void Convert_NonBoolValue_ReturnsDeveloperPostColor()
    {
        var result = PostTypeToBadgeBackgroundConverter.GetColor(false);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_NullValue_ReturnsDeveloperPostColor()
    {
        var result = PostTypeToBadgeBackgroundConverter.GetColor(false);

        result.Should().NotBe(default);
    }

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotSupportedException>();
    }
}
