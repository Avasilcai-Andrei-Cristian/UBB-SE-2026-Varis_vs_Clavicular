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
    private const int MaximumFeedbackLength = 500;

    private readonly CompanyStatusService _companyStatusService;
    private readonly MatchService _matchService;
    private readonly ITestingModuleAdapter _testingModuleAdapter;
    private readonly SessionContext _session;

    private readonly RelayCommand _refreshCommand;

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

                RaiseContactVisibilityProperties();

                RaiseCommandStates();
            }
        }
    }

    public Match? SelectedMatch
    {
        get => _selectedMatch;
        private set
        {
            if (SetProperty(ref _selectedMatch, value))
            {
                RaiseContactVisibilityProperties();
            }
        }
    }

    public string ContactEmailDisplay
    {
        get
        {
            if (SelectedApplicant is null)
            {
                return string.Empty;
            }

            return CanRevealContact
                ? SelectedApplicant.User.Email
                : MaskEmail(SelectedApplicant.User.Email);
        }
    }

    public string ContactPhoneDisplay
    {
        get
        {
            if (SelectedApplicant is null)
            {
                return string.Empty;
            }

            return CanRevealContact
                ? SelectedApplicant.User.Phone
                : MaskPhone(SelectedApplicant.User.Phone);
        }
    }

    private bool CanRevealContact => SelectedMatch?.Status == MatchStatus.Accepted;

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

            
            CancelEvaluation();

            if (Applications.Count == 0)
            {
                PageMessage = "No applicants found with status Accepted, Rejected, or In Review.";
            }
            else
            {
                PageMessage = $"{Applications.Count} applicant(s) are Accepted, Rejected, or In Review.";
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

            if (result.Match.Status is MatchStatus.Applied or MatchStatus.Advanced)
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
        if (string.IsNullOrWhiteSpace(FeedbackMessage))
        {
            ValidationErrorFeedback = "Feedback is required.";
            return false;
        }

        if (FeedbackMessage.Trim().Length > MaximumFeedbackLength)
        {
            ValidationErrorFeedback = $"Feedback must be {MaximumFeedbackLength} characters or fewer.";
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
            await _matchService.SubmitDecisionAsync(SelectedMatch.MatchId, SelectedDecision.Value, FeedbackMessage.Trim());
            PageMessage = "Decision saved successfully.";
            await LoadApplicationsAsync();
            return true;
        }
        catch (Exception)
        {
            PageMessage = string.Empty;
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
    }

    private void RaiseContactVisibilityProperties()
    {
        OnPropertyChanged(nameof(ContactEmailDisplay));
        OnPropertyChanged(nameof(ContactPhoneDisplay));
    }

    private void ReportError(string message)
    {
        PageMessage = string.Empty;
        ErrorOccurred?.Invoke(message);
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return string.Empty;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return "***@***";
        }

        return email[0] + new string('*', atIndex - 1) + email[atIndex..];
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone) || phone.Length < 4)
        {
            return "***";
        }

        return phone[..2] + new string('*', phone.Length - 5) + phone[^3..];
    }
}
