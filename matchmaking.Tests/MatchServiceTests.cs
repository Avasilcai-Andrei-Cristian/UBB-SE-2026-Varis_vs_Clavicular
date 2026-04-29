using System;
using System.Collections.Generic;
using System.Linq;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Services;
namespace matchmaking.Tests;
public sealed class MatchServiceTests
{
    private const int MatchId = 10;
    private const int UserId = 5;
    private const int JobId = 8;
    private const int CompanyId = 9;
    [Fact]
    public void CreatePendingApplication_throws_when_match_exists()
    {
        var repository = new FakeMatchRepository
        {
            ExistingMatch = new Match { MatchId = MatchId, UserId = UserId, JobId = JobId, Status = MatchStatus.Applied }
        };
        var service = new MatchService(repository, new FakeJobService());
        Action act = () => service.CreatePendingApplication(UserId, JobId);
        act.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void CreatePendingApplication_inserts_match_with_applied_status()
    {
        var repository = new FakeMatchRepository();
        var service = new MatchService(repository, new FakeJobService());
        var result = service.CreatePendingApplication(UserId, JobId);
        result.Should().BeGreaterThan(0);
        repository.Inserted.Should().ContainSingle();
        repository.Inserted[0].Status.Should().Be(MatchStatus.Applied);
    }
    [Fact]
    public void SubmitDecision_reject_requires_feedback()
    {
        var repository = new FakeMatchRepository
        {
            Matches = { [MatchId] = new Match { MatchId = MatchId, Status = MatchStatus.Applied } }
        };
        var service = new MatchService(repository, new FakeJobService());
        Action act = () => service.SubmitDecision(MatchId, MatchStatus.Rejected, " ");
        act.Should().Throw<ArgumentException>();
    }
    [Fact]
    public void SubmitDecision_blocks_invalid_transition()
    {
        var repository = new FakeMatchRepository
        {
            Matches = { [MatchId] = new Match { MatchId = MatchId, Status = MatchStatus.Accepted } }
        };
        var service = new MatchService(repository, new FakeJobService());
        Action act = () => service.SubmitDecision(MatchId, MatchStatus.Rejected, "No");
        act.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void SubmitDecision_updates_status_and_feedback()
    {
        var repository = new FakeMatchRepository
        {
            Matches = { [MatchId] = new Match { MatchId = MatchId, Status = MatchStatus.Applied } }
        };
        var service = new MatchService(repository, new FakeJobService());
        service.SubmitDecision(MatchId, MatchStatus.Accepted, "  Ok ");
        repository.Updated.Should().ContainSingle();
        repository.Updated[0].Status.Should().Be(MatchStatus.Accepted);
        repository.Updated[0].FeedbackMessage.Should().Be("Ok");
    }
    [Fact]
    public void Advance_allows_only_applied_matches()
    {
        var repository = new FakeMatchRepository
        {
            Matches = { [MatchId] = new Match { MatchId = MatchId, Status = MatchStatus.Accepted } }
        };
        var service = new MatchService(repository, new FakeJobService());
        Action act = () => service.Advance(MatchId);
        act.Should().Throw<InvalidOperationException>();
    }
    [Fact]
    public void Advance_sets_status_to_advanced()
    {
        var repository = new FakeMatchRepository
        {
            Matches = { [MatchId] = new Match { MatchId = MatchId, Status = MatchStatus.Applied } }
        };
        var service = new MatchService(repository, new FakeJobService());
        service.Advance(MatchId);
        repository.Updated.Should().ContainSingle();
        repository.Updated[0].Status.Should().Be(MatchStatus.Advanced);
    }
    [Fact]
    public void RevertToApplied_clears_feedback_and_sets_status()
    {
        var repository = new FakeMatchRepository
        {
            Matches = { [MatchId] = new Match { MatchId = MatchId, Status = MatchStatus.Rejected, FeedbackMessage = "No" } }
        };
        var service = new MatchService(repository, new FakeJobService());
        service.RevertToApplied(MatchId);
        repository.Updated.Should().ContainSingle();
        repository.Updated[0].Status.Should().Be(MatchStatus.Applied);
        repository.Updated[0].FeedbackMessage.Should().BeEmpty();
    }
    [Fact]
    public void IsDecisionTransitionAllowed_allows_only_defined_transitions()
    {
        var service = new MatchService(new FakeMatchRepository(), new FakeJobService());
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Applied }, MatchStatus.Accepted).Should().BeTrue();
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Applied }, MatchStatus.Rejected).Should().BeTrue();
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Applied }, MatchStatus.Advanced).Should().BeTrue();
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Advanced }, MatchStatus.Accepted).Should().BeTrue();
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Advanced }, MatchStatus.Rejected).Should().BeTrue();
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Advanced }, MatchStatus.Advanced).Should().BeFalse();
        service.IsDecisionTransitionAllowed(new Match { Status = MatchStatus.Accepted }, MatchStatus.Rejected).Should().BeFalse();
    }
    [Fact]
    public async Task GetByCompanyIdAsync_filters_by_company_job_ids()
    {
        var repository = new FakeMatchRepository
        {
            AllMatches =
            [
                new Match { MatchId = 1, JobId = JobId },
                new Match { MatchId = 2, JobId = 300 }
            ]
        };
        var jobService = new FakeJobService
        {
            Jobs = [new Job { JobId = JobId, CompanyId = CompanyId }]
        };
        var service = new MatchService(repository, jobService);
        var result = await service.GetByCompanyIdAsync(CompanyId);
        result.Should().ContainSingle();
        result[0].MatchId.Should().Be(1);
    }
    private sealed class FakeMatchRepository : IMatchRepository
    {
        public Match? ExistingMatch { get; set; }
        public Dictionary<int, Match> Matches { get; } = new();
        public List<Match> Inserted { get; } = [];
        public List<Match> Updated { get; } = [];
        public List<Match> AllMatches { get; set; } = [];
        public Match? GetById(int matchId) => Matches.TryGetValue(matchId, out var match) ? match : null;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => ExistingMatch;
        public IReadOnlyList<Match> GetAll() => AllMatches.Count > 0 ? AllMatches : Matches.Values.ToList();
        public void Add(Match match) => Inserted.Add(match);
        public void Update(Match match) => Updated.Add(match);
        public void Remove(int matchId) { }
        public int InsertReturningId(Match match)
        {
            Inserted.Add(match);
            return 1;
        }
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
}
