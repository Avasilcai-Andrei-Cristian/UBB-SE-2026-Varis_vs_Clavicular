namespace matchmaking.Tests;

public sealed class ViewModelHelperCoverageTests
{
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
}
