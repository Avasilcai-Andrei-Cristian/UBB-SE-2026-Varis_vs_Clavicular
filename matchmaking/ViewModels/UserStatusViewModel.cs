using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using matchmaking.Domain.Enums;
using matchmaking.Models;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class UserStatusViewModel : ObservableObject
{
    private readonly UserStatusService    _userStatusService;
    private readonly SkillGapService      _skillGapService;
    private readonly JobSkillService      _jobSkillService;

    private bool   _isLoading;
    private bool   _hasError;
    private bool   _isEmpty;
    private bool   _showCards;
    private bool   _hasSkillGapMessage;
    private bool   _showSkillData;
    private string _emptyMessage         = string.Empty;
    private string _currentFilter        = "All";
    private string _skillGapMessage      = string.Empty;
    private string _skillGapSummaryText  = string.Empty;
    private bool   _showGoToRecommendations;

    // ── Collections ────────────────────────────────────────────────────────────
    public ObservableCollection<ApplicationCardModel>  AppliedJobs          { get; } = new();
    public ObservableCollection<ApplicationCardModel>  FilteredJobs         { get; } = new();
    public ObservableCollection<UnderscoredSkillModel> UnderscoredSkills    { get; } = new();
    public ObservableCollection<MissingSkillModel>     SkillGapMissingSkills { get; } = new();

    // ── State properties ───────────────────────────────────────────────────────
    public bool   IsLoading             { get => _isLoading;            set => SetProperty(ref _isLoading, value); }
    public bool   HasError              { get => _hasError;             set => SetProperty(ref _hasError, value); }
    public bool   IsEmpty               { get => _isEmpty;              set => SetProperty(ref _isEmpty, value); }
    public bool   ShowCards             { get => _showCards;            set => SetProperty(ref _showCards, value); }
    public bool   HasSkillGapMessage    { get => _hasSkillGapMessage;   set => SetProperty(ref _hasSkillGapMessage, value); }
    public bool   ShowSkillData         { get => _showSkillData;        set => SetProperty(ref _showSkillData, value); }
    public bool   ShowGoToRecommendations { get => _showGoToRecommendations; set => SetProperty(ref _showGoToRecommendations, value); }
    public string EmptyMessage          { get => _emptyMessage;         set => SetProperty(ref _emptyMessage, value); }
    public string CurrentFilter         { get => _currentFilter;        set => SetProperty(ref _currentFilter, value); }
    public string SkillGapMessage       { get => _skillGapMessage;      set => SetProperty(ref _skillGapMessage, value); }
    public string SkillGapSummaryText   { get => _skillGapSummaryText;  set => SetProperty(ref _skillGapSummaryText, value); }

    // Sidebar section header visibility
    public bool HasUnderscoredSkills    => UnderscoredSkills.Count > 0;
    public bool HasSidebarMissingSkills => SkillGapMissingSkills.Count > 0;

    // ── Commands ───────────────────────────────────────────────────────────────
    public ICommand RefreshCommand { get; }

    // ── Constructor ────────────────────────────────────────────────────────────
    public UserStatusViewModel()
    {
        var connectionString = App.Configuration.SqlConnectionString;
        var matchRepo        = new UserStatusMatchRepository(connectionString);
        var jobRepo          = new JobRepository();
        var companyRepo      = new CompanyRepository();
        var skillRepo        = new SkillRepository();
        var jobSkillRepo     = new JobSkillRepository();

        var jobService     = new JobService(jobRepo);
        var companyService = new CompanyService(companyRepo);
        var skillService   = new SkillService(skillRepo);
        _jobSkillService   = new JobSkillService(jobSkillRepo);

        _userStatusService = new UserStatusService(matchRepo, jobService, companyService, skillService, _jobSkillService);
        _skillGapService   = new SkillGapService(matchRepo, _jobSkillService, skillService);

        RefreshCommand = new RelayCommand(Refresh);

        UnderscoredSkills.CollectionChanged += OnSidebarCollectionChanged;
        SkillGapMissingSkills.CollectionChanged += OnSidebarCollectionChanged;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Task 2 – Load all matches for the current user.</summary>
    public async Task LoadMatches()
    {
        IsLoading  = true;
        HasError   = false;
        IsEmpty    = false;
        ShowCards  = false;

        try
        {
            var userId = App.Session.CurrentUserId ?? 1;

            var applications = await Task.Run(() => _userStatusService.GetApplicationsForUser(userId));
            var summary      = await Task.Run(() => _skillGapService.GetSummary(userId));
            var missing      = await Task.Run(() => _skillGapService.GetMissingSkills(userId));
            var underscored  = await Task.Run(() => _skillGapService.GetUnderscoredSkills(userId));

            AppliedJobs.Clear();
            foreach (var app in applications)
                AppliedJobs.Add(app);

            ApplyFilter(CurrentFilter);

            // Skill gap sidebar
            UnderscoredSkills.Clear();
            SkillGapMissingSkills.Clear();

            if (!summary.HasRejections)
            {
                SkillGapMessage      = "No rejections yet — keep applying to see your skill insights.";
                HasSkillGapMessage   = true;
                ShowSkillData        = false;
            }
            else if (!summary.HasSkillGaps)
            {
                SkillGapMessage      = "Great news — your skills meet the requirements of all jobs you've applied to.";
                HasSkillGapMessage   = true;
                ShowSkillData        = false;
            }
            else
            {
                SkillGapSummaryText  = $"{summary.MissingSkillsCount} missing skills · {summary.SkillsToImproveCount} skills to improve";
                HasSkillGapMessage   = false;
                ShowSkillData        = true;

                foreach (var skill in underscored)  UnderscoredSkills.Add(skill);
                foreach (var skill in missing)      SkillGapMissingSkills.Add(skill);
            }
        }
        catch
        {
            HasError  = true;
            ShowCards = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>Task 3 – Clear and reload.</summary>
    public void Refresh()
    {
        AppliedJobs.Clear();
        FilteredJobs.Clear();
        UnderscoredSkills.Clear();
        SkillGapMissingSkills.Clear();
        HasSkillGapMessage = false;
        ShowSkillData      = false;
        _ = LoadMatches();
    }

    public void ApplyFilter(string filter)
    {
        CurrentFilter = filter;
        FilteredJobs.Clear();

        var source = filter switch
        {
            "Applied"  => AppliedJobs.Where(a => a.Status == MatchStatus.Applied),
            "Accepted" => AppliedJobs.Where(a => a.Status == MatchStatus.Accepted),
            "Rejected" => AppliedJobs.Where(a => a.Status == MatchStatus.Rejected),
            _          => AppliedJobs.AsEnumerable()
        };

        foreach (var job in source)
            FilteredJobs.Add(job);

        if (FilteredJobs.Count == 0)
        {
            IsEmpty    = true;
            ShowCards  = false;
            EmptyMessage             = AppliedJobs.Count == 0
                ? "You haven't applied to any jobs yet. Head to the Recommendations page to get started."
                : "No applications match this filter.";
            ShowGoToRecommendations  = AppliedJobs.Count == 0;
        }
        else
        {
            IsEmpty    = false;
            ShowCards  = true;
            ShowGoToRecommendations = false;
        }
    }

    /// <summary>Expose job skills for the detail dialog (UI concern stays in code-behind).</summary>
    public System.Collections.Generic.IReadOnlyList<Domain.Entities.JobSkill> GetJobSkills(int jobId)
        => _jobSkillService.GetByJobId(jobId);

    // ── Private ────────────────────────────────────────────────────────────────

    private void OnSidebarCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasUnderscoredSkills));
        OnPropertyChanged(nameof(HasSidebarMissingSkills));
    }
}
