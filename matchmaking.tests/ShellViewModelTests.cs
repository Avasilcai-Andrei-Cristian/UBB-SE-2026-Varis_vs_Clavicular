namespace matchmaking.Tests;

public sealed class ShellViewModelTests
{
    [Fact]
    public void ActivePage_WhenSetToRecommendations_UpdatesDerivedFlags()
    {
        var viewModel = new ShellViewModel(() => { }, () => { }, () => { });

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
    public void RecommendationsCommand_WhenExecuted_InvokesProvidedAction()
    {
        var recommendationsCalled = 0;
        var viewModel = new ShellViewModel(
            () => recommendationsCalled++,
            () => { },
            () => { });

        viewModel.RecommendationsCommand.Execute(null);

        recommendationsCalled.Should().Be(1);
    }

    [Fact]
    public void MyStatusCommand_WhenExecuted_InvokesProvidedAction()
    {
        var myStatusCalled = 0;
        var viewModel = new ShellViewModel(
            () => { },
            () => myStatusCalled++,
            () => { });

        viewModel.MyStatusCommand.Execute(null);

        myStatusCalled.Should().Be(1);
    }

    [Fact]
    public void ChatCommand_WhenExecuted_InvokesProvidedAction()
    {
        var chatCalled = 0;
        var viewModel = new ShellViewModel(
            () => { },
            () => { },
            () => chatCalled++);

        viewModel.ChatCommand.Execute(null);

        chatCalled.Should().Be(1);
    }
}
