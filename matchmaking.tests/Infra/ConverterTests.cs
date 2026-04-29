using Windows.UI;

namespace matchmaking.Tests;

public class ConverterTests
{
    private readonly BoolToVisibilityConverter boolConverter = new();
    private readonly MatchStatusToTextConverter textConverter = new();

    [Fact]
    public void BoolToVisibility_true_value_returns_Visible()
    {
        var result = boolConverter.Convert(true, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Visible);
    }

    [Fact]
    public void BoolToVisibility_false_value_returns_Collapsed()
    {
        var result = boolConverter.Convert(false, typeof(Visibility), null, string.Empty);

        result.Should().Be(Visibility.Collapsed);
    }

    [Fact]
    public void MatchStatusToColor_Accepted_returns_green_color()
    {
        var color = MatchStatusToColorConverter.GetColor(MatchStatus.Accepted);

        color.Should().Be(Color.FromArgb(255, 76, 175, 80));
    }

    [Fact]
    public void MatchStatusToColor_Rejected_returns_red_color()
    {
        var color = MatchStatusToColorConverter.GetColor(MatchStatus.Rejected);

        color.Should().Be(Color.FromArgb(255, 244, 67, 54));
    }

    [Fact]
    public void MatchStatusToColor_Applied_returns_blue_default_color()
    {
        var color = MatchStatusToColorConverter.GetColor(MatchStatus.Applied);

        color.Should().Be(Color.FromArgb(255, 33, 150, 243));
    }

    [Fact]
    public void MatchStatusToText_Accepted_returns_Accepted_string()
    {
        var result = textConverter.Convert(MatchStatus.Accepted, typeof(string), null, string.Empty);

        result.Should().Be("Accepted");
    }

    [Fact]
    public void MatchStatusToText_Rejected_returns_Rejected_string()
    {
        var result = textConverter.Convert(MatchStatus.Rejected, typeof(string), null, string.Empty);

        result.Should().Be("Rejected");
    }

    [Fact]
    public void MatchStatusToText_Applied_returns_Applied_string()
    {
        var result = textConverter.Convert(MatchStatus.Applied, typeof(string), null, string.Empty);

        result.Should().Be("Applied");
    }
}
