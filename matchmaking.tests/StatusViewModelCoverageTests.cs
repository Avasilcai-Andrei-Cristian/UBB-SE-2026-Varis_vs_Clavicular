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

    private static (CompanyStatusViewModel ViewModel, SessionContext Session, Match Match, User User) CreateCompanyStatusHarness(MatchStatus status)
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, status, "feedback");

        var session = new SessionContext();
        session.LoginAsCompany(company.CompanyId);

        var jobRepository = new FakeJobRepository(new[] { job });
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 70);
        var viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
                new UserService(new FakeUserRepository(new[] { user })),
                new JobService(jobRepository),
                new SkillService(new FakeSkillRepository(new[] { skill }))),
            new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
            new FakeTestingModuleAdapter(),
            session);

        return (viewModel, session, match, user);
    }
}
