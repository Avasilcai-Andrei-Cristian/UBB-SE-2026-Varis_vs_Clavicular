using System.Collections.Generic;
using System.Threading.Tasks;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.DTOs;
using matchmaking.Services;
namespace matchmaking.Tests;
[Collection("AppState")]
public sealed class CompanyStatusViewModelTests
{
    private const int CompanyId = 20;
    private const int MatchId = 3;
    private const int JobId = 200;
    private const int UserId = 7;
    [Fact]
    public async Task LoadApplicationsAsync_reports_error_when_not_in_company_mode()
    {
        var session = new SessionContext();
        session.LoginAsUser(UserId);
        var vm = CreateViewModel(session: session);
        var errors = new List<string>();
        vm.ErrorOccurred += errors.Add;
        await vm.LoadApplicationsAsync();
        errors.Should().ContainSingle().Which.Should().Be("Company mode is not active.");
        vm.Applications.Should().BeEmpty();
    }
    [Fact]
    public async Task LoadApplicationsAsync_sets_message_when_no_results()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var companyService = new FakeCompanyStatusService();
        var vm = CreateViewModel(session: session, companyStatusService: companyService);
        await vm.LoadApplicationsAsync();
        vm.PageMessage.Should().Be("No applicants found with status Accepted, Rejected, or In Review.");
    }
    [Fact]
    public async Task LoadApplicationsAsync_populates_applications_and_page_message()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var result = BuildApplicant(MatchStatus.Accepted);
        var companyService = new FakeCompanyStatusService { Applicants = [result] };
        var vm = CreateViewModel(session: session, companyStatusService: companyService);
        await vm.LoadApplicationsAsync();
        vm.Applications.Should().ContainSingle();
        vm.PageMessage.Should().Be("1 applicant(s) are Accepted, Rejected, or In Review.");
    }
    [Fact]
    public async Task LoadEvaluationAsync_sets_selected_applicant_and_match_data()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var result = BuildApplicant(MatchStatus.Accepted);
        var companyService = new FakeCompanyStatusService { ApplicantByMatchId = result };
        var vm = CreateViewModel(session: session, companyStatusService: companyService);
        var loaded = await vm.LoadEvaluationAsync(MatchId);
        loaded.Should().BeTrue();
        vm.SelectedApplicant.Should().BeSameAs(result);
        vm.SelectedMatch.Should().BeSameAs(result.Match);
        vm.SelectedDecision.Should().Be(MatchStatus.Accepted);
        vm.FeedbackMessage.Should().Be("Feedback");
    }
    [Fact]
    public void ValidateFeedback_requires_non_empty_value()
    {
        var vm = CreateViewModel();
        vm.FeedbackMessage = " ";
        vm.ValidateFeedback().Should().BeFalse();
        vm.ValidationErrorFeedback.Should().Be("Feedback is required.");
    }
    [Fact]
    public void ValidateDecision_requires_selected_match_and_valid_decision()
    {
        var vm = CreateViewModel();
        vm.SelectedDecision = MatchStatus.Accepted;
        vm.ValidateDecision().Should().BeFalse();
        vm.ValidationErrorDecision.Should().Be("Select an applicant first.");
    }
    [Fact]
    public async Task SubmitDecisionAsync_returns_false_when_validation_fails()
    {
        var vm = CreateViewModel();
        var result = await vm.SubmitDecisionAsync();
        result.Should().BeFalse();
    }
    [Fact]
    public async Task SubmitDecisionAsync_calls_match_service_and_refreshes()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var result = BuildApplicant(MatchStatus.Advanced);
        var companyService = new FakeCompanyStatusService { Applicants = [result], ApplicantByMatchId = result };
        var matchService = new FakeMatchService();
        var vm = CreateViewModel(session: session, companyStatusService: companyService, matchService: matchService);
        await vm.LoadEvaluationAsync(MatchId);
        vm.SelectedDecision = MatchStatus.Accepted;
        vm.FeedbackMessage = " Approved ";
        var submitted = await vm.SubmitDecisionAsync();
        submitted.Should().BeTrue();
        matchService.Submitted.Should().ContainSingle();
        matchService.Submitted[0].Should().Be((MatchId, MatchStatus.Accepted, "Approved"));
        vm.PageMessage.Should().Be("Decision saved successfully.");
    }
    [Fact]
    public void CancelEvaluation_clears_selection_and_validation()
    {
        var vm = CreateViewModel();
        vm.SelectedApplicant = BuildApplicant(MatchStatus.Accepted);
        vm.SelectedDecision = MatchStatus.Accepted;
        vm.FeedbackMessage = "Ok";
        vm.CancelEvaluation();
        vm.SelectedApplicant.Should().BeNull();
        vm.SelectedDecision.Should().BeNull();
        vm.FeedbackMessage.Should().BeEmpty();
        vm.HasValidationErrors.Should().BeFalse();
    }
    [Fact]
    public async Task LoadEvaluationAsync_returns_false_when_applicant_missing()
    {
        var session = new SessionContext();
        session.LoginAsCompany(CompanyId);
        var companyService = new FakeCompanyStatusService();
        var vm = CreateViewModel(session: session, companyStatusService: companyService);
        var errors = new List<string>();
        vm.ErrorOccurred += errors.Add;
        var loaded = await vm.LoadEvaluationAsync(MatchId);
        loaded.Should().BeFalse();
        errors.Should().ContainSingle().Which.Should().Be("Selected applicant could not be loaded.");
    }
    private static CompanyStatusViewModel CreateViewModel(SessionContext? session = null, FakeCompanyStatusService? companyStatusService = null, FakeMatchService? matchService = null, FakeTestingModuleAdapter? testingModuleAdapter = null)
    {
        return new CompanyStatusViewModel(
            companyStatusService ?? new FakeCompanyStatusService(),
            matchService ?? new FakeMatchService(),
            testingModuleAdapter ?? new FakeTestingModuleAdapter(),
            session ?? new SessionContext());
    }
    private static UserApplicationResult BuildApplicant(MatchStatus status)
    {
        return new UserApplicationResult
        {
            User = new User { UserId = UserId, Email = "company@acme.com", Phone = "0700" },
            Match = new Match { MatchId = MatchId, UserId = UserId, JobId = JobId, Status = status, FeedbackMessage = "Feedback" },
            Job = new Job { JobId = JobId, CompanyId = CompanyId },
            CompatibilityScore = 88,
            UserSkills = []
        };
    }
    private sealed class FakeCompanyStatusService : ICompanyStatusService
    {
        public List<UserApplicationResult> Applicants { get; set; } = [];
        public UserApplicationResult? ApplicantByMatchId { get; set; }
        public Task<UserApplicationResult?> GetApplicantByMatchIdAsync(int companyId, int matchId) => Task.FromResult(ApplicantByMatchId);
        public Task<IReadOnlyList<UserApplicationResult>> GetApplicantsForCompanyAsync(int companyId) => Task.FromResult<IReadOnlyList<UserApplicationResult>>(Applicants);
    }
    private sealed class FakeMatchService : IMatchService
    {
        public List<(int MatchId, MatchStatus Decision, string Feedback)> Submitted { get; } = [];
        public Task AcceptAsync(int matchId, string feedback) => Task.CompletedTask;
        public void Advance(int matchId) { }
        public int CreatePendingApplication(int userId, int jobId) => 1;
        public IReadOnlyList<Match> GetAllMatches() => [];
        public Task<IReadOnlyList<Match>> GetByCompanyIdAsync(int companyId) => Task.FromResult<IReadOnlyList<Match>>([]);
        public Match? GetById(int matchId) => null;
        public Match? GetByUserIdAndJobId(int userId, int jobId) => null;
        public bool IsDecisionTransitionAllowed(Match current, MatchStatus next) => true;
        public void Reject(int matchId, string feedback) { }
        public Task RejectAsync(int matchId, string feedback) => Task.CompletedTask;
        public void RemoveApplication(int matchId) { }
        public void RevertToApplied(int matchId) { }
        public void SubmitDecision(int matchId, MatchStatus decision, string feedback) => Submitted.Add((matchId, decision, feedback));
        public Task SubmitDecisionAsync(int matchId, MatchStatus decision, string feedback)
        {
            SubmitDecision(matchId, decision, feedback);
            return Task.CompletedTask;
        }
    }
    private sealed class FakeTestingModuleAdapter : ITestingModuleAdapter
    {
        public Task<TestResult?> GetResultForMatchAsync(int matchId) => Task.FromResult<TestResult?>(null);
        public Task<TestResult?> GetLatestResultForCandidateAsync(int externalUserId, int positionId) => Task.FromResult<TestResult?>(null);
        public Task<IReadOnlyList<TestResult>> GetResultHistoryForCandidateAsync(int externalUserId, int positionId) => Task.FromResult<IReadOnlyList<TestResult>>([]);
    }
}
