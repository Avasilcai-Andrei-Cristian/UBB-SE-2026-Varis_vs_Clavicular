namespace matchmaking.Tests;

public sealed class ShellViewModelTests
{
    [Fact]
    public void ActivePage_WhenSetToRecommendations_UpdatesDerivedFlags()
    {
        var recommendationsCalled = 0;
        var myStatusCalled = 0;
        var chatCalled = 0;
        var viewModel = new ShellViewModel(
            () => recommendationsCalled++,
            () => myStatusCalled++,
            () => chatCalled++);

        viewModel.ActivePage = "Recommendations";

        viewModel.IsRecommendationsActive.Should().BeTrue();
        viewModel.IsMyStatusActive.Should().BeFalse();
        viewModel.IsChatActive.Should().BeFalse();
    }

    [Fact]
    public void ActivePage_WhenSetToChat_UpdatesDerivedFlags()
    {
        var viewModel = new ShellViewModel(() => { }, () => { }, () => { });

        viewModel.ActivePage = "Chat";

        viewModel.IsChatActive.Should().BeTrue();
        viewModel.IsRecommendationsActive.Should().BeFalse();
        viewModel.IsMyStatusActive.Should().BeFalse();
    }

    [Fact]
    public void ActivePage_WhenSetToMyStatus_UpdatesDerivedFlags()
    {
        var viewModel = new ShellViewModel(() => { }, () => { }, () => { });

        viewModel.ActivePage = "MyStatus";

        viewModel.IsMyStatusActive.Should().BeTrue();
        viewModel.IsRecommendationsActive.Should().BeFalse();
        viewModel.IsChatActive.Should().BeFalse();
    }

    [Fact]
    public void Commands_WhenExecuted_InvokeProvidedActions()
    {
        var recommendationsCalled = 0;
        var myStatusCalled = 0;
        var chatCalled = 0;
        var viewModel = new ShellViewModel(
            () => recommendationsCalled++,
            () => myStatusCalled++,
            () => chatCalled++);

        viewModel.RecommendationsCommand.Execute(null);
        viewModel.MyStatusCommand.Execute(null);
        viewModel.ChatCommand.Execute(null);

        recommendationsCalled.Should().Be(1);
        myStatusCalled.Should().Be(1);
        chatCalled.Should().Be(1);
    }
}
