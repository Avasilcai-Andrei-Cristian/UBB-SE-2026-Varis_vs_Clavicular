namespace matchmaking.Tests;

public sealed class CompanyStatusViewModelTests
{
    [Fact]
    public async Task LoadApplicationsAsync_WhenCompanyHasApplicants_PopulatesApplicationsAndMessage()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        await harness.ViewModel.LoadApplicationsAsync();

        harness.ViewModel.Applications.Should().NotBeEmpty();
        harness.ViewModel.PageMessage.Should().Contain("applicant(s)");
    }

    [Fact]
    public async Task LoadEvaluationAsync_WhenApplicantExists_PopulatesSelectedState()
    {
        var testResult = new TestResult { IsValid = true };
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter(testResult));

        var result = await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        result.Should().BeTrue();
        harness.ViewModel.SelectedApplicant.Should().NotBeNull();
        harness.ViewModel.SelectedDecision.Should().Be(MatchStatus.Accepted);
        harness.ViewModel.LastTestResult.Should().NotBeNull();
    }

    [Fact]
    public void ValidateDecision_WhenNothingIsSelected_ReturnsError()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        var result = harness.ViewModel.ValidateDecision();

        result.Should().BeFalse();
        harness.ViewModel.ValidationErrorDecision.Should().Be("Select an applicant first.");
    }

    [Fact]
    public void ValidateFeedback_WhenMessageIsTooLong_ReturnsError()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        harness.ViewModel.FeedbackMessage = new string('a', 501);

        harness.ViewModel.ValidateFeedback().Should().BeFalse();
        harness.ViewModel.ValidationErrorFeedback.Should().Contain("500 characters or fewer");
    }

    [Fact]
    public void CancelEvaluation_WhenStateExists_ClearsSelection()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        harness.ViewModel.SelectedApplicant = harness.Result;
        harness.ViewModel.CancelEvaluation();

        harness.ViewModel.SelectedApplicant.Should().BeNull();
        harness.ViewModel.HasValidationErrors.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitDecisionAsync_WhenValidationFails_ReturnsFalse()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);
        harness.ViewModel.SelectedDecision = null;

        (await harness.ViewModel.SubmitDecisionAsync()).Should().BeFalse();
    }

    private static (CompanyStatusViewModel ViewModel, Match Match, UserApplicationResult Result) CreateHarness(MatchStatus status, ITestingModuleAdapter adapter)
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
            adapter,
            session);

        var result = new UserApplicationResult
        {
            User = user,
            Job = job,
            Match = match
        };

        return (viewModel, match, result);
    }
}
