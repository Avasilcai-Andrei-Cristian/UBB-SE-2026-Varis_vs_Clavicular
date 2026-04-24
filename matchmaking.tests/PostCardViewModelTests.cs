namespace matchmaking.Tests;

public sealed class PostCardViewModelTests
{
    [Fact]
    public void Constructor_WhenPostHasKeyword_SetsDerivedDisplayValues()
    {
        var post = TestDataFactory.CreatePost(parameterType: PostParameterType.RelevantKeyword, value: "csharp");
        var interactions = new[]
        {
            TestDataFactory.CreateInteraction(interactionId: 1, developerId: 1, postId: post.PostId, type: InteractionType.Like),
            TestDataFactory.CreateInteraction(interactionId: 2, developerId: 2, postId: post.PostId, type: InteractionType.Dislike)
        };

        var viewModel = new PostCardViewModel(
            post,
            interactions,
            "Alice Pop",
            currentDeveloperId: 1,
            likePost: _ => { },
            dislikePost: _ => { });

        viewModel.AuthorName.Should().Be("Alice Pop");
        viewModel.AuthorInitial.Should().Be("A");
        viewModel.IsKeyword.Should().BeTrue();
        viewModel.TypeLabel.Should().Be("Keyword");
        viewModel.ParameterDisplayName.Should().Be("relevant keyword");
        viewModel.ValueDisplay.Should().Be("csharp");
        viewModel.LikeCount.Should().Be(1);
        viewModel.DislikeCount.Should().Be(1);
        viewModel.IsLikedByCurrentUser.Should().BeTrue();
        viewModel.IsDislikedByCurrentUser.Should().BeFalse();
    }

    [Fact]
    public void Constructor_WhenPostHasNoAuthorName_UsesFallbackInitial()
    {
        var post = TestDataFactory.CreatePost(parameterType: PostParameterType.WeightedDistanceScoreWeight, value: "42");

        var viewModel = new PostCardViewModel(
            post,
            Array.Empty<Interaction>(),
            string.Empty,
            currentDeveloperId: 1,
            likePost: _ => { },
            dislikePost: _ => { });

        viewModel.AuthorInitial.Should().Be("?");
        viewModel.TypeLabel.Should().Be("Parameter");
        viewModel.IsKeyword.Should().BeFalse();
    }

    [Fact]
    public void LikeCommand_WhenExecuted_InvokesLikeCallbackWithPostId()
    {
        var post = TestDataFactory.CreatePost(postId: 7);
        var likedIds = new List<int>();
        var viewModel = new PostCardViewModel(
            post,
            Array.Empty<Interaction>(),
            "Alice Pop",
            currentDeveloperId: 1,
            likePost: id => likedIds.Add(id),
            dislikePost: _ => { });

        viewModel.LikeCommand.Execute(null);

        likedIds.Should().Equal(7);
    }

    [Fact]
    public void DislikeCommand_WhenExecuted_InvokesDislikeCallbackWithPostId()
    {
        var post = TestDataFactory.CreatePost(postId: 7);
        var dislikedIds = new List<int>();
        var viewModel = new PostCardViewModel(
            post,
            Array.Empty<Interaction>(),
            "Alice Pop",
            currentDeveloperId: 1,
            likePost: _ => { },
            dislikePost: id => dislikedIds.Add(id));

        viewModel.DislikeCommand.Execute(null);

        dislikedIds.Should().Equal(7);
    }
}
