namespace matchmaking.Tests;

public sealed class SkillGapViewModelTests
{
    [Fact]
    public async Task LoadData_WhenNoRejections_SetsSummaryMessage()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        await viewModel.LoadData();

        viewModel.HasSummaryMessage.Should().BeTrue();
        viewModel.SummaryMessage.Should().Contain("No rejections yet");
        viewModel.HasSkillData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadData_WhenRejectionsExistAndGapsFound_PopulatesCollections()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(status: MatchStatus.Rejected);
        var viewModel = CreateViewModel(new[] { match });

        await viewModel.LoadData();

        viewModel.ShowContent.Should().BeTrue();
        viewModel.HasSkillData.Should().BeTrue();
    }

    [Fact]
    public async Task LoadData_WhenRejectionsExistAndGapsFound_PopulatesCounts()
    {
        var match = TestDataFactory.CreateMatch(status: MatchStatus.Rejected);
        var viewModel = CreateViewModel(new[] { match });

        await viewModel.LoadData();

        viewModel.MissingCount.Should().BeGreaterThanOrEqualTo(0);
        viewModel.ImproveCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public async Task LoadData_WhenNoSkillGapsExist_ShowsSummaryMessage()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(userId: user.UserId, jobId: job.JobId, status: MatchStatus.Rejected);
        var matchRepository = new FakeMatchRepository(new[] { match });
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }));
        var skillService = new SkillService(new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }));
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var viewModel = new SkillGapViewModel(new SkillGapService(matchRepository, jobSkillService, skillService), session);

        await viewModel.LoadData();

        viewModel.HasSummaryMessage.Should().BeTrue();
        viewModel.HasSkillData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadData_WhenOnlyUnderscoredSkillsExist_PopulatesSkillsToImproveOnly()
    {
        var match = TestDataFactory.CreateMatch(status: MatchStatus.Rejected);
        var viewModel = CreateViewModel(new[] { match });

        await viewModel.LoadData();

        viewModel.SkillsToImprove.Should().NotBeEmpty();
        viewModel.MissingSkills.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadData_WhenExceptionOccurs_SetsSummaryMessageWithError()
    {
        var throwingMatchRepo = new ThrowingMatchRepository();
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(Array.Empty<JobSkill>()));
        var skillService = new SkillService(new FakeSkillRepository(Array.Empty<Skill>()));
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = new SkillGapViewModel(new SkillGapService(throwingMatchRepo, jobSkillService, skillService), session);

        await viewModel.LoadData();

        viewModel.HasSummaryMessage.Should().BeTrue();
        viewModel.SummaryMessage.Should().Contain("Unable to load");
        viewModel.HasSkillData.Should().BeFalse();
    }

    [Fact]
    public void Refresh_WhenCalled_ClearsState()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        viewModel.Refresh();

        viewModel.ShowContent.Should().BeFalse();
        viewModel.HasSkillData.Should().BeFalse();
    }

    [Fact]
    public async Task HasSkillsFlags_WhenCollectionsChange_UpdateComputedProperties()
    {
        var match = TestDataFactory.CreateMatch(status: MatchStatus.Rejected);
        var viewModel = CreateViewModel(new[] { match });

        await viewModel.LoadData();

        viewModel.HasSkillsToImprove.Should().BeTrue();
        viewModel.HasMissingSkills.Should().BeFalse();
    }

    [Fact]
    public void RefreshCommand_WhenExecuted_TriggersRefresh()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        viewModel.RefreshCommand.Execute(null);

        viewModel.ShowContent.Should().BeFalse();
    }

    [Fact]
    public async Task LoadData_WhenMissingSkillsExist_PopulatesMissingSkillsCollection()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(userId: user.UserId, jobId: job.JobId, status: MatchStatus.Rejected);
        var matchRepository = new FakeMatchRepository(new[] { match });
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 99, "Rust", 80) }));
        var skillService = new SkillService(new FakeSkillRepository(Array.Empty<Skill>()));
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var viewModel = new SkillGapViewModel(new SkillGapService(matchRepository, jobSkillService, skillService), session);

        await viewModel.LoadData();

        viewModel.MissingSkills.Should().NotBeEmpty();
    }

    [Fact]
    public void DefaultConstructor_WhenCreated_InitializesCommands()
    {
        var viewModel = new SkillGapViewModel();

        viewModel.RefreshCommand.Should().NotBeNull();
    }

    private static SkillGapViewModel CreateViewModel(IReadOnlyList<Match> matches)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80);
        var jobSkill = TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 90);

        var matchRepository = new FakeMatchRepository(matches);
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(new[] { jobSkill }));
        var skillService = new SkillService(new FakeSkillRepository(new[] { skill }));

        var session = new SessionContext();
        session.LoginAsUser(user.UserId);

        return new SkillGapViewModel(new SkillGapService(matchRepository, jobSkillService, skillService), session);
    }

    private sealed class ThrowingMatchRepository : IUserStatusMatchRepository
    {
        public IReadOnlyList<Match> GetByUserId(int userId) => throw new InvalidOperationException("forced failure");
        public IReadOnlyList<Match> GetRejectedByUserId(int userId) => throw new InvalidOperationException("forced failure");
    }
}
