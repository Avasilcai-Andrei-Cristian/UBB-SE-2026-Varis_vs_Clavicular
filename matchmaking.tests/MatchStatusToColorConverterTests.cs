namespace matchmaking.Tests.Converters;

public class MatchStatusToColorConverterTests
{
    private readonly MatchStatusToColorConverter converter = new MatchStatusToColorConverter();

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_WithMatchStatus_ReturnsColor(MatchStatus status)
    {
        var result = MatchStatusToColorConverter.GetColor(status);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_AcceptedAndRejected_ReturnDifferentColors()
    {
        var acceptedBrush = MatchStatusToColorConverter.GetColor(MatchStatus.Accepted);
        var rejectedBrush = MatchStatusToColorConverter.GetColor(MatchStatus.Rejected);

        acceptedBrush.Should().NotBe(rejectedBrush);
    }

    [Fact]
    public void Convert_AppliedAndAccepted_ReturnDifferentColors()
    {
        var appliedBrush = MatchStatusToColorConverter.GetColor(MatchStatus.Applied);
        var acceptedBrush = MatchStatusToColorConverter.GetColor(MatchStatus.Accepted);

        appliedBrush.Should().NotBe(acceptedBrush);
    }

    [Fact]
    public void GetColor_WithAdvancedStatus_ReturnsSameFallbackAsApplied()
    {
        var advancedColor = MatchStatusToColorConverter.GetColor(MatchStatus.Advanced);

        advancedColor.Should().Be(MatchStatusToColorConverter.GetColor(MatchStatus.Applied));
    }

    [Fact]
    public void GetColor_WithUnknownEnumValue_ReturnsAppliedColor()
    {
        var unknownStatus = (MatchStatus)999;

        var result = MatchStatusToColorConverter.GetColor(unknownStatus);

        result.Should().Be(MatchStatusToColorConverter.GetColor(MatchStatus.Applied));
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }

    [Fact]
    public void ConvertBack_WhenInvokedWithNonNullParameter_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(MatchStatus.Applied, typeof(object), "param", string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}
