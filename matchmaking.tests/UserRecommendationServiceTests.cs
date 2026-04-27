using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Tests;

public sealed class UserRecommendationServiceTests
{
    [Fact]
    public void GetNextCard_WhenNoMatchingJobsExist_ReturnsNull()
    {
        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: Array.Empty<Job>(),
            skills: Array.Empty<Skill>(),
            jobSkills: Array.Empty<JobSkill>(),
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(1, UserMatchmakingFilters.Empty());

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_WhenMatchingJobExists_ReturnsCardWithCompanyInformation()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(user.UserId, UserMatchmakingFilters.Empty());

        result.Should().NotBeNull();
        result!.Company.CompanyName.Should().Be("TechNova");
        result.TopSkillLabels.Should().NotBeEmpty();
    }

    [Fact]
    public void GetNextCard_WhenUserNotFound_ThrowsInvalidOperationException()
    {
        var service = CreateService(
            users: Array.Empty<User>(),
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: Array.Empty<JobSkill>(),
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        Action act = () => service.GetNextCard(1, UserMatchmakingFilters.Empty());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GetNextCard_WhenCompanyMissingForJob_ThrowsInvalidOperationException()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob(companyId: 999);

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            companies: Array.Empty<Company>(),
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        Action act = () => service.GetNextCard(user.UserId, UserMatchmakingFilters.Empty());

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecalculateTopCardIgnoringCooldown_WhenFilteredOut_ReturnsNull()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var filters = UserMatchmakingFilters.Empty();
        filters.EmploymentTypes.Add("Contract");

        var result = service.RecalculateTopCardIgnoringCooldown(user.UserId, filters);

        result.Should().BeNull();
    }

    [Fact]
    public void RecalculateTopCardIgnoringCooldown_WhenExistingMatchAlreadyExists_ReturnsNull()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var existingMatch = TestDataFactory.CreateMatch(11, user.UserId, job.JobId, MatchStatus.Applied);

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: new[] { existingMatch },
            recommendations: Array.Empty<Recommendation>());

        var result = service.RecalculateTopCardIgnoringCooldown(user.UserId, UserMatchmakingFilters.Empty());

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_WhenLocationFilterDoesNotMatch_ReturnsNull()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        job.Location = "Cluj";
        var filters = UserMatchmakingFilters.Empty();
        filters.LocationSubstring = "Bucharest";

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(user.UserId, filters);

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_WhenSkillFilterDoesNotMatchAnyJobSkill_ReturnsNull()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var filters = UserMatchmakingFilters.Empty();
        filters.SkillIds.Add(999);

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(user.UserId, filters);

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_WhenLocationAndSkillFiltersMatch_ReturnsCard()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        job.Location = "Cluj-Napoca";
        var filters = UserMatchmakingFilters.Empty();
        filters.LocationSubstring = "cluj";
        filters.SkillIds.Add(1);

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(user.UserId, filters);

