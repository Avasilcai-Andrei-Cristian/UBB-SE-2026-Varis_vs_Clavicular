using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;

namespace matchmaking.Tests;

public sealed class ProfileAndDeveloperCoverageTests
{
    [Fact]
    public void CompanyProfileViewModel_Load_WhenCompanyExists_PopulatesContactAndJobs()
    {
        var company = TestDataFactory.CreateCompany(3);
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var viewModel = new CompanyProfileViewModel(new FakeCompanyRepository(new[] { company }), new FakeJobRepository(new[] { job }));

        viewModel.Load(company.CompanyId);

        viewModel.Name.Should().Be(company.CompanyName);
        viewModel.Contact.Should().Contain(company.Email);
        viewModel.Jobs.Should().Contain("1 job");
    }

    [Fact]
    public void CompanyProfileViewModel_Load_WhenCompanyIdIsInvalid_SetsUnknownCompany()
    {
        var viewModel = new CompanyProfileViewModel(new FakeCompanyRepository(Array.Empty<Company>()), new FakeJobRepository(Array.Empty<Job>()));

        viewModel.Load(0);

        viewModel.Name.Should().Be("Unknown company");
        viewModel.Contact.Should().BeEmpty();
    }

    [Fact]
    public void CompanyProfileViewModel_Load_WhenCompanyMissing_SetsNotFoundCompany()
    {
        var viewModel = new CompanyProfileViewModel(new FakeCompanyRepository(Array.Empty<Company>()), new FakeJobRepository(Array.Empty<Job>()));

        viewModel.Load(99);

        viewModel.Name.Should().Be("Company not found");
    }

    [Fact]
    public void JobPostViewModel_Load_WhenJobExists_PopulatesFields()
    {
        var job = TestDataFactory.CreateJob();
        var viewModel = new JobPostViewModel(new FakeJobRepository(new[] { job }));

        viewModel.Load(job.JobId);

        viewModel.Title.Should().Be(job.JobTitle);
        viewModel.Meta.Should().Contain(job.Location);
        viewModel.Description.Should().Be(job.JobDescription);
    }

    [Fact]
    public void JobPostViewModel_Load_WhenJobIdIsInvalid_SetsUnknownJob()
    {
        var viewModel = new JobPostViewModel(new FakeJobRepository(Array.Empty<Job>()));

        viewModel.Load(0);

        viewModel.Title.Should().Be("Unknown job");
    }

    [Fact]
    public void JobPostViewModel_Load_WhenJobMissing_SetsNotFoundJob()
    {
        var viewModel = new JobPostViewModel(new FakeJobRepository(Array.Empty<Job>()));

        viewModel.Load(99);

        viewModel.Title.Should().Be("Job not found");
    }

    [Fact]
    public void UserProfileViewModel_Load_WhenUserExists_PopulatesProfile()
    {
        var user = TestDataFactory.CreateUser(8);
        var viewModel = new UserProfileViewModel(new FakeUserRepository(new[] { user }));

        viewModel.Load(user.UserId);

        viewModel.Name.Should().Be(user.Name);
        viewModel.Meta.Should().Contain(user.Location);
        viewModel.Contact.Should().Contain(user.Email);
        viewModel.Resume.Should().Be(user.Resume);
    }

    [Fact]
    public void UserProfileViewModel_Load_WhenUserIdIsInvalid_SetsUnknownUser()
    {
        var viewModel = new UserProfileViewModel(new FakeUserRepository(Array.Empty<User>()));

        viewModel.Load(0);

        viewModel.Name.Should().Be("Unknown user");
    }

    [Fact]
    public void UserProfileViewModel_Load_WhenUserMissing_SetsNotFoundUser()
    {
        var viewModel = new UserProfileViewModel(new FakeUserRepository(Array.Empty<User>()));

        viewModel.Load(99);

        viewModel.Name.Should().Be("User not found");
    }

    [Fact]
    public void DeveloperViewModel_ValidateDeveloperPostInput_WhenKeywordIsUppercase_ReturnsError()
    {
        var viewModel = CreateDeveloperViewModel();

        var result = viewModel.ValidateDeveloperPostInput("relevant keyword", "CSharp");

        result.Should().Be("Keyword must be all lowercase.");
    }

    [Fact]
    public void DeveloperViewModel_ValidateDeveloperPostInput_WhenWeightIsOutOfRange_ReturnsError()
    {
        var viewModel = CreateDeveloperViewModel();

        var result = viewModel.ValidateDeveloperPostInput("preference score weight", "101");

        result.Should().Be("Weight value must be a number between 0 and 100.");
    }

