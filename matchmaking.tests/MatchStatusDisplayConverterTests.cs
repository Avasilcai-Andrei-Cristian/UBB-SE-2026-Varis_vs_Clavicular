using Windows.UI;

namespace matchmaking.Tests.Views.Converters;

public class MatchStatusDisplayConverterTests
{
    private readonly MatchStatusDisplayConverter converter = new MatchStatusDisplayConverter();

    [Theory]
    [InlineData(MatchStatus.Accepted, "Accepted")]
    [InlineData(MatchStatus.Rejected, "Rejected")]
    [InlineData(MatchStatus.Advanced, "In Review")]
    [InlineData(MatchStatus.Applied, "Pending Review")]
    public void Convert_LabelMode_ReturnsExpectedLabel(MatchStatus status, string expected)
    {
        var result = converter.Convert(status, typeof(string), "Label", string.Empty);

        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_LabelMode_WithNonMatchStatus_ReturnsPendingReview()
    {
        var result = converter.Convert("not a status", typeof(string), "Label", string.Empty);

        result.Should().Be("Pending Review");
    }

    [Fact]
    public void Convert_LabelMode_WithNull_ReturnsPendingReview()
    {
        var result = converter.Convert(null, typeof(string), "Label", string.Empty);

        result.Should().Be("Pending Review");
    }

    [Fact]
    public void GetBackgroundColor_WhenAccepted_ReturnsGreenTint()
    {
        MatchStatusDisplayConverter.GetBackgroundColor(MatchStatus.Accepted)
            .Should().Be(Color.FromArgb(0xFF, 0xDC, 0xFC, 0xE7));
    }

    [Fact]
    public void GetBackgroundColor_WhenRejected_ReturnsRedTint()
    {
        MatchStatusDisplayConverter.GetBackgroundColor(MatchStatus.Rejected)
            .Should().Be(Color.FromArgb(0xFF, 0xFE, 0xE2, 0xE2));
    }

    [Theory]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void GetBackgroundColor_WhenAppliedOrAdvanced_ReturnsYellowTint(MatchStatus status)
    {
        MatchStatusDisplayConverter.GetBackgroundColor(status)
            .Should().Be(Color.FromArgb(0xFF, 0xFE, 0xF3, 0xC7));
    }

    [Fact]
    public void GetForegroundColor_WhenAccepted_ReturnsDarkGreen()
    {
        MatchStatusDisplayConverter.GetForegroundColor(MatchStatus.Accepted)
            .Should().Be(Color.FromArgb(0xFF, 0x16, 0x65, 0x34));
    }

    [Fact]
    public void GetForegroundColor_WhenRejected_ReturnsDarkRed()
    {
        MatchStatusDisplayConverter.GetForegroundColor(MatchStatus.Rejected)
            .Should().Be(Color.FromArgb(0xFF, 0x99, 0x1B, 0x1B));
    }

    [Theory]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void GetForegroundColor_WhenAppliedOrAdvanced_ReturnsDarkOrange(MatchStatus status)
    {
        MatchStatusDisplayConverter.GetForegroundColor(status)
            .Should().Be(Color.FromArgb(0xFF, 0x92, 0x40, 0x0E));
    }

    [Fact]
    public void Convert_BackgroundMode_AcceptedAndRejected_HaveDifferentColors()
    {
        var acceptedBrush = MatchStatusDisplayConverter.GetBackgroundColor(MatchStatus.Accepted);
        var rejectedBrush = MatchStatusDisplayConverter.GetBackgroundColor(MatchStatus.Rejected);

        acceptedBrush.Should().NotBe(rejectedBrush);
    }

    [Fact]
    public void Convert_ForegroundMode_AcceptedAndRejected_HaveDifferentColors()
    {
        var acceptedBrush = MatchStatusDisplayConverter.GetForegroundColor(MatchStatus.Accepted);
        var rejectedBrush = MatchStatusDisplayConverter.GetForegroundColor(MatchStatus.Rejected);

        acceptedBrush.Should().NotBe(rejectedBrush);
    }

    [Fact]
    public void Convert_UnknownMode_ReturnsEmptyString()
    {
        var result = converter.Convert(MatchStatus.Accepted, typeof(string), "SomeOtherMode", string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void Convert_NullMode_ReturnsEmptyString()
    {
        var result = converter.Convert(MatchStatus.Accepted, typeof(string), null, string.Empty);

        result.Should().Be(string.Empty);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotSupportedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotSupportedException>();
    }
}
