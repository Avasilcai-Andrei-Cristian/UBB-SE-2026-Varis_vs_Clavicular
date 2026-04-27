namespace matchmaking.Tests;

public sealed class UserStatusViewModelTests
{
    [Fact]
    public async Task LoadMatches_WhenUserHasNoApplications_SetsEmptyState()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        await viewModel.LoadMatches();

        viewModel.IsEmpty.Should().BeTrue();
        viewModel.ShowGoToRecommendations.Should().BeTrue();
        viewModel.EmptyMessage.Should().Contain("haven't applied");
    }

    [Fact]
    public void ApplyFilter_WhenNoMatchingApplications_SetsEmptyMessage()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        viewModel.ApplyFilter("Applied");

        viewModel.IsEmpty.Should().BeTrue();
        viewModel.ShowGoToRecommendations.Should().BeTrue();
    }

    [Fact]
    public async Task LoadMatches_WhenApplicationsExistAndSkillGapsExist_ShowsSkillData()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Rejected, "feedback");
        var viewModel = CreateViewModel(new[] { rejectedMatch });

        await viewModel.LoadMatches();

        viewModel.IsEmpty.Should().BeFalse();
        viewModel.ShowSkillData.Should().BeTrue();
    }

    [Fact]
    public async Task LoadMatches_WhenAllJobsMeetRequirements_ShowsSummaryMessage()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var acceptedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var viewModel = CreateViewModel(new[] { acceptedMatch });

        await viewModel.LoadMatches();

        viewModel.HasSkillGapMessage.Should().BeTrue();
        viewModel.ShowSkillData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadMatches_WhenCurrentFilterMatchesAccepted_ShowsCards()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var acceptedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var viewModel = CreateViewModel(new[] { acceptedMatch });

        await viewModel.LoadMatches();
        viewModel.ApplyFilter("Accepted");

        viewModel.ShowCards.Should().BeTrue();
        viewModel.IsEmpty.Should().BeFalse();
    }

    [Fact]
    public async Task ApplyFilter_WhenAcceptedMatchExists_ShowsMatchingCard()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var acceptedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var viewModel = CreateViewModel(new[] { acceptedMatch });

        await viewModel.LoadMatches();
        viewModel.ApplyFilter("Accepted");

        viewModel.FilteredJobs.Should().ContainSingle();
        viewModel.ShowCards.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyFilter_WhenRejectedMatchExists_ShowsMatchingCard()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Rejected, "feedback");
        var viewModel = CreateViewModel(new[] { rejectedMatch });

        await viewModel.LoadMatches();
        viewModel.ApplyFilter("Rejected");

        viewModel.FilteredJobs.Should().ContainSingle();
        viewModel.ShowCards.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyFilter_WhenAllFilterSelected_ShowsAllJobs()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var acceptedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var viewModel = CreateViewModel(new[] { acceptedMatch });

        await viewModel.LoadMatches();
        viewModel.ApplyFilter("All");

        viewModel.FilteredJobs.Count.Should().Be(viewModel.AppliedJobs.Count);
        viewModel.ShowCards.Should().BeTrue();
    }

    [Fact]
    public async Task ApplyFilter_WhenAppliedJobsNotEmptyButNoneMatchFilter_SetsNoMatchMessage()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var acceptedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var viewModel = CreateViewModel(new[] { acceptedMatch });

        await viewModel.LoadMatches();
        viewModel.ApplyFilter("Rejected");

        viewModel.IsEmpty.Should().BeTrue();
        viewModel.EmptyMessage.Should().Contain("No applications match");
        viewModel.ShowGoToRecommendations.Should().BeFalse();
    }

    [Fact]
    public void Refresh_WhenCalled_ClearsCollections()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        viewModel.Refresh();

        viewModel.AppliedJobs.Should().BeEmpty();
        viewModel.FilteredJobs.Should().BeEmpty();
    }

    [Fact]
    public async Task SidebarFlags_WhenSkillCollectionsArePopulated_ReturnTrue()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Rejected, "feedback");
        var viewModel = CreateViewModel(new[] { rejectedMatch });

        await viewModel.LoadMatches();

        viewModel.HasUnderscoredSkills.Should().BeTrue();
        viewModel.HasSidebarMissingSkills.Should().BeFalse();
    }

    [Fact]
    public async Task GetJobSkills_WhenJobIdIsProvided_ReturnsJobSkills()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var acceptedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Accepted, "feedback");
        var viewModel = CreateViewModel(new[] { acceptedMatch });

        await viewModel.LoadMatches();
        var skills = viewModel.GetJobSkills(job.JobId);

        skills.Should().NotBeEmpty();
        skills.Should().ContainSingle(jobSkill => jobSkill.JobId == job.JobId);
    }

    [Fact]
    public async Task LoadMatches_WhenRejectedApplicationsHaveNoGaps_ShowsPositiveSummaryMessage()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Rejected, "feedback");
        var matchRepository = new FakeMatchRepository(new[] { rejectedMatch });
        var jobRepository = new FakeJobRepository(new[] { job });
        var companyRepository = new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) });
        var skillRepository = new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 95) });
        var jobSkillRepository = new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) });
        var jobSkillService = new JobSkillService(jobSkillRepository);
        var viewModel = new UserStatusViewModel(
            new UserStatusService(matchRepository, new JobService(jobRepository), new CompanyService(companyRepository), new SkillService(skillRepository), jobSkillService),
            new SkillGapService(matchRepository, jobSkillService, new SkillService(skillRepository)),
            jobSkillService,
            new SessionContext());

        await viewModel.LoadMatches();

        viewModel.HasSkillGapMessage.Should().BeTrue();
        viewModel.ShowSkillData.Should().BeFalse();
    }

    [Fact]
    public async Task LoadMatches_WhenMissingSkillsExist_PopulatesMissingSkillsSidebar()
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var rejectedMatch = TestDataFactory.CreateMatch(1, user.UserId, job.JobId, MatchStatus.Rejected, "feedback");
        var matchRepository = new FakeMatchRepository(new[] { rejectedMatch });
        var jobRepository = new FakeJobRepository(new[] { job });
        var companyRepository = new FakeCompanyRepository(new[] { TestDataFactory.CreateCompany(job.CompanyId) });
        var skillRepository = new FakeSkillRepository(Array.Empty<Skill>());
        var jobSkillRepository = new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 99, "Rust", 70) });
        var jobSkillService = new JobSkillService(jobSkillRepository);
        var session = new SessionContext();
        session.LoginAsUser(user.UserId);
        var viewModel = new UserStatusViewModel(
            new UserStatusService(matchRepository, new JobService(jobRepository), new CompanyService(companyRepository), new SkillService(skillRepository), jobSkillService),
            new SkillGapService(matchRepository, jobSkillService, new SkillService(skillRepository)),
            jobSkillService,
            session);

        await viewModel.LoadMatches();

        viewModel.SkillGapMissingSkills.Should().NotBeEmpty();
    }

    [Fact]
    public async Task LoadMatches_WhenServiceThrows_SetsErrorFlags()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var jobSkillService = new JobSkillService(new FakeJobSkillRepository(Array.Empty<JobSkill>()));
        var viewModel = new UserStatusViewModel(
            new UserStatusService(
                new ThrowingMatchRepository(),
                new JobService(new FakeJobRepository(Array.Empty<Job>())),
                new CompanyService(new FakeCompanyRepository(Array.Empty<Company>())),
                new SkillService(new FakeSkillRepository(Array.Empty<Skill>())),
                jobSkillService),
            new SkillGapService(new ThrowingMatchRepository(), jobSkillService, new SkillService(new FakeSkillRepository(Array.Empty<Skill>()))),
            jobSkillService,
            session);

        await viewModel.LoadMatches();

        viewModel.HasError.Should().BeTrue();
        viewModel.ShowCards.Should().BeFalse();
    }

    [Fact]
    public void RefreshCommand_WhenExecuted_InvokesRefresh()
    {
        var viewModel = CreateViewModel(Array.Empty<Match>());

        viewModel.RefreshCommand.Execute(null);

        viewModel.AppliedJobs.Should().BeEmpty();
    }

    [Fact]
    public void DefaultConstructor_WhenCreated_InitializesRefreshCommand()
    {
        var viewModel = new UserStatusViewModel();

        viewModel.RefreshCommand.Should().NotBeNull();
    }

    private sealed class ThrowingMatchRepository : IUserStatusMatchRepository
    {
        public IReadOnlyList<Match> GetByUserId(int userId) => throw new InvalidOperationException("forced failure");
        public IReadOnlyList<Match> GetRejectedByUserId(int userId) => throw new InvalidOperationException("forced failure");
    }

    private static UserStatusViewModel CreateViewModel(IReadOnlyList<Match> matches)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var company = TestDataFactory.CreateCompany();
        var jobSkills = new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 70) };

        var matchRepository = new FakeMatchRepository(matches);
        var jobRepository = new FakeJobRepository(new[] { job });
        var companyRepository = new FakeCompanyRepository(new[] { company });
        var skillRepository = new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 40) });
        var jobSkillRepository = new FakeJobSkillRepository(jobSkills);

        var jobService = new JobService(jobRepository);
        var companyService = new CompanyService(companyRepository);
        var skillService = new SkillService(skillRepository);
        var jobSkillService = new JobSkillService(jobSkillRepository);
        var userStatusService = new UserStatusService(matchRepository, jobService, companyService, skillService, jobSkillService);
        var skillGapService = new SkillGapService(matchRepository, jobSkillService, skillService);

        var session = new SessionContext();
        session.LoginAsUser(user.UserId);

        return new UserStatusViewModel(userStatusService, skillGapService, jobSkillService, session);
    }
}
