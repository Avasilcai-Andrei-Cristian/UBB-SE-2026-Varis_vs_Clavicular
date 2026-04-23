using System;
using System.ComponentModel;
using matchmaking.algorithm;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace matchmaking.Views.Pages;

public sealed partial class UserRecommendationPageView : Page
{
    private readonly UserRecommendationViewModel _viewModel;

    public UserRecommendationPageView()
    {
        InitializeComponent();

        var connectionString = App.Configuration.SqlConnectionString;
        var jobRepository = new JobRepository();
        var jobService = new JobService(jobRepository);
        var matchService = new MatchService(
            new SqlMatchRepository(connectionString),
            jobService);
        var recommendationRepository = new SqlRecommendationRepository(connectionString);
        var cooldownHours = App.Configuration.RecommendationCooldownHours;
        var cooldownPeriod = TimeSpan.FromHours(cooldownHours > 0 ? cooldownHours : 24);
        var cooldownService = new CooldownService(recommendationRepository, cooldownPeriod);
        var algorithm = new RecommendationAlgorithm(
            new SqlPostRepository(connectionString),
            new SqlInteractionRepository(connectionString));

        var recommendationService = new UserRecommendationService(
            new UserRepository(),
            jobRepository,
            new SkillRepository(),
            new JobSkillRepository(),
            new CompanyRepository(),
            matchService,
            recommendationRepository,
            cooldownService,
            algorithm);

        _viewModel = new UserRecommendationViewModel(recommendationService, App.Session);
        _viewModel.ErrorOccurred += OnViewModelErrorOccurred;
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;

        DataContext = _viewModel;
        Loaded += OnLoadedAsync;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is null
            || e.PropertyName == nameof(UserRecommendationViewModel.IsLoading)
            || e.PropertyName == nameof(UserRecommendationViewModel.CurrentJob)
            || e.PropertyName == nameof(UserRecommendationViewModel.IsDetailOpen)
            || e.PropertyName == nameof(UserRecommendationViewModel.CanUndo)
            || e.PropertyName == nameof(UserRecommendationViewModel.HasCard)
            || e.PropertyName == nameof(UserRecommendationViewModel.ShowEmptyDeck))
        {
            UpdateView();
        }
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        if (App.Session.CurrentMode == AppMode.UserMode && App.Session.CurrentUserId is not null)
        {
            await _viewModel.InitializeAsync();
            UpdateView();
        }
    }

    private async void OnApplyFiltersClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.ApplyFiltersAsync();
        UpdateView();
    }

    private void OnResetDraftFiltersClick(object sender, RoutedEventArgs e)
    {
        _viewModel.ResetDraftFilters();
    }

    private void OnOpenFiltersClick(object sender, RoutedEventArgs e)
    {
        _viewModel.IsFilterOpen = true;
    }

    private void OnRefreshClick(object sender, RoutedEventArgs e)
    {
        _viewModel.LoadRecommendations();
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

    private async void OnLikeClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.LikeAsync();
        UpdateView();
    }

    private async void OnDismissClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.DismissAsync();
        UpdateView();
    }

    private async void OnUndoClick(object sender, RoutedEventArgs e)
    {
        await _viewModel.UndoAsync();
        UpdateView();
    }

    private void UpdateView()
    {
        if (_viewModel.IsLoading)
        {
            ShowLoading();
            return;
        }

        if (!_viewModel.HasCard)
        {
            ShowEmptyState();
            return;
        }

        if (_viewModel.IsDetailOpen)
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
        DismissButton.IsEnabled = enabled;
        LikeButton.IsEnabled = enabled;
        UndoButton.IsEnabled = _viewModel.CanUndo;
    }

    private void BindCardData()
    {
        var job = _viewModel.CurrentJob;
        if (job is null)
        {
            return;
        }

        var name = job.Company.CompanyName;
        CardCompanyInitial.Text = string.IsNullOrWhiteSpace(job.Company.LogoText)
            ? (name.Length > 0 ? name[..1].ToUpperInvariant() : "?")
            : job.Company.LogoText;
        CardCompanyNameText.Text = name;
        CardJobTitleText.Text = job.JobTitleLine;
        CardMatchScoreText.Text = $"{job.CompatibilityScore:F0}%";
        CardLocationEmploymentText.Text = job.LocationEmploymentLine;
        CardTopSkillsList.ItemsSource = job.TopSkillLabels;
        CardDescriptionExcerptText.Text = job.DescriptionExcerpt;

        UndoButton.IsEnabled = _viewModel.CanUndo;
    }

    private void BindExpandedData()
    {
        var job = _viewModel.CurrentJob;
        if (job is null)
        {
            return;
        }

        ExpandedMatchScoreText.Text = $"{job.CompatibilityScore:F0}% Match";
        ExpandedCompanyText.Text = job.Company.CompanyName;
        ExpandedJobTitleText.Text = job.JobTitleLine;
        ExpandedLocationText.Text = job.LocationEmploymentLine;
        ExpandedJobDescriptionText.Text = job.Job.JobDescription;
        ExpandedAllSkillsList.ItemsSource = job.AllSkillLabels;
        ExpandedContactText.Text = job.ContactLine;
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
