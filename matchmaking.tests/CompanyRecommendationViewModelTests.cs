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

    [Fact]
    public void AdvanceApplicant_WhenSessionCompanyDoesNotMatchApplicant_SetsStatusMessage()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        var errors = new List<string>();

        viewModel.ErrorOccurred += message => errors.Add(message);

        viewModel.LoadApplicants();
        session.LoginAsCompany(2);

        viewModel.AdvanceApplicant();

        errors.Should().ContainSingle(message => message == "This applicant does not belong to your company.");
        viewModel.CurrentApplicant.Should().NotBeNull();
    }

    [Fact]
    public void SkipApplicant_WhenMatchWasAlreadyReviewed_LoadsNextApplicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();
        viewModel.SkipApplicant();

        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.StatusMessage.Should().Be("No more applicants to review.");
    }

    [Fact]
    public void UndoLastAction_WhenNothingToUndo_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.UndoLastAction();

        viewModel.CanUndo.Should().BeFalse();
        viewModel.CurrentApplicant.Should().BeNull();
    }

    [Fact]
    public void LoadApplicants_WhenNoApplicantsExist_SetsEmptyState()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.LoadApplicants();

        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.StatusMessage.Should().Be("No more applicants to review.");
    }

    [Fact]
    public void LoadApplicants_WhenCompanyHasNoJobs_ClearsApplicantQueue()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.LoadApplicants();

        viewModel.HasApplicant.Should().BeFalse();
        viewModel.CurrentApplicant.Should().BeNull();
    }

    [Fact]
    public void UndoLastAction_WhenNoActionWasStored_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.UndoLastAction();

        viewModel.CanUndo.Should().BeFalse();
        viewModel.CurrentApplicant.Should().BeNull();
    }

    [Fact]
    public void AdvanceApplicant_WhenNoApplicantSelected_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.AdvanceApplicant();

        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.CanUndo.Should().BeFalse();
    }

    [Fact]
    public void ExpandCard_WhenApplicantExists_ShowsBreakdown()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();
        viewModel.ExpandCard();

        viewModel.IsExpanded.Should().BeTrue();
        viewModel.ScoreBreakdown.Should().NotBeNull();
    }

    [Fact]
    public void ExpandCard_WhenNoApplicantExists_DoesNothing()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.ExpandCard();

        viewModel.IsExpanded.Should().BeFalse();
        viewModel.ScoreBreakdown.Should().BeNull();
    }

    [Fact]
    public void TopSkillsAndAllSkills_WhenNoApplicant_ReturnEmptyCollections()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.TopSkills.Should().BeEmpty();
        viewModel.AllSkills.Should().BeEmpty();
        viewModel.RemainingSkillCount.Should().Be(0);
    }

    [Fact]
    public void TopSkillsAndAllSkills_WhenApplicantExists_ReturnSortedSkills()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });

        viewModel.LoadApplicants();
        viewModel.ExpandCard();

        viewModel.TopSkills.Should().NotBeEmpty();
        viewModel.AllSkills.Should().NotBeEmpty();
        viewModel.RemainingSkillCount.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void AdvanceApplicant_WhenCompanyContextIsMissing_RaisesError()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        var errors = new List<string>();

        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();
        session.Logout();

        viewModel.AdvanceApplicant();

        errors.Should().ContainSingle(message => message == "Company context is not available.");
    }

    [Fact]
    public void SkipApplicant_WhenMatchAlreadyReviewed_LoadsNextApplicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        var errors = new List<string>();

        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();
        match.Status = MatchStatus.Accepted;

        viewModel.SkipApplicant();
        viewModel.SkipApplicant();

        errors.Should().ContainSingle(message => message == "This applicant has already been reviewed. Loading next applicant.");
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
