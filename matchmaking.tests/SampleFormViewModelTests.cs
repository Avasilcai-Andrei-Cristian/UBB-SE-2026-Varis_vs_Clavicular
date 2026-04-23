namespace matchmaking.Tests;

public sealed class SampleFormViewModelTests
{
    [Fact]
    public void FormTitle_ReturnsExpectedText()
    {
        var viewModel = new SampleFormViewModel();

        viewModel.FormTitle.Should().Be("Demo Form Section");
    }

    [Fact]
    public void Properties_WhenSet_UpdateStoredValues()
    {
        var viewModel = new SampleFormViewModel();

        viewModel.FirstField = "alpha";
        viewModel.SecondField = "beta";

        viewModel.FirstField.Should().Be("alpha");
        viewModel.SecondField.Should().Be("beta");
    }

    [Fact]
    public void Commands_WhenExecuted_DoNotThrow()
    {
        var viewModel = new SampleFormViewModel();

        Action act = () =>
        {
            viewModel.PrimaryActionCommand.Execute(null);
            viewModel.SecondaryActionCommand.Execute(null);
        };

        act.Should().NotThrow();
    }
}
