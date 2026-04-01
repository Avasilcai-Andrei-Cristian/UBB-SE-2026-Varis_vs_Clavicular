using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.Domain.Enums;
using matchmaking.ViewModels;
using matchmaking.Views.Pages;
using System;
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

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        var input = new NumberBox
        {
            NavigateToRecommendations();
        }
    }

    

    private void NavigateToRecommendations()
    {
        if (App.Session.CurrentMode == AppMode.CompanyMode && App.Session.CurrentCompanyId is not null)
        {
            NavigateIfPageExists("matchmaking.Views.Pages.CompanyMatchmakingPage");
            return;
        }

        if (App.Session.CurrentMode == AppMode.UserMode && App.Session.CurrentUserId is not null)
        {
            if (NavigateIfPageExists("matchmaking.Views.Pages.UserMatchmakingPageView"))
            {
                return;
            }

            if (NavigateIfPageExists("matchmaking.Views.Pages.UserRecommendationPageView"))
            {
                return;
            }
        }
    }

    private void NavigateToMyStatus()
    {
        if (App.Session.CurrentMode == AppMode.CompanyMode && App.Session.CurrentCompanyId is not null)
        {
            NavigateIfPageExists("matchmaking.Views.Pages.CompanyStatusPage");
            return;
        }

        if (App.Session.CurrentMode == AppMode.UserMode && App.Session.CurrentUserId is not null)
        {
            NavigateIfPageExists("matchmaking.Views.Pages.UserStatusPage");
            return;
        }
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

    private bool NavigateIfPageExists(string pageTypeName)
    {
        var pageType = typeof(ShellView).Assembly.GetType(pageTypeName);
        if (pageType is null)
        {
            return false;
        }

        Navigate(pageType);
        return true;
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
