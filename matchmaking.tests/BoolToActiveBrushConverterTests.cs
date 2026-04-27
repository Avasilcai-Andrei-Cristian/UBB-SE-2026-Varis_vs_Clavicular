namespace matchmaking.Tests.Views.Converters;

public class BoolToActiveBrushConverterTests
{
    private static readonly Windows.UI.Color ExpectedActiveColor = Windows.UI.Color.FromArgb(0xFF, 0x25, 0x63, 0xEB);
    private static readonly Windows.UI.Color ExpectedInactiveColor = Windows.UI.Color.FromArgb(0xFF, 0x6B, 0x6B, 0x6B);

    private readonly BoolToActiveBrushConverter converter = new BoolToActiveBrushConverter();

    [Fact]
    public void GetColor_ActiveAndInactiveStates_ReturnExpectedDistinctColors()
    {
        var activeColor = BoolToActiveBrushConverter.GetColor(true);
        var inactiveColor = BoolToActiveBrushConverter.GetColor(false);

        activeColor.Should().Be(ExpectedActiveColor);
        inactiveColor.Should().Be(ExpectedInactiveColor);
        inactiveColor.Should().NotBe(activeColor);
    }

    [Fact]
    public void ConvertBack_WhenInvoked_ThrowsNotSupportedException()
    {
        var act = () => converter.ConvertBack(null, typeof(object), null, string.Empty);

        act.Should().Throw<NotSupportedException>();
    }
}
