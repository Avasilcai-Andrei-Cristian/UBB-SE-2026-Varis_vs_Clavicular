using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using matchmaking.Domain.Enums;
using matchmaking.Domain.Session;
using matchmaking.Models;
using matchmaking.Repositories;
using matchmaking.Services;

namespace matchmaking.ViewModels;

public class UserStatusViewModel : ObservableObject
{
    private const int DefaultCurrentUserId = 1;

    private readonly UserStatusService _userStatusService;
    private readonly SkillGapService _skillGapService;
    private readonly JobSkillService _jobSkillService;

    private bool _isLoading;
    private bool _hasError;
    private bool _isEmpty;
    private bool _showCards;
    private bool _hasSkillGapMessage;
    private bool _showSkillData;
    private string _emptyMessage = string.Empty;
    private string _currentFilter = "All";
    private string _skillGapMessage = string.Empty;
    private string _skillGapSummaryText = string.Empty;
    private bool _showGoToRecommendations;


    public ObservableCollection<ApplicationCardModel> AppliedJobs { get; } = new();
    public ObservableCollection<ApplicationCardModel> FilteredJobs { get; } = new();
    public ObservableCollection<UnderscoredSkillModel> UnderscoredSkills    { get; } = new();
    public ObservableCollection<MissingSkillModel>     SkillGapMissingSkills { get; } = new();

    
    public bool IsLoading { get => _isLoading; set => SetProperty(ref _isLoading, value); }
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }
    public bool IsEmpty { get => _isEmpty; set => SetProperty(ref _isEmpty, value); }
    public bool ShowCards { get => _showCards; set => SetProperty(ref _showCards, value); }
    public bool HasSkillGapMessage { get => _hasSkillGapMessage; set => SetProperty(ref _hasSkillGapMessage, value); }
    public bool ShowSkillData { get => _showSkillData; set => SetProperty(ref _showSkillData, value); }
    public bool ShowGoToRecommendations { get => _showGoToRecommendations; set => SetProperty(ref _showGoToRecommendations, value); }
    public string EmptyMessage { get => _emptyMessage; set => SetProperty(ref _emptyMessage, value); }
    public string CurrentFilter { get => _currentFilter; set => SetProperty(ref _currentFilter, value); }
    public string SkillGapMessage { get => _skillGapMessage; set => SetProperty(ref _skillGapMessage, value); }
    public string SkillGapSummaryText { get => _skillGapSummaryText; set => SetProperty(ref _skillGapSummaryText, value); }

    
    public bool HasUnderscoredSkills => UnderscoredSkills.Count > 0;
    public bool HasSidebarMissingSkills => SkillGapMissingSkills.Count > 0;

    public ICommand RefreshCommand { get; }

  
    public UserStatusViewModel()
    {
        var connectionString = App.Configuration.SqlConnectionString;
        var matchRepository = new UserStatusMatchRepository(connectionString);
        var jobRepository = new JobRepository();
        var companyRepository = new CompanyRepository();
        var skillRepository = new SkillRepository();
        var jobSkillRepository = new JobSkillRepository();

        var jobService = new JobService(jobRepository);
        var companyService = new CompanyService(companyRepository);
        var skillService = new SkillService(skillRepository);
        _jobSkillService = new JobSkillService(jobSkillRepository);

        _userStatusService = new UserStatusService(matchRepository, jobService, companyService, skillService, _jobSkillService);
        _skillGapService = new SkillGapService(matchRepository, _jobSkillService, skillService);

        RefreshCommand = new RelayCommand(Refresh);

        UnderscoredSkills.CollectionChanged += OnSidebarCollectionChanged;
        SkillGapMissingSkills.CollectionChanged += OnSidebarCollectionChanged;
    }

    public UserStatusViewModel(
        UserStatusService userStatusService,
        SkillGapService skillGapService,
        JobSkillService jobSkillService,
        SessionContext sessionContext)
    {
        _userStatusService = userStatusService;
        _skillGapService = skillGapService;
        _jobSkillService = jobSkillService;

        RefreshCommand = new RelayCommand(Refresh);

        UnderscoredSkills.CollectionChanged += OnSidebarCollectionChanged;
        SkillGapMissingSkills.CollectionChanged += OnSidebarCollectionChanged;
    }

   
    public async Task LoadMatches()
    {
        IsLoading  = true;
        HasError   = false;
        IsEmpty    = false;
        ShowCards  = false;

        try
        {
            var userId = App.Session.CurrentUserId ?? DefaultCurrentUserId;

            var applications = await Task.Run(() => _userStatusService.GetApplicationsForUser(userId));
            var summary = await Task.Run(() => _skillGapService.GetSummary(userId));
            var missingSkills = await Task.Run(() => _skillGapService.GetMissingSkills(userId));
            var underscoredSkills = await Task.Run(() => _skillGapService.GetUnderscoredSkills(userId));

            AppliedJobs.Clear();
            foreach (var application in applications)
            {
                AppliedJobs.Add(application);
            }

            ApplyFilter(CurrentFilter);

         
            UnderscoredSkills.Clear();
            SkillGapMissingSkills.Clear();

            if (!summary.HasRejections)
            {
                SkillGapMessage = "No rejections yet keep applying to see your skill insights.";
                HasSkillGapMessage = true;
                ShowSkillData = false;
            }
            else if (!summary.HasSkillGaps)
            {
                SkillGapMessage = "Great news - your skills meet the requirements of all jobs you've applied to.";
                HasSkillGapMessage = true;
                ShowSkillData = false;
            }
            else
            {
                SkillGapSummaryText = $"{summary.MissingSkillsCount} missing skills · {summary.SkillsToImproveCount} skills to improve";
                HasSkillGapMessage = false;
                ShowSkillData = true;

                foreach (var skill in underscoredSkills)
                {
                    UnderscoredSkills.Add(skill);
                }

                foreach (var skill in missingSkills)
                {
                    SkillGapMissingSkills.Add(skill);
                }
            }
        }
        catch
        {
            HasError = true;
            ShowCards = false;
        }
        finally
        {
            IsLoading = false;
        }
    }

    
    public void Refresh()
    {
        AppliedJobs.Clear();
        FilteredJobs.Clear();
        UnderscoredSkills.Clear();
        SkillGapMissingSkills.Clear();
        HasSkillGapMessage = false;
        ShowSkillData = false;
        _ = LoadMatches();
    }

    public void ApplyFilter(string filter)
    {
        CurrentFilter = filter;
        FilteredJobs.Clear();

        var filteredApplications = filter switch
        {
            "Applied" => AppliedJobs.Where(a => a.Status == MatchStatus.Applied),
            "Accepted" => AppliedJobs.Where(a => a.Status == MatchStatus.Accepted),
            "Rejected" => AppliedJobs.Where(a => a.Status == MatchStatus.Rejected),
            _ => AppliedJobs.AsEnumerable()
        };

        foreach (var application in filteredApplications)
        {
            FilteredJobs.Add(application);
        }

        if (FilteredJobs.Count == 0)
        {
            IsEmpty = true;
            ShowCards = false;
            EmptyMessage = AppliedJobs.Count == 0
                ? "You haven't applied to any jobs yet. Head to the Recommendations page to get started."
                : "No applications match this filter.";
            ShowGoToRecommendations = AppliedJobs.Count == 0;
        }
        else
        {
            IsEmpty = false;
            ShowCards = true;
            ShowGoToRecommendations = false;
        }
    }

    public System.Collections.Generic.IReadOnlyList<Domain.Entities.JobSkill> GetJobSkills(int jobId)
    {
        return _jobSkillService.GetByJobId(jobId);
    }

    private void OnSidebarCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(HasUnderscoredSkills));
        OnPropertyChanged(nameof(HasSidebarMissingSkills));
    }
}
