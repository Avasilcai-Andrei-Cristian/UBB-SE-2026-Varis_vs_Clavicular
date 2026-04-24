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

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_BackgroundMode_ReturnsSolidColorBrush(MatchStatus status)
    {
        var result = MatchStatusDisplayConverter.GetBackgroundColor(status);

        result.Should().NotBe(default);
    }

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_ForegroundMode_ReturnsSolidColorBrush(MatchStatus status)
    {
        var result = MatchStatusDisplayConverter.GetForegroundColor(status);

        result.Should().NotBe(default);
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
