using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;
using matchmaking.Services;
namespace matchmaking.Tests;
public sealed class CompanyRecommendationServiceTests
{
    private const int CompanyId = 10;
    private const int FirstJobId = 100;
    private const int SecondJobId = 200;
    private const int FirstUserId = 1;
    private const int SecondUserId = 2;
    [Fact]
    public void LoadApplicants_with_no_jobs_returns_empty_queue()
    {
        var jobService = new FakeJobService();
        var service = CreateService(jobService: jobService);
        service.LoadApplicants(CompanyId);
        service.HasMore.Should().BeFalse();
        service.GetNextApplicant().Should().BeNull();
    }
    [Fact]
    public void LoadApplicants_orders_by_compatibility_score_descending()
    {
        var jobService = new FakeJobService
        {
            Jobs =
            [
                new Job { JobId = FirstJobId, CompanyId = CompanyId },
                new Job { JobId = SecondJobId, CompanyId = CompanyId }
            ]
        };
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = FirstUserId, JobId = FirstJobId, Status = MatchStatus.Applied },
                new Match { MatchId = 2, UserId = SecondUserId, JobId = SecondJobId, Status = MatchStatus.Applied }
            ]
        };
        var algorithm = new FakeAlgorithm
        {
            Scores =
            {
                [FirstJobId] = 40,
                [SecondJobId] = 90
            }
        };
        var service = CreateService(jobService: jobService, matchService: matchService, algorithm: algorithm);
        service.LoadApplicants(CompanyId);
        var applicant = service.GetNextApplicant();
        applicant.Should().NotBeNull();
        applicant!.Job.JobId.Should().Be(SecondJobId);
        applicant.CompatibilityScore.Should().Be(90);
    }
    [Fact]
    public void LoadApplicants_skips_matches_that_are_not_applied()
    {
        var jobService = new FakeJobService
        {
            Jobs =
            [
                new Job { JobId = FirstJobId, CompanyId = CompanyId }
            ]
        };
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = FirstUserId, JobId = FirstJobId, Status = MatchStatus.Accepted }
            ]
        };
        var service = CreateService(jobService: jobService, matchService: matchService);
        service.LoadApplicants(CompanyId);
        service.GetNextApplicant().Should().BeNull();
    }
    [Fact]
    public void LoadApplicants_skips_items_when_user_or_job_is_missing()
    {
        var jobService = new FakeJobService
        {
            Jobs =
            [
                new Job { JobId = FirstJobId, CompanyId = CompanyId }
            ]
        };
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = FirstUserId, JobId = FirstJobId, Status = MatchStatus.Applied }
            ]
        };
        var userService = new FakeUserService();
        var service = CreateService(jobService: jobService, matchService: matchService, userService: userService);
        service.LoadApplicants(CompanyId);
        service.GetNextApplicant().Should().BeNull();
    }
    [Fact]
    public void MoveToNext_advances_through_the_queue()
    {
        var (service, _) = BuildServiceWithTwoApplicants();
        service.LoadApplicants(CompanyId);
        var first = service.GetNextApplicant();
        service.MoveToNext();
        var second = service.GetNextApplicant();
        first!.Match.MatchId.Should().Be(1);
        second!.Match.MatchId.Should().Be(2);
    }
    [Fact]
    public void MoveToPrevious_does_not_go_before_the_first_applicant()
    {
        var (service, _) = BuildServiceWithTwoApplicants();
        service.LoadApplicants(CompanyId);
        service.MoveToNext();
        service.MoveToPrevious();
        var applicant = service.GetNextApplicant();
        applicant!.Match.MatchId.Should().Be(1);
    }
    [Fact]
    public void GetBreakdown_returns_algorithm_result()
    {
        var breakdown = new CompatibilityBreakdown { OverallScore = 88 };
        var algorithm = new FakeAlgorithm { Breakdown = breakdown };
        var (service, applicant) = BuildServiceWithSingleApplicant(algorithm: algorithm);
        var result = service.GetBreakdown(applicant);
        result.Should().BeSameAs(breakdown);
    }
    [Fact]
    public void HasMore_is_false_when_queue_is_exhausted()
    {
        var (service, _) = BuildServiceWithTwoApplicants();
        service.LoadApplicants(CompanyId);
        service.MoveToNext();
        service.MoveToNext();
        service.HasMore.Should().BeFalse();
    }
    private static CompanyRecommendationService CreateService(FakeMatchService? matchService = null, FakeUserService? userService = null, FakeJobService? jobService = null, FakeSkillService? skillService = null, FakeJobSkillService? jobSkillService = null, FakeAlgorithm? algorithm = null)
    {
        return new CompanyRecommendationService(
            matchService ?? new FakeMatchService(),
            userService ?? new FakeUserService { Users = [new User { UserId = FirstUserId }, new User { UserId = SecondUserId }] },
            jobService ?? new FakeJobService(),
            skillService ?? new FakeSkillService(),
            jobSkillService ?? new FakeJobSkillService(),
            algorithm ?? new FakeAlgorithm());
    }
    private static (CompanyRecommendationService Service, UserApplicationResult Applicant) BuildServiceWithSingleApplicant(FakeAlgorithm? algorithm = null)
    {
        var job = new Job { JobId = FirstJobId, CompanyId = CompanyId };
        var service = CreateService(
            matchService: new FakeMatchService
            {
                Matches = [new Match { MatchId = 1, UserId = FirstUserId, JobId = FirstJobId, Status = MatchStatus.Applied }]
            },
            jobService: new FakeJobService { Jobs = [job] },
            algorithm: algorithm ?? new FakeAlgorithm());
        service.LoadApplicants(CompanyId);
        var applicant = service.GetNextApplicant();
        return (service, applicant!);
    }
    private static (CompanyRecommendationService Service, List<Job> Jobs) BuildServiceWithTwoApplicants()
    {
        var jobs = new List<Job>
        {
            new Job { JobId = FirstJobId, CompanyId = CompanyId },
            new Job { JobId = SecondJobId, CompanyId = CompanyId }
        };
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = FirstUserId, JobId = FirstJobId, Status = MatchStatus.Applied },
                new Match { MatchId = 2, UserId = SecondUserId, JobId = SecondJobId, Status = MatchStatus.Applied }
            ]
        };
        var algorithm = new FakeAlgorithm
        {
            Scores =
            {
                [FirstJobId] = 50,
                [SecondJobId] = 60
            }
        };
        var service = CreateService(jobService: new FakeJobService { Jobs = jobs }, matchService: matchService, algorithm: algorithm);
        return (service, jobs);
    }
    private sealed class FakeMatchService : IMatchService
    {
        public List<Match> Matches { get; set; } = [];
        public Task AcceptAsync(int matchId, string feedback) => Task.CompletedTask;
        public void Advance(int matchId) { }
        public int CreatePendingApplication(int userId, int jobId) => 1;
        public IReadOnlyList<Match> GetAllMatches() => Matches;
        public Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId) => Task.FromResult<IReadOnlyList<Match>>(Matches);
        public Match? GetById(int matchId) => Matches.FirstOrDefault(match => match.MatchId == matchId);
        public Match? GetByUserIdAndJobId(int userId, int jobId) => Matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
        public bool IsDecisionTransitionAllowed(Match current, MatchStatus next) => true;
        public void Reject(int matchId, string feedback) { }
        public Task RejectAsync(int matchId, string feedback) => Task.CompletedTask;
        public void RemoveApplication(int matchId) { }
        public void RevertToApplied(int matchId) { }
        public void SubmitDecision(int matchId, MatchStatus decision, string feedback) { }
        public Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback) => Task.CompletedTask;
    }
    private sealed class FakeUserService : IUserService
    {
        public List<User> Users { get; set; } = [];
        public User? GetById(int userId) => Users.FirstOrDefault(user => user.UserId == userId);
        public IReadOnlyList<User> GetAll() => Users;
        public void Add(User user) { }
        public void Update(User user) { }
        public void Remove(int userId) { }
    }
    private sealed class FakeJobService : IJobService
    {
        public List<Job> Jobs { get; set; } = [];
        public Job? GetById(int jobId) => Jobs.FirstOrDefault(job => job.JobId == jobId);
        public IReadOnlyList<Job> GetAll() => Jobs;
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => Jobs.Where(job => job.CompanyId == companyId).ToList();
        public void Add(Job job) { }
        public void Update(Job job) { }
        public void Remove(int jobId) { }
    }
    private sealed class FakeSkillService : ISkillService
    {
        public Dictionary<int, List<Skill>> SkillsByUserId { get; set; } = new();
        public IReadOnlyList<Skill> GetByUserId(int userId) => SkillsByUserId.TryGetValue(userId, out var skills) ? skills : [];
        public Skill? GetById(int userId, int skillId) => null;
        public IReadOnlyList<Skill> GetAll() => SkillsByUserId.SelectMany(pair => pair.Value).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => [];
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }
    private sealed class FakeJobSkillService : IJobSkillService
    {
        public Dictionary<int, List<JobSkill>> SkillsByJobId { get; set; } = new();
        public IReadOnlyList<JobSkill> GetByJobId(int jobId) => SkillsByJobId.TryGetValue(jobId, out var skills) ? skills : [];
        public JobSkill? GetById(int jobId, int skillId) => null;
        public IReadOnlyList<JobSkill> GetAll() => SkillsByJobId.SelectMany(pair => pair.Value).ToList();
        public void Add(JobSkill jobSkill) { }
        public void Update(JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) { }
    }
    private sealed class FakeAlgorithm : IRecommendationAlgorithm
    {
        public Dictionary<int, double> Scores { get; } = new();
        public CompatibilityBreakdown? Breakdown { get; set; }
        public double CalculateCompatibilityScore(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills) => Scores.TryGetValue(job.JobId, out var score) ? score : 0;
        public CompatibilityBreakdown CalculateScoreBreakdown(User user, Job job, IReadOnlyList<Skill> userSkills, IReadOnlyList<Skill> jobSkills) => Breakdown ?? new CompatibilityBreakdown();
    }
}
