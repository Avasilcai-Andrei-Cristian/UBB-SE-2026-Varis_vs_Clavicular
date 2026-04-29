namespace matchmaking.Tests;

public class SmallVmTests
{
    [Fact]
    public void PostCardViewModel_sets_author_counts_and_current_user_interaction_correctly()
    {
        var post = new Post { PostId = 1, DeveloperId = 10, ParameterType = PostParameterType.RelevantKeyword, Value = "csharp" };
        var interactions = new List<Interaction>
        {
            new() { InteractionId = 1, DeveloperId = 10, PostId = 1, Type = InteractionType.Like },
            new() { InteractionId = 2, DeveloperId = 20, PostId = 1, Type = InteractionType.Like },
            new() { InteractionId = 3, DeveloperId = 30, PostId = 1, Type = InteractionType.Dislike }
        };
        int? likedId = null;
        var vm = new PostCardViewModel(post, interactions, "Alice", 10, id => likedId = id, _ => { });

        vm.AuthorName.Should().Be("Alice");
        vm.AuthorInitial.Should().Be("A");
        vm.IsKeyword.Should().BeTrue();
        vm.LikeCount.Should().Be(2);
        vm.DislikeCount.Should().Be(1);
        vm.IsLikedByCurrentUser.Should().BeTrue();
        vm.LikeCommand.Execute(null);
        likedId.Should().Be(1);
    }

    [Fact]
    public void JobPostViewModel_Load_with_valid_job_sets_Title_Meta_and_Description()
    {
        var repo = new FakeJobRepository();
        repo.Add(new Job { JobId = 5, JobTitle = "Dev", Location = "Cluj", EmploymentType = "Full-time", JobDescription = "Write code.", CompanyId = 1 });
        var vm = new JobPostViewModel(repo);

        vm.Load(5);

        vm.Title.Should().Be("Dev");
        vm.Meta.Should().Be("Cluj · Full-time");
        vm.Description.Should().Be("Write code.");
    }

    [Fact]
    public void ShellViewModel_defaults_to_MyStatus_landing_page()
    {
        var vm = new ShellViewModel(() => { }, () => { }, () => { });

        vm.ActivePage.Should().Be("MyStatus");
        vm.IsMyStatusActive.Should().BeTrue();
        vm.IsRecommendationsActive.Should().BeFalse();
        vm.IsChatActive.Should().BeFalse();
    }

    [Fact]
    public void DeveloperViewModel_Posts_is_populated_from_service_on_construction()
    {
        var svc = new FakeDeveloperService();
        svc.SeedPost(new Post { PostId = 1, DeveloperId = 7, ParameterType = PostParameterType.RelevantKeyword, Value = "go" });
        var session = new SessionContext();
        session.LoginAsDeveloper(7);

        var vm = new DeveloperViewModel(svc, session);

        vm.Posts.Should().HaveCount(1);
    }

    [Fact]
    public void CompanyProfileViewModel_Load_with_valid_company_sets_Name_Contact_and_Jobs()
    {
        var companyRepo = new FakeCompanyRepository();
        companyRepo.Add(new Company { CompanyId = 1, CompanyName = "Acme", Email = "hr@acme.com", Phone = "0700000001" });
        var jobRepo = new FakeJobRepository();
        jobRepo.Add(new Job { JobId = 1, CompanyId = 1, JobTitle = "Dev", JobDescription = "d", Location = "Cluj", EmploymentType = "Full-time" });
        var vm = new CompanyProfileViewModel(companyRepo, jobRepo);

        vm.Load(1);

        vm.Name.Should().Be("Acme");
        vm.Contact.Should().Be("hr@acme.com · 0700000001");
        vm.Jobs.Should().Contain("1 job(s)");
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        private readonly Dictionary<int, Job> jobs = new();

        public void Add(Job job) => jobs[job.JobId] = job;
        public Job? GetById(int jobId) => jobs.TryGetValue(jobId, out var j) ? j : null;
        public IReadOnlyList<Job> GetAll() => new List<Job>(jobs.Values);
        public IReadOnlyList<Job> GetByCompanyId(int companyId) =>
            jobs.Values.Where(j => j.CompanyId == companyId).ToList();
        public void Update(Job job) => jobs[job.JobId] = job;
        public void Remove(int jobId) => jobs.Remove(jobId);
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        private readonly Dictionary<int, Company> companies = new();

        public void Add(Company company) => companies[company.CompanyId] = company;
        public Company? GetById(int companyId) => companies.TryGetValue(companyId, out var c) ? c : null;
        public IReadOnlyList<Company> GetAll() => new List<Company>(companies.Values);
        public void Update(Company company) => companies[company.CompanyId] = company;
        public void Remove(int companyId) => companies.Remove(companyId);
    }

    private sealed class FakeDeveloperService : IDeveloperService
    {
        private readonly List<Post> posts = new();
        private readonly List<Interaction> interactions = new();

        public void SeedPost(Post post) => posts.Add(post);
        public IReadOnlyList<Post> GetPosts() => posts;
        public IReadOnlyList<Interaction> GetInteractions() => interactions;
        public Developer? GetDeveloperById(int developerId) =>
            new Developer { DeveloperId = developerId, Name = $"Dev{developerId}" };
        public void AddPost(int developerId, string parameter, string value) { }
        public void AddInteraction(int developerId, int postId, InteractionType type) { }
        public void RemoveInteraction(int interactionId) { }
    }
}
