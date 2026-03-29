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
        Navigate(typeof(SampleFormPage));
    }

    private void NavigateToMyStatus()
    {
        Navigate(typeof(CompanyStatusPage));
    }

    private void NavigateToChat()
    {
        Navigate(typeof(SampleFormPage));
    }

    private void Navigate(Type pageType)
    {
        if (ContentHostFrame.CurrentSourcePageType == pageType)
        {
            return;
        }

        ContentHostFrame.Navigate(pageType);
    }
}
