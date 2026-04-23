namespace matchmaking.Tests;

public sealed class FilterCheckItemTests
{
    [Fact]
    public void Constructor_WhenCreated_SetsLabelAndUncheckedState()
    {
        var item = new FilterCheckItem("Full-time");

        item.Label.Should().Be("Full-time");
        item.IsChecked.Should().BeFalse();
    }

    [Fact]
    public void IsChecked_WhenUpdated_PersistsValue()
    {
        var item = new FilterCheckItem("Hybrid");

        item.IsChecked = true;

        item.IsChecked.Should().BeTrue();
    }
}
