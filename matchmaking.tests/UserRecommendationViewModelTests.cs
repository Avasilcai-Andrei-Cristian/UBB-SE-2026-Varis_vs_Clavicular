namespace matchmaking.Tests;

[Collection("AppState")]
public sealed class UserRecommendationViewModelTests
{
    [Fact]
    public async Task InitializeAsync_WhenRecommendationExists_PopulatesCurrentJob()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });

            await viewModel.InitializeAsync();

            viewModel.HasCard.Should().BeTrue();
            viewModel.CurrentJob.Should().NotBeNull();
            viewModel.ShowEmptyDeck.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenUserSessionIsMissing_SetsErrorMessage()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).Logout();

            await viewModel.InitializeAsync();

            viewModel.HasError.Should().BeTrue();
            viewModel.ErrorMessage.Should().Be("User session is not available.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenNoJobsExist_ShowsEmptyDeck()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, Array.Empty<Job>());

            await viewModel.InitializeAsync();

            viewModel.CurrentJob.Should().BeNull();
            viewModel.ShowEmptyDeck.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void LoadRecommendations_WhenDatabaseUnavailable_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), false);
        SetAppFlag(nameof(App.DatabaseConnectionError), "Database unavailable.");

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            var raised = string.Empty;
            viewModel.ErrorOccurred += message => raised = message;

            viewModel.LoadRecommendations();

            viewModel.HasError.Should().BeTrue();
            viewModel.ErrorMessage.Should().Be("Database unavailable.");
            raised.Should().Be("Database unavailable.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_AndUndoAsync_RestoreTheCurrentCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });

            await viewModel.InitializeAsync();
            var originalCard = viewModel.CurrentJob;

            await viewModel.LikeAsync();
            viewModel.CanUndo.Should().BeTrue();
            viewModel.CurrentJob.Should().BeNull();

            await viewModel.UndoAsync();
            viewModel.CurrentJob.Should().Be(originalCard);
            viewModel.CanUndo.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task ResetDraftFilters_ClearsSelections()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
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
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_AndUndoAsync_RestoreTheCurrentCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var job = TestDataFactory.CreateJob();
            var viewModel = CreateViewModel(user, new[] { job });

            await viewModel.InitializeAsync();
            var originalCard = viewModel.CurrentJob;

            await viewModel.DismissAsync();
            viewModel.CanUndo.Should().BeTrue();

            await viewModel.UndoAsync();
            viewModel.CurrentJob.Should().Be(originalCard);
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void LoadRecommendations_WhenSessionIsNotUserMode_SetsErrorMessage()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).Logout();

            viewModel.LoadRecommendations();

            viewModel.HasError.Should().BeTrue();
            viewModel.ErrorMessage.Should().Be("User session is not available.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task ApplyFiltersAsync_WhenFiltersAreSelected_SetsFilterState()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.DraftEmploymentSelections[0].IsChecked = true;
            viewModel.DraftExperienceSelections[0].IsChecked = true;
            viewModel.DraftSkillSelections[0].IsChecked = true;
            viewModel.DraftLocation = "Cluj";

            await viewModel.ApplyFiltersAsync();

            viewModel.IsFilterOpen.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenNoCurrentJob_DoesNothing()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            await viewModel.DismissAsync();

            viewModel.CurrentJob.Should().BeNull();
            viewModel.CanUndo.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task InitializeAsync_WhenFirstJobIsOnCooldown_UsesFallbackCard()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
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
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenNoCurrentJob_DoesNothing()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            GetSession(viewModel).LoginAsCompany(1);

            await viewModel.LikeAsync();

            viewModel.ErrorMessage.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void ResetDraftFilters_WhenNothingIsSelected_KeepsSelectionsCleared()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.ResetDraftFilters();

            viewModel.DraftEmploymentSelections.Should().OnlyContain(item => !item.IsChecked);
            viewModel.DraftLocation.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task LikeAsync_WhenSessionIsNotUserMode_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

            await viewModel.LikeAsync();

            viewModel.ErrorMessage.Should().BeEmpty();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task DismissAsync_WhenSessionIsNotUserMode_RaisesError()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });
            await viewModel.InitializeAsync();
            SetSessionMode(GetSession(viewModel), AppMode.CompanyMode);

            await viewModel.DismissAsync();

            viewModel.ErrorMessage.Should().Be("Invalid session for this action.");
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public async Task OpenAndCloseDetailCommands_UpdateDetailState()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            await viewModel.InitializeAsync();
            viewModel.OpenDetailCommand.Execute(null);
            viewModel.IsDetailOpen.Should().BeTrue();

            viewModel.CloseDetailCommand.Execute(null);
            viewModel.IsDetailOpen.Should().BeFalse();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
    }

    [Fact]
    public void OpenFiltersCommand_SetsFilterOpen()
    {
        var previousAvailability = GetAppFlag<bool>(nameof(App.IsDatabaseConnectionAvailable));
        var previousError = GetAppFlag<string>(nameof(App.DatabaseConnectionError));
        SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), true);
        SetAppFlag(nameof(App.DatabaseConnectionError), string.Empty);

        try
        {
            var user = TestDataFactory.CreateUser();
            var viewModel = CreateViewModel(user, new[] { TestDataFactory.CreateJob() });

            viewModel.OpenFiltersCommand.Execute(null);

            viewModel.IsFilterOpen.Should().BeTrue();
        }
        finally
        {
            SetAppFlag(nameof(App.IsDatabaseConnectionAvailable), previousAvailability);
            SetAppFlag(nameof(App.DatabaseConnectionError), previousError);
        }
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

    private static void SetSessionMode(SessionContext session, AppMode mode)
    {
        typeof(SessionContext).GetProperty(nameof(SessionContext.CurrentMode))!.SetValue(session, mode);
    }
}
