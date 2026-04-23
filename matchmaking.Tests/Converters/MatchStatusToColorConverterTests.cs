namespace matchmaking.Tests.Converters;

public class MatchStatusToColorConverterTests
{
    private readonly MatchStatusToColorConverter converter = new();

    [Theory]
    [InlineData(MatchStatus.Accepted)]
    [InlineData(MatchStatus.Rejected)]
    [InlineData(MatchStatus.Applied)]
    [InlineData(MatchStatus.Advanced)]
    public void Convert_WithMatchStatus_ReturnsSolidColorBrush(MatchStatus status)
    {
        var result = MatchStatusToColorConverter.GetColor(status);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_WithNonMatchStatusValue_ReturnsSolidColorBrush()
    {
        var result = MatchStatusToColorConverter.GetColor(MatchStatus.Applied);

        result.Should().NotBe(default);
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsSolidColorBrush()
    {
        var result = MatchStatusToColorConverter.GetColor(MatchStatus.Applied);

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
    public void ConvertBack_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}