    [Fact]
    public void DeveloperViewModel_AddDeveloperPost_WhenSessionExists_AddsPost()
    {
        var service = CreateDeveloperService();
        var session = new SessionContext();
        session.LoginAsDeveloper(1);
        var viewModel = new DeveloperViewModel(service, session);

        viewModel.AddDeveloperPost("relevant keyword", "cloud");

        service.PostRepository.AddedPosts.Should().ContainSingle();
        service.PostRepository.AddedPosts[0].DeveloperId.Should().Be(1);
    }

    [Fact]
    public void DeveloperViewModel_HandleLikePost_WhenNoExistingInteraction_AddsLike()
    {
        var service = CreateDeveloperService();
        var session = new SessionContext();
        session.LoginAsDeveloper(1);
        var viewModel = new DeveloperViewModel(service, session);

        viewModel.HandleLikePost(1);

        service.InteractionRepository.AddedInteractions.Should().ContainSingle(item => item.Type == InteractionType.Like);
    }

    private static DeveloperViewModel CreateDeveloperViewModel()
    {
        return new DeveloperViewModel(CreateDeveloperService(), CreateDeveloperSession());
    }

    private static SessionContext CreateDeveloperSession()
    {
        var session = new SessionContext();
        session.LoginAsDeveloper(1);
        return session;
    }

    private static TestDeveloperService CreateDeveloperService()
    {
        var developers = new[] { new Developer { DeveloperId = 1, Name = "Alice" } };
        var posts = new[] { TestDataFactory.CreatePost(postId: 1, developerId: 1) };
        var interactions = Array.Empty<Interaction>();
        return new TestDeveloperService(
            new FakeDeveloperRepository(developers),
            new FakePostRepository(posts),
            new FakeInteractionRepository(interactions));
    }

    private sealed class TestDeveloperService : DeveloperService
    {
        public FakePostRepository PostRepository { get; }
        public FakeInteractionRepository InteractionRepository { get; }

        public TestDeveloperService(FakeDeveloperRepository developerRepository, FakePostRepository postRepository, FakeInteractionRepository interactionRepository)
            : base(developerRepository, postRepository, interactionRepository)
        {
            PostRepository = postRepository;
            InteractionRepository = interactionRepository;
        }
    }

    private sealed class FakeDeveloperRepository : IDeveloperRepository
    {
        private readonly IReadOnlyList<Developer> developers;

        public FakeDeveloperRepository(IReadOnlyList<Developer> developers)
        {
            this.developers = developers;
        }

        public Developer? GetById(int developerId) => developers.FirstOrDefault(item => item.DeveloperId == developerId);
        public IReadOnlyList<Developer> GetAll() => developers;
        public void Add(Developer developer) { }
        public void Update(Developer developer) { }
        public void Remove(int developerId) { }
    }

    private sealed class FakePostRepository : IPostRepository
    {
        private readonly List<Post> posts;

        public FakePostRepository(IReadOnlyList<Post> posts)
        {
            this.posts = posts.ToList();
        }

        public List<Post> AddedPosts { get; } = new List<Post>();
        public Post? GetById(int postId) => posts.FirstOrDefault(item => item.PostId == postId);
        public IReadOnlyList<Post> GetAll() => posts;
        public IReadOnlyList<Post> GetByDeveloperId(int developerId) => posts.Where(item => item.DeveloperId == developerId).ToList();
        public void Add(Post post) => AddedPosts.Add(post);
        public void Update(Post post) { }
        public void Remove(int postId) { }
    }

    private sealed class FakeInteractionRepository : IInteractionRepository
    {
        private readonly List<Interaction> interactions;

        public FakeInteractionRepository(IReadOnlyList<Interaction> interactions)
        {
            this.interactions = interactions.ToList();
        }

        public List<Interaction> AddedInteractions { get; } = new List<Interaction>();
        public List<Interaction> UpdatedInteractions { get; } = new List<Interaction>();
        public List<int> RemovedInteractionIds { get; } = new List<int>();

        public Interaction? GetById(int interactionId) => interactions.FirstOrDefault(item => item.InteractionId == interactionId);
        public IReadOnlyList<Interaction> GetAll() => interactions;
        public IReadOnlyList<Interaction> GetByDeveloperId(int developerId) => interactions.Where(item => item.DeveloperId == developerId).ToList();
        public IReadOnlyList<Interaction> GetByPostId(int postId) => interactions.Where(item => item.PostId == postId).ToList();
        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) => interactions.FirstOrDefault(item => item.DeveloperId == developerId && item.PostId == postId);
        public void Add(Interaction interaction) => AddedInteractions.Add(interaction);
        public void Update(Interaction interaction) => UpdatedInteractions.Add(interaction);
        public void Remove(int interactionId) => RemovedInteractionIds.Add(interactionId);
    }
}
