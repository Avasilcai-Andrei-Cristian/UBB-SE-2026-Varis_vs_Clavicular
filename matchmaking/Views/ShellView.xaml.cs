using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.ViewModels;
using matchmaking.Views.Pages;

namespace matchmaking.Views;

public sealed partial class ShellView : UserControl
{
    private readonly ShellViewModel _viewModel;

    public ShellView()
    {
        InitializeComponent();
        _viewModel = new ShellViewModel(
            onRecommendations: NavigateToRecommendations,
            onMyStatus: NavigateToMyStatus,
            onChat: NavigateToChat);
        DataContext = _viewModel;

        HeaderControl.RecommendationsRequested += OnRecommendationsRequested;
        HeaderControl.MyStatusRequested += OnMyStatusRequested;
        HeaderControl.ChatRequested += OnChatRequested;

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (ContentHostFrame.Content is null)
        {
            NavigateToRecommendations();
        }
    }

    private void NavigateToRecommendations()
    {
        Navigate(typeof(UserRecommendationPageView));
    }

    private void NavigateToMyStatus()
    {
        Navigate(typeof(CompanyStatusPage));
    }

    private void NavigateToChat()
    {
        Navigate(typeof(ChatPageView));
    }

    private void Navigate(Type pageType)
    {
        if (ContentHostFrame.CurrentSourcePageType == pageType)
        {
            return;
        }

        ContentHostFrame.Navigate(pageType);
    }

    private void OnRecommendationsRequested(object? sender, EventArgs e)
    {
        NavigateToRecommendations();
    }

    private void OnMyStatusRequested(object? sender, EventArgs e)
    {
        NavigateToMyStatus();
    }

    private void OnChatRequested(object? sender, EventArgs e)
    {
        NavigateToChat();
    }
}
