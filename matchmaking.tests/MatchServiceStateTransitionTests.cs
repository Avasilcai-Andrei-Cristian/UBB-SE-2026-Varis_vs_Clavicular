using matchmaking.Domain.Enums;

namespace matchmaking.Tests;

public sealed class MatchServiceStateTransitionTests
{
    [Fact]
    public void CreatePendingApplication_WhenNoExistingMatch_InsertsAppliedMatch()
    {
        var repository = new FakeMatchRepository(Array.Empty<Match>());
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        var createdId = service.CreatePendingApplication(1, 100);

        createdId.Should().Be(1);
        repository.InsertedMatches.Should().ContainSingle();
        repository.InsertedMatches[0].Should().BeEquivalentTo(
            new Match { UserId = 1, JobId = 100, Status = MatchStatus.Applied, FeedbackMessage = string.Empty },
            options => options
                .Including(item => item.UserId)
                .Including(item => item.JobId)
                .Including(item => item.Status)
                .Including(item => item.FeedbackMessage));
    }

    [Fact]
    public void CreatePendingApplication_WhenExistingMatch_ThrowsInvalidOperationException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 2, userId: 1, jobId: 100, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        Action act = () => service.CreatePendingApplication(1, 100);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SubmitDecision_WhenRejectedWithoutFeedback_ThrowsArgumentException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 6, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        Action act = () => service.SubmitDecision(6, MatchStatus.Rejected, "   ");

