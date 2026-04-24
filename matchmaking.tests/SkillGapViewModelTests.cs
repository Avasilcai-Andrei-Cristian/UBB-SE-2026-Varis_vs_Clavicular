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
    public void Refresh_WhenCalled_ClearsState()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        viewModel.Refresh();

        viewModel.ShowContent.Should().BeFalse();
        viewModel.HasSkillData.Should().BeFalse();
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
}
