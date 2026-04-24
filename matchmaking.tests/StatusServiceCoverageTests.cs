namespace matchmaking.Tests;

public sealed class StatusServiceCoverageTests
{
    [Fact]
    public void SkillGapService_GetMissingSkills_WhenNoRejections_ReturnsEmpty()
    {
        var service = CreateSkillGapService(Array.Empty<Match>());

        var result = service.GetMissingSkills(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SkillGapService_GetUnderscoredSkills_WhenNoRejections_ReturnsEmpty()
    {
        var service = CreateSkillGapService(Array.Empty<Match>());

        var result = service.GetUnderscoredSkills(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SkillGapService_GetSummary_WhenNoRejections_ReturnsNoGapSummary()
    {
        var service = CreateSkillGapService(Array.Empty<Match>());

        var summary = service.GetSummary(1);

        summary.HasRejections.Should().BeFalse();
        summary.HasSkillGaps.Should().BeFalse();
    }

    [Fact]
    public async Task CompanyStatusService_GetApplicantsForCompanyAsync_WhenUnknownEntitiesArePresent_SkipsThem()
    {
        var company = TestDataFactory.CreateCompany();
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");

        var jobRepository = new FakeJobRepository(new[] { job });
        var service = new CompanyStatusService(
            new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
            new LocalUserService(new[] { user }),
            new JobService(new FakeJobRepository(Array.Empty<Job>())),
            new LocalSkillService(Array.Empty<Skill>()));

        var applicants = await service.GetApplicantsForCompanyAsync(company.CompanyId);

        applicants.Should().BeEmpty();
    }

    [Fact]
    public void UserStatusService_GetApplicationsForUser_WhenJobMissing_SkipsMatch()
    {
        var match = TestDataFactory.CreateMatch(1, 1, 999, MatchStatus.Accepted, "feedback");
        var service = CreateUserStatusService(new[] { match }, Array.Empty<Job>());

        var applications = service.GetApplicationsForUser(1);

        applications.Should().BeEmpty();
    }

    [Fact]
    public void AddInteraction_WhenInteractionAlreadyExists_UpdatesExistingRecordInsteadOfAdding()
    {
        var interaction = TestDataFactory.CreateInteraction(1, 1, 1, InteractionType.Like);
        var service = new DeveloperService(
            new LocalDeveloperRepository(new[] { new Developer { DeveloperId = 1, Name = "Alice" } }),
            new LocalPostRepository(new[] { TestDataFactory.CreatePost(postId: 1, developerId: 1) }),
            new LocalInteractionRepository(new[] { interaction }));

        service.AddInteraction(1, 1, InteractionType.Dislike);

        interaction.Type.Should().Be(InteractionType.Dislike);
    }

    private static SkillGapService CreateSkillGapService(IReadOnlyList<Match> matches)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80);
        var jobSkill = TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 90);

        var matchRepository = new FakeMatchRepository(matches);
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(new[] { jobSkill }));
        var skillService = new SkillService(new FakeSkillRepository(new[] { skill }));

        return new SkillGapService(matchRepository, jobSkillService, skillService);
    }

    private static UserStatusService CreateUserStatusService(IReadOnlyList<Match> matches, IReadOnlyList<Job> jobs)
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 40);
        var jobSkill = TestDataFactory.CreateJobSkill(jobs.FirstOrDefault()?.JobId ?? 100, 1, "C#", 70);

        var matchRepository = new FakeMatchRepository(matches);
        var jobRepository = new FakeJobRepository(jobs);
        var companyRepository = new FakeCompanyRepository(new[] { company });
        var skillRepository = new FakeSkillRepository(new[] { skill });
        var jobSkillRepository = new FakeJobSkillRepository(new[] { jobSkill });

        return new UserStatusService(
            matchRepository,
            new JobService(jobRepository),
            new CompanyService(companyRepository),
            new SkillService(skillRepository),
            new JobSkillService(jobSkillRepository));
    }

    private sealed class LocalUserService : IUserService
    {
        private readonly IReadOnlyList<User> users;

        public LocalUserService(IReadOnlyList<User> users) => this.users = users;

        public User? GetById(int userId) => users.FirstOrDefault(item => item.UserId == userId);
        public IReadOnlyList<User> GetAll() => users;
        public void Add(User user) { }
        public void Update(User user) { }
        public void Remove(int userId) { }
    }

    private sealed class LocalDeveloperRepository : IDeveloperRepository
    {
        private readonly IReadOnlyList<Developer> developers;

        public LocalDeveloperRepository(IReadOnlyList<Developer> developers) => this.developers = developers;

        public Developer? GetById(int developerId) => developers.FirstOrDefault(item => item.DeveloperId == developerId);
    }

    private sealed class LocalPostRepository : IPostRepository
    {
        private readonly List<Post> posts;

        public LocalPostRepository(IReadOnlyList<Post> posts) => this.posts = posts.ToList();

        public IReadOnlyList<Post> GetAll() => posts;
        public void Add(Post post) => posts.Add(post);
    }

    private sealed class LocalInteractionRepository : IInteractionRepository
    {
        private readonly List<Interaction> interactions;

        public LocalInteractionRepository(IReadOnlyList<Interaction> interactions) => this.interactions = interactions.ToList();

        public IReadOnlyList<Interaction> GetAll() => interactions;
        public Interaction? GetByDeveloperIdAndPostId(int developerId, int postId) => interactions.FirstOrDefault(item => item.DeveloperId == developerId && item.PostId == postId);
        public Interaction? GetById(int interactionId) => interactions.FirstOrDefault(item => item.InteractionId == interactionId);
        public IReadOnlyList<Interaction> GetByDeveloperId(int developerId) => interactions.Where(item => item.DeveloperId == developerId).ToList();
        public IReadOnlyList<Interaction> GetByPostId(int postId) => interactions.Where(item => item.PostId == postId).ToList();
        public void Add(Interaction interaction) => interactions.Add(interaction);
        public void Update(Interaction interaction) { }
        public void Remove(int interactionId) { }
    }

    private sealed class LocalSkillService : ISkillService
    {
        private readonly IReadOnlyList<Skill> skills;

        public LocalSkillService(IReadOnlyList<Skill> skills) => this.skills = skills;

        public Skill? GetById(int userId, int skillId) => skills.FirstOrDefault(item => item.UserId == userId && item.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => skills;
        public IReadOnlyList<Skill> GetByUserId(int userId) => skills.Where(item => item.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skills.GroupBy(item => item.SkillId).Select(group => (group.Key, group.First().SkillName)).ToList();
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }

}
