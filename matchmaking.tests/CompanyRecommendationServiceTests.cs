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
        var service = CreateService(jobs: Array.Empty<Job>());

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
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            matches: new[] { match });

        service.LoadApplicants(job.CompanyId);

        service.HasMore.Should().BeTrue();
        service.GetNextApplicant()!.Match.MatchId.Should().Be(match.MatchId);
    }

    [Fact]
    public void GetBreakdown_WhenApplicantExists_ReturnsBreakdown()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Applied);

        var service = CreateService(
            users: new[] { user },
            jobs: new[] { job },
            skills: new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) },
            jobSkills: new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) },
            matches: new[] { match });

        service.LoadApplicants(job.CompanyId);
        var applicant = service.GetNextApplicant();

        service.GetBreakdown(applicant!).Should().NotBeNull();
    }

    [Fact]
    public void MoveToNext_WhenOnlyOneApplicantExists_ExhaustsQueue()
    {
        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            matches: new[] { TestDataFactory.CreateMatch(1, 1, 100, MatchStatus.Applied) });
        service.LoadApplicants(1);

        service.MoveToNext();

        service.HasMore.Should().BeFalse();
    }

    [Fact]
    public void MoveToPrevious_WhenAdvancedPastFirst_RestoresFirstApplicant()
    {
        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            matches: new[] { TestDataFactory.CreateMatch(1, 1, 100, MatchStatus.Applied) });
        service.LoadApplicants(1);
        var firstApplicant = service.GetNextApplicant();
        service.MoveToNext();

        service.MoveToPrevious();

        service.GetNextApplicant().Should().Be(firstApplicant);
    }

    [Fact]
    public void MoveToPrevious_WhenAlreadyAtBeginning_DoesNotGoNegative()
    {
        var service = CreateService(
            users: new[] { TestDataFactory.CreateUser() },
            jobs: new[] { TestDataFactory.CreateJob() },
            skills: Array.Empty<Skill>(),
            jobSkills: new[] { TestDataFactory.CreateJobSkill(100, 1, "C#", 80) },
            matches: new[] { TestDataFactory.CreateMatch(1, 1, 100, MatchStatus.Applied) });
        service.LoadApplicants(1);
        var firstApplicant = service.GetNextApplicant();

        service.MoveToPrevious();

        service.GetNextApplicant().Should().Be(firstApplicant);
    }

    [Fact]
    public void LoadApplicants_WhenMatchReferencesMissingUserOrJob_SkipsInvalidEntries()
    {
        var validUser = TestDataFactory.CreateUser(userId: 1);
        var validJob = TestDataFactory.CreateJob(jobId: 100, companyId: 1);
        var missingUserMatch = TestDataFactory.CreateMatch(matchId: 1, userId: 999, jobId: 100, status: MatchStatus.Applied);
        var missingJobMatch = TestDataFactory.CreateMatch(matchId: 2, userId: 1, jobId: 999, status: MatchStatus.Applied);

        var service = CreateService(
            users: new[] { validUser },
            jobs: new[] { validJob },
            skills: Array.Empty<Skill>(),
            jobSkills: Array.Empty<JobSkill>(),
            matches: new[] { missingUserMatch, missingJobMatch });

        service.LoadApplicants(1);

        service.GetNextApplicant().Should().BeNull();
        service.HasMore.Should().BeFalse();
    }

    private static CompanyRecommendationService CreateService(
        IReadOnlyList<User>? users = null,
        IReadOnlyList<Job>? jobs = null,
        IReadOnlyList<Skill>? skills = null,
        IReadOnlyList<JobSkill>? jobSkills = null,
        IReadOnlyList<Match>? matches = null)
    {
        var userService = new FakeUserService(new FakeUserRepository(users ?? Array.Empty<User>()));
        var jobRepository = new FakeJobRepository(jobs ?? Array.Empty<Job>());
        var jobService = new FakeJobService(jobRepository);
        var skillRepository = new FakeSkillRepository(skills ?? Array.Empty<Skill>());
        var skillService = new FakeSkillService(skillRepository);
        var jobSkillRepository = new FakeJobSkillRepository(jobSkills ?? Array.Empty<JobSkill>());
        var jobSkillService = new FakeJobSkillService(jobSkillRepository);
        var matchService = new MatchService(new FakeMatchRepository(matches ?? Array.Empty<Match>()), new FakeJobService(jobRepository));
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

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly List<Match> matches;

        public FakeMatchRepository(IReadOnlyList<Match> matches)
        {
            this.matches = matches.ToList();
        }

        public Match? GetById(int matchId) => matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => matches;
        public void Add(Match match) => matches.Add(match);
        public void Update(Match match)
        {
        }

        public void Remove(int matchId)
        {
        }

        public int InsertReturningId(Match match) => 1;
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
