using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using matchmaking.Domain.Entities;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.DTOs;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class CompanyStatusViewModel : ObservableObject
{
    private readonly CompanyStatusService _companyStatusService;
    private readonly MatchService _matchService;
    private readonly ITestingModuleAdapter _testingModuleAdapter;
    private readonly SessionContext _session;

    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _submitDecisionCommand;
    private readonly RelayCommand _cancelEvaluationCommand;

    private UserApplicationResult? _selectedApplicant;
    private Match? _selectedMatch;
    private MatchStatus? _selectedDecision;
    private string _feedbackMessage = string.Empty;
    private bool _isLoading;
    private string _validationErrorDecision = string.Empty;
    private string _validationErrorFeedback = string.Empty;
    private bool _hasValidationErrors;
    private TestResult? _lastTestResult;
    private string _pageMessage = string.Empty;

    public event Action<string>? ErrorOccurred;

    public CompanyStatusViewModel(
        CompanyStatusService companyStatusService,
        MatchService matchService,
        ITestingModuleAdapter testingModuleAdapter,
        SessionContext session)
    {
        _companyStatusService = companyStatusService;
        _matchService = matchService;
        _testingModuleAdapter = testingModuleAdapter;
        _session = session;

        _refreshCommand = new RelayCommand(async () => await RefreshAsync(), () => !IsLoading);
        _submitDecisionCommand = new RelayCommand(async () => await SubmitDecisionAsync(), CanSubmitDecision);
        _cancelEvaluationCommand = new RelayCommand(CancelEvaluation, () => SelectedApplicant is not null);
    }

    public ObservableCollection<UserApplicationResult> Applications { get; } = [];
    public ObservableCollection<MatchStatus> DecisionOptions { get; } = [MatchStatus.Accepted, MatchStatus.Rejected];

    public UserApplicationResult? SelectedApplicant
    {
        get => _selectedApplicant;
        set
        {
            if (SetProperty(ref _selectedApplicant, value))
            {
                if (value is null)
                {
                    SelectedMatch = null;
                    SelectedDecision = null;
                    FeedbackMessage = string.Empty;
                    LastTestResult = null;
                }

                RaiseCommandStates();
            }
        }
    }

    public Match? SelectedMatch
    {
        get => _selectedMatch;
        private set => SetProperty(ref _selectedMatch, value);
    }

    public MatchStatus? SelectedDecision
    {
        get => _selectedDecision;
        set
        {
            if (SetProperty(ref _selectedDecision, value))
            {
                ValidateDecision();
                RaiseCommandStates();
            }
        }
    }

    public string FeedbackMessage
    {
        get => _feedbackMessage;
        set
        {
            if (SetProperty(ref _feedbackMessage, value))
            {
                ValidateFeedback();
                RaiseCommandStates();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public string ValidationErrorDecision
    {
        get => _validationErrorDecision;
        private set => SetProperty(ref _validationErrorDecision, value);
    }

    public string ValidationErrorFeedback
    {
        get => _validationErrorFeedback;
        private set => SetProperty(ref _validationErrorFeedback, value);
    }

    public bool HasValidationErrors
    {
        get => _hasValidationErrors;
        private set => SetProperty(ref _hasValidationErrors, value);
    }

    public TestResult? LastTestResult
    {
        get => _lastTestResult;
        private set => SetProperty(ref _lastTestResult, value);
    }

    public string PageMessage
    {
        get => _pageMessage;
        private set => SetProperty(ref _pageMessage, value);
    }

    public ICommand RefreshCommand => _refreshCommand;
    public ICommand SubmitDecisionCommand => _submitDecisionCommand;
    public ICommand CancelEvaluationCommand => _cancelEvaluationCommand;

    public async Task LoadApplicationsAsync()
    {
        if (_session.CurrentMode != AppMode.CompanyMode || _session.CurrentCompanyId is null)
        {
            Applications.Clear();
            CancelEvaluation();
            ReportError("Company mode is not active.");
            return;
        }

        IsLoading = true;
        PageMessage = string.Empty;

        try
        {
            var results = await _companyStatusService.GetApplicantsForCompanyAsync(_session.CurrentCompanyId.Value);

            Applications.Clear();
            foreach (var result in results)
            {
                Applications.Add(result);
            }

            // Requirement workflow starts from applicant list and enters evaluation only via Review action.
            CancelEvaluation();

            if (Applications.Count == 0)
            {
                PageMessage = "No applicants found for this company yet.";
            }
            else
            {
                var pendingCount = Applications.Count(result => result.Match.Status == MatchStatus.Applied);
                if (pendingCount == 0)
                {
                    PageMessage = "No applicants are currently pending review. All are already accepted or rejected.";
                }
                else
                {
                    PageMessage = $"{pendingCount} applicant(s) pending review.";
                }
            }
        }
        catch (Exception ex)
        {
            Applications.Clear();
            CancelEvaluation();
            ReportError($"Could not load applicants: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }

        RaiseCommandStates();
    }

    public async Task<bool> LoadEvaluationAsync(int matchId)
    {
        if (_session.CurrentCompanyId is null)
        {
            ReportError("Company context is not available.");
            return false;
        }

        try
        {
            var result = await _companyStatusService.GetApplicantByMatchIdAsync(_session.CurrentCompanyId.Value, matchId);
            if (result is null)
            {
                ReportError("Selected applicant could not be loaded.");
                return false;
            }

            SelectedApplicant = result;

            SelectedMatch = result.Match;

            if (result.Match.Status == MatchStatus.Applied)
            {
                SelectedDecision = null;
            }
            else
            {
                SelectedDecision = result.Match.Status;
            }

            FeedbackMessage = result.Match.FeedbackMessage;

            ValidateAll();

            LastTestResult = await LoadLatestTestResultAsync(result);
            PageMessage = string.Empty;

            RaiseCommandStates();
            return true;
        }
        catch (Exception ex)
        {
            ReportError($"Could not load applicant details: {ex.Message}");
            return false;
        }
    }

    public bool ValidateDecision()
    {
        if (SelectedMatch is null)
        {
            ValidationErrorDecision = "Select an applicant first.";
            return false;
        }

        if (SelectedDecision is null || SelectedDecision == MatchStatus.Applied)
        {
            ValidationErrorDecision = "Select a valid decision (Accepted or Rejected).";
            return false;
        }

        ValidationErrorDecision = string.Empty;
        return true;
    }

    public bool ValidateFeedback()
    {
        if (SelectedDecision == MatchStatus.Rejected && string.IsNullOrWhiteSpace(FeedbackMessage))
        {
            ValidationErrorFeedback = "Feedback is required when rejecting an applicant.";
            return false;
        }

        ValidationErrorFeedback = string.Empty;
        return true;
    }

    public bool ValidateAll()
    {
        var decisionValid = ValidateDecision();
        var feedbackValid = ValidateFeedback();
        HasValidationErrors = !(decisionValid && feedbackValid);
        return !HasValidationErrors;
    }

    public async Task<bool> SubmitDecisionAsync()
    {
        if (SelectedMatch is null || SelectedDecision is null)
        {
            ValidateAll();
            return false;
        }

        if (!ValidateAll())
        {
            return false;
        }

        try
        {
            await _matchService.SubmitDecisionAsync(SelectedMatch.MatchId, SelectedDecision.Value, FeedbackMessage);
            PageMessage = "Decision saved successfully.";
            await LoadApplicationsAsync();
            return true;
        }
        catch (Exception ex)
        {
            ReportError($"Could not submit decision: {ex.Message}");
            return false;
        }
    }

    public void CancelEvaluation()
    {
        SetProperty(ref _selectedApplicant, null, nameof(SelectedApplicant));
        SelectedMatch = null;
        SelectedDecision = null;
        FeedbackMessage = string.Empty;
        LastTestResult = null;
        ClearValidationErrors();
        RaiseCommandStates();
    }

    public Task RefreshAsync()
    {
        return LoadApplicationsAsync();
    }

    private async Task<TestResult?> LoadLatestTestResultAsync(UserApplicationResult applicant)
    {
        try
        {
            var result = await _testingModuleAdapter
                .GetLatestResultForCandidateAsync(applicant.User.UserId, applicant.Job.JobId);

            if (result is null)
            {
                return null;
            }

            result.MatchId = applicant.Match.MatchId;
            result.UserId = applicant.User.UserId;
            result.JobId = applicant.Job.JobId;
            result.FeedbackMessage = applicant.Match.FeedbackMessage;
            result.Decision = applicant.Match.Status;
            return result;
        }
        catch
        {
            return new TestResult
            {
                MatchId = applicant.Match.MatchId,
                UserId = applicant.User.UserId,
                JobId = applicant.Job.JobId,
                ExternalUserId = applicant.User.UserId,
                PositionId = applicant.Job.JobId,
                Decision = applicant.Match.Status,
                FeedbackMessage = applicant.Match.FeedbackMessage,
                IsValid = false,
                ValidationErrors = ["Testing module is currently unavailable."]
            };
        }
    }

    private void ClearValidationErrors()
    {
        ValidationErrorDecision = string.Empty;
        ValidationErrorFeedback = string.Empty;
        HasValidationErrors = false;
    }

    private void RaiseCommandStates()
    {
        _refreshCommand.RaiseCanExecuteChanged();
        _submitDecisionCommand.RaiseCanExecuteChanged();
        _cancelEvaluationCommand.RaiseCanExecuteChanged();
    }

    private bool CanSubmitDecision()
    {
        return !IsLoading && SelectedMatch is not null && SelectedDecision is not null;
    }

    private void ReportError(string message)
    {
        PageMessage = string.Empty;
        ErrorOccurred?.Invoke(message);
    }
}
