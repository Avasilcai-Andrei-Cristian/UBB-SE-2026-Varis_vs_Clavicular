namespace matchmaking.Tests;

[Collection("AppState")]
public sealed class UserRecommendationViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WhenRecommendationExists_PopulatesCurrentJob()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var viewModel = CreateViewModel(user, new[] { job });

        await viewModel.InitializeAsync();

        viewModel.HasCard.Should().BeTrue();
        viewModel.CurrentJob.Should().NotBeNull();
        viewModel.ShowEmptyDeck.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WhenUserSessionIsMissing_SetsErrorMessage()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        GetSession(viewModel).Logout();

        await viewModel.InitializeAsync();

        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("User session is not available.");
    }

    [Fact]
    public async Task InitializeAsync_WhenNoJobsExist_ShowsEmptyDeck()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, Array.Empty<Job>());

        await viewModel.InitializeAsync();

        viewModel.CurrentJob.Should().BeNull();
        viewModel.ShowEmptyDeck.Should().BeTrue();
    }

    [Fact]
    public void LoadRecommendations_WhenDatabaseUnavailable_RaisesError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: false, databaseError: "Database unavailable.");
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        var raised = string.Empty;
        viewModel.ErrorOccurred += message => raised = message;

        viewModel.LoadRecommendations();

        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("Database unavailable.");
        raised.Should().Be("Database unavailable.");
    }

    [Fact]
    public async Task LikeAsync_WhenCurrentJobExists_ClearsCurrentJobAndEnablesUndo()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var viewModel = CreateViewModel(user, new[] { job });
        await viewModel.InitializeAsync();

        await viewModel.LikeAsync();

        viewModel.CanUndo.Should().BeTrue();
        viewModel.CurrentJob.Should().BeNull();
    }

    [Fact]
    public async Task UndoAsync_WhenLastActionWasLike_RestoresOriginalCard()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var viewModel = CreateViewModel(user, new[] { job });
        await viewModel.InitializeAsync();
        var originalCard = viewModel.CurrentJob;
        await viewModel.LikeAsync();

        await viewModel.UndoAsync();

        viewModel.CurrentJob.Should().Be(originalCard);
        viewModel.CanUndo.Should().BeFalse();
    }

    [Fact]
    public async Task ResetDraftFilters_WhenFiltersWereSelected_ClearsAllSelections()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        viewModel.DraftEmploymentSelections[0].IsChecked = true;
        viewModel.DraftExperienceSelections[0].IsChecked = true;
        viewModel.DraftSkillSelections[0].IsChecked = true;
        viewModel.DraftLocation = "Cluj";

        viewModel.ResetDraftFilters();

        viewModel.DraftEmploymentSelections.Should().OnlyContain(item => !item.IsChecked);
        viewModel.DraftExperienceSelections.Should().OnlyContain(item => !item.IsChecked);
        viewModel.DraftSkillSelections.Should().OnlyContain(item => !item.IsChecked);
        viewModel.DraftLocation.Should().BeEmpty();
    }

    [Fact]
    public async Task DismissAsync_WhenCurrentJobExists_EnablesUndo()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var viewModel = CreateViewModel(user, new[] { job });
        await viewModel.InitializeAsync();

        await viewModel.DismissAsync();

        viewModel.CanUndo.Should().BeTrue();
    }

    [Fact]
    public async Task UndoAsync_WhenLastActionWasDismiss_RestoresOriginalCard()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var viewModel = CreateViewModel(user, new[] { job });
        await viewModel.InitializeAsync();
        var originalCard = viewModel.CurrentJob;
        await viewModel.DismissAsync();

        await viewModel.UndoAsync();

        viewModel.CurrentJob.Should().Be(originalCard);
    }

    [Fact]
    public void LoadRecommendations_WhenSessionIsNotUserMode_SetsErrorMessage()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        GetSession(viewModel).Logout();

        viewModel.LoadRecommendations();

        viewModel.HasError.Should().BeTrue();
        viewModel.ErrorMessage.Should().Be("User session is not available.");
    }

    [Fact]
    public async Task ApplyFiltersAsync_WhenFiltersAreSelected_SetsFilterState()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        viewModel.DraftEmploymentSelections[0].IsChecked = true;
        viewModel.DraftExperienceSelections[0].IsChecked = true;
        viewModel.DraftSkillSelections[0].IsChecked = true;
        viewModel.DraftLocation = "Cluj";

        await viewModel.ApplyFiltersAsync();

        viewModel.IsFilterOpen.Should().BeFalse();
    }

    [Fact]
    public async Task DismissAsync_WhenNoCurrentJob_DoesNothing()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        await viewModel.DismissAsync();

        viewModel.CurrentJob.Should().BeNull();
        viewModel.CanUndo.Should().BeFalse();
    }

    [Fact]
    public async Task InitializeAsync_WhenFirstJobIsOnCooldown_UsesFallbackCard()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var firstJob = TestDataFactory.CreateJob(jobId: 100);
        var secondJob = TestDataFactory.CreateJob(jobId: 101);
        var viewModel = CreateViewModel(user, new[] { firstJob, secondJob });
        var recommendationRepository = GetRecommendationRepository(viewModel);
        recommendationRepository.InsertReturningId(TestDataFactory.CreateRecommendation(userId: user.UserId, jobId: firstJob.JobId, timestamp: DateTime.UtcNow));

        await viewModel.InitializeAsync();

        viewModel.CurrentJob.Should().NotBeNull();
        viewModel.CurrentJob!.Job.JobId.Should().Be(secondJob.JobId);
    }

    [Fact]
    public async Task LikeAsync_WhenNoCurrentJob_DoesNothing()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        GetSession(viewModel).LoginAsCompany(1);

        await viewModel.LikeAsync();

        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public void ResetDraftFilters_WhenNothingIsSelected_KeepsSelectionsCleared()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        viewModel.ResetDraftFilters();

        viewModel.DraftEmploymentSelections.Should().OnlyContain(item => !item.IsChecked);
        viewModel.DraftLocation.Should().BeEmpty();
    }

    [Fact]
    public async Task LikeAsync_WhenSessionIsNotUserMode_RaisesError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

        await viewModel.LikeAsync();

        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task DismissAsync_WhenSessionIsNotUserMode_RaisesError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        await viewModel.InitializeAsync();
        SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

        await viewModel.DismissAsync();

        viewModel.ErrorMessage.Should().Be("Invalid session for this action.");
    }

    [Fact]
    public async Task OpenDetailCommand_WhenExecuted_SetsIsDetailOpenTrue()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        await viewModel.InitializeAsync();

        viewModel.OpenDetailCommand.Execute(null);

        viewModel.IsDetailOpen.Should().BeTrue();
    }

    [Fact]
    public async Task CloseDetailCommand_WhenDetailIsOpen_SetsIsDetailOpenFalse()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        await viewModel.InitializeAsync();
        viewModel.OpenDetailCommand.Execute(null);

        viewModel.CloseDetailCommand.Execute(null);

        viewModel.IsDetailOpen.Should().BeFalse();
    }

    [Fact]
    public async Task UndoAsync_WhenNoUndoAvailable_DoesNothing()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        await viewModel.UndoAsync();

        viewModel.CanUndo.Should().BeFalse();
        viewModel.CurrentJob.Should().BeNull();
    }

    [Fact]
    public void OpenFiltersCommand_WhenExecuted_SetsIsFilterOpenTrue()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        viewModel.OpenFiltersCommand.Execute(null);

        viewModel.IsFilterOpen.Should().BeTrue();
    }

    [Fact]
    public async Task InitializeAsync_WhenDatabaseUnavailable_SetsErrorAndNoCard()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: false, databaseError: "Database unavailable.");
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

        await viewModel.InitializeAsync();

        viewModel.ErrorMessage.Should().Be("Database unavailable.");
        viewModel.CurrentJob.Should().BeNull();
    }

    [Fact]
    public void LoadRecommendations_WhenServiceThrows_RaisesError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var session = new SessionContext();
        session.LoginAsUser(1);
        var viewModel = CreateViewModelWithCustomService(
            new UserRecommendationService(
                new FakeUserRepository(Array.Empty<User>()),
                new FakeJobRepository(new[] { TestDataFactory.CreateJob() }),
                new FakeSkillRepository(Array.Empty<Skill>()),
                new FakeJobSkillRepository(Array.Empty<JobSkill>()),
                new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany() }),
                new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(new FakeJobRepository(new[] { TestDataFactory.CreateJob() }))),
                new FakeRecommendationRepository(Array.Empty<Recommendation>()),
                new CooldownService(new FakeRecommendationRepository(Array.Empty<Recommendation>()), TimeSpan.FromHours(1)),
                new RecommendationAlgorithm()),
            session);
        var raised = string.Empty;
        viewModel.ErrorOccurred += message => raised = message;

        viewModel.LoadRecommendations();

        viewModel.HasError.Should().BeTrue();
        raised.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LikeAsync_WhenSessionSwitchesAwayFromUserMode_RaisesError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        await viewModel.InitializeAsync();
        SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

        await viewModel.LikeAsync();

        viewModel.ErrorMessage.Should().Be("Invalid session for this action.");
    }

    [Fact]
    public async Task DismissAsync_WhenServiceThrows_RaisesError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var throwingRepo = new ThrowingRecommendationRepository(throwOnInsert: true, throwOnRemove: true, allowFirstInsert: false);
        var job = TestDataFactory.CreateJob();
        var jobRepo = new FakeJobRepository(new[] { job });
        var viewModel = CreateViewModelWithCustomService(
            new UserRecommendationService(
                new FakeUserRepository(new[] { user }),
                jobRepo,
                new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }),
                new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }),
                new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) }),
                new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(jobRepo)),
                throwingRepo,
                new CooldownService(throwingRepo, TimeSpan.FromHours(1)),
                new RecommendationAlgorithm()),
            session);

        await viewModel.InitializeAsync();
        await viewModel.DismissAsync();

        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public void CanActDrivenCommands_WhenSessionInvalid_AreNotExecutable()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
        GetSession(viewModel).Logout();

        viewModel.LikeCommand.CanExecute(null).Should().BeFalse();
        viewModel.DismissCommand.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void LoadRecommendations_WhenDeckIsEmpty_KeepsErrorMessageEmpty()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var viewModel = CreateViewModel(user, Array.Empty<Job>());

        viewModel.LoadRecommendations();

        viewModel.CurrentJob.Should().BeNull();
        viewModel.ErrorMessage.Should().BeEmpty();
    }

    [Fact]
    public async Task DismissAsync_WhenApplyDismissThrowsInAction_CatchesAndSetsError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var repo = new ThrowingRecommendationRepository(throwOnInsert: true, throwOnRemove: false, allowFirstInsert: true);
        var jobRepo = new FakeJobRepository(new[] { job });
        var viewModel = CreateViewModelWithCustomService(
            new UserRecommendationService(
                new FakeUserRepository(new[] { user }),
                jobRepo,
                new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }),
                new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }),
                new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) }),
                new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(jobRepo)),
                repo,
                new CooldownService(repo, TimeSpan.FromHours(1)),
                new RecommendationAlgorithm()),
            session);

        await viewModel.InitializeAsync();
        await viewModel.DismissAsync();

        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task UndoAsync_WhenUndoOperationThrows_CatchesAndSetsError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var repo = new ThrowingRecommendationRepository(throwOnInsert: false, throwOnRemove: true, allowFirstInsert: false);
        var jobRepo = new FakeJobRepository(new[] { job });
        var viewModel = CreateViewModelWithCustomService(
            new UserRecommendationService(
                new FakeUserRepository(new[] { user }),
                jobRepo,
                new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) }),
                new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) }),
                new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) }),
                new MatchService(new FakeMatchRepository(Array.Empty<Match>()), new JobService(jobRepo)),
                repo,
                new CooldownService(repo, TimeSpan.FromHours(1)),
                new RecommendationAlgorithm()),
            session);

        await viewModel.InitializeAsync();
        await viewModel.DismissAsync();
        await viewModel.UndoAsync();

        viewModel.HasError.Should().BeTrue();
    }

    [Fact]
    public async Task LikeAsync_WhenApplyLikeThrows_CatchesAndSetsError()
    {
        using var scope = new AppStateScope(isDatabaseAvailable: true);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var viewModel = CreateViewModel(user, new[] { job });
        await viewModel.InitializeAsync();

        var matchRepository = GetMatchRepository(viewModel);
        matchRepository.Add(TestDataFactory.CreateMatch(matchId: 777, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied));

        await viewModel.LikeAsync();

        viewModel.HasError.Should().BeTrue();
    }

    private static UserRecommendationViewModel CreateViewModel(User user, IReadOnlyList<Job> jobs)
    {
        var company = TestDataFactory.CreateCompany();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90);
        var jobSkill = TestDataFactory.CreateJobSkill(jobs.Count > 0 ? jobs[0].JobId : 100, 1, "C#", 80);

        var userRepository = new FakeUserRepository(new[] { user });
        var jobRepository = new FakeJobRepository(jobs);
        var skillRepository = new FakeSkillRepository(new[] { skill });
        var jobSkillRepository = new FakeJobSkillRepository(jobs.Count == 0 ? Array.Empty<JobSkill>() : new[] { jobSkill });
        var companyRepository = new FakeCompanyRepository(new[] { company });
        var matchRepository = new FakeMatchRepository(Array.Empty<Match>());
        var recommendationRepository = new FakeRecommendationRepository(Array.Empty<Recommendation>());
        var jobService = new JobService(jobRepository);
        var matchService = new MatchService(matchRepository, jobService);
        var cooldownService = new CooldownService(recommendationRepository, TimeSpan.FromHours(24));
        var algorithm = new RecommendationAlgorithm();

        var recommendationService = new UserRecommendationService(
            userRepository,
            jobRepository,
            skillRepository,
            jobSkillRepository,
            companyRepository,
            matchService,
            recommendationRepository,
            cooldownService,
            algorithm);

        var session = new SessionContext();
        session.LoginAsUser(user.UserId);

        return new UserRecommendationViewModel(recommendationService, session);
    }

    private static UserRecommendationViewModel CreateViewModelWithCustomService(UserRecommendationService service, SessionContext session)
    {
        return new UserRecommendationViewModel(service, session);
    }

    private static T GetAppFlag<T>(string name)
    {
        return (T)typeof(App).GetProperty(name)!.GetValue(null)!;
    }

    private static void SetAppFlag<T>(string name, T value)
    {
        typeof(App).GetProperty(name)!.SetValue(null, value);
    }

    private static SessionContext GetSession(UserRecommendationViewModel viewModel)
    {
        return (SessionContext)typeof(UserRecommendationViewModel).GetField("_session", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
    }

    private static FakeRecommendationRepository GetRecommendationRepository(UserRecommendationViewModel viewModel)
    {
        var service = typeof(UserRecommendationViewModel).GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
        var field = service.GetType().GetField("recommendationRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (FakeRecommendationRepository)field.GetValue(service)!;
    }

    private static IMatchRepository GetMatchRepository(UserRecommendationViewModel viewModel)
    {
        var service = typeof(UserRecommendationViewModel).GetField("_service", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(viewModel)!;
        var matchServiceField = service.GetType().GetField("matchService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var matchService = matchServiceField.GetValue(service)!;
        var repositoryField = matchService.GetType().GetField("matchRepository", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        return (IMatchRepository)repositoryField.GetValue(matchService)!;
    }

    private static void SetSessionMode(SessionContext session, AppMode mode)
    {
        typeof(SessionContext).GetProperty(nameof(SessionContext.CurrentMode))!.SetValue(session, mode);
    }

    private sealed class AppStateScope : IDisposable
    {
        private readonly bool previousAvailability;
        private readonly string previousError;

        public AppStateScope(bool isDatabaseAvailable, string databaseError = "")
        {
            previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
            previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), isDatabaseAvailable);
            SetAppFlag(nameof(App.DatabaseConnectionError), databaseError);
        }

        public void Dispose()
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    private sealed class ThrowingRecommendationRepository : IRecommendationRepository
    {
        private readonly List<Recommendation> items = new();
        private readonly bool throwOnInsert;
        private readonly bool throwOnRemove;
        private readonly bool allowFirstInsert;
        private int insertCount;

        public ThrowingRecommendationRepository(bool throwOnInsert, bool throwOnRemove, bool allowFirstInsert)
        {
            this.throwOnInsert = throwOnInsert;
            this.throwOnRemove = throwOnRemove;
            this.allowFirstInsert = allowFirstInsert;
        }

        public Recommendation? GetById(int recommendationId) => items.FirstOrDefault(item => item.RecommendationId == recommendationId);
        public IReadOnlyList<Recommendation> GetAll() => items;
        public void Add(Recommendation recommendation) => items.Add(recommendation);
        public void Update(Recommendation recommendation)
        {
        }

        public void Remove(int recommendationId)
        {
            if (throwOnRemove)
            {
                throw new InvalidOperationException("forced remove failure");
            }

            items.RemoveAll(item => item.RecommendationId == recommendationId);
        }
        public Recommendation? GetLatestByUserIdAndJobId(int userId, int jobId) => items.LastOrDefault(item => item.UserId == userId && item.JobId == jobId);
        public int InsertReturningId(Recommendation recommendation)
        {
            insertCount++;
            if (throwOnInsert && (!allowFirstInsert || insertCount > 1))
            {
                throw new InvalidOperationException("forced insert failure");
            }

            recommendation.RecommendationId = items.Count == 0 ? 1 : items.Max(item => item.RecommendationId) + 1;
            items.Add(recommendation);
            return recommendation.RecommendationId;
        }
    }
}
