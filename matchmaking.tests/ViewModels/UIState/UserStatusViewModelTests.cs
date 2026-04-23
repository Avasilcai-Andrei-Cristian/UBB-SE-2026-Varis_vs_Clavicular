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

    private static UserStatusViewModel CreateViewModel(IReadOnlyList<Match> matches)
    {
        var user = TestDataFactory.CreateUser();
        var job = TestDataFactory.CreateJob();
        var company = TestDataFactory.CreateCompany();
        var jobSkills = new[] { TestDataFactory.CreateJobSkill(job.JobId, 1, "C#", 80) };

        var matchRepository = new FakeMatchRepository(matches);
        var jobRepository = new FakeJobRepository(new[] { job });
        var companyRepository = new FakeCompanyRepository(new[] { company });
        var skillRepository = new FakeSkillRepository(new[] { TestDataFactory.CreateSkill(user.UserId, 1, "C#", 80) });
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
