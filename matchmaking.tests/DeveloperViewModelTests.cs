namespace matchmaking.Tests;

public sealed class DeveloperViewModelTests
{
    [Fact]
    public void ValidateDeveloperPostInput_WhenRelevantKeywordEmpty_ReturnsError()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("relevant keyword", string.Empty);

        result.Should().Be("Keyword cannot be empty.");
    }

    [Fact]
    public void ValidateDeveloperPostInput_WhenMitigationFactorBelowOne_ReturnsError()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("mitigation factor", "0");

        result.Should().Be("Mitigation factor must be a number greater than or equal to 1.");
    }

    [Fact]
    public void ValidateDeveloperPostInput_WhenKeywordIsLowercase_ReturnsNull()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("relevant keyword", "csharp");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateDeveloperPostInput_WhenWeightIsOutOfRange_ReturnsError()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("weight", "101");

        result.Should().Be("Weight value must be a number between 0 and 100.");
    }

    [Fact]
    public void ValidateDeveloperPostInput_WhenKeywordContainsUppercase_ReturnsError()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("relevant keyword", "CSharp");

        result.Should().Be("Keyword must be all lowercase.");
    }

    [Fact]
    public void ValidateDeveloperPostInput_WhenMitigationFactorIsValid_ReturnsNull()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("mitigation factor", "1");

        result.Should().BeNull();
    }

    [Fact]
    public void ValidateDeveloperPostInput_WhenWeightIsValid_ReturnsNull()
    {
        var viewModel = CreateViewModel();

        var result = viewModel.ValidateDeveloperPostInput("weight", "50");

        result.Should().BeNull();
    }

    [Fact]
    public void RefreshPosts_WhenCalled_RebuildsPostsCollection()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.Posts.Should().ContainSingle();
    }

    [Fact]
    public void AddDeveloperPost_WhenSessionIsMissing_Throws()
    {
        var viewModel = new DeveloperViewModel(CreateService(), new SessionContext());

        Action act = () => viewModel.AddDeveloperPost("weight", "10");

        act.Should().Throw<InvalidOperationException>().WithMessage("No developer session is active.");
    }

    [Fact]
    public void HandleLikePost_WhenNoInteractionExists_AddsLike()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.HandleLikePost(1);

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.LikeCount == 1 && post.IsLikedByCurrentUser);
    }

    [Fact]
    public void HandleLikePost_WhenLikeAlreadyExists_RemovesLike()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.HandleLikePost(1);
        viewModel.HandleLikePost(1);

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.LikeCount == 0 && !post.IsLikedByCurrentUser);
    }

    [Fact]
    public void HandleDislikePost_WhenLikeExists_SwitchesToDislike()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.HandleLikePost(1);
        viewModel.HandleDislikePost(1);

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.DislikeCount == 1 && post.IsDislikedByCurrentUser);
    }

    [Fact]
    public void HandleDislikePost_WhenNoInteractionExists_AddsDislike()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.HandleDislikePost(1);

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.DislikeCount == 1 && post.IsDislikedByCurrentUser);
    }

    [Fact]
    public void HandleDislikePost_WhenDislikeAlreadyExists_RemovesDislike()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.HandleDislikePost(1);
        viewModel.HandleDislikePost(1);

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.DislikeCount == 0 && !post.IsDislikedByCurrentUser);
    }

    [Fact]
    public void HandleLikePost_WhenDislikeExists_SwitchesToLike()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());
        viewModel.HandleDislikePost(1);

        viewModel.HandleLikePost(1);

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.LikeCount == 1 && post.IsLikedByCurrentUser);
        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1 && post.DislikeCount == 0 && !post.IsDislikedByCurrentUser);
    }

    [Fact]
    public void Refresh_WhenCalled_RebuildsPostCards()
    {
        var viewModel = CreateViewModel();

        viewModel.Refresh();

        viewModel.Posts.Should().ContainSingle(post => post.PostId == 1);
    }

    private static DeveloperViewModel CreateViewModel()
    {
        return new DeveloperViewModel(CreateService(), CreateSession());
    }

    private static DeveloperService CreateService()
    {
        var developers = new[] { new Developer { DeveloperId = 1, Name = "Alice" } };
        var posts = new[] { TestDataFactory.CreatePost(postId: 1, developerId: 1) };
        var interactions = Array.Empty<Interaction>();

        return new DeveloperService(
            new FakeDeveloperRepository(developers),
            new FakePostRepository(posts),
            new FakeInteractionRepository(interactions));
    }

    private static SessionContext CreateSession()
    {
        var session = new SessionContext();
        session.LoginAsDeveloper(1);
        return session;
    }

    private sealed class FakeDeveloperRepository : IDeveloperRepository
    {
        private readonly IReadOnlyList<Developer> _developers;

        public FakeDeveloperRepository(IReadOnlyList<Developer> developers) => _developers = developers;

        public Developer? GetById(int developerId) => _developers.FirstOrDefault(item => item.DeveloperId == developerId);
        public IReadOnlyList<Developer> GetAll() => _developers;
        public void Add(Developer developer) { }
        public void Update(Developer developer) { }
        public void Remove(int developerId) { }
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly List<Post> _posts;

        public FakePostRepository(IReadOnlyList<Post> posts) => _posts = posts.ToList();

        public Post? GetById(int postId) => _posts.FirstOrDefault(item => item.PostId == postId);
        public IReadOnlyList<Post> GetAll() => _posts;
        public IReadOnlyList<Post> GetByDeveloperId(int developerId) => _posts.Where(item => item.DeveloperId == developerId).ToList();
        public void Add(Post post) => _posts.Add(post);
        public void Update(Post post) { }
        public void Remove(int postId) { }
    }

    private sealed class FakeInteractionRepository : IInteractionRepository
    {
        private readonly List<Interaction> _interactions;

        public FakeInteractionRepository(IReadOnlyList<Interaction> interactions) => _interactions = interactions.ToList();

        public Interaction? GetById(int interactionId) => _interactions.FirstOrDefault(item => item.InteractionId == interactionId);
        public IReadOnlyList<Interaction> GetAll() => _interactions;
        public IReadOnlyList<Interaction> GetByDeveloperId(int developerId) => _interactions.Where(item => item.DeveloperId == developerId).ToList();
        public IReadOnlyList<Interaction> GetByPostId(int postId) => _interactions.Where(item => item.PostId == postId).ToList();
        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) => _interactions.FirstOrDefault(item => item.DeveloperId == developerId && item.PostId == postId);
        public void Add(Interaction interaction) => _interactions.Add(interaction);
        public void Update(Interaction interaction)
        {
            var index = _interactions.FindIndex(item => item.InteractionId == interaction.InteractionId);
            if (index >= 0)
            {
                _interactions[index] = interaction;
            }
        }

        public void Remove(int interactionId) => _interactions.RemoveAll(item => item.InteractionId == interactionId);
    }
}
