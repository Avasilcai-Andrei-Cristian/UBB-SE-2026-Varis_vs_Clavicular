using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using matchmaking.Domain.Enums;
using matchmaking.Repositories;
using matchmaking.Services;
using matchmaking.ViewModels;

namespace matchmaking.Views.Pages;

public sealed partial class CompanyStatusPage : Page
{
    private readonly CompanyStatusViewModel _viewModel;
    private readonly SolidColorBrush _defaultBorderBrush = new(Microsoft.UI.Colors.LightGray);
    private readonly SolidColorBrush _invalidBorderBrush = new(Microsoft.UI.Colors.IndianRed);
    private int? _initialMatchId;

    public CompanyStatusPage()
    {
        InitializeComponent();

        var session = App.Session;

        var jobService = new JobService(new JobRepository());
        var matchService = new MatchService(
            new SqlMatchRepository(App.Configuration.SqlConnectionString),
            jobService);

        _viewModel = new CompanyStatusViewModel(
            new CompanyStatusService(
                matchService,
                new UserService(new UserRepository()),
                jobService,
                new SkillService(new SkillRepository())),
            matchService,
            new TestingModuleAdapterStub(),
            session);

        _viewModel.ErrorOccurred += OnViewModelErrorOccurred;

        DataContext = _viewModel;
        Loaded += OnLoadedAsync;
    }

    private async void OnLoadedAsync(object sender, RoutedEventArgs e)
    {
        if (App.Session.CurrentMode != AppMode.CompanyMode || App.Session.CurrentCompanyId is null)
        {
            return;
        }

        await _viewModel.LoadApplicationsAsync();

        if (_initialMatchId is int matchId)
        {
            _initialMatchId = null;
            var loaded = await _viewModel.LoadEvaluationAsync(matchId);
            if (loaded)
            {
                ShowEvaluation();
                ResetValidationVisuals();
                return;
            }
        }

        ShowApplicantList();
        ResetValidationVisuals();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);

        if (e.Parameter is int matchId && matchId > 0)
        {
            _initialMatchId = matchId;
        }
    }

    private async void OnReviewApplicantClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button reviewButton)
        {
            return;
        }

        if (!int.TryParse(reviewButton.Tag?.ToString(), out var matchId))
        {
            return;
        }

        var loaded = await _viewModel.LoadEvaluationAsync(matchId);
        if (!loaded)
        {
            ShowApplicantList();
            return;
        }

        ShowEvaluation();
        ResetValidationVisuals();
    }

    private async void OnSubmitDecisionClickAsync(object sender, RoutedEventArgs e)
    {
        ResetValidationVisuals();

        var saved = await _viewModel.SubmitDecisionAsync();
        if (_viewModel.HasValidationErrors)
        {
            ApplyValidationVisuals();
            await ShowDialogAsync("Validation", "At least one field is invalid.");
            return;
        }

        if (!saved)
        {
            await ShowDialogAsync("Operation Failed", "We're sorry, an error occurred. The decision was not saved. Please try again.");
            return;
        }

        await ShowDialogAsync("Success", "Decision submitted successfully");
        ShowApplicantList();
    }

    private async void OnCancelEvaluationClickAsync(object sender, RoutedEventArgs e)
    {
        var confirmed = await ShowConfirmationAsync(
            "Cancel Evaluation",
            "Are you sure you want to cancel the evaluation?");

        if (!confirmed)
        {
            return;
        }

        _viewModel.CancelEvaluation();
        ShowApplicantList();
    }

    private void ShowApplicantList()
    {
        ApplicantListPanel.Visibility = Visibility.Visible;
        EvaluationPanel.Visibility = Visibility.Collapsed;
    }

    private void ShowEvaluation()
    {
        ApplicantListPanel.Visibility = Visibility.Collapsed;
        EvaluationPanel.Visibility = Visibility.Visible;
    }

    private void ResetValidationVisuals()
    {
        DecisionFieldBorder.BorderBrush = _defaultBorderBrush;
        FeedbackFieldBorder.BorderBrush = _defaultBorderBrush;
    }

    private void ApplyValidationVisuals()
    {
        DecisionFieldBorder.BorderBrush = string.IsNullOrWhiteSpace(_viewModel.ValidationErrorDecision)
            ? _defaultBorderBrush
            : _invalidBorderBrush;

        FeedbackFieldBorder.BorderBrush = string.IsNullOrWhiteSpace(_viewModel.ValidationErrorFeedback)
            ? _defaultBorderBrush
            : _invalidBorderBrush;
    }

    private async void OnViewModelErrorOccurred(string message)
    {
        await ShowDialogAsync("Operation Failed", message);
    }

    private async System.Threading.Tasks.Task ShowDialogAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            CloseButtonText = "OK",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    private async System.Threading.Tasks.Task<bool> ShowConfirmationAsync(string title, string content)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = content,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = XamlRoot
        };

        var result = await dialog.ShowAsync();
        return result == ContentDialogResult.Primary;
    }
}
