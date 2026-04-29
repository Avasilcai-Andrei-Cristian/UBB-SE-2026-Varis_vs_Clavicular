using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Models;
using matchmaking.Services;
namespace matchmaking.Tests;
[Collection("AppState")]
public sealed class UserStatusViewModelTests
{
    private const int UserId = 7;
    private const int JobId = 100;
    private const int MatchId = 1;
    [Fact]
    public async Task LoadMatches_populates_cards_and_filters()
    {
        var session = new SessionContext();
        session.LoginAsUser(UserId);
        var userStatusService = new FakeUserStatusService
        {
            Applications =
            [
                new ApplicationCardModel { MatchId = MatchId, JobId = JobId, Status = MatchStatus.Applied, CompanyName = "Acme" }
            ]
        };
        var skillGapService = new FakeSkillGapService
        {
            Summary = new SkillGapSummaryModel { HasRejections = false, HasSkillGaps = false }
        };
        var vm = CreateViewModel(userStatusService, skillGapService, new FakeJobSkillService());
        await vm.LoadMatches();
        vm.AppliedJobs.Should().ContainSingle();
        vm.FilteredJobs.Should().ContainSingle();
        vm.IsLoading.Should().BeFalse();
        vm.HasSkillGapMessage.Should().BeTrue();
    }
    [Fact]
    public void ApplyFilter_sets_empty_message_when_no_matches()
    {
        var vm = CreateViewModel(new FakeUserStatusService(), new FakeSkillGapService(), new FakeJobSkillService());
        vm.ApplyFilter("Accepted");
        vm.IsEmpty.Should().BeTrue();
        vm.EmptyMessage.Should().Be("You haven't applied to any jobs yet. Head to the Recommendations page to get started.");
        vm.ShowGoToRecommendations.Should().BeTrue();
    }
    [Fact]
    public void ApplyFilter_filters_by_status()
    {
        var userStatusService = new FakeUserStatusService
        {
            Applications =
            [
                new ApplicationCardModel { MatchId = 1, JobId = 1, Status = MatchStatus.Applied },
                new ApplicationCardModel { MatchId = 2, JobId = 2, Status = MatchStatus.Accepted }
            ]
        };
        var vm = CreateViewModel(userStatusService, new FakeSkillGapService(), new FakeJobSkillService());
        vm.AppliedJobs.Add(userStatusService.Applications[0]);
        vm.AppliedJobs.Add(userStatusService.Applications[1]);
        vm.ApplyFilter("Accepted");
        vm.FilteredJobs.Should().ContainSingle();
        vm.FilteredJobs[0].Status.Should().Be(MatchStatus.Accepted);
        vm.ShowCards.Should().BeTrue();
    }
    [Fact]
    public async Task LoadMatches_populates_skill_gap_data_when_available()
    {
        var skillGapService = new FakeSkillGapService
        {
            Summary = new SkillGapSummaryModel { HasRejections = true, HasSkillGaps = true, MissingSkillsCount = 1, SkillsToImproveCount = 1 },
            Missing = [new MissingSkillModel { SkillName = "SQL", RejectedJobCount = 2 }],
            Underscored = [new UnderscoredSkillModel { SkillName = "C#", UserScore = 40, AverageRequiredScore = 70 }]
        };
        var vm = CreateViewModel(new FakeUserStatusService(), skillGapService, new FakeJobSkillService());
        await vm.LoadMatches();
        vm.ShowSkillData.Should().BeTrue();
        vm.SkillGapSummaryText.Should().Be("1 missing skills · 1 skills to improve");
        vm.UnderscoredSkills.Should().ContainSingle();
        vm.SkillGapMissingSkills.Should().ContainSingle();
    }
    [Fact]
    public void Refresh_clears_collections_and_reloads()
    {
        var vm = CreateViewModel(new FakeUserStatusService(), new FakeSkillGapService(), new FakeJobSkillService());
        vm.AppliedJobs.Add(new ApplicationCardModel { MatchId = 1, JobId = 2, Status = MatchStatus.Applied });
        vm.FilteredJobs.Add(new ApplicationCardModel { MatchId = 1, JobId = 2, Status = MatchStatus.Applied });
        vm.UnderscoredSkills.Add(new UnderscoredSkillModel { SkillName = "SQL" });
        vm.SkillGapMissingSkills.Add(new MissingSkillModel { SkillName = "Docker" });
        vm.Refresh();
        vm.AppliedJobs.Should().BeEmpty();
        vm.FilteredJobs.Should().BeEmpty();
        vm.UnderscoredSkills.Should().BeEmpty();
        vm.SkillGapMissingSkills.Should().BeEmpty();
    }
    [Fact]
    public void GetJobSkills_delegates_to_service()
    {
        var jobSkillService = new FakeJobSkillService
        {
            SkillsByJobId = { [JobId] = [new Domain.Entities.JobSkill { JobId = JobId, SkillId = 1, SkillName = "SQL", Score = 60 }] }
        };
        var vm = CreateViewModel(new FakeUserStatusService(), new FakeSkillGapService(), jobSkillService);
        var skills = vm.GetJobSkills(JobId);
        skills.Should().ContainSingle();
        skills[0].SkillName.Should().Be("SQL");
    }
    private static UserStatusViewModel CreateViewModel(IUserStatusService userStatusService, ISkillGapService skillGapService, IJobSkillService jobSkillService)
    {
        var session = new SessionContext();
        session.LoginAsUser(UserId);
        return new UserStatusViewModel(userStatusService, skillGapService, jobSkillService, session);
    }
    private sealed class FakeUserStatusService : IUserStatusService
    {
        public List<ApplicationCardModel> Applications { get; set; } = [];
        public IReadOnlyList<ApplicationCardModel> GetApplicationsForUser(int userId) => Applications;
    }
    private sealed class FakeSkillGapService : ISkillGapService
    {
        public SkillGapSummaryModel Summary { get; set; } = new();
        public List<MissingSkillModel> Missing { get; set; } = [];
        public List<UnderscoredSkillModel> Underscored { get; set; } = [];
        public IReadOnlyList<MissingSkillModel> GetMissingSkills(int userId) => Missing;
        public SkillGapSummaryModel GetSummary(int userId) => Summary;
        public IReadOnlyList<UnderscoredSkillModel> GetUnderscoredSkills(int userId) => Underscored;
    }
    private sealed class FakeJobSkillService : IJobSkillService
    {
        public Dictionary<int, List<Domain.Entities.JobSkill>> SkillsByJobId { get; set; } = new();
        public IReadOnlyList<Domain.Entities.JobSkill> GetByJobId(int jobId) => SkillsByJobId.TryGetValue(jobId, out var list) ? list : [];
        public Domain.Entities.JobSkill? GetById(int jobId, int skillId) => null;
        public IReadOnlyList<Domain.Entities.JobSkill> GetAll() => [];
        public void Add(Domain.Entities.JobSkill jobSkill) { }
        public void Update(Domain.Entities.JobSkill jobSkill) { }
        public void Remove(int jobId, int skillId) { }
    }
}
