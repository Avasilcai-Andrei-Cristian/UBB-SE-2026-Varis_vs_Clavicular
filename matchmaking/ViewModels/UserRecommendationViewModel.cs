using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using matchmaking;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.DTOs;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public sealed class UserRecommendationViewModel : ObservableObject
{
    private readonly UserRecommendationService _service;
    private readonly SessionContext _session;

    public event Action<string>? ErrorOccurred;

    private readonly RelayCommand _refreshCommand;
    private readonly RelayCommand _likeCommand;
    private readonly RelayCommand _dismissCommand;
    private readonly RelayCommand _undoCommand;
    private readonly RelayCommand _openFiltersCommand;
    private readonly RelayCommand _applyFiltersCommand;
    private readonly RelayCommand _resetFiltersCommand;
    private readonly RelayCommand _openDetailCommand;
    private readonly RelayCommand _closeDetailCommand;

    private JobRecommendationResult? _currentJob;
    private bool _isLoading;
    private string _errorMessage = string.Empty;
    private bool _isFilterOpen;
    private bool _isDetailOpen;
    private bool _canUndo;
    private bool _undoConsumedThisSession;
    private UndoSnapshot? _undoSnapshot;

    private readonly UserMatchmakingFilters _appliedFilters = UserMatchmakingFilters.Empty();
    private string _draftLocation = string.Empty;

    public UserRecommendationViewModel(UserRecommendationService service, SessionContext session)
    {
        _service = service;
        _session = session;

        foreach (var label in EmploymentTypeOptions)
        {
            DraftEmploymentSelections.Add(new FilterCheckItem(label));
        }

        foreach (var label in ExperienceLevelOptions)
        {
            DraftExperienceSelections.Add(new FilterCheckItem(label));
        }

        foreach (var (skillId, name) in new SkillRepository().GetDistinctSkillCatalog())
        {
            DraftSkillSelections.Add(new SkillFilterItem(skillId, name));
        }

        _refreshCommand = new RelayCommand(LoadRecommendations, () => !IsLoading);
        _likeCommand = new RelayCommand(async () => await LikeAsync(), () => CanAct());
        _dismissCommand = new RelayCommand(async () => await DismissAsync(), () => CanAct());
        _undoCommand = new RelayCommand(async () => await UndoAsync(), () => CanUndo && !IsLoading);
        _openFiltersCommand = new RelayCommand(() => IsFilterOpen = true);
        _applyFiltersCommand = new RelayCommand(async () => await ApplyFiltersAsync());
        _resetFiltersCommand = new RelayCommand(ResetDraftFilters);
        _openDetailCommand = new RelayCommand(ExpandCard, () => CurrentJob is not null);
        _closeDetailCommand = new RelayCommand(CollapseCard);
    }

    public static IReadOnlyList<string> EmploymentTypeOptions { get; } =
    [
        "Full-time", "Part-time", "Internship", "Volunteer", "Remote", "Hybrid"
    ];

    public static IReadOnlyList<string> ExperienceLevelOptions { get; } =
    [
        "Internship", "Entry level", "Mid-senior level", "Director", "Executive"
    ];

    public ObservableCollection<FilterCheckItem> DraftEmploymentSelections { get; } = [];
    public ObservableCollection<FilterCheckItem> DraftExperienceSelections { get; } = [];
    public ObservableCollection<SkillFilterItem> DraftSkillSelections { get; } = [];

    public ICommand RefreshCommand => _refreshCommand;
    public ICommand LikeCommand => _likeCommand;
    public ICommand DismissCommand => _dismissCommand;
    public ICommand UndoCommand => _undoCommand;
    public ICommand OpenFiltersCommand => _openFiltersCommand;
    public ICommand ApplyFiltersCommand => _applyFiltersCommand;
    public ICommand ResetFiltersCommand => _resetFiltersCommand;
    public ICommand OpenDetailCommand => _openDetailCommand;
    public ICommand CloseDetailCommand => _closeDetailCommand;

    public JobRecommendationResult? CurrentJob
    {
        get => _currentJob;
        private set
        {
            if (SetProperty(ref _currentJob, value))
            {
                OnPropertyChanged(nameof(HasCard));
                OnPropertyChanged(nameof(ShowEmptyDeck));
                RaiseCommands();
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
                OnPropertyChanged(nameof(ShowEmptyDeck));
                RaiseCommands();
            }
        }
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
                OnPropertyChanged(nameof(ShowEmptyDeck));
            }
        }
    }

    public bool IsFilterOpen
    {
        get => _isFilterOpen;
        set => SetProperty(ref _isFilterOpen, value);
    }

    public bool IsDetailOpen
    {
        get => _isDetailOpen;
        set
        {
            if (SetProperty(ref _isDetailOpen, value))
            {
                _openDetailCommand.RaiseCanExecuteChanged();
            }
        }
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

    public string DraftLocation
    {
        get => _draftLocation;
        set => SetProperty(ref _draftLocation, value);
    }

    public void ExpandCard() => IsDetailOpen = true;

    public void CollapseCard() => IsDetailOpen = false;

    public bool HasCard => CurrentJob is not null;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    public bool ShowEmptyDeck =>
        !IsLoading
        && CurrentJob is null
        && string.IsNullOrEmpty(ErrorMessage)
        && App.IsDatabaseConnectionAvailable
        && _session.CurrentUserId is not null
        && _session.CurrentMode == AppMode.UserMode;

    private bool CanAct() =>
        CurrentJob is not null
        && !IsLoading
        && App.IsDatabaseConnectionAvailable
        && _session.CurrentUserId is not null
        && _session.CurrentMode == AppMode.UserMode;

    public async Task InitializeAsync()
    {
        await LoadDeckAsync(persistShownForTopCard: true);
    }

    public void LoadRecommendations()
    {
        if (!App.IsDatabaseConnectionAvailable)
        {
            ReportError(App.DatabaseConnectionError);
            CurrentJob = null;
            return;
        }

        if (_session.CurrentUserId is null || _session.CurrentMode != AppMode.UserMode)
        {
            ReportError("User session is not available.");
            CurrentJob = null;
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            var userId = _session.CurrentUserId.Value;
            var next = _service.GetNextCard(userId, _appliedFilters)
                ?? _service.RecalculateTopCardIgnoringCooldown(userId, _appliedFilters);
            CurrentJob = next;
            if (next is null)
            {
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ReportError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadDeckAsync(bool persistShownForTopCard)
    {
        if (!App.IsDatabaseConnectionAvailable)
        {
            ReportError(App.DatabaseConnectionError);
            CurrentJob = null;
            return;
        }

        if (_session.CurrentUserId is null || _session.CurrentMode != AppMode.UserMode)
        {
            ReportError("User session is not available.");
            CurrentJob = null;
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await Task.Yield();
            var userId = _session.CurrentUserId.Value;
            var next = persistShownForTopCard
                ? _service.GetNextCard(userId, _appliedFilters)
                    ?? _service.RecalculateTopCardIgnoringCooldown(userId, _appliedFilters)
                : _service.GetNextCard(userId, _appliedFilters)
                    ?? _service.RecalculateTopCardIgnoringCooldown(userId, _appliedFilters);
            CurrentJob = next;
            if (next is null)
            {
                ErrorMessage = string.Empty;
            }
        }
        catch (Exception ex)
        {
            ReportError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LikeAsync()
    {
        var job = CurrentJob;
        if (job is null || _session.CurrentUserId is null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await Task.Yield();
            var userId = _session.CurrentUserId.Value;
            if (_session.CurrentMode != AppMode.UserMode)
            {
                ReportError("Invalid session for this action.");
                return;
            }

            var matchId = _service.ApplyLike(userId, job);
            if (!_undoConsumedThisSession)
            {
                _undoSnapshot = new UndoSnapshot
                {
                    Card = job,
                    WasApply = true,
                    MatchId = matchId,
                    RecommendationId = null
                };
                CanUndo = true;
            }
            IsDetailOpen = false;
            await AdvanceAfterActionAsync(userId);
        }
        catch (Exception ex)
        {
            ReportError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task DismissAsync()
    {
        var job = CurrentJob;
        if (job is null || _session.CurrentUserId is null)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await Task.Yield();
            var userId = _session.CurrentUserId.Value;
            if (_session.CurrentMode != AppMode.UserMode)
            {
                ReportError("Invalid session for this action.");
                return;
            }

            var recId = _service.ApplyDismiss(userId, job);
            if (!_undoConsumedThisSession)
            {
                _undoSnapshot = new UndoSnapshot
                {
                    Card = job,
                    WasApply = false,
                    MatchId = null,
                    RecommendationId = recId
                };
                CanUndo = true;
            }
            IsDetailOpen = false;
            await AdvanceAfterActionAsync(userId);
        }
        catch (Exception ex)
        {
            ReportError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private Task AdvanceAfterActionAsync(int userId)
    {
        var next = _service.GetNextCard(userId, _appliedFilters);
        CurrentJob = next;
        return Task.CompletedTask;
    }

    public async Task UndoAsync()
    {
        var snap = _undoSnapshot;
        if (snap is null || !CanUndo)
        {
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;
        try
        {
            await Task.Yield();
            if (snap.WasApply && snap.MatchId is { } mid)
            {
                _service.UndoLike(mid, snap.Card.DisplayRecommendationId);
            }
            else if (!snap.WasApply && snap.RecommendationId is { } rid)
            {
                _service.UndoDismiss(rid, snap.Card.DisplayRecommendationId);
            }

            CurrentJob = snap.Card;
            _undoSnapshot = null;
            _undoConsumedThisSession = true;
            CanUndo = false;
        }
        catch (Exception ex)
        {
            ReportError(ex.Message);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task ApplyFiltersAsync()
    {
        _appliedFilters.EmploymentTypes.Clear();
        foreach (var item in DraftEmploymentSelections.Where(i => i.IsChecked))
        {
            _appliedFilters.EmploymentTypes.Add(item.Label);
        }

        _appliedFilters.ExperienceLevels.Clear();
        foreach (var item in DraftExperienceSelections.Where(i => i.IsChecked))
        {
            _appliedFilters.ExperienceLevels.Add(item.Label);
        }

        _appliedFilters.LocationSubstring = DraftLocation.Trim();
        _appliedFilters.SkillIds.Clear();
        foreach (var s in DraftSkillSelections.Where(s => s.IsChecked))
        {
            _appliedFilters.SkillIds.Add(s.SkillId);
        }

        IsFilterOpen = false;
        await LoadDeckAsync(persistShownForTopCard: true);
    }

    public void ResetDraftFilters()
    {
        foreach (var item in DraftEmploymentSelections)
        {
            item.IsChecked = false;
        }

        foreach (var item in DraftExperienceSelections)
        {
            item.IsChecked = false;
        }

        foreach (var item in DraftSkillSelections)
        {
            item.IsChecked = false;
        }

        DraftLocation = string.Empty;
    }

    private void RaiseCommands()
    {
        _refreshCommand.RaiseCanExecuteChanged();
        _likeCommand.RaiseCanExecuteChanged();
        _dismissCommand.RaiseCanExecuteChanged();
        _undoCommand.RaiseCanExecuteChanged();
        _openDetailCommand.RaiseCanExecuteChanged();
    }

    private void ReportError(string message)
    {
        ErrorMessage = message;
        ErrorOccurred?.Invoke(message);
    }

    private sealed class UndoSnapshot
    {
        public required JobRecommendationResult Card { get; init; }
        public bool WasApply { get; init; }
        public int? MatchId { get; init; }
        public int? RecommendationId { get; init; }
    }

}
