namespace matchmaking.Tests;

public sealed class SampleFormViewModelTests
{
    [Fact]
    public void FormTitle_WhenAccessed_ReturnsExpectedText()
    {
        var viewModel = new SampleFormViewModel();

        viewModel.FormTitle.Should().Be("Demo Form Section");
    }

    [Fact]
    public void FirstField_WhenSet_UpdatesStoredValue()
    {
        var viewModel = new SampleFormViewModel();

        viewModel.FirstField = "alpha";

        viewModel.FirstField.Should().Be("alpha");
    }

    [Fact]
    public void SecondField_WhenSet_UpdatesStoredValue()
    {
        var viewModel = new SampleFormViewModel();

        viewModel.SecondField = "beta";

        viewModel.SecondField.Should().Be("beta");
    }

    [Fact]
    public void PrimaryActionCommand_WhenExecuted_DoesNotThrow()
    {
        var viewModel = new SampleFormViewModel();

        Action act = () => viewModel.PrimaryActionCommand.Execute(null);

        act.Should().NotThrow();
    }

    [Fact]
    public void SecondaryActionCommand_WhenExecuted_DoesNotThrow()
    {
        var viewModel = new SampleFormViewModel();

        Action act = () => viewModel.SecondaryActionCommand.Execute(null);

        act.Should().NotThrow();
    }
}
