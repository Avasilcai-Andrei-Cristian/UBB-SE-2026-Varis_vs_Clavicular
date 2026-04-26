using matchmaking.Models;

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
    public void UserStatusService_GetApplicationsForUser_WhenCompanyIsNull_UsesUnknownCompanyName()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob(companyId: 999);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Applied, "feedback");

        var matchRepository = new FakeMatchRepository(new[] { match });
        var jobRepository = new FakeJobRepository(new[] { job });
        var companyRepository = new FakeCompanyRepository(Array.Empty<Company>());
        var skillRepository = new FakeSkillRepository(Array.Empty<Skill>());
        var jobSkillRepository = new FakeJobSkillRepository(Array.Empty<JobSkill>());

        var service = new UserStatusService(
            matchRepository,
            new JobService(jobRepository),
            new CompanyService(companyRepository),
            new SkillService(skillRepository),
            new JobSkillService(jobSkillRepository));

        var applications = service.GetApplicationsForUser(user.UserId);

        applications.Should().ContainSingle(app => app.CompanyName == "Unknown Company");
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

    [Fact]
    public void SkillGapService_GetUnderscoredSkills_WhenSameSkillAppearsInMultipleRejectedJobs_AveragesRequiredScores()
    {
        var user = TestDataFactory.CreateUser();
        var job1 = TestDataFactory.CreateJob(jobId: 100, companyId: 1);
        var job2 = TestDataFactory.CreateJob(jobId: 101, companyId: 1);
        var match1 = TestDataFactory.CreateMatch(matchId: 1, userId: user.UserId, jobId: job1.JobId, status: MatchStatus.Rejected);
        var match2 = TestDataFactory.CreateMatch(matchId: 2, userId: user.UserId, jobId: job2.JobId, status: MatchStatus.Rejected);

        var service = new SkillGapService(
            new FakeMatchRepository(new[] { match1, match2 }),
            new JobSkillService(new FakeJobSkillRepository(new[]
            {
                TestDataFactory.CreateJobSkill(job1.JobId, 1, "C#", 80),
                TestDataFactory.CreateJobSkill(job2.JobId, 1, "C#", 100),
            })),
            new SkillService(new FakeSkillRepository(new[]
            {
                TestDataFactory.CreateSkill(user.UserId, 1, "C#", 60),
            })));

        var result = service.GetUnderscoredSkills(user.UserId);

        result.Should().ContainSingle().Which.Should().BeEquivalentTo(
            new UnderscoredSkillModel { SkillName = "C#", UserScore = 60, AverageRequiredScore = 90 },
            options => options
                .Including(item => item.SkillName)
                .Including(item => item.UserScore)
                .Including(item => item.AverageRequiredScore));
    }

    [Fact]
    public void SkillGapService_GetUnderscoredSkills_WhenMultipleSkillsExist_SortsByLargestGapFirst()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(userId: user.UserId, jobId: job.JobId, status: MatchStatus.Rejected);

        var service = new SkillGapService(
            new FakeMatchRepository(new[] { match }),
            new JobSkillService(new FakeJobSkillRepository(new[]
            {
                TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 90),
                TestDataFactory.CreateJobSkill(job.JobId, 2, "Python", 90),
            })),
            new SkillService(new FakeSkillRepository(new[]
            {
                TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80),
                TestDataFactory.CreateSkill(user.UserId, 2, "Python", 70),
            })));

        var result = service.GetUnderscoredSkills(user.UserId);

        result.Should().HaveCount(2);
        result[0].SkillName.Should().Be("Python");
        result[1].SkillName.Should().Be("C#");
    }

    [Fact]
    public void SkillGapService_GetMissingSkills_WhenSkillsAppearInMultipleRejections_SortsByFrequencyDescending()
    {
        var user = TestDataFactory.CreateUser();
        var job1 = TestDataFactory.CreateJob(jobId: 200, companyId: 1);
        var job2 = TestDataFactory.CreateJob(jobId: 201, companyId: 1);
        var job3 = TestDataFactory.CreateJob(jobId: 202, companyId: 1);
        var job4 = TestDataFactory.CreateJob(jobId: 203, companyId: 1);

        var service = new SkillGapService(
            new FakeMatchRepository(new[]
            {
                TestDataFactory.CreateMatch(matchId: 10, userId: user.UserId, jobId: job1.JobId, status: MatchStatus.Rejected),
                TestDataFactory.CreateMatch(matchId: 11, userId: user.UserId, jobId: job2.JobId, status: MatchStatus.Rejected),
                TestDataFactory.CreateMatch(matchId: 12, userId: user.UserId, jobId: job3.JobId, status: MatchStatus.Rejected),
                TestDataFactory.CreateMatch(matchId: 13, userId: user.UserId, jobId: job4.JobId, status: MatchStatus.Rejected),
            }),
            new JobSkillService(new FakeJobSkillRepository(new[]
            {
                TestDataFactory.CreateJobSkill(job1.JobId, 10, "Rust", 80),
                TestDataFactory.CreateJobSkill(job2.JobId, 10, "Rust", 80),
                TestDataFactory.CreateJobSkill(job3.JobId, 10, "Rust", 80),
                TestDataFactory.CreateJobSkill(job4.JobId, 11, "Go", 70),
            })),
            new SkillService(new FakeSkillRepository(Array.Empty<Skill>())));

        var result = service.GetMissingSkills(user.UserId);

        result.Should().HaveCount(2);
        result[0].SkillName.Should().Be("Rust");
        result[1].SkillName.Should().Be("Go");
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
        public IReadOnlyList<Developer> GetAll() => developers;
        public void Add(Developer developer) { }
        public void Update(Developer developer) { }
        public void Remove(int developerId) { }
    }

    private sealed class LocalPostRepository : IPostRepository
    {
        private readonly List<Post> posts;

        public LocalPostRepository(IReadOnlyList<Post> posts) => this.posts = posts.ToList();

        public Post? GetById(int postId) => posts.FirstOrDefault(item => item.PostId == postId);
        public IReadOnlyList<Post> GetAll() => posts;
        public IReadOnlyList<Post> GetByDeveloperId(int developerId) => posts.Where(item => item.DeveloperId == developerId).ToList();
        public void Add(Post post) => posts.Add(post);
        public void Update(Post post) { }
        public void Remove(int postId) { }
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
