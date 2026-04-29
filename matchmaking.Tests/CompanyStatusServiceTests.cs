using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.DTOs;
using matchmaking.Services;
namespace matchmaking.Tests;
public sealed class CompanyStatusServiceTests
{
    private const int CompanyId = 10;
    private const int CompanyJobId = 100;
    private const int OtherJobId = 200;
    private const int UserId = 5;
    [Fact]
    public async Task GetApplicantsForCompanyAsync_filters_to_visible_statuses_and_sorts_by_score()
    {
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Accepted, FeedbackMessage = "ok" },
                new Match { MatchId = 2, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Advanced, FeedbackMessage = "next" },
                new Match { MatchId = 3, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Rejected, FeedbackMessage = "no" },
                new Match { MatchId = 4, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Applied, FeedbackMessage = "pending" }
            ]
        };
        var userService = new FakeUserService
        {
            Users = [new User { UserId = UserId, Location = "Cluj", PreferredEmploymentType = "Full-time" }]
        };
        var jobService = new FakeJobService
        {
            Jobs =
            [
                new Job { JobId = CompanyJobId, CompanyId = CompanyId, Location = "Cluj", EmploymentType = "Full-time" }
            ]
        };
        var skillService = new FakeSkillService
        {
            SkillsByUserId =
            {
                [UserId] = new List<Skill>
                {
                    new Skill { UserId = UserId, SkillId = 1, Score = 70 },
                    new Skill { UserId = UserId, SkillId = 2, Score = 90 }
                }
            }
        };
        var service = new CompanyStatusService(matchService, userService, jobService, skillService);
        var result = await service.GetApplicantsForCompanyAsync(CompanyId);
        result.Should().HaveCount(3);
        result.Select(item => item.Match.MatchId).Should().NotContain(4);
        result[0].CompatibilityScore.Should().BeGreaterThanOrEqualTo(result[1].CompatibilityScore);
    }
    [Fact]
    public async Task GetApplicantsForCompanyAsync_skips_missing_user_or_job()
    {
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Accepted }
            ]
        };
        var userService = new FakeUserService();
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = CompanyJobId, CompanyId = CompanyId }]
        };
        var service = new CompanyStatusService(matchService, userService, jobService, new FakeSkillService());
        var result = await service.GetApplicantsForCompanyAsync(CompanyId);
        result.Should().BeEmpty();
    }
    [Fact]
    public async Task GetApplicantByMatchIdAsync_returns_matching_applicant()
    {
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Accepted }
            ]
        };
        var userService = new FakeUserService
        {
            Users = [new User { UserId = UserId, Location = "Cluj", PreferredEmploymentType = "Full-time" }]
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = CompanyJobId, CompanyId = CompanyId, Location = "Cluj", EmploymentType = "Full-time" }]
        };
        var skillService = new FakeSkillService();
        var service = new CompanyStatusService(matchService, userService, jobService, skillService);
        var result = await service.GetApplicantByMatchIdAsync(CompanyId, 1);
        result.Should().NotBeNull();
        result!.Match.MatchId.Should().Be(1);
    }
    [Fact]
    public async Task GetApplicantsForCompanyAsync_uses_default_score_when_no_skills()
    {
        var matchService = new FakeMatchService
        {
            Matches =
            [
                new Match { MatchId = 1, UserId = UserId, JobId = CompanyJobId, Status = MatchStatus.Accepted }
            ]
        };
        var userService = new FakeUserService
        {
            Users = [new User { UserId = UserId, Location = "Bucharest", PreferredEmploymentType = "Remote" }]
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = CompanyJobId, CompanyId = CompanyId, Location = "Cluj", EmploymentType = "Full-time" }]
        };
        var service = new CompanyStatusService(matchService, userService, jobService, new FakeSkillService());
        var result = await service.GetApplicantsForCompanyAsync(CompanyId);
        result.Should().ContainSingle();
        result[0].CompatibilityScore.Should().Be(0);
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
        public IReadOnlyList<Skill> GetByUserId(int userId) => SkillsByUserId.TryGetValue(userId, out var list) ? list : [];
        public Skill? GetById(int userId, int skillId) => null;
        public IReadOnlyList<Skill> GetAll() => SkillsByUserId.SelectMany(pair => pair.Value).ToList();
        public IReadOnlyList<(int SkillId, string Name)> GetDistinctSkillCatalog() => [];
        public void Add(Skill skill) { }
        public void Update(Skill skill) { }
        public void Remove(int userId, int skillId) { }
    }
}
