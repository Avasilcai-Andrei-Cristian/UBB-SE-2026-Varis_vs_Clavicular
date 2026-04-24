namespace matchmaking.Tests;

public sealed class ViewModelHelperCoverageTests
{
    [Fact]
    public void Options_WhenAccessed_ContainsRelevantKeywordEntry()
    {
        DeveloperPostOptions.Options.Should().Contain(item => item.Content == "relevant keyword");
    }

    [Fact]
    public void Options_WhenAccessed_ContainsMitigationFactorTag()
    {
        DeveloperPostOptions.Options.Should().Contain(item => item.Tag == "mitigation factor");
    }

    [Fact]
    public void RelayCommand_WhenCanExecuteIsFalse_ReturnsFalse()
    {
        var command = new RelayCommand(() => { }, () => false);

        command.CanExecute(null).Should().BeFalse();
    }

    [Fact]
    public void RelayCommand_WhenExecuteIsCalled_InvokesAction()
    {
        var count = 0;
        var command = new RelayCommand(() => count++);

        command.Execute(null);

        count.Should().Be(1);
    }

    [Fact]
    public void SetProperty_WhenValueIsNew_ReturnsTrue()
    {
        var model = new TestObservableObject();

        var result = model.UpdateValue(1);

        result.Should().BeTrue();
    }

    [Fact]
    public void SetProperty_WhenValueIsUnchanged_ReturnsFalse()
    {
        var model = new TestObservableObject();
        model.UpdateValue(1);

        var result = model.UpdateValue(1);

        result.Should().BeFalse();
    }

    [Fact]
    public void SetProperty_WhenValueChangesAfterAnUnchangedSet_ReturnsTrue()
    {
        var model = new TestObservableObject();
        model.UpdateValue(1);

        var result = model.UpdateValue(2);

        result.Should().BeTrue();
    }

    [Fact]
    public void CompanyRecommendationViewModel_WhenNoApplicantAvailable_ShowsEmptyCollectionsForSkills()
    {
        var session = new SessionContext();
        session.LoginAsCompany(1);
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var skill = TestDataFactory.CreateSkill(user.UserId, 1, "C#", 90);
        var jobSkill = TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80);

        var matchRepository = new FakeMatchRepository(Array.Empty<Match>());
        var jobRepository = new FakeJobRepository(new[] { job });
        var jobService = new JobService(jobRepository);
        var recommendationService = new CompanyRecommendationService(
            new MatchService(matchRepository, jobService),
            new UserService(new FakeUserRepository(new[] { user })),
            jobService,
            new SkillService(new FakeSkillRepository(new[] { skill })),
            new JobSkillService(new FakeJobSkillRepository(new[] { jobSkill })),
            new RecommendationAlgorithm());

        var viewModel = new CompanyRecommendationViewModel(recommendationService, new MatchService(matchRepository, jobService), session);

        viewModel.LoadApplicants();
        viewModel.TopSkills.Should().BeEmpty();
        viewModel.AllSkills.Should().BeEmpty();
        viewModel.RemainingSkillCount.Should().Be(0);
    }

    private sealed class TestObservableObject : ObservableObject
    {
        private int value;

        public bool UpdateValue(int newValue)
        {
            return SetProperty(ref value, newValue);
        }
    }
}