        act.Should().Throw<ArgumentException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenDecisionIsNotAcceptedOrRejected_ThrowsArgumentException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 7, status: MatchStatus.Applied)
        ]);
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        Action act = () => service.SubmitDecision(7, MatchStatus.Advanced, "ok");

        act.Should().Throw<ArgumentException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenTransitionNotAllowed_ThrowsInvalidOperationException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 8, status: MatchStatus.Accepted)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.SubmitDecision(8, MatchStatus.Rejected, "No longer needed");

        act.Should().Throw<InvalidOperationException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenValidInput_UpdatesStatusFeedbackAndTimestamp()
    {
        var match = TestDataFactory.CreateMatch(matchId: 10, status: MatchStatus.Applied, feedback: string.Empty);
        var before = match.Timestamp;
        var repository = new FakeMatchRepository(new[] { match });
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        service.SubmitDecision(10, MatchStatus.Accepted, "  Great fit  ");

        repository.UpdatedMatches.Should().ContainSingle();
        match.Should().BeEquivalentTo(
            new Match { Status = MatchStatus.Accepted, FeedbackMessage = "Great fit" },
            options => options
                .Including(item => item.Status)
                .Including(item => item.FeedbackMessage));
        match.Timestamp.Should().BeAfter(before);
    }

    [Fact]
    public void Advance_WhenApplied_ChangesStatusToAdvanced()
    {
        var match = TestDataFactory.CreateMatch(matchId: 11, status: MatchStatus.Applied);
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.Advance(11);

        repository.UpdatedMatches.Should().ContainSingle();
        match.Status.Should().Be(MatchStatus.Advanced);
    }

    [Fact]
    public void Advance_WhenNotApplied_ThrowsInvalidOperationException()
    {
        var repository = new FakeMatchRepository([
            TestDataFactory.CreateMatch(matchId: 12, status: MatchStatus.Rejected)
        ]);
        var service = new MatchService(repository, new FakeJobService([]));

        Action act = () => service.Advance(12);

        act.Should().Throw<InvalidOperationException>();
        repository.UpdatedMatches.Should().BeEmpty();
    }

    [Fact]
    public void RevertToApplied_WhenCalled_ResetsFeedbackAndStatus()
    {
        var match = TestDataFactory.CreateMatch(matchId: 13, status: MatchStatus.Rejected, feedback: "not enough exp");
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.RevertToApplied(13);

        repository.UpdatedMatches.Should().ContainSingle();
        match.Should().BeEquivalentTo(
            new Match { Status = MatchStatus.Applied, FeedbackMessage = string.Empty },
            options => options
                .Including(item => item.Status)
                .Including(item => item.FeedbackMessage));
    }

    [Fact]
    public async Task GetByCompanyIdAsync_WhenCompanyHasJobs_ReturnsOnlyMatchingJobsSortedByTimestamp()
    {
        var currentUtcTime = DateTime.UtcNow;
        var matchingOlder = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        matchingOlder.Timestamp = currentUtcTime.AddMinutes(-20);
        var matchingNewer = TestDataFactory.CreateMatch(matchId: 2, userId: 2, jobId: 101, status: MatchStatus.Advanced);
        matchingNewer.Timestamp = currentUtcTime.AddMinutes(-5);
        var otherCompany = TestDataFactory.CreateMatch(matchId: 3, userId: 3, jobId: 999, status: MatchStatus.Applied);
        otherCompany.Timestamp = currentUtcTime.AddMinutes(-1);

        var repository = new FakeMatchRepository(new[] { matchingOlder, matchingNewer, otherCompany });
        IReadOnlyList<Job> jobs = new[] { TestDataFactory.CreateJob(jobId: 100, companyId: 1), TestDataFactory.CreateJob(jobId: 101, companyId: 1) };
        var service = new MatchService(repository, new FakeJobService(jobs));

        var result = await service.GetByCompanyIdAsync(1);

        result.Select(item => item.MatchId).Should().Equal(2, 1);
    }

    [Fact]
    public async Task GetByCompanyIdAsync_WhenCompanyHasNoJobs_ReturnsEmptyList()
    {
        var repository = new FakeMatchRepository(new[]
        {
            TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied)
        });
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        var result = await service.GetByCompanyIdAsync(1);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SubmitDecision_WhenMatchDoesNotExist_ThrowsKeyNotFoundException()
    {
        var repository = new FakeMatchRepository(Array.Empty<Match>());
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        Action act = () => service.SubmitDecision(42, MatchStatus.Accepted, "ok");

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void RevertToApplied_WhenMatchDoesNotExist_ThrowsKeyNotFoundException()
    {
        var repository = new FakeMatchRepository(Array.Empty<Match>());
        var service = new MatchService(repository, new FakeJobService(Array.Empty<Job>()));

        Action act = () => service.RevertToApplied(42);

        act.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public async Task AcceptAsync_WhenCalled_SubmitsAcceptedDecision()
    {
        var match = TestDataFactory.CreateMatch(matchId: 21, status: MatchStatus.Applied, feedback: string.Empty);
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        await service.AcceptAsync(21, "  accepted  ");

        match.Should().BeEquivalentTo(
            new Match { Status = MatchStatus.Accepted, FeedbackMessage = "accepted" },
            options => options
                .Including(item => item.Status)
                .Including(item => item.FeedbackMessage));
        repository.UpdatedMatches.Should().ContainSingle(item => item.MatchId == 21);
    }

    [Fact]
    public async Task RejectAsync_WhenCalled_SubmitsRejectedDecision()
    {
        var match = TestDataFactory.CreateMatch(matchId: 22, status: MatchStatus.Applied, feedback: string.Empty);
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        await service.RejectAsync(22, "  rejected  ");

        match.Should().BeEquivalentTo(
            new Match { Status = MatchStatus.Rejected, FeedbackMessage = "rejected" },
            options => options
                .Including(item => item.Status)
                .Including(item => item.FeedbackMessage));
        repository.UpdatedMatches.Should().ContainSingle(item => item.MatchId == 22);
    }

    [Fact]
    public void IsDecisionTransitionAllowed_WhenCurrentIsRejected_ReturnsFalse()
    {
        var service = new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new FakeJobService(Array.Empty<Job>()));
        var rejected = TestDataFactory.CreateMatch(status: MatchStatus.Rejected);

        service.IsDecisionTransitionAllowed(rejected, MatchStatus.Accepted).Should().BeFalse();
        service.IsDecisionTransitionAllowed(rejected, MatchStatus.Rejected).Should().BeFalse();
        service.IsDecisionTransitionAllowed(rejected, MatchStatus.Advanced).Should().BeFalse();
    }

    [Theory]
    [InlineData(MatchStatus.Applied, MatchStatus.Accepted, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Rejected, true)]
    [InlineData(MatchStatus.Applied, MatchStatus.Advanced, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Accepted, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Rejected, true)]
    [InlineData(MatchStatus.Advanced, MatchStatus.Applied, false)]
    [InlineData(MatchStatus.Accepted, MatchStatus.Rejected, false)]
    public void IsDecisionTransitionAllowed_WhenEvaluatingTransition_ReturnsExpectedFlag(
        MatchStatus currentStatus, MatchStatus requestedStatus, bool expected)
    {
        var service = new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new FakeJobService(Array.Empty<Job>()));
        var match = TestDataFactory.CreateMatch(status: currentStatus);

        var result = service.IsDecisionTransitionAllowed(match, requestedStatus);

        result.Should().Be(expected);
    }

    [Fact]
    public void FullTransition_WhenAppliedThenAdvancedThenAccepted_MatchEndsAccepted()
    {
        var match = TestDataFactory.CreateMatch(matchId: 50, status: MatchStatus.Applied, feedback: string.Empty);
        var repository = new FakeMatchRepository([match]);
        var service = new MatchService(repository, new FakeJobService([]));

        service.Advance(50);
        service.SubmitDecision(50, MatchStatus.Accepted, "Great candidate");

        repository.UpdatedMatches.Should().HaveCount(2);
        match.Should().BeEquivalentTo(
            new Match { Status = MatchStatus.Accepted, FeedbackMessage = "Great candidate" },
            options => options
                .Including(item => item.Status)
                .Including(item => item.FeedbackMessage));
    }

    private sealed class FakeMatchRepository : IMatchRepository
    {
        private readonly List<Match> matches;

        public FakeMatchRepository(IReadOnlyList<Match> matches)
        {
            this.matches = matches.ToList();
        }

        public List<Match> InsertedMatches { get; } = new List<Match>();
        public List<Match> UpdatedMatches { get; } = new List<Match>();
        public List<int> RemovedIds { get; } = new List<int>();

        public Match? GetById(int matchId) => matches.FirstOrDefault(item => item.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => matches;
        public void Add(Match match) => matches.Add(match);
        public void Update(Match match) => UpdatedMatches.Add(match);
        public void Remove(int matchId) => RemovedIds.Add(matchId);

        public int InsertReturningId(Match match)
        {
            var nextId = matches.Count == 0 ? 1 : matches.Max(item => item.MatchId) + 1;
            match.MatchId = nextId;
            matches.Add(match);
            InsertedMatches.Add(match);
            return nextId;
        }

        public Match? GetByUserIdAndJobId(int userId, int jobId) =>
            matches.FirstOrDefault(item => item.UserId == userId && item.JobId == jobId);
    }

    private sealed class FakeJobService : IJobService
    {
        private readonly IReadOnlyList<Job> jobs;

        public FakeJobService(IReadOnlyList<Job> jobs)
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
}
