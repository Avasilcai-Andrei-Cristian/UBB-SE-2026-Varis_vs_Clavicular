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

    [Fact]
    public async Task LoadEvaluationAsync_WhenTestingModuleIsUnavailable_ReturnsFallbackResult()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new ThrowingTestingModuleAdapter());

        var result = await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        result.Should().BeTrue();
        harness.ViewModel.LastTestResult.Should().NotBeNull();
        harness.ViewModel.LastTestResult!.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task SubmitDecisionAsync_WhenValid_SavesDecision()
    {
        var harness = CreateHarness(MatchStatus.Advanced, new FakeTestingModuleAdapter(new TestResult { IsValid = true }));

        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);
        harness.ViewModel.SelectedDecision = MatchStatus.Rejected;
        harness.ViewModel.FeedbackMessage = "Looks good overall.";

        (await harness.ViewModel.SubmitDecisionAsync()).Should().BeTrue();
    }

    [Fact]
    public async Task SubmitDecisionAsync_WhenNoApplicantSelected_ReturnsFalse()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        (await harness.ViewModel.SubmitDecisionAsync()).Should().BeFalse();
        harness.ViewModel.HasValidationErrors.Should().BeTrue();
    }

    [Fact]
    public async Task LoadApplicationsAsync_WhenNoApplicantsExist_SetsEmptyMessage()
    {
        var harness = CreateHarness(MatchStatus.Applied, new FakeTestingModuleAdapter());

        await harness.ViewModel.LoadApplicationsAsync();

        harness.ViewModel.Applications.Should().BeEmpty();
        harness.ViewModel.PageMessage.Should().Contain("No applicants found");
    }

    [Fact]
    public async Task LoadEvaluationAsync_WhenTestingModuleReturnsNull_SetsError()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter(null));

        var result = await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        result.Should().BeTrue();
        harness.ViewModel.LastTestResult.Should().BeNull();
    }

    [Fact]
    public void ValidateFeedback_WhenMessageIsValid_ReturnsTrue()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        harness.ViewModel.FeedbackMessage = "Looks good.";

        harness.ViewModel.ValidateFeedback().Should().BeTrue();
    }

    [Fact]
    public async Task SelectedApplicant_WhenSetToNull_ClearsEvaluationState()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter(new TestResult { IsValid = true }));
        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        harness.ViewModel.SelectedApplicant = null;

        harness.ViewModel.SelectedMatch.Should().BeNull();
        harness.ViewModel.SelectedDecision.Should().BeNull();
        harness.ViewModel.FeedbackMessage.Should().BeEmpty();
        harness.ViewModel.LastTestResult.Should().BeNull();
    }

    [Fact]
    public void ContactDisplays_WhenNoApplicantSelected_AreEmpty()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        harness.ViewModel.ContactEmailDisplay.Should().BeEmpty();
        harness.ViewModel.ContactPhoneDisplay.Should().BeEmpty();
    }

    [Fact]
    public async Task ContactDisplays_WhenAcceptedDecision_Selected_ShowUnmaskedValues()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());
        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        harness.ViewModel.ContactEmailDisplay.Should().Be(harness.Result.User.Email);
        harness.ViewModel.ContactPhoneDisplay.Should().Be(harness.Result.User.Phone);
    }

    [Fact]
    public async Task ContactDisplays_WhenNotAccepted_ShowMaskedValues()
    {
        var harness = CreateHarness(MatchStatus.Rejected, new FakeTestingModuleAdapter());
        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        harness.ViewModel.ContactEmailDisplay.Should().NotBe(harness.Result.User.Email);
        harness.ViewModel.ContactPhoneDisplay.Should().NotBe(harness.Result.User.Phone);
    }

    [Fact]
    public async Task LoadApplicationsAsync_WhenCompanyModeIsInactive_RaisesError()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());
        var errors = new List<string>();
        harness.ViewModel.ErrorOccurred += message => errors.Add(message);
        var session = GetSession(harness.ViewModel);
        session.Logout();

        await harness.ViewModel.LoadApplicationsAsync();

        errors.Should().ContainSingle(message => message == "Company mode is not active.");
        harness.ViewModel.Applications.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadApplicationsAsync_WhenUnderlyingServiceThrows_RaisesError()
    {
        var harness = CreateThrowingHarness();
        var errors = new List<string>();
        harness.ViewModel.ErrorOccurred += message => errors.Add(message);

        await harness.ViewModel.LoadApplicationsAsync();

        errors.Should().ContainSingle(message => message.StartsWith("Could not load applicants:", StringComparison.Ordinal));
        harness.ViewModel.Applications.Should().BeEmpty();
    }

    [Fact]
    public async Task LoadEvaluationAsync_WhenApplicantMissing_ReturnsFalseAndRaisesError()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());
        var errors = new List<string>();
        harness.ViewModel.ErrorOccurred += message => errors.Add(message);

        var result = await harness.ViewModel.LoadEvaluationAsync(9999);

        result.Should().BeFalse();
        errors.Should().ContainSingle(message => message == "Selected applicant could not be loaded.");
    }

    [Fact]
    public async Task RefreshAsync_WhenCalled_ExecutesLoadApplications()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        await harness.ViewModel.RefreshAsync();

        harness.ViewModel.Applications.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SubmitDecisionAsync_WhenMatchServiceThrows_ReturnsFalseAndClearsPageMessage()
    {
        var harness = CreateThrowingSubmitHarness();
        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);
        harness.ViewModel.SelectedDecision = MatchStatus.Accepted;
        harness.ViewModel.FeedbackMessage = "valid";

        var result = await harness.ViewModel.SubmitDecisionAsync();

        result.Should().BeFalse();
        harness.ViewModel.PageMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task Masking_WhenEmailOrPhoneIsShort_UsesFallbackMasks()
    {
        var harness = CreateHarness(MatchStatus.Rejected, new FakeTestingModuleAdapter());
        harness.Result.User.Email = "a@x";
        harness.Result.User.Phone = "12";
        harness.Match.UserId = harness.Result.User.UserId;
        harness.Match.JobId = harness.Result.Job.JobId;

        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        harness.ViewModel.ContactEmailDisplay.Should().Be("***@***");
        harness.ViewModel.ContactPhoneDisplay.Should().Be("***");
    }

    [Fact]
    public void IsLoadingAndRefreshCommand_WhenAccessed_AreAvailable()
    {
        var harness = CreateHarness(MatchStatus.Accepted, new FakeTestingModuleAdapter());

        harness.ViewModel.IsLoading.Should().BeFalse();
        harness.ViewModel.RefreshCommand.Should().NotBeNull();
    }

    [Fact]
    public async Task LoadEvaluationAsync_WhenUnderlyingServiceThrows_ReturnsFalseAndRaisesError()
    {
        var harness = CreateThrowingHarness();
        var errors = new List<string>();
        harness.ViewModel.ErrorOccurred += message => errors.Add(message);

        var result = await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        result.Should().BeFalse();
        errors.Should().ContainSingle(message => message.StartsWith("Could not load applicant details:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SubmitDecisionAsync_WhenValidationFailsAfterSelection_ReturnsFalse()
    {
        var harness = CreateHarness(MatchStatus.Advanced, new FakeTestingModuleAdapter(new TestResult { IsValid = true }));
        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);
        harness.ViewModel.SelectedDecision = MatchStatus.Rejected;
        harness.ViewModel.FeedbackMessage = "   ";

        var result = await harness.ViewModel.SubmitDecisionAsync();

        result.Should().BeFalse();
    }

    [Fact]
    public async Task ContactDisplay_WhenEmailIsWhitespace_ReturnsEmptyMaskedEmail()
    {
        var harness = CreateHarness(MatchStatus.Rejected, new FakeTestingModuleAdapter());
        harness.Result.User.Email = "   ";

        await harness.ViewModel.LoadEvaluationAsync(harness.Match.MatchId);

        harness.ViewModel.ContactEmailDisplay.Should().BeEmpty();
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
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 70);
        var viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
                new UserService(new FakeUserRepository(new[] { user })),
                new JobService(jobRepository),
                new SkillService(new FakeSkillRepository(new[] { skill }))),
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

    private static (CompanyStatusViewModel ViewModel, Match Match, UserApplicationResult Result) CreateThrowingHarness()
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var session = new SessionContext();
        session.LoginAsCompany(company.CompanyId);

        var throwingJobService = new ThrowingJobService();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 70);
        var viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                new MatchService(new FakeMatchRepository(new[] { match }), throwingJobService),
                new UserService(new FakeUserRepository(new[] { user })),
                throwingJobService,
                new SkillService(new FakeSkillRepository(new[] { skill }))),
            new MatchService(new FakeMatchRepository(new[] { match }), throwingJobService),
            new FakeTestingModuleAdapter(),
            session);

        return (viewModel, match, new UserApplicationResult { User = user, Job = job, Match = match });
    }

    private static (CompanyStatusViewModel ViewModel, Match Match, UserApplicationResult Result) CreateThrowingSubmitHarness()
    {
        var user = TestDataFactory.CreateUser();
        var company = TestDataFactory.CreateCompany();
        var job = TestDataFactory.CreateJob(companyId: company.CompanyId);
        var match = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Advanced, "feedback");
        var session = new SessionContext();
        session.LoginAsCompany(company.CompanyId);
        var jobRepository = new FakeJobRepository(new[] { job });
        var throwingMatchService = new MatchService(new ThrowingUpdateMatchRepository(new[] { match }), new JobService(jobRepository));
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 70);

        var viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                new MatchService(new FakeMatchRepository(new[] { match }), new JobService(jobRepository)),
                new UserService(new FakeUserRepository(new[] { user })),
                new JobService(jobRepository),
                new SkillService(new FakeSkillRepository(new[] { skill }))),
            throwingMatchService,
            new FakeTestingModuleAdapter(new TestResult { IsValid = true }),
            session);

        return (viewModel, match, new UserApplicationResult { User = user, Job = job, Match = match });
    }

    private static SessionContext GetSession(CompanyStatusViewModel viewModel)
    {
        return (SessionContext)typeof(CompanyStatusViewModel)
            .GetField("_session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
            .GetValue(viewModel)!;
    }

    private sealed class ThrowingJobService : IJobService
    {
        public Job? GetById(int jobId) => throw new InvalidOperationException("forced load failure");
        public IReadOnlyList<Job> GetAll() => throw new InvalidOperationException("forced load failure");
        public IReadOnlyList<Job> GetByCompanyId(int companyId) => throw new InvalidOperationException("forced load failure");
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

    private sealed class ThrowingUpdateMatchRepository : IMatchRepository
    {
        private readonly List<Match> matches;

        public ThrowingUpdateMatchRepository(IReadOnlyList<Match> matches)
        {
            this.matches = matches.ToList();
        }

        public Match? GetById(int matchId) => matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => matches;
        public void Add(Match match) => matches.Add(match);
        public void Update(Match match) => throw new InvalidOperationException("forced submit failure");
        public void Remove(int matchId) => matches.RemoveAll(match => match.MatchId == matchId);
        public int InsertReturningId(Match match) => 1;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
    }

    private sealed class ThrowingTestingModuleAdapter : ITestingModuleAdapter
    {
        public Task<TestResult?> GetResultForMatchAsync(int matchId) => throw new InvalidOperationException();
        public Task<TestResult?> GetLatestResultForCandidateAsync(int externalUserId, int positionId) => throw new InvalidOperationException();
        public Task<IReadOnlyList<TestResult>> GetResultHistoryForCandidateAsync(int externalUserId, int positionId) => throw new InvalidOperationException();
    }
}