        result.Should().NotBeNull();
    }

    [Fact]
    public void GetNextCard_WhenExperienceFilterDoesNotMatchUserBucket_ReturnsNull()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var filters = UserMatchmakingFilters.Empty();
        filters.ExperienceLevels.Add("Executive");

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(user.UserId, filters);

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_WhenExperienceFilterMatchesUserBucket_ReturnsCard()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var filters = UserMatchmakingFilters.Empty();
        filters.ExperienceLevels.Add("Entry");

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var result = service.GetNextCard(user.UserId, filters);

        result.Should().NotBeNull();
    }

    [Fact]
    public void ApplyLike_WhenNoExistingMatch_CreatesPendingApplication()
    {
        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());
        card.Should().NotBeNull();
        var matchId = service.ApplyLike(1, card!);

        matchId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyDismiss_WhenCardIsDismissed_CreatesRecommendationRecord()
    {
        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: Array.Empty<Match>(),
            recommendations: Array.Empty<Recommendation>());

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty());
        card.Should().NotBeNull();
        var recommendationId = service.ApplyDismiss(1, card!);

        recommendationId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UndoLike_WhenMatchExists_RemovesApplicationAndDisplayRecommendation()
    {
        var repository = new FakeRecommendationRepository(new[]
        {
            TestDataFactory.CreateRecommendation(9, 1, 100)
        });
        var matchRepository = new FakeMatchRepository(new[]
        {
            TestDataFactory.CreateMatch(3, 1, 100, MatchStatus.Applied)
        });

        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: matchRepository.Matches,
            recommendations: repository.Recommendations,
            recommendationRepository: repository,
            matchRepository: matchRepository);

        service.UndoLike(3, 9);

        matchRepository.RemovedIds.Should().Contain(3);
        repository.RemovedIds.Should().Contain(9);
    }

    [Fact]
    public void UndoDismiss_WhenRecommendationExists_RemovesRecommendation()
    {
        var repository = new FakeRecommendationRepository([
            TestDataFactory.CreateRecommendation(5, 1, 100)
        ]);

        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: Array.Empty<Match>(),
            recommendations: repository.Recommendations,
            recommendationRepository: repository);

        service.UndoDismiss(5, null);

        repository.RemovedIds.Should().Contain(5);
    }

    [Fact]
    public void ApplyLike_WhenMatchAlreadyExists_ThrowsInvalidOperationException()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(3, user.UserId, job.JobId, MatchStatus.Applied);

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany(job.CompanyId) },
            matches: new[] { match },
            recommendations: Array.Empty<Recommendation>());

        var card = service.GetNextCard(user.UserId, UserMatchmakingFilters.Empty());

        card.Should().BeNull();
        Action act = () => service.ApplyLike(user.UserId, new JobRecommendationResult { Job = job, Company = TestDataFactory.CreateCompany(job.CompanyId) });

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void UndoDismiss_WhenDisplayRecommendationMatchesDismissId_DoesNotRemoveTwice()
    {
        var repository = new FakeRecommendationRepository([
            TestDataFactory.CreateRecommendation(5, 1, 100)
        ]);

        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            companies: new[] { TestDataFactory.CreateCompany() },
            matches: Array.Empty<Match>(),
            recommendations: repository.Recommendations,
            recommendationRepository: repository);

        service.UndoDismiss(5, 5);

        repository.RemovedIds.Should().Equal(5);
    }

    [Theory]
    [InlineData(0, "Internship")]
    [InlineData(2, "Entry")]
    [InlineData(5, "MidSenior")]
    [InlineData(8, "Director")]
    [InlineData(12, "Executive")]
    public void MapUserYearsToExperienceBucket_WhenYearsProvided_ReturnsMatchingBucket(int years, string expected)
    {
        var result = UserRecommendationService.MapUserYearsToExperienceBucket(years);

        result.Should().Be(expected);
    }

    private static UserRecommendationService CreateService(
        IReadOnlyList<User> users,
        IReadOnlyList<Job> jobs,
        IReadOnlyList<Skill> skills,
        IReadOnlyList<JobSkill> jobSkills,
        IReadOnlyList<Company> companies,
        IReadOnlyList<Match> matches,
        IReadOnlyList<Recommendation> recommendations,
        FakeRecommendationRepository? recommendationRepository = null,
        FakeMatchRepository? matchRepository = null)
    {
        var userRepository = new FakeUserRepository(users);
        var jobRepository = new FakeJobRepository(jobs);
        var skillRepository = new FakeSkillRepository(skills);
        var jobSkillRepository = new FakeJobSkillRepository(jobSkills);
        var companyRepository = new FakeCompanyRepository(companies);
        var localMatchRepository = matchRepository ?? new FakeMatchRepository(matches);
        var localRecommendationRepository = recommendationRepository ?? new FakeRecommendationRepository(recommendations);
        var cooldownService = new CooldownService(localRecommendationRepository, TimeSpan.FromHours(24));
        var algorithm = new RecommendationAlgorithm();
        var matchService = new MatchService(localMatchRepository, new FakeJobService(jobRepository));

        return new UserRecommendationService(
            userRepository,
            jobRepository,
            skillRepository,
            jobSkillRepository,
            companyRepository,
            matchService,
            localRecommendationRepository,
            cooldownService,
            algorithm);
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        private readonly IReadOnlyList<User> users;

        public FakeUserRepository(IReadOnlyList<User> users)
        {
            this.users = users;
        }

        public User? GetById(int userId) => users.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => users;
        public void Add(User user)
        {
        }

        public void Update(User user)
        {
        }

        public void Remove(int userId)
        {
        }
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        private readonly IReadOnlyList<Job> jobs;

        public FakeJobRepository(IReadOnlyList<Job> jobs)
        {
            this.jobs = jobs;
        }

        public Job? GetById(int jobId) => jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job)
        {
        }

        public void Update(Job job)
        {
        }

        public void Remove(int jobId)
        {
        }
    }

    private sealed class FakeSkillRepository : ISkillRepository
    {
        private readonly IReadOnlyList<Skill> skills;

        public FakeSkillRepository(IReadOnlyList<Skill> skills)
        {
            this.skills = skills;
        }

        public Skill? GetById(int userId, int skillId) => skills.FirstOrDefault(skill => skill.UserId == userId && skill.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => skills;
        public IReadOnlyList<Skill> GetByUserId(int userId) => skills.Where(skill => skill.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => skills.GroupBy(skill => skill.SkillId).Select(group => (group.Key, group.First().SkillName)).ToList();
        public void Add(Skill skill)
        {
        }

        public void Update(Skill skill)
        {
        }

        public void Remove(int userId, int skillId)
        {
        }
    }

    private sealed class FakeJobSkillRepository : IJobSkillRepository
    {
        private readonly IReadOnlyList<JobSkill> jobSkills;

        public FakeJobSkillRepository(IReadOnlyList<JobSkill> jobSkills)
        {
            this.jobSkills = jobSkills;
        }

        public JobSkill? GetById(int jobId, int skillId) => jobSkills.FirstOrDefault(jobSkill => jobSkill.JobId == jobId && jobSkill.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => jobSkills;
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => jobSkills.Where(jobSkill => jobSkill.JobId == jobId).ToList();
        public void Add(JobSkill jobSkill)
        {
        }

        public void Update(JobSkill jobSkill)
        {
        }

        public void Remove(int jobId, int skillId)
        {
        }
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        private readonly IReadOnlyList<Company> companies;

        public FakeCompanyRepository(IReadOnlyList<Company> companies)
        {
            this.companies = companies;
        }

        public Company? GetById(int companyId) => companies.FirstOrDefault(company => company.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => companies;
        public void Add(Company company)
        {
        }

        public void Update(Company company)
        {
        }

        public void Remove(int companyId)
        {
        }
    }

    private sealed class FakeRecommendationRepository : IRecommendationRepository
    {
        public FakeRecommendationRepository(IReadOnlyList<Recommendation> recommendations) => Recommendations = recommendations.ToList();
        public List<Recommendation> Recommendations { get; }
        public List<int> RemovedIds { get; } = new List<int>();
        public Recommendation? GetById(int recommendationId) => Recommendations.FirstOrDefault(recommendation => recommendation.RecommendationId == recommendationId);
        public IReadOnlyList<Recommendation> GetAll() => Recommendations;
        public void Add(Recommendation recommendation) => Recommendations.Add(recommendation);
        public void Update(Recommendation recommendation)
        {
        }

        public void Remove(int recommendationId) => RemovedIds.Add(recommendationId);
        public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) => Recommendations.Where(recommendation => recommendation.UserId == userId && recommendation.JobId == jobId).OrderByDescending(recommendation => recommendation.Timestamp).FirstOrDefault();
        public int InsertReturningId(Recommendation recommendation)
        {
            var nextId = Recommendations.Count == 0 ? 1 : Recommendations.Max(item => item.RecommendationId) + 1;
            recommendation.RecommendationId = nextId;
            Recommendations.Add(recommendation);
            return nextId;
        }
    }

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly List<Match> matches;

        public FakeMatchRepository(IReadOnlyList<Match> matches)
        {
            this.matches = matches.ToList();
        }

        public List<Match> Matches => matches;
        public List<int> RemovedIds { get; } = new List<int>();
        public Match? GetById(int matchId) => matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => matches;
        public void Add(Match match) => matches.Add(match);
        public void Update(Match match)
        {
        }

        public void Remove(int matchId) => RemovedIds.Add(matchId);
        public int InsertReturningId(Match match)
        {
            var nextId = matches.Count == 0 ? 1 : matches.Max(item => item.MatchId) + 1;
            match.MatchId = nextId;
            matches.Add(match);
            return nextId;
        }
        public Match? GetByUserIdAndJobId(int userId, int jobId) => matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
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
}
