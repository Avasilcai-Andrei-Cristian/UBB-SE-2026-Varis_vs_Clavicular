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
    public void AdvanceApplicant_WhenCurrentApplicantExists_ClearsCurrentAndEnablesUndo()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        viewModel.LoadApplicants();

        viewModel.AdvanceApplicant();

        viewModel.CurrentApplicant.Should().BeNull();
        viewModel.CanUndo.Should().BeTrue();
    }

    [Fact]
    public void UndoLastAction_WhenApplicantWasAdvanced_RestoresOriginalApplicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        viewModel.LoadApplicants();
        var firstApplicant = viewModel.CurrentApplicant;
        viewModel.AdvanceApplicant();

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

    [Fact]
    public void AdvanceApplicant_WhenUndoAlreadyUsed_DoesNotStoreNewUndoEntry()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        viewModel.LoadApplicants();

        viewModel.AdvanceApplicant();
        viewModel.UndoLastAction();
        viewModel.AdvanceApplicant();

        viewModel.CanUndo.Should().BeFalse();
        viewModel.CurrentApplicant.Should().BeNull();
    }

    [Fact]
    public void CommandProperties_WhenAccessed_ReturnCommandInstances()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.AdvanceCommand.Should().NotBeNull();
        viewModel.SkipCommand.Should().NotBeNull();
        viewModel.UndoCommand.Should().NotBeNull();
        viewModel.RefreshCommand.Should().NotBeNull();
        viewModel.ExpandCommand.Should().NotBeNull();
        viewModel.CollapseCommand.Should().NotBeNull();
    }

    [Fact]
    public void MaskedContact_WhenNoApplicant_ReturnsEmptyStrings()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModel(session, Array.Empty<Match>());

        viewModel.MaskedEmail.Should().BeEmpty();
        viewModel.MaskedPhone.Should().BeEmpty();
    }

    [Fact]
    public void MaskedContact_WhenRevealFlagEnabled_ReturnsRawValues()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        viewModel.LoadApplicants();

        SetPrivateField(viewModel, "_isContactRevealed", true);

        viewModel.MaskedEmail.Should().Contain("@");
        viewModel.MaskedPhone.Should().NotBeEmpty();
    }

    [Fact]
    public void SkipApplicant_WhenSessionCompanyDoesNotMatchApplicant_RaisesError()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        var errors = new List<string>();
        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();
        session.LoginAsCompany(2);

        viewModel.SkipApplicant();

        errors.Should().ContainSingle(message => message == "This applicant does not belong to your company.");
    }

    [Fact]
    public void AdvanceApplicant_WhenApplicantAlreadyReviewed_LoadsNextApplicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        var errors = new List<string>();
        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();
        match.Status = MatchStatus.Rejected;

        viewModel.AdvanceApplicant();

        errors.Should().ContainSingle(message => message == "This applicant has already been reviewed. Loading next applicant.");
        viewModel.CurrentApplicant.Should().BeNull();
    }

    [Fact]
    public void SkipApplicant_WhenMatchServiceThrows_RaisesError()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModelWithThrowingUpdate(session, forSkipPath: true);
        var errors = new List<string>();
        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();

        viewModel.SkipApplicant();

        errors.Should().ContainSingle(message => message.StartsWith("Could not skip applicant:", StringComparison.Ordinal));
    }

    [Fact]
    public void UndoLastAction_WhenRevertThrows_RaisesError()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModelWithThrowingUpdate(session, forSkipPath: false);
        var errors = new List<string>();
        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();
        viewModel.AdvanceApplicant();

        viewModel.UndoLastAction();

        errors.Should().ContainSingle(message => message.StartsWith("Could not undo:", StringComparison.Ordinal));
    }

    [Fact]
    public void LoadApplicants_WhenRecommendationServiceThrows_RaisesError()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModelWithThrowingLoad(session);
        var errors = new List<string>();
        viewModel.ErrorOccurred += message => errors.Add(message);

        viewModel.LoadApplicants();

        viewModel.CurrentApplicant.Should().BeNull();
        errors.Should().ContainSingle(message => message.StartsWith("Could not load applicants:", StringComparison.Ordinal));
    }

    [Fact]
    public void AdvanceApplicant_WhenMatchServiceThrows_RaisesError()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var viewModel = CreateViewModelWithThrowingUpdate(session, forSkipPath: false, throwOnAdvance: true);
        var errors = new List<string>();
        viewModel.ErrorOccurred += message => errors.Add(message);
        viewModel.LoadApplicants();

        viewModel.AdvanceApplicant();

        errors.Should().ContainSingle(message => message.StartsWith("Could not advance applicant:", StringComparison.Ordinal));
    }

    [Fact]
    public void MaskedContact_WhenEmailAndPhoneAreInvalid_UsesFallbackMasks()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var user = TestDataFactory.CreateUser();
        user.Email = "a@x";
        user.Phone = "12";
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied);
        var viewModel = CreateViewModelWithCustomUser(session, user, job, new[] { match });
        viewModel.LoadApplicants();

        viewModel.MaskedEmail.Should().Be("***@***");
        viewModel.MaskedPhone.Should().Be("***");
    }

    [Fact]
    public void MaskedContact_WhenEmailIsWhitespace_ReturnsEmptyEmail()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var user = TestDataFactory.CreateUser();
        user.Email = "   ";
        user.Phone = "0712345678";
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied);
        var viewModel = CreateViewModelWithCustomUser(session, user, job, new[] { match });
        viewModel.LoadApplicants();

        viewModel.MaskedEmail.Should().BeEmpty();
    }

    [Fact]
    public void MaskedPhone_WhenPhoneHasEnoughDigits_ReturnsMaskedPattern()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: 1, jobId: 100, status: MatchStatus.Applied);
        var viewModel = CreateViewModel(session, new[] { match });
        viewModel.LoadApplicants();

        viewModel.MaskedPhone.Should().Contain("*");
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

    private static CompanyRecommendationViewModel CreateViewModelWithThrowingUpdate(SessionContext session, bool forSkipPath, bool throwOnAdvance = false)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied);
        var matchRepository = new ThrowingUpdateMatchRepository(
            new[] { match },
            throwOnAdvance ? MatchStatus.Advanced : (forSkipPath ? MatchStatus.Rejected : MatchStatus.Applied));
        var jobRepository = new FakeJobRepository(new[] { job });
        var skillRepository = new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) });
        var jobSkillRepository = new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) });
        var jobService = new JobService(jobRepository);
        var recommendationService = new CompanyRecommendationService(
            new MatchService(matchRepository, jobService),
            new UserService(new FakeUserRepository(new[] { user })),
            jobService,
            new SkillService(skillRepository),
            new JobSkillService(jobSkillRepository),
            new RecommendationAlgorithm());
        var viewModel = new CompanyRecommendationViewModel(recommendationService, new MatchService(matchRepository, jobService), session);

        return viewModel;
    }

    private static CompanyRecommendationViewModel CreateViewModelWithCustomUser(SessionContext session, User user, Job job, IReadOnlyList<Match> matches)
    {
        var matchRepository = new FakeMatchRepository(matches);
        var jobRepository = new FakeJobRepository(new[] { job });
        var skillRepository = new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) });
        var jobSkillRepository = new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) });
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

    private static CompanyRecommendationViewModel CreateViewModelWithThrowingLoad(SessionContext session)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var match = TestDataFactory.CreateMatch(matchId: 1, userId: user.UserId, jobId: job.JobId, status: MatchStatus.Applied);
        var matchRepository = new FakeMatchRepository(new[] { match });
        var throwingJobService = new ThrowingJobService();
        var recommendationService = new CompanyRecommendationService(
            new MatchService(matchRepository, throwingJobService),
            new UserService(new FakeUserRepository(new[] { user })),
            throwingJobService,
            new SkillService(new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90) })),
            new JobSkillService(new FakeJobSkillRepository(new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) })),
            new RecommendationAlgorithm());

        return new CompanyRecommendationViewModel(
            recommendationService,
            new MatchService(matchRepository, throwingJobService),
            session);
    }

    private static void SetPrivateField(CompanyRecommendationViewModel viewModel, string fieldName, object value)
    {
        viewModel.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.SetValue(viewModel, value);
    }

    private sealed class ThrowingUpdateMatchRepository : IMatchRepository
    {
        private readonly List<Match> matches;
        private readonly MatchStatus throwOnStatus;

        public ThrowingUpdateMatchRepository(IReadOnlyList<Match> matches, MatchStatus throwOnStatus)
        {
            this.matches = matches.ToList();
            this.throwOnStatus = throwOnStatus;
        }

        public Match? GetById(int matchId) => matches.FirstOrDefault(match => match.MatchId == matchId);
        public IReadOnlyList<Match> GetAll() => matches;
        public void Add(Match match) => matches.Add(match);
        public void Update(Match match)
        {
            if (match.Status == throwOnStatus)
            {
                throw new InvalidOperationException("forced update failure");
            }
        }
        public void Remove(int matchId) => matches.RemoveAll(match => match.MatchId == matchId);
        public int InsertReturningId(Match match) => 1;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => matches.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
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
}
