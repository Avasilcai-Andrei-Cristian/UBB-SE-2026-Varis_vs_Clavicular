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
            users: [TestDataFactory.CreateUser()],
            jobs: [],
            skills: [],
            jobSkills: [],
            companies: [TestDataFactory.CreateCompany()],
            matches: [],
            recommendations: []);

        var result = service.GetNextCard(1, UserMatchmakingFilters.Empty());

        result.Should().BeNull();
    }

    [Fact]
    public void GetNextCard_WhenMatchingJobExists_ReturnsCardWithCompanyInformation()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();

        var service = CreateService(
            users: [user],
            jobs: [job],
            skills: [TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90)],
            jobSkills: [TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80)],
            companies: [TestDataFactory.CreateCompany(job.CompanyId)],
            matches: [],
            recommendations: []);

        var result = service.GetNextCard(user.UserId, UserMatchmakingFilters.Empty());

        result.Should().NotBeNull();
        result!.Company.CompanyName.Should().Be("TechNova");
        result.TopSkillLabels.Should().NotBeEmpty();
    }

    [Fact]
    public void ApplyLike_WhenNoExistingMatchCreatesPendingApplication()
    {
        var service = CreateService(
            users: [TestDataFactory.CreateUser()],
            jobs: [TestDataFactory.CreateJob()],
            skills: [],
            jobSkills: [TestDataFactory.CreateJobSkill(100, 1, "C#", 80)],
            companies: [TestDataFactory.CreateCompany()],
            matches: [],
            recommendations: []);

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty())!;
        var matchId = service.ApplyLike(1, card);

        matchId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ApplyDismiss_WhenCardIsDismissed_CreatesRecommendationRecord()
    {
        var service = CreateService(
            users: [TestDataFactory.CreateUser()],
            jobs: [TestDataFactory.CreateJob()],
            skills: [],
            jobSkills: [TestDataFactory.CreateJobSkill(100, 1, "C#", 80)],
            companies: [TestDataFactory.CreateCompany()],
            matches: [],
            recommendations: []);

        var card = service.GetNextCard(1, UserMatchmakingFilters.Empty())!;
        var recommendationId = service.ApplyDismiss(1, card);

        recommendationId.Should().BeGreaterThan(0);
    }

    [Fact]
    public void UndoLike_WhenMatchExists_RemovesApplicationAndDisplayRecommendation()
    {
        var repository = new FakeRecommendationRepository([
            TestDataFactory.CreateRecommendation(9, 1, 100)
        ]);
        var matchRepository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(3, 1, 100, MatchStatus.Applied)
        ]);

        var service = CreateService(
            users: [TestDataFactory.CreateUser()],
            jobs: [TestDataFactory.CreateJob()],
            skills: [],
            jobSkills: [TestDataFactory.CreateJobSkill(100, 1, "C#", 80)],
            companies: [TestDataFactory.CreateCompany()],
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
            users: [TestDataFactory.CreateUser()],
            jobs: [TestDataFactory.CreateJob()],
            skills: [],
            jobSkills: [TestDataFactory.CreateJobSkill(100, 1, "C#", 80)],
            companies: [TestDataFactory.CreateCompany()],
            matches: [],
            recommendations: repository.Recommendations,
            recommendationRepository: repository);

        service.UndoDismiss(5, null);

        repository.RemovedIds.Should().Contain(5);
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
        private readonly IReadOnlyList<User> _users;
        public FakeUserRepository(IReadOnlyList<User> users) => _users = users;
        public User? GetById(int userId) => _users.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => _users;
        public void Add(User user) { }
        public void Update(User user) { }
        public void Remove(int userId) { }
    }

    private sealed class FakeJobRepository : IJobRepository
    {
        private readonly IReadOnlyList<Job> _jobs;
        public FakeJobRepository(IReadOnlyList<Job> jobs) => _jobs = jobs;
        public Job? GetById(int jobId) => _jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => _jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => _jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) { }
        public void Update(Job job) { }
        public void Remove(int jobId) { }
    }

    private sealed class FakeSkillRepository : ISkillRepository
    {
        private readonly IReadOnlyList<Skill> _skills;
        public FakeSkillRepository(IReadOnlyList<Skill> skills) => _skills = skills;
        public Skill? GetById(int userId, int skillId) => _skills.FirstOrDefault(skill => skill.UserId == userId && skill.SkillId == skillId);
        public IReadOnlyList<Skill> GetAll() => _skills;
        public IReadOnlyList<Skill> GetByUserId(int userId) => _skills.Where(skill => skill.UserId == userId).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => _skills.GroupBy(skill => skill.SkillId).Select(group => (group.Key, group.First().SkillName)).ToList();
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }

    private sealed class FakeJobSkillRepository : IJobSkillRepository
    {
        private readonly IReadOnlyList<JobSkill> _jobSkills;
        public FakeJobSkillRepository(IReadOnlyList<JobSkill> jobSkills) => _jobSkills = jobSkills;
        public JobSkill? GetById(int jobId, int skillId) => _jobSkills.FirstOrDefault(jobSkill => jobSkill.JobId == jobId && jobSkill.SkillId == skillId);
        public IReadOnlyList<JobSkill> GetAll() => _jobSkills;
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => _jobSkills.Where(jobSkill => jobSkill.JobId == jobId).ToList();
        public void Add(JobSkill jobSkill) { }
        public void Update(JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) { }
    }

    private sealed class FakeCompanyRepository : ICompanyRepository
    {
        private readonly IReadOnlyList<Company> _companies;
        public FakeCompanyRepository(IReadOnlyList<Company> companies) => _companies = companies;
        public Company? GetById(int companyId) => _companies.FirstOrDefault(company => company.CompanyId == companyId);
        public IReadOnlyList<Company> GetAll() => _companies;
        public void Add(Company company) { }
        public void Update(Company company) { }
        public void Remove(int companyId) { }
    }

    private sealed class FakeRecommendationRepository : IRecommendationRepository
    {
        public FakeRecommendationRepository(IReadOnlyList<Recommendation> recommendations) => Recommendations = recommendations.ToList();
        public List<Recommendation> Recommendations { get; }
        public List<int> RemovedIds { get; } = [];
        public Recommendation? GetById(int recommendationId) => Recommendations.FirstOrDefault(recommendation => recommendation.RecommendationId == recommendationId);
        public IReadOnlyList<Recommendation> GetAll() => Recommendations;
        public void Add(Recommendation recommendation) => Recommendations.Add(recommendation);
        public void Update(Recommendation recommendation) { }
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
        public FakeMatchRepository(IReadOnlyList<Match> matches) => Matches = matches.ToList();
        public List<Match> Matches { get; }
        public List<int> RemovedIds { get; } = [];
        public Match? GetById(int matchId) => Matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => Matches;
        public void Add(Match match) => Matches.Add(match);
        public void Update(Match match) { }
        public void Remove(int matchId) => RemovedIds.Add(matchId);
        public int InsertReturningId(Match match)
        {
            var nextId = Matches.Count == 0 ? 1 : Matches.Max(item => item.MatchId) + 1;
            match.MatchId = nextId;
            Matches.Add(match);
            return nextId;
        }
        public Match? GetByUserIdAndJobId(int userId, int jobId) => Matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
    }

    private sealed class FakeJobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        public FakeJobService(IJobRepository jobRepository) => _jobRepository = jobRepository;
        public Job? GetById(int jobId) => _jobRepository.GetById(jobId);
        public IReadOnlyList<Job> GetAll() => _jobRepository.GetAll();
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => _jobRepository.GetByCompanyId(companyId);
        public void Add(Job job) => _jobRepository.Add(job);
        public void Update(Job job) => _jobRepository.Update(job);
        public void Remove(int jobId) => _jobRepository.Remove(jobId);
    }
}
