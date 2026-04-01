using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using matchmaking.Models;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class SkillGapViewModel : ObservableObject
{
    private readonly SkillGapService _skillGapService;

    private bool   _isLoading;
    private bool   _showContent;
    private bool   _hasSkillData;
    private bool   _hasSummaryMessage;
    private string _summaryMessage  = string.Empty;
    private int    _missingCount;
    private int    _improveCount;

    // ── Collections ────────────────────────────────────────────────────────────
    public ObservableCollection<UnderscoredSkillModel> SkillsToImprove { get; } = new();
    public ObservableCollection<MissingSkillModel>     MissingSkills   { get; } = new();

    // ── State properties ───────────────────────────────────────────────────────
    public bool   IsLoading         { get => _isLoading;         set => SetProperty(ref _isLoading, value); }
    public bool   ShowContent       { get => _showContent;       set => SetProperty(ref _showContent, value); }
    public bool   HasSkillData      { get => _hasSkillData;      set => SetProperty(ref _hasSkillData, value); }
    public bool   HasSummaryMessage { get => _hasSummaryMessage; set => SetProperty(ref _hasSummaryMessage, value); }
    public string SummaryMessage    { get => _summaryMessage;    set => SetProperty(ref _summaryMessage, value); }
    public int    MissingCount      { get => _missingCount;      set => SetProperty(ref _missingCount, value); }
    public int    ImproveCount      { get => _improveCount;      set => SetProperty(ref _improveCount, value); }

    // Section header visibility (derived from collection size)
    public bool HasSkillsToImprove => SkillsToImprove.Count > 0;
    public bool HasMissingSkills   => MissingSkills.Count > 0;

    // ── Commands ───────────────────────────────────────────────────────────────
    public ICommand RefreshCommand { get; }

    // ── Constructor ────────────────────────────────────────────────────────────
    public SkillGapViewModel()
    {
        var connectionString = App.Configuration.SqlConnectionString;
        var matchRepo        = new UserStatusMatchRepository(connectionString);
        var skillRepo        = new SkillRepository();
        var jobSkillRepo     = new JobSkillRepository();
        var skillService     = new SkillService(skillRepo);
        var jobSkillService  = new JobSkillService(jobSkillRepo);

        _skillGapService = new SkillGapService(matchRepo, jobSkillService, skillService);
        RefreshCommand   = new RelayCommand(Refresh);

        SkillsToImprove.CollectionChanged += OnCollectionChanged;
        MissingSkills.CollectionChanged   += OnCollectionChanged;
    }

    // ── Public API ─────────────────────────────────────────────────────────────

    /// <summary>Task 8 – Load all skill gap data for the current user.</summary>
    public async Task LoadData()
    {
        IsLoading         = true;
        ShowContent       = false;
        HasSkillData      = false;
        HasSummaryMessage = false;

        try
        {
            var userId      = App.Session.CurrentUserId ?? 1;
            var summary     = await Task.Run(() => _skillGapService.GetSummary(userId));
            var missing     = await Task.Run(() => _skillGapService.GetMissingSkills(userId));
            var underscored = await Task.Run(() => _skillGapService.GetUnderscoredSkills(userId));

            SkillsToImprove.Clear();
            MissingSkills.Clear();

            if (!summary.HasRejections)
            {
                SummaryMessage    = "No rejections yet — keep applying to see your skill insights.";
                HasSummaryMessage = true;
                HasSkillData      = false;
            }
            else if (!summary.HasSkillGaps)
            {
                SummaryMessage    = "Great news — your skills meet the requirements of all jobs you've applied to.";
                HasSummaryMessage = true;
                HasSkillData      = false;
            }
            else
            {
                MissingCount    = summary.MissingSkillsCount;
                ImproveCount    = summary.SkillsToImproveCount;
                HasSummaryMessage = false;
                HasSkillData      = true;

                foreach (var skill in underscored) SkillsToImprove.Add(skill);
                foreach (var skill in missing)     MissingSkills.Add(skill);
            }
        }
        catch
        {
            SummaryMessage    = "Unable to load skill gap data. Please try again.";
            HasSummaryMessage = true;
            HasSkillData      = false;
        }
        finally
        {
            IsLoading   = false;
            ShowContent = true;
        }
    }

    /// <summary>Task 9 – Clear and reload.</summary>
    public void Refresh()
    {
        SkillsToImprove.Clear();
        MissingSkills.Clear();
        HasSkillData      = false;
        HasSummaryMessage = false;
        ShowContent       = false;
        _ = LoadData();
    }

    // ── Private ────────────────────────────────────────────────────────────────

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasSkillsToImprove));
        OnPropertyChanged(nameof(HasMissingSkills));
    }
}
