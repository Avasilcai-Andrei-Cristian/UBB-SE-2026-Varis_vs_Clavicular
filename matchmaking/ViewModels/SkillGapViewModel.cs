using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using matchmaking.Domain.Session;
using matchmaking.Models;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class SkillGapViewModel : ObservableObject
{
    private readonly SkillGapService skillGapService;

    private bool isLoading;
    private bool showContent;
    private bool hasSkillData;
    private bool hasSummaryMessage;
    private string summaryMessage = string.Empty;
    private int missingCount;
    private int improveCount;

    public ObservableCollection<UnderscoredSkillModel> SkillsToImprove { get; } = new ObservableCollection<UnderscoredSkillModel>();
    public ObservableCollection<MissingSkillModel> MissingSkills { get; } = new ObservableCollection<MissingSkillModel>();

    public bool IsLoading { get => isLoading; set => SetProperty(ref isLoading, value); }
    public bool ShowContent { get => showContent; set => SetProperty(ref showContent, value); }
    public bool HasSkillData { get => hasSkillData; set => SetProperty(ref hasSkillData, value); }
    public bool HasSummaryMessage { get => hasSummaryMessage; set => SetProperty(ref hasSummaryMessage, value); }
    public string SummaryMessage { get => summaryMessage; set => SetProperty(ref summaryMessage, value); }
    public int MissingCount { get => missingCount; set => SetProperty(ref missingCount, value); }
    public int ImproveCount { get => improveCount; set => SetProperty(ref improveCount, value); }

    public bool HasSkillsToImprove => SkillsToImprove.Count > 0;
    public bool HasMissingSkills => MissingSkills.Count > 0;

    public ICommand RefreshCommand { get; }

    public SkillGapViewModel()
    {
        var connectionString = App.Configuration.SqlConnectionString;
        var matchRepo = new UserStatusMatchRepository(connectionString);
        var skillRepo = new SkillRepository();
        var jobSkillRepo = new JobSkillRepository();
        var skillService = new SkillService(skillRepo);
        var jobSkillService = new JobSkillService(jobSkillRepo);

        skillGapService = new SkillGapService(matchRepo, jobSkillService, skillService);
        RefreshCommand = new RelayCommand(Refresh);

        SkillsToImprove.CollectionChanged += OnCollectionChanged;
        MissingSkills.CollectionChanged   += OnCollectionChanged;
    }

    public SkillGapViewModel(SkillGapService skillGapService, SessionContext sessionContext)
    {
        this.skillGapService = skillGapService;

        RefreshCommand = new RelayCommand(Refresh);

        SkillsToImprove.CollectionChanged += OnCollectionChanged;
        MissingSkills.CollectionChanged += OnCollectionChanged;
    }

    public async Task LoadData()
    {
        IsLoading = true;
        ShowContent = false;
        HasSkillData = false;
        HasSummaryMessage = false;

        try
        {
            var userId = App.Session.CurrentUserId ?? 1;
            var summary = await Task.Run(() => skillGapService.GetSummary(userId));
            var missing = await Task.Run(() => skillGapService.GetMissingSkills(userId));
            var underscored = await Task.Run(() => skillGapService.GetUnderscoredSkills(userId));

            SkillsToImprove.Clear();
            MissingSkills.Clear();

            if (!summary.HasRejections)
            {
                SummaryMessage = "No rejections yet keep applying to see your skill insights.";
                HasSummaryMessage = true;
                HasSkillData = false;
            }
            else if (!summary.HasSkillGaps)
            {
                SummaryMessage = "Great news - your skills meet the requirements of all jobs you've applied to.";
                HasSummaryMessage = true;
                HasSkillData = false;
            }
            else
            {
                MissingCount = summary.MissingSkillsCount;
                ImproveCount = summary.SkillsToImproveCount;
                HasSummaryMessage = false;
                HasSkillData = true;

                foreach (var skill in underscored)
                {
                    SkillsToImprove.Add(skill);
                }

                foreach (var skill in missing)
                {
                    MissingSkills.Add(skill);
                }
            }
        }
        catch
        {
            SummaryMessage = "Unable to load skill gap data. Please try again.";
            HasSummaryMessage = true;
            HasSkillData = false;
        }
        finally
        {
            IsLoading = false;
            ShowContent = true;
        }
    }

    public void Refresh()
    {
        SkillsToImprove.Clear();
        MissingSkills.Clear();
        HasSkillData = false;
        HasSummaryMessage = false;
        ShowContent = false;
        _ = LoadData();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasSkillsToImprove));
        OnPropertyChanged(nameof(HasMissingSkills));
    }
}
