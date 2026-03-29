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
    private readonly RelayCommand _unmaskContactCommand;

    private UserApplicationResult? _selectedApplicant;
    private Match? _selectedMatch;
    private MatchStatus? _selectedDecision;
    private string _feedbackMessage = string.Empty;
    private bool _isContactUnmasked;
    private string _maskedEmail = string.Empty;
    private string _maskedPhone = string.Empty;
    private string _unmaskedEmail = string.Empty;
    private string _unmaskedPhone = string.Empty;
    private bool _isLoading;
    private string _validationErrorDecision = string.Empty;
    private string _validationErrorFeedback = string.Empty;
    private bool _hasValidationErrors;
    private TestResult? _lastTestResult;

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
        _unmaskContactCommand = new RelayCommand(UnmaskContactInfo, CanUnmaskContact);
    }

    public ObservableCollection<UserApplicationResult> Applications { get; } = [];

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
                    IsContactUnmasked = false;
                    LastTestResult = null;
                    UpdateContactMasks();
                }
                else
                {
                    _ = LoadEvaluationAsync(value.Match.MatchId);
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

    public bool IsContactUnmasked
    {
        get => _isContactUnmasked;
        private set
        {
            if (SetProperty(ref _isContactUnmasked, value))
            {
                UpdateContactMasks();
                RaiseCommandStates();
            }
        }
    }

    public string MaskedEmail
    {
        get => _maskedEmail;
        private set => SetProperty(ref _maskedEmail, value);
    }

    public string MaskedPhone
    {
        get => _maskedPhone;
        private set => SetProperty(ref _maskedPhone, value);
    }

    public string UnmaskedEmail
    {
        get => _unmaskedEmail;
        private set => SetProperty(ref _unmaskedEmail, value);
    }

    public string UnmaskedPhone
    {
        get => _unmaskedPhone;
        private set => SetProperty(ref _unmaskedPhone, value);
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

    public ICommand RefreshCommand => _refreshCommand;
    public ICommand SubmitDecisionCommand => _submitDecisionCommand;
    public ICommand CancelEvaluationCommand => _cancelEvaluationCommand;
    public ICommand UnmaskContactCommand => _unmaskContactCommand;

    public async Task LoadApplicationsAsync()
    {
        if (_session.CurrentMode != AppMode.CompanyMode || _session.CurrentCompanyId is null)
        {
            Applications.Clear();
            CancelEvaluation();
            return;
        }

        IsLoading = true;

        try
        {
            var results = await _companyStatusService.GetApplicantsForCompanyAsync(_session.CurrentCompanyId.Value);
            var selectedMatchId = SelectedMatch?.MatchId;

            Applications.Clear();
            foreach (var result in results)
            {
                Applications.Add(result);
            }

            if (selectedMatchId is int matchId && Applications.Any(item => item.Match.MatchId == matchId))
            {
                await LoadEvaluationAsync(matchId);
            }
            else if (Applications.Count > 0)
            {
                await LoadEvaluationAsync(Applications[0].Match.MatchId);
            }
            else
            {
                CancelEvaluation();
            }
        }
        finally
        {
            IsLoading = false;
        }

        RaiseCommandStates();
    }

    public async Task LoadEvaluationAsync(int matchId)
    {
        if (_session.CurrentCompanyId is null)
        {
            return;
        }

        var result = await _companyStatusService.GetApplicantByMatchIdAsync(_session.CurrentCompanyId.Value, matchId);
        if (result is null)
        {
            return;
        }

        _selectedApplicant = result;
        _selectedMatch = result.Match;

        if (result.Match.Status == MatchStatus.Applied)
        {
            _selectedDecision = null;
        }
        else
        {
            _selectedDecision = result.Match.Status;
        }

        _feedbackMessage = result.Match.FeedbackMessage;
        _isContactUnmasked = false;

        ValidateAll();
        UpdateContactMasks();

        LastTestResult = await LoadLatestTestResultAsync(result);

        RaiseCommandStates();
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

    public async Task SubmitDecisionAsync()
    {
        if (SelectedMatch is null || SelectedDecision is null)
        {
            ValidateAll();
            return;
        }

        if (!ValidateAll())
        {
            return;
        }

        await _matchService.SubmitDecisionAsync(SelectedMatch.MatchId, SelectedDecision.Value, FeedbackMessage);
        await LoadApplicationsAsync();
    }

    public void UnmaskContactInfo()
    {
        if (SelectedApplicant is null)
        {
            return;
        }

        IsContactUnmasked = true;
    }

    public void CancelEvaluation()
    {
        _selectedApplicant = null;
        _selectedMatch = null;
        _selectedDecision = null;
        _feedbackMessage = string.Empty;
        _isContactUnmasked = false;
        LastTestResult = null;

        UpdateContactMasks();
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

    private void UpdateContactMasks()
    {
        var email = SelectedApplicant?.User.Email ?? string.Empty;
        var phone = SelectedApplicant?.User.Phone ?? string.Empty;

        if (IsContactUnmasked)
        {
            UnmaskedEmail = email;
            UnmaskedPhone = phone;
        }
        else
        {
            UnmaskedEmail = string.Empty;
            UnmaskedPhone = string.Empty;
        }

        MaskedEmail = MaskEmail(email);
        MaskedPhone = MaskPhone(phone);
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
        _unmaskContactCommand.RaiseCanExecuteChanged();
    }

    private bool CanSubmitDecision()
    {
        return !IsLoading && SelectedMatch is not null && SelectedDecision is not null;
    }

    private bool CanUnmaskContact()
    {
        return !IsLoading && SelectedApplicant is not null && !IsContactUnmasked;
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return string.Empty;
        }

        var atIndex = email.IndexOf('@');
        if (atIndex <= 1)
        {
            return "***" + email[atIndex..];
        }

        return email[0] + new string('*', atIndex - 1) + email[atIndex..];
    }

    private static string MaskPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return string.Empty;
        }

        var trimmed = phone.Trim();
        if (trimmed.Length <= 4)
        {
            return new string('*', trimmed.Length);
        }

        var prefix = trimmed[..2];
        var suffix = trimmed[^2..];
        return prefix + new string('*', trimmed.Length - 4) + suffix;
    }
}
