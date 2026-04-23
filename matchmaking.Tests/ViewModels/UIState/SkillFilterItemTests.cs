namespace matchmaking.Tests;

public sealed class SkillFilterItemTests
{
    [Fact]
    public void Constructor_WhenCreated_SetsIdentityAndUncheckedState()
    {
        var item = new SkillFilterItem(7, "C#");

        item.SkillId.Should().Be(7);
        item.Name.Should().Be("C#");
        item.IsChecked.Should().BeFalse();
    }

    [Fact]
    public void IsChecked_WhenUpdated_PersistsValue()
    {
        var item = new SkillFilterItem(7, "C#");

        item.IsChecked = true;

        item.IsChecked.Should().BeTrue();
    }
}
