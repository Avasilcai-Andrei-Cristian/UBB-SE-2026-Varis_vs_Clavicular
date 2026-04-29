using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.DTOs;
using matchmaking.Services;
namespace matchmaking.Tests;
[Collection("AppState")]
public sealed class CompanyRecommendationViewModelTests
{
    private const int CompanyId = 7;
    private const int MatchId = 11;
    private const int OtherCompanyId = 99;
    [Fact]
    public void LoadApplicants_sets_status_message_when_not_in_company_mode()
    {
        var session = new SessionContext();
        session.LoginAsUser(1);
        var vm = CreateViewModel(session: session);
        vm.LoadApplicants();
        vm.StatusMessage.Should().Be("Company mode is not active.");
        vm.CurrentApplicant.Should().BeNull();
    }
    [Fact]
    public void LoadApplicants_sets_no_more_message_when_service_returns_null()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var service = new FakeCompanyRecommendationService();
        var vm = CreateViewModel(session: session, recommendationService: service);
        vm.LoadApplicants();
        vm.StatusMessage.Should().Be("No more applicants to review.");
        vm.HasApplicant.Should().BeFalse();
    }
    [Fact]
    public void LoadApplicants_sets_current_applicant_from_service()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var applicant = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied);
        var service = new FakeCompanyRecommendationService { NextApplicant = applicant };
        var vm = CreateViewModel(session: session, recommendationService: service);
        vm.LoadApplicants();
        vm.CurrentApplicant.Should().BeSameAs(applicant);
        vm.HasApplicant.Should().BeTrue();
    }
    [Fact]
    public void AdvanceApplicant_reports_error_when_applicant_does_not_match_company()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var applicant = BuildApplicant(companyId: OtherCompanyId, matchStatus: MatchStatus.Applied);
        var service = new FakeCompanyRecommendationService { NextApplicant = applicant };
        var vm = CreateViewModel(session: session, recommendationService: service);
        var errors = new List<string>();
        vm.ErrorOccurred += errors.Add;
        vm.LoadApplicants();
        vm.AdvanceApplicant();
        errors.Should().ContainSingle().Which.Should().Be("This applicant does not belong to your company.");
    }
    [Fact]
    public void AdvanceApplicant_moves_to_next_and_sets_undo()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var first = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: MatchId, userId: 1, jobId: 100);
        var second = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: 12, userId: 2, jobId: 200);
        var service = new FakeCompanyRecommendationService { Queue = [first, second] };
        var matchService = new FakeMatchService { ById = new Dictionary<int, Match> { [MatchId] = first.Match, [12] = second.Match } };
        var vm = CreateViewModel(session: session, recommendationService: service, matchService: matchService);
        vm.LoadApplicants();
        vm.AdvanceApplicant();
        matchService.AdvancedMatches.Should().ContainSingle().Which.Should().Be(MatchId);
        vm.CurrentApplicant.Should().BeSameAs(second);
        vm.CanUndo.Should().BeTrue();
    }
    [Fact]
    public void SkipApplicant_rejects_and_moves_to_next()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var first = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: MatchId, userId: 1, jobId: 100);
        var second = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: 12, userId: 2, jobId: 200);
        var service = new FakeCompanyRecommendationService { Queue = [first, second] };
        var matchService = new FakeMatchService { ById = new Dictionary<int, Match> { [MatchId] = first.Match, [12] = second.Match } };
        var vm = CreateViewModel(session: session, recommendationService: service, matchService: matchService);
        vm.LoadApplicants();
        vm.SkipApplicant();
        matchService.RejectedMatches.Should().ContainSingle().Which.Should().Be(MatchId);
        vm.CurrentApplicant.Should().BeSameAs(second);
    }
    [Fact]
    public void ValidateApplicantState_loads_next_and_reports_error_when_status_is_not_applied()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var first = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Accepted, matchId: MatchId, userId: 1, jobId: 100);
        var second = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: 12, userId: 2, jobId: 200);
        var service = new FakeCompanyRecommendationService { Queue = [first, second] };
        var matchService = new FakeMatchService
        {
            ById = new Dictionary<int, Match>
            {
                [MatchId] = new Match { MatchId = MatchId, UserId = 1, JobId = 100, Status = MatchStatus.Accepted },
                [12] = second.Match
            }
        };
        var errors = new List<string>();
        var vm = CreateViewModel(session: session, recommendationService: service, matchService: matchService);
        vm.ErrorOccurred += errors.Add;
        vm.LoadApplicants();
        vm.AdvanceApplicant();
        vm.CurrentApplicant.Should().BeSameAs(second);
        errors.Should().ContainSingle().Which.Should().Be("This applicant has already been reviewed. Loading next applicant.");
    }
    [Fact]
    public void UndoLastAction_reverts_to_applied_and_restores_applicant()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var first = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: MatchId, userId: 1, jobId: 100);
        var second = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, matchId: 12, userId: 2, jobId: 200);
        var service = new FakeCompanyRecommendationService { Queue = [first, second] };
        var matchService = new FakeMatchService { ById = new Dictionary<int, Match> { [MatchId] = first.Match, [12] = second.Match } };
        var vm = CreateViewModel(session: session, recommendationService: service, matchService: matchService);
        vm.LoadApplicants();
        vm.AdvanceApplicant();
        vm.UndoLastAction();
        matchService.RevertedMatches.Should().ContainSingle().Which.Should().Be(MatchId);
        vm.CurrentApplicant.Should().BeSameAs(first);
        vm.CanUndo.Should().BeFalse();
    }
    [Fact]
    public void ExpandCard_loads_score_breakdown_and_sets_expanded()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var applicant = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied);
        var breakdown = new CompatibilityBreakdown { OverallScore = 77 };
        var service = new FakeCompanyRecommendationService { NextApplicant = applicant, Breakdown = breakdown };
        var vm = CreateViewModel(session: session, recommendationService: service);
        vm.LoadApplicants();
        vm.ExpandCard();
        vm.IsExpanded.Should().BeTrue();
        vm.ScoreBreakdown.Should().BeSameAs(breakdown);
    }
    [Fact]
    public void Masked_contact_is_hidden_until_revealed()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var applicant = BuildApplicant(companyId: CompanyId, matchStatus: MatchStatus.Applied, email: "a@example.com", phone: "0712345678");
        var service = new FakeCompanyRecommendationService { NextApplicant = applicant };
        var vm = CreateViewModel(session: session, recommendationService: service);
        vm.LoadApplicants();
        vm.MaskedEmail.Should().Be("a***@example.com");
        vm.MaskedPhone.Should().Be("07*****678");
    }
    private static CompanyRecommendationViewModel CreateViewModel(SessionContext? session = null, FakeCompanyRecommendationService? recommendationService = null, FakeMatchService? matchService = null)
    {
        return new CompanyRecommendationViewModel(
            recommendationService ?? new FakeCompanyRecommendationService(),
            matchService ?? new FakeMatchService(),
            session ?? new SessionContext());
    }
    private static UserApplicationResult BuildApplicant(int companyId, MatchStatus matchStatus, int matchId = MatchId, int userId = 1, int jobId = 100, string email = "alice@example.com", string phone = "0700123456")
    {
        return new UserApplicationResult
        {
            User = new User { UserId = userId, Email = email, Phone = phone },
            Match = new Match { MatchId = matchId, UserId = userId, JobId = jobId, Status = matchStatus },
            Job = new Job { JobId = jobId, CompanyId = companyId },
            CompatibilityScore = 55,
            UserSkills = new List<Skill> { new Skill { SkillId = 1, SkillName = "C#", Score = 80 } }
        };
    }
    private sealed class FakeCompanyRecommendationService : ICompanyRecommendationService
    {
        public List<UserApplicationResult> Queue { get; set; } = [];
        public UserApplicationResult? NextApplicant { get; set; }
        public CompatibilityBreakdown? Breakdown { get; set; }
        private int currentIndex;
        public bool HasMore => Queue.Count > 0 && currentIndex < Queue.Count;
        public CompatibilityBreakdown? GetBreakdown(UserApplicationResult applicant) => Breakdown;
        public UserApplicationResult? GetNextApplicant() => Queue.Count > 0 ? Queue.ElementAtOrDefault(currentIndex) : NextApplicant;
        public void LoadApplicants(int companyId)
        {
            currentIndex = 0;
        }
        public void MoveToNext()
        {
            currentIndex++;
        }
        public void MoveToPrevious()
        {
            if (currentIndex > 0)
            {
                currentIndex--;
            }
        }
    }
    private sealed class FakeMatchService : IMatchService
    {
        public Dictionary<int, Match> ById { get; set; } = new();
        public List<int> AdvancedMatches { get; } = [];
        public List<int> RejectedMatches { get; } = [];
        public List<int> RevertedMatches { get; } = [];
        public Task AcceptAsync(int matchId, string feedback) => Task.CompletedTask;
        public void Advance(int matchId) => AdvancedMatches.Add(matchId);
        public int CreatePendingApplication(int userId, int jobId) => 1;
        public IReadOnlyList<Match> GetAllMatches() => ById.Values.ToList();
        public Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId) => Task.FromResult<IReadOnlyList<Match>>(ById.Values.ToList());
        public Match? GetById(int matchId) => ById.TryGetValue(matchId, out var match) ? match : null;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => ById.Values.FirstOrDefault(match => match.UserId == userId && match.JobId == jobId);
        public bool IsDecisionTransitionAllowed(Match current, MatchStatus next) => true;
        public void Reject(int matchId, string feedback) => RejectedMatches.Add(matchId);
        public Task RejectAsync(int matchId, string feedback) => Task.CompletedTask;
        public void RemoveApplication(int matchId) { }
        public void RevertToApplied(int matchId) => RevertedMatches.Add(matchId);
        public void SubmitDecision(int matchId, MatchStatus decision, string feedback) { }
        public Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback) => Task.CompletedTask;
    }
}
