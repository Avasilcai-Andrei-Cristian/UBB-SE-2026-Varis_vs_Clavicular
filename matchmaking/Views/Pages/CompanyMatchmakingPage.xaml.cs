using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.algorithm;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;

namespace matchmaking.Views.Pages;

public sealed partial class CompanyMatchmakingPage : Page
{
    private readonly CompanyRecommendationViewModel _viewModel;

    public CompanyMatchmakingPage()
    {
        InitializeComponent();

        var connectionString = App.Configuration.SqlConnectionString;
        var jobService = new JobService(new JobRepository());
        var skillService = new SkillService(new SkillRepository());
        var jobSkillService = new JobSkillService(new JobSkillRepository());
        var matchService = new MatchService(
            new SqlMatchRepository(connectionString),
            jobService);

        var algorithm = new RecommendationAlgorithm(
            new SqlPostRepository(connectionString),
            new SqlInteractionRepository(connectionString));

        var recommendationService = new CompanyRecommendationService(
            matchService,
            new UserService(new UserRepository()),
            jobService,
            skillService,
            jobSkillService,
            algorithm);

        _viewModel = new CompanyRecommendationViewModel(
            recommendationService,
            matchService,
            App.Session);

        _viewModel.ErrorOccurred += OnViewModelErrorOccurred;

        DataContext = _viewModel;
        Loaded += OnLoadedAsync;
    }

    private void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadApplicants();
        UpdateView();
    }

    private void OnAdvanceClick(object sender, RoutedEventArgs e)
    {
        _viewModel.AdvanceApplicant();
        UpdateView();
    }

    private void OnSkipClick(object sender, RoutedEventArgs e)
    {
        _viewModel.SkipApplicant();
        UpdateView();
    }

    private void OnUndoClick(object sender, RoutedEventArgs e)
    {
        _viewModel.UndoLastAction();
        UpdateView();
    }

    private void OnExpandClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ExpandCard();
        UpdateView();
    }

    private void OnCollapseClick(object sender, RoutedEventArgs e)
    {
        _viewModel.CollapseCard();
        UpdateView();
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadApplicants();
        UpdateView();
    }

    private void UpdateView()
    {
        if (_viewModel.IsLoading)
        {
            ShowLoading();
            return;
        }

        if (!_viewModel.HasApplicant)
        {
            ShowEmptyState();
            return;
        }

        if (_viewModel.IsExpanded)
        {
            ShowExpandedView();
        }
        else
        {
            ShowCardView();
        }
    }

    private void ShowCardView()
    {
        CardViewPanel.Visibility = Visibility.Visible;
        ExpandedViewPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Visible;

        UpdateActionButtons(enabled: true);
        BindCardData();
    }

    private void ShowExpandedView()
    {
        CardViewPanel.Visibility = Visibility.Collapsed;
        ExpandedViewPanel.Visibility = Visibility.Visible;
        EmptyStatePanel.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Collapsed;

        BindExpandedData();
    }

    private void ShowEmptyState()
    {
        CardViewPanel.Visibility = Visibility.Collapsed;
        ExpandedViewPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Visible;
        LoadingPanel.Visibility = Visibility.Collapsed;
        ActionButtonsPanel.Visibility = Visibility.Visible;

        UpdateActionButtons(enabled: false);
    }

    private void ShowLoading()
    {
        CardViewPanel.Visibility = Visibility.Collapsed;
        ExpandedViewPanel.Visibility = Visibility.Collapsed;
        EmptyStatePanel.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Visible;
        ActionButtonsPanel.Visibility = Visibility.Collapsed;
    }

    private void UpdateActionButtons(bool enabled)
    {
        SkipButton.IsEnabled = enabled;
        AdvanceButton.IsEnabled = enabled;
        UndoButton.IsEnabled = _viewModel.CanUndo;
    }

    private void BindCardData()
    {
        var applicant = _viewModel.CurrentApplicant;
        if (applicant is null)
        {
            return;
        }

        AvatarInitial.Text = applicant.User.Name.Length > 0
            ? applicant.User.Name[..1].ToUpperInvariant()
            : "?";
        ApplicantNameText.Text = applicant.User.Name;
        JobTitleText.Text = string.IsNullOrWhiteSpace(applicant.Job.JobTitle)
            ? applicant.Job.JobDescription
            : applicant.Job.JobTitle;
        MatchScoreText.Text = $"{applicant.CompatibilityScore:F0}%";
        LocationText.Text = applicant.User.Location;
        ExperienceText.Text = $"{applicant.User.YearsOfExperience} yrs";
        EducationText.Text = applicant.User.Education;

        TopSkillsList.ItemsSource = _viewModel.TopSkills;

        if (_viewModel.RemainingSkillCount > 0)
        {
            MoreSkillsText.Text = $"+{_viewModel.RemainingSkillCount} more skills";
            MoreSkillsText.Visibility = Visibility.Visible;
        }
        else
        {
            MoreSkillsText.Visibility = Visibility.Collapsed;
        }

        UndoButton.IsEnabled = _viewModel.CanUndo;
    }

    private void BindExpandedData()
    {
        var applicant = _viewModel.CurrentApplicant;
        if (applicant is null)
        {
            return;
        }

        ExpandedNameText.Text = applicant.User.Name;
        ExpandedJobText.Text = $"Applied for: {(string.IsNullOrWhiteSpace(applicant.Job.JobTitle) ? applicant.Job.JobDescription : applicant.Job.JobTitle)}";
        ExpandedMatchScoreText.Text = $"{applicant.CompatibilityScore:F0}% Match";
        ExpandedLocationText.Text = applicant.User.Location;
        ExpandedExperienceText.Text = $"{applicant.User.YearsOfExperience} years";
        ExpandedEducationText.Text = applicant.User.Education;
        ResumeText.Text = string.IsNullOrWhiteSpace(applicant.User.Resume)
            ? "No resume provided."
            : applicant.User.Resume;
        JobDescriptionText.Text = applicant.Job.JobDescription;

        AllSkillsList.ItemsSource = _viewModel.AllSkills;

        var breakdown = _viewModel.ScoreBreakdown;
        if (breakdown is not null)
        {
            BreakdownSkillText.Text = $"{breakdown.SkillScore:F1}";
            BreakdownKeywordText.Text = $"{breakdown.KeywordScore:F1}";
            BreakdownPreferenceText.Text = $"{breakdown.PreferenceScore:F1}";
            BreakdownPromotionText.Text = $"{breakdown.PromotionScore:F1}";
        }

        ContactEmailText.Text = _viewModel.MaskedEmail;
        ContactPhoneText.Text = _viewModel.MaskedPhone;
    }

    private async void OnViewModelErrorOccurred(string message)
    {
        var dialog = new ContentDialog
        {
            Title = "Error",
            Content = message,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }
}
