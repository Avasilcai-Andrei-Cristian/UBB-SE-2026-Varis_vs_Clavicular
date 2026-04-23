using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using matchmaking.Domain.Enums;
using matchmaking.ViewModels;
using matchmaking.Views.Controls;
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
            onMyStatus:        NavigateToMyStatus,
            onChat:            NavigateToChat);
        DataContext = _viewModel;

        InitializeHeader();

        if (App.Session.CurrentMode == AppMode.DeveloperMode)
        {
            NavigateToDeveloperPage();
        }
    }

    private void InitializeHeader()
    {
        if (App.Session.CurrentMode == AppMode.DeveloperMode)
        {
            HeaderSlot.Content = new DeveloperHeaderControl();
            return;
        }

        var appHeader = new AppHeaderControl();
        appHeader.RecommendationsRequested += OnRecommendationsRequested;
        appHeader.MyStatusRequested        += OnMyStatusRequested;
        appHeader.ChatRequested            += OnChatRequested;
        HeaderSlot.Content = appHeader;
    }

    private void NavigateToRecommendations()
    {
        _viewModel.ActivePage = "Recommendations";

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
        _viewModel.ActivePage = "MyStatus";

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

    private void NavigateToDeveloperPage()
    {
        if (App.Session.CurrentMode == AppMode.DeveloperMode && App.Session.CurrentDeveloperId is not null)
        {
            NavigateIfPageExists("matchmaking.Views.Pages.DeveloperPage");
            return;
        }
    }

    private void NavigateToChat()
    {
        if (App.Session.CurrentMode == AppMode.DeveloperMode)
        {
            return;
        }

        if (App.Session.CurrentMode == AppMode.UserMode && App.Session.CurrentUserId is null)
        {
            return;
        }

        if (App.Session.CurrentMode == AppMode.CompanyMode && App.Session.CurrentCompanyId is null)
        {
            return;
        }

        _viewModel.ActivePage = "Chat";
        Navigate(typeof(ChatPageView));
    }

    private void Navigate(Type pageType)
    {
        if (ContentHostFrame.CurrentSourcePageType == pageType)
            return;

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
        => NavigateToRecommendations();

    private void OnMyStatusRequested(object? sender, EventArgs e)
        => NavigateToMyStatus();

    private void OnChatRequested(object? sender, EventArgs e)
        => NavigateToChat();
}
