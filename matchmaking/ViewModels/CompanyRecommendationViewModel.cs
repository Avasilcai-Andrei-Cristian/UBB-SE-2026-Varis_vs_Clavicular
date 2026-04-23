using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.DTOs;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class CompanyRecommendationViewModel : ObservableObject
{
    private readonly CompanyRecommendationService _recommendationService;
    private readonly MatchService _matchService;
    private readonly SessionContext _session;

    private readonly RelayCommand _advanceCommand;
    private readonly RelayCommand _skipCommand;
    private readonly RelayCommand _undoCommand;
    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _expandCommand;
    private readonly RelayCommand _collapseCommand;

    private UserApplicationResult? _currentApplicant;
    private CompatibilityBreakdown? _scoreBreakdown;
    private bool _hasApplicant;
    private bool _isExpanded;
    private bool _isContactRevealed;
    private bool _canUndo;
    private bool _isLoading;
    private string _statusMessage = string.Empty;

    private UndoEntry? _lastUndoEntry;
    private bool _undoUsed;

    public event Action<string>? ErrorOccurred;

    public CompanyRecommendationViewModel(
        CompanyRecommendationService recommendationService,
        MatchService matchService,
        SessionContext session)
    {
        _recommendationService = recommendationService;
        _matchService = matchService;
        _session = session;

        _advanceCommand = new RelayCommand(AdvanceApplicant, () => HasApplicant && !IsLoading);
        _skipCommand = new RelayCommand(SkipApplicant, () => HasApplicant && !IsLoading);
        _undoCommand = new RelayCommand(UndoLastAction, () => CanUndo && !IsLoading);
        _refreshCommand = new RelayCommand(LoadApplicants);
        _expandCommand = new RelayCommand(ExpandCard, () => HasApplicant);
        _collapseCommand = new RelayCommand(CollapseCard);
    }

    public UserApplicationResult? CurrentApplicant
    {
        get => _currentApplicant;
        private set
        {
            if (SetProperty(ref _currentApplicant, value))
            {
                HasApplicant = value is not null;
                IsContactRevealed = false;
                IsExpanded = false;
                ScoreBreakdown = null;
            }
        }
    }

    public CompatibilityBreakdown? ScoreBreakdown
    {
        get => _scoreBreakdown;
        private set => SetProperty(ref _scoreBreakdown, value);
    }

    public bool HasApplicant
    {
        get => _hasApplicant;
        private set
        {
            if (SetProperty(ref _hasApplicant, value))
            {
                RaiseCommandStates();
            }
        }
    }

    public bool IsExpanded
    {
        get => _isExpanded;
        private set => SetProperty(ref _isExpanded, value);
    }

    public bool IsContactRevealed
    {
        get => _isContactRevealed;
        private set => SetProperty(ref _isContactRevealed, value);
    }

    public bool CanUndo
    {
        get => _canUndo;
        private set
        {
            if (SetProperty(ref _canUndo, value))
            {
                _undoCommand.RaiseCanExecuteChanged();
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

    public string StatusMessage
    {
        get => _statusMessage;
        private set => SetProperty(ref _statusMessage, value);
    }

    public string MaskedEmail
    {
        get
        {
            if (CurrentApplicant is null)
            {
                return string.Empty;
            }

            if (IsContactRevealed)
            {
                return CurrentApplicant.User.Email;
            }

            return MaskEmail(CurrentApplicant.User.Email);
        }
    }

    public string MaskedPhone
    {
        get
        {
            if (CurrentApplicant is null)
            {
                return string.Empty;
            }

            if (IsContactRevealed)
            {
                return CurrentApplicant.User.Phone;
            }

            return MaskPhone(CurrentApplicant.User.Phone);
        }
    }

    public IReadOnlyList<SkillDisplay> TopSkills
    {
        get
        {
            if (CurrentApplicant is null)
            {
                return new List<SkillDisplay>();
            }

            return CurrentApplicant.UserSkills
                .OrderByDescending(s => s.Score)
                .Take(5)
                .Select(s => new SkillDisplay { Name = s.SkillName, Score = s.Score })
                .ToList();
        }
    }

    public int RemainingSkillCount
    {
        get
        {
            if (CurrentApplicant is null)
            {
                return 0;
            }

            return Math.Max(0, CurrentApplicant.UserSkills.Count - 5);
        }
    }

    public IReadOnlyList<SkillDisplay> AllSkills
    {
        get
        {
            if (CurrentApplicant is null)
            {
                return new List<SkillDisplay>();
            }

            return CurrentApplicant.UserSkills
                .OrderByDescending(s => s.Score)
                .Select(s => new SkillDisplay { Name = s.SkillName, Score = s.Score })
                .ToList();
        }
    }

    public ICommand AdvanceCommand => _advanceCommand;
    public ICommand SkipCommand => _skipCommand;
    public ICommand UndoCommand => _undoCommand;
    public ICommand RefreshCommand => _refreshCommand;
    public ICommand ExpandCommand => _expandCommand;
    public ICommand CollapseCommand => _collapseCommand;

    public void LoadApplicants()
    {
        if (_session.CurrentMode != AppMode.CompanyMode || _session.CurrentCompanyId is null)
        {
            CurrentApplicant = null;
            StatusMessage = "Company mode is not active.";
            return;
        }

        IsLoading = true;
        StatusMessage = string.Empty;

        try
        {
            _recommendationService.LoadApplicants(_session.CurrentCompanyId.Value);
            LoadNextApplicant();
        }
        catch (Exception ex)
        {
            CurrentApplicant = null;
            ReportError($"Could not load applicants: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void LoadNextApplicant()
    {
        var next = _recommendationService.GetNextApplicant();
        CurrentApplicant = next;

        if (next is null)
        {
            StatusMessage = "No more applicants to review.";
        }
        else
        {
            StatusMessage = string.Empty;
        }

        RaiseDerivedPropertyChanges();
    }

    public void AdvanceApplicant()
    {
        if (CurrentApplicant is null)
        {
            return;
        }

        if (!ValidateSession())
        {
            return;
        }

        if (!ValidateApplicantState())
        {
            return;
        }

        try
        {
            _matchService.Advance(CurrentApplicant.Match.MatchId);

            StoreForUndo();
            _recommendationService.MoveToNext();
            LoadNextApplicant();
        }
        catch (Exception ex)
        {
            ReportError($"Could not advance applicant: {ex.Message}");
        }
    }

    public void SkipApplicant()
    {
        if (CurrentApplicant is null)
        {
            return;
        }

        if (!ValidateSession())
        {
            return;
        }

        if (!ValidateApplicantState())
        {
            return;
        }

        try
        {
            _matchService.Reject(CurrentApplicant.Match.MatchId, "Rejected on first pass");

            StoreForUndo();
            _recommendationService.MoveToNext();
            LoadNextApplicant();
        }
        catch (Exception ex)
        {
            ReportError($"Could not skip applicant: {ex.Message}");
        }
    }

    public void UndoLastAction()
    {
        if (_lastUndoEntry is null)
        {
            return;
        }

        try
        {
            _matchService.RevertToApplied(_lastUndoEntry.Applicant.Match.MatchId);
            _recommendationService.MoveToPrevious();

            CurrentApplicant = _lastUndoEntry.Applicant;
            IsContactRevealed = false;
            StatusMessage = string.Empty;

            _lastUndoEntry = null;
            _undoUsed = true;
            CanUndo = false;

            RaiseDerivedPropertyChanges();
        }
        catch (Exception ex)
        {
            ReportError($"Could not undo: {ex.Message}");
        }
    }

    public void ExpandCard()
    {
        if (CurrentApplicant is null)
        {
            return;
        }

        ScoreBreakdown = _recommendationService.GetBreakdown(CurrentApplicant);
        IsExpanded = true;

        RaiseDerivedPropertyChanges();
    }

    public void CollapseCard()
    {
        IsExpanded = false;
    }

    private bool ValidateSession()
    {
        if (_session.CurrentCompanyId is null)
        {
            ReportError("Company context is not available.");
            return false;
        }

        if (CurrentApplicant!.Job.CompanyId != _session.CurrentCompanyId.Value)
        {
            ReportError("This applicant does not belong to your company.");
            return false;
        }

        return true;
    }

    private bool ValidateApplicantState()
    {
        var freshMatch = _matchService.GetById(CurrentApplicant!.Match.MatchId);
        if (freshMatch is null || freshMatch.Status != MatchStatus.Applied)
        {
            ReportError("This applicant has already been reviewed. Loading next applicant.");
            _recommendationService.MoveToNext();
            LoadNextApplicant();
            return false;
        }

        return true;
    }

    private void StoreForUndo()
    {
        if (!_undoUsed)
        {
            _lastUndoEntry = new UndoEntry { Applicant = CurrentApplicant! };
            CanUndo = true;
        }
    }

    private void RaiseDerivedPropertyChanges()
    {
        OnPropertyChanged(nameof(TopSkills));
        OnPropertyChanged(nameof(RemainingSkillCount));
        OnPropertyChanged(nameof(AllSkills));
        OnPropertyChanged(nameof(MaskedEmail));
        OnPropertyChanged(nameof(MaskedPhone));
    }

    private void RaiseCommandStates()
    {
        _advanceCommand.RaiseCanExecuteChanged();
        _skipCommand.RaiseCanExecuteChanged();
        _undoCommand.RaiseCanExecuteChanged();
        _refreshCommand.RaiseCanExecuteChanged();
        _expandCommand.RaiseCanExecuteChanged();
    }

    private void ReportError(string message)
    {
        StatusMessage = string.Empty;
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

public class SkillDisplay
{
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; }
}

public class UndoEntry
{
    public required UserApplicationResult Applicant { get; set; }
}
