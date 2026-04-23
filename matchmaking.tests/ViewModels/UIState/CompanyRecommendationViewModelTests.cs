namespace matchmaking.Tests;

public sealed class CompanyRecommendationViewModelTests
{
    [Fact]
    public void LoadApplicants_WhenSessionIsNotCompanyMode_SetsStatusMessage()
    {
        var viewModel = CreateViewModel(new SessionContext(), Array.Empty<Match>());

        viewModel.LoadApplicants();

        viewModel.StatusMessage.Should().Be("Company mode is not active.");
        viewModel.HasApplicant.Should().BeFalse();
    }

    [Fact]
    public void LoadApplicants_WhenApplicantExists_PopulatesCurrentApplicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();

        viewModel.HasApplicant.Should().BeTrue();
        viewModel.CurrentApplicant.Should().NotBeNull();
        viewModel.StatusMessage.Should().BeEmpty();
    }

    [Fact]
    public void ExpandCard_WhenApplicantExists_SetsScoreBreakdown()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();
        viewModel.ExpandCard();

        viewModel.IsExpanded.Should().BeTrue();
        viewModel.ScoreBreakdown.Should().NotBeNull();
        viewModel.MaskedEmail.Should().NotBeEmpty();
    }

    [Fact]
    public void AdvanceApplicant_AndUndoLastAction_RestoreApplicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();
        var firstApplicant = viewModel.CurrentApplicant;

        viewModel.AdvanceApplicant();
        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.CanUndo.Should().BeTrue();

        viewModel.UndoLastAction();

        viewModel.CurrentApplicant.Should().Be(firstApplicant);
        viewModel.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void CollapseCard_WhenExpanded_SetsExpansionFalse()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();
        viewModel.ExpandCard();
        viewModel.CollapseCard();

        viewModel.IsExpanded.Should().BeFalse();
    }

    private static CompanyRecommendationViewModel CreateViewModel(SessionContext session, IReadOnlyList<Match> matches)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90);
        var jobSkill = TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80);

        var matchRepository = new FakeMatchRepository(matches);
        var jobRepository = new FakeJobRepository(new[] { job });
        var skillRepository = new FakeSkillRepository(new[] { skill });
        var jobSkillRepository = new FakeJobSkillRepository(new[] { jobSkill });

        var jobService = new JobService(jobRepository);
        var recommendationService = new CompanyRecommendationService(
            new MatchService(matchRepository, jobService),
            new UserService(new FakeUserRepository(new[] { user })),
            jobService,
            new SkillService(skillRepository),
            new JobSkillService(jobSkillRepository),
            new RecommendationAlgorithm());

        return new CompanyRecommendationViewModel(recommendationService, new MatchService(matchRepository, jobService), session);
    }
}
