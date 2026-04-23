using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Tests;

public sealed class CoreBehaviorCoverageTests
{
    [Fact]
    public void RunOrThrow_WhenQuickTestRuns_DoesNotThrow()
    {
        Action act = RecommendationAlgorithmQuickTest.RunOrThrow;

        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0, "Internship")]
    [InlineData(1, "Internship")]
    [InlineData(2, "Entry")]
    [InlineData(4, "MidSenior")]
    [InlineData(7, "Director")]
    [InlineData(10, "Executive")]
    public void MapUserYearsToExperienceBucket_WhenYearsOfExperienceVary_ReturnsExpectedBucketForRecommendations(int yearsOfExperience, string expected)
    {
        UserRecommendationService.MapUserYearsToExperienceBucket(yearsOfExperience).Should().Be(expected);
    }

    [Fact]
    public void GetNextCard_WhenUserDoesNotExist_ThrowsInvalidOperationException()
    {
        var service = CreateUserRecommendationService(Array.Empty<User>(), Array.Empty<Job>(), Array.Empty<Skill>(), Array.Empty<JobSkill>(), Array.Empty<Company>(), Array.Empty<Match>(), Array.Empty<Recommendation>());

        Action act = () => service.GetNextCard(1, UserMatchmakingFilters.Empty());

        act.Should().Throw<InvalidOperationException>().WithMessage("User not found.*");
    }

    [Fact]
    public void RecalculateTopCardIgnoringCooldown_WhenRecommendationIsRecent_ReturnsCard()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var recommendation = TestDataFactory.CreateRecommendation(1, user.UserId, job.JobId, DateTime.UtcNow.AddMinutes(-5));

        var service = CreateUserRecommendationService(
            new[] { user },
            new[] { job },
            new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) },
            new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            Array.Empty<Match>(),
            new[] { recommendation });

        var card = service.RecalculateTopCardIgnoringCooldown(user.UserId, UserMatchmakingFilters.Empty());

        card.Should().NotBeNull();
        card!.Job.JobId.Should().Be(job.JobId);
    }

    [Fact]
    public void ApplyLike_WhenMatchAlreadyExists_ThrowsInvalidOperationException()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied);
        var card = new JobRecommendationResult { Job = job, Company = TestDataFactory.CreateCompany(job.CompanyId) };

        var service = CreateUserRecommendationService(
            new[] { user },
            new[] { job },
            new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) },
            new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            new[] { match },
            Array.Empty<Recommendation>());

        Action act = () => service.ApplyLike(user.UserId, card);

        act.Should().Throw<InvalidOperationException>().WithMessage("Already applied*");
    }

    [Fact]
    public void IsOnCooldown_WhenCooldownPeriodIsNonPositive_UsesDefaultCooldownPeriod()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var recommendation = TestDataFactory.CreateRecommendation(1, user.UserId, job.JobId, DateTime.UtcNow.AddHours(-1));
        var service = new CooldownService(new FakeRecommendationRepository(new[] { recommendation }), TimeSpan.Zero);

        service.IsOnCooldown(user.UserId, job.JobId, DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public void IsOnCooldown_WhenTimestampIsLocal_NormalizesToUtc()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var localTimestamp = DateTime.SpecifyKind(DateTime.Now.AddHours(-1), DateTimeKind.Local);
        var recommendation = TestDataFactory.CreateRecommendation(1, user.UserId, job.JobId, localTimestamp);
        var service = new CooldownService(new FakeRecommendationRepository(new[] { recommendation }), TimeSpan.FromHours(24));

        service.IsOnCooldown(user.UserId, job.JobId, DateTime.UtcNow).Should().BeTrue();
    }

    [Fact]
    public async Task GetApplicantsForCompanyAsync_WhenUserHasNoSkills_ReturnsZeroCompatibilityScore()
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "ok");

        var service = CreateCompanyStatusService(
            new[] { user },
            new[] { company },
            new[] { job },
            Array.Empty<Skill>(),
            new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            new[] { match });

        var applicants = await service.GetApplicantsForCompanyAsync(company.CompanyId);

        applicants.Should().ContainSingle();
        applicants[0].CompatibilityScore.Should().Be(0);
    }

    [Fact]
    public void GetApplicationsForUser_WhenJobIsMissing_SkipsMissingJob()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "ok");

        var service = CreateUserStatusService(
            new[] { user },
            Array.Empty<Company>(),
            Array.Empty<Job>(),
            new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            new[] { match });

        var applications = service.GetApplicationsForUser(user.UserId);

        applications.Should().BeEmpty();
    }

    private static UserRecommendationService CreateUserRecommendationService(
        IReadOnlyList<User> users,
        IReadOnlyList<Job> jobs,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Company> companies,
        IReadOnlyList<Match> matches,
        IReadOnlyList<Recommendation> recommendations)
    {
        var userRepository = new FakeUserRepository(users);
        var jobRepository = new FakeJobRepository(jobs);
        var skillRepository = new FakeSkillRepository(skills);
        var jobSkillRepository = new FakeJobSkillRepository(jobSkills);
        var companyRepository = new FakeCompanyRepository(companies);
        var matchService = new MatchService(new FakeMatchRepository(matches), new FakeJobService(jobRepository));
        var cooldownService = new CooldownService(new FakeRecommendationRepository(recommendations), TimeSpan.FromHours(24));

        return new UserRecommendationService(
            userRepository,
            jobRepository,
            skillRepository,
            jobSkillRepository,
            companyRepository,
            matchService,
            new FakeRecommendationRepository(recommendations),
            cooldownService,
            new RecommendationAlgorithm());
    }

    private static CompanyStatusService CreateCompanyStatusService(
        IReadOnlyList<User> users,
        IReadOnlyList<Company> companies,
        IReadOnlyList<Job> jobs,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Match> matches)
    {
        var jobRepository = new FakeJobRepository(jobs);
        return new CompanyStatusService(
            new MatchService(new FakeMatchRepository(matches), new FakeJobService(jobRepository)),
            new FakeUserService(new FakeUserRepository(users)),
            new FakeJobService(jobRepository),
            new FakeSkillService(new FakeSkillRepository(skills)));
    }

    private static UserStatusService CreateUserStatusService(
        IReadOnlyList<User> users,
        IReadOnlyList<Company> companies,
        IReadOnlyList<Job> jobs,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Match> matches)
    {
        var jobRepository = new FakeJobRepository(jobs);
        return new UserStatusService(
            new FakeMatchRepository(matches),
            new FakeJobService(jobRepository),
            new FakeCompanyService(new FakeCompanyRepository(companies)),
            new FakeSkillService(new FakeSkillRepository(skills)),
            new FakeJobSkillService(new FakeJobSkillRepository(jobSkills)));
    }

    private sealed class FakeCompanyService : ICompanyService
    {
        private readonly ICompanyRepository companyRepository;

        public FakeCompanyService(ICompanyRepository companyRepository)
        {
            this.companyRepository = companyRepository;
        }

        public Company? GetById(int companyId) => companyRepository.GetById(companyId);
        public IReadOnlyList<Company> GetAll() => companyRepository.GetAll();
        public void Add(Company company) => companyRepository.Add(company);
        public void Update(Company company) => companyRepository.Update(company);
        public void Remove(int companyId) => companyRepository.Remove(companyId);
    }

    private sealed class FakeUserService : IUserService
    {
        private readonly IUserRepository userRepository;

        public FakeUserService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public User? GetById(int userId) => userRepository.GetById(userId);
        public IReadOnlyList<User> GetAll() => userRepository.GetAll();
        public void Add(User user) => userRepository.Add(user);
        public void Update(User user) => userRepository.Update(user);
        public void Remove(int userId) => userRepository.Remove(userId);
    }

    private sealed class FakeJobService : IJobService
    {
        private readonly IJobRepository jobRepository;

        public FakeJobService(IJobRepository jobRepository)
        {
            this.jobRepository = jobRepository;
        }

        public Job? GetById(int jobId) => jobRepository.GetById(jobId);
        public IReadOnlyList<Job> GetAll() => jobRepository.GetAll();
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobRepository.GetByCompanyId(companyId);
        public void Add(Job job) => jobRepository.Add(job);
        public void Update(Job job) => jobRepository.Update(job);
        public void Remove(int jobId) => jobRepository.Remove(jobId);
    }

    private sealed class FakeSkillService : ISkillService
    {
        private readonly ISkillRepository skillRepository;

        public FakeSkillService(ISkillRepository skillRepository)
        {
            this.skillRepository = skillRepository;
        }

        public Skill? GetById(int userId, int skillId) => skillRepository.GetById(userId, skillId);
        public IReadOnlyList<Skill> GetAll() => skillRepository.GetAll();
        public IReadOnlyList<Skill> GetByUserId(int userId) => skillRepository.GetByUserId(userId);
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skillRepository.GetDistinctSkillCatalog();
        public void Add(Skill skill) => skillRepository.Add(skill);
        public void Update(Skill skill) => skillRepository.Update(skill);
        public void Remove(int userId, int skillId) => skillRepository.Remove(userId, skillId);
    }

    private sealed class FakeJobSkillService : IJobSkillService
    {
        private readonly IJobSkillRepository jobSkillRepository;

        public FakeJobSkillService(IJobSkillRepository jobSkillRepository)
        {
            this.jobSkillRepository = jobSkillRepository;
        }

        public JobSkill? GetById(int jobId, int skillId) => jobSkillRepository.GetById(jobId, skillId);
        public IReadOnlyList<JobSkill> GetAll() => jobSkillRepository.GetAll();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => jobSkillRepository.GetByJobId(jobId);
        public void Add(JobSkill jobSkill) => jobSkillRepository.Add(jobSkill);
        public void Update(JobSkill jobSkill) => jobSkillRepository.Update(jobSkill);
        public void Remove(int jobId, int skillId) => jobSkillRepository.Remove(jobId, skillId);
    }
}
