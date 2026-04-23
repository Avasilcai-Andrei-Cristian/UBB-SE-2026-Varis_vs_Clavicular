namespace matchmaking.Tests.Converters;

public class MatchStatusToTextConverterTests
{
    private readonly MatchStatusToTextConverter converter = new MatchStatusToTextConverter();

    [Theory]
    [InlineData(MatchStatus.Accepted, "Accepted")]
    [InlineData(MatchStatus.Rejected, "Rejected")]
    [InlineData(MatchStatus.Applied, "Applied")]
    [InlineData(MatchStatus.Advanced, "Applied")]
    public void Convert_WithMatchStatus_ReturnsExpectedText(MatchStatus status, string expected)
    {
        var result = converter.Convert(status, typeof(string), null, string.Empty);

        result.Should().Be(expected);
    }

    [Fact]
    public void Convert_WithStringValue_ReturnsAppliedText()
    {
        var result = converter.Convert("not a status", typeof(string), null, string.Empty);

        result.Should().Be("Applied");
    }

    [Fact]
    public void Convert_WithNullValue_ReturnsAppliedText()
    {
        var result = converter.Convert(null, typeof(string), null, string.Empty);

        result.Should().Be("Applied");
    }

    [Fact]
    public void Convert_WithIntegerValue_ReturnsAppliedText()
    {
        var result = converter.Convert(42, typeof(string), null, string.Empty);

        result.Should().Be("Applied");
    }

    [Fact]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotImplementedException>();
    }
}
