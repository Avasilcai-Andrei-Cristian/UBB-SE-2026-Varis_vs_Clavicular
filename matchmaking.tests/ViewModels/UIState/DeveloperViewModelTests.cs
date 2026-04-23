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

        var result = viewModel.ValidateDeveloperPostInput("mitigation factor", "0.5");

        result.Should().Be("Mitigation factor must be a number greater than or equal to 1.");
    }

    [Fact]
    public void RefreshPosts_WhenCalled_RebuildsPostsCollection()
    {
        var service = CreateService();
        var viewModel = new DeveloperViewModel(service, CreateSession());

        viewModel.Posts.Should().ContainSingle();
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
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly List<Post> _posts;

        public FakePostRepository(IReadOnlyList<Post> posts) => _posts = posts.ToList();

        public IReadOnlyList<Post> GetAll() => _posts;
        public void Add(Post post) => _posts.Add(post);
    }

    private sealed class FakeInteractionRepository : IInteractionRepository
    {
        private readonly List<Interaction> _interactions;

        public FakeInteractionRepository(IReadOnlyList<Interaction> interactions) => _interactions = interactions.ToList();

        public IReadOnlyList<Interaction> GetAll() => _interactions;
        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) => _interactions.FirstOrDefault(item => item.DeveloperId == developerId && item.PostId == postId);
        public void Add(Interaction interaction) => _interactions.Add(interaction);
        public void Update(Interaction interaction) { }
        public void Remove(int interactionId) { }
    }
}
