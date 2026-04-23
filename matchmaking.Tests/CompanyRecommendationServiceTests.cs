using matchmaking.algorithm;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;

namespace matchmaking.Tests;

public sealed class CompanyRecommendationServiceTests
{
    [Fact]
    public void LoadApplicants_WhenCompanyHasNoJobs_LeavesQueueEmpty()
    {
        var service = CreateService(jobs: []);

        service.LoadApplicants(1);

        service.HasMore.Should().BeFalse();
        service.GetNextApplicant().Should().BeNull();
    }

    [Fact]
    public void LoadApplicants_WhenAppliedCandidatesExist_SortsByCompatibilityScore()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Applied);

        var service = CreateService(
            users: [user],
            jobs: [job],
            skills: [TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90)],
            jobSkills: [TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80)],
            matches: [match]);

        service.LoadApplicants(job.CompanyId);

        service.HasMore.Should().BeTrue();
        service.GetNextApplicant()!.Match.MatchId.Should().Be(match.MatchId);
        service.GetBreakdown(service.GetNextApplicant()!).Should().NotBeNull();
    }

    [Fact]
    public void MoveToNext_AndMoveToPrevious_AdjustCurrentIndex()
    {
        var service = CreateService(
            users: [TestDataFactory.CreateUser()],
            jobs: [TestDataFactory.CreateJob()],
            skills: [],
            jobSkills: [TestDataFactory.CreateJobSkill(100, 1, "C#", 80)],
            matches: [TestDataFactory.CreateMatch(1, 1, 100, MatchStatus.Applied)]);

        service.LoadApplicants(1);
        service.HasMore.Should().BeTrue();
        var firstApplicant = service.GetNextApplicant();
        service.MoveToNext();
        service.HasMore.Should().BeFalse();
        service.MoveToPrevious();
        service.GetNextApplicant().Should().Be(firstApplicant);
    }

    private static CompanyRecommendationService CreateService(
        IReadOnlyList<User>? users = null,
        IReadOnlyList<Job>? jobs = null,
        IReadOnlyList<Skill>? skills = null,
        IReadOnlyList<JobSkill>? jobSkills = null,
        IReadOnlyList<Match>? matches = null)
    {
        var userService = new FakeUserService(new FakeUserRepository(users ?? []));
        var jobRepository = new FakeJobRepository(jobs ?? []);
        var jobService = new FakeJobService(jobRepository);
        var skillRepository = new FakeSkillRepository(skills ?? []);
        var skillService = new FakeSkillService(skillRepository);
        var jobSkillRepository = new FakeJobSkillRepository(jobSkills ?? []);
        var jobSkillService = new FakeJobSkillService(jobSkillRepository);
        var matchService = new MatchService(new FakeMatchRepository(matches ?? []), new FakeJobService(jobRepository));
        var algorithm = new RecommendationAlgorithm();

        return new CompanyRecommendationService(
            matchService,
            userService,
            jobService,
            skillService,
            jobSkillService,
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

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly List<Match> _matches;
        public FakeMatchRepository(IReadOnlyList<Match> matches) => _matches = matches.ToList();
        public Match? GetById(int matchId) => _matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => _matches;
        public void Add(Match match) => _matches.Add(match);
        public void Update(Match match) { }
        public void Remove(int matchId) { }
        public int InsertReturningId(Match match) => 1;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => _matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
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

    private sealed class FakeUserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        public FakeUserService(IUserRepository userRepository) => _userRepository = userRepository;
        public User? GetById(int userId) => _userRepository.GetById(userId);
        public IReadOnlyList<User> GetAll() => _userRepository.GetAll();
        public void Add(User user) => _userRepository.Add(user);
        public void Update(User user) => _userRepository.Update(user);
        public void Remove(int userId) => _userRepository.Remove(userId);
    }

    private sealed class FakeSkillService : ISkillService
    {
        private readonly ISkillRepository _skillRepository;
        public FakeSkillService(ISkillRepository skillRepository) => _skillRepository = skillRepository;
        public Skill? GetById(int userId, int skillId) => _skillRepository.GetById(userId, skillId);
        public IReadOnlyList<Skill> GetAll() => _skillRepository.GetAll();
        public IReadOnlyList<Skill> GetByUserId(int userId) => _skillRepository.GetByUserId(userId);
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => _skillRepository.GetDistinctSkillCatalog();
        public void Add(Skill skill) => _skillRepository.Add(skill);
        public void Update(Skill skill) => _skillRepository.Update(skill);
        public void Remove(int userId, int skillId) => _skillRepository.Remove(userId, skillId);
    }

    private sealed class FakeJobSkillService : IJobSkillService
    {
        private readonly IJobSkillRepository _jobSkillRepository;
        public FakeJobSkillService(IJobSkillRepository jobSkillRepository) => _jobSkillRepository = jobSkillRepository;
        public JobSkill? GetById(int jobId, int skillId) => _jobSkillRepository.GetById(jobId, skillId);
        public IReadOnlyList<JobSkill> GetAll() => _jobSkillRepository.GetAll();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => _jobSkillRepository.GetByJobId(jobId);
        public void Add(JobSkill jobSkill) => _jobSkillRepository.Add(jobSkill);
        public void Update(JobSkill jobSkill) => _jobSkillRepository.Update(jobSkill);
        public void Remove(int jobId, int skillId) => _jobSkillRepository.Remove(jobId, skillId);
    }
}
