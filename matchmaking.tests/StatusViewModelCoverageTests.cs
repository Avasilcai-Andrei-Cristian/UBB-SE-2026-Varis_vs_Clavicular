using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;

namespace matchmaking.Tests;

[Collection("AppState")]
public sealed class StatusViewModelCoverageTests
{
    [Fact]
    public async Task CompanyStatusViewModel_LoadEvaluationAsync_WhenMatchIsAccepted_RevealsContactAndClearsDecision()
    {
        var data = CreateCompanyStatusHarness(MatchStatus.Accepted);

        await data.ViewModel.LoadEvaluationAsync(data.Match.MatchId);

        data.ViewModel.SelectedDecision.Should().Be(MatchStatus.Accepted);
        data.ViewModel.ContactEmailDisplay.Should().Contain("@");
        data.ViewModel.ContactPhoneDisplay.Should().Contain(data.User.Phone[^3..]);
    }

    [Fact]
    public async Task CompanyStatusViewModel_LoadEvaluationAsync_WhenCompanyContextIsMissing_ReturnsFalse()
    {
        var data = CreateCompanyStatusHarness(MatchStatus.Accepted);
        data.Session.Logout();

        var result = await data.ViewModel.LoadEvaluationAsync(data.Match.MatchId);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CompanyStatusViewModel_LoadApplicationsAsync_WhenSessionIsNotCompanyMode_SetsErrorMessage()
    {
        var data = CreateCompanyStatusHarness(MatchStatus.Accepted);
        data.Session.Logout();

        await data.ViewModel.LoadApplicationsAsync();

        data.ViewModel.PageMessage.Should().BeEmpty();
        data.ViewModel.Applications.Should().BeEmpty();
    }

    [Fact]
    public async Task SkillGapViewModel_LoadData_WhenNoRejections_SetsSummaryMessage()
    {
        var user = TestDataFactory.CreateUser();
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var viewModel = CreateSkillGapViewModel(Array.Empty<Match>());

        await viewModel.LoadData();

        viewModel.HasSummaryMessage.Should().BeTrue();
        viewModel.SummaryMessage.Should().Contain("No rejections yet");
    }

    [Fact]
    public async Task SkillGapViewModel_LoadData_WhenRejectionsExistAndGapsExist_PopulatesCollections()
    {
        var previousSession = GetAppSession();
        var session = new SessionContext();
        session.LoginAsUser(1);
        SetAppSession(session);

        try
        {
        var viewModel = CreateSkillGapViewModel(new[]
        {
            TestDataFactory.CreateMatch(1, 1, 100, MatchStatus.Rejected, "review")
        });

        await viewModel.LoadData();

        viewModel.HasSkillData.Should().BeTrue();
        }
        finally
        {
            SetAppSession(previousSession);
        }
    }

    private static SkillGapViewModel CreateSkillGapViewModel(IReadOnlyList<Match> matches)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 40);
        var jobSkill = TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70);
        var matchRepository = new FakeMatchRepository(matches);
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(new[] { jobSkill }));
        var skillService = new SkillService(new FakeSkillRepository(new[] { skill }));
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);

        return new SkillGapViewModel(new SkillGapService(matchRepository, jobSkillService, skillService), session);
    }

    private static (CompanyStatusViewModel ViewModel, SessionContext Session, Match Match, User User) CreateCompanyStatusHarness(MatchStatus status)
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, status, "feedback");

        var session = new SessionContext();
        session.LoginAsCompany(company.CompanyId);

        var jobRepository = new FakeJobRepository(new[] { job });
        var viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
                new UserService(new UserRepository()),
                new JobService(jobRepository),
                new SkillService(new SkillRepository())),
            new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
            new FakeTestingModuleAdapter(),
            session);

        return (viewModel, session, match, user);
    }

    private static SessionContext? GetAppSession()
    {
        return (SessionContext?)typeof(App).GetProperty(nameof(App.Session))!.GetValue(null);
    }

    private static void SetAppSession(SessionContext? session)
    {
        typeof(App).GetProperty(nameof(App.Session))!.SetValue(null, session);
    }
}
