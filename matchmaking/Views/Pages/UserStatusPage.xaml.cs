using System;
using Microsoft.UI;
using Microsoft.UI.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI;
using matchmaking.Models;
using matchmaking.ViewModels;
using System.Runtime.InteropServices.WindowsRuntime;
using matchmaking.Views;
namespace matchmaking.Views.Pages;

public sealed partial class UserStatusPage : Page
{
    private readonly UserStatusViewModel _userStatusViewModel;

    public UserStatusPage()
    {
        InitializeComponent();

        _userStatusViewModel = new UserStatusViewModel();
        DataContext = _userStatusViewModel;

        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetActiveFilter(FilterAll);
        await _userStatusViewModel.LoadMatches();
    }

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button filterButton)
        {
            return;
        }

        SetActiveFilter(filterButton);
        _userStatusViewModel.ApplyFilter(filterButton.Tag?.ToString() ?? "All");
    }

    private void SetActiveFilter(Button activeBtn)
    {
        foreach (var filterButton in new[] { FilterAll, FilterApplied, FilterAccepted, FilterRejected })
        {
            if (filterButton == activeBtn)
            {
                filterButton.Background = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
                filterButton.Foreground = new SolidColorBrush(Colors.White);
                filterButton.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
            }
            else
            {
                filterButton.Background = new SolidColorBrush(Colors.White);
                filterButton.Foreground = new SolidColorBrush(Colors.Black);
                filterButton.BorderBrush = new SolidColorBrush(Colors.Black);
            }
        }
    }

    private async void ViewJobDetails_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ApplicationCardModel model })
        {
            var payload = new UserStatusJobDetailPayload
            {
                Card = model,
                JobSkills = _userStatusViewModel.GetJobSkills(model.JobId)
            };

            Frame.Navigate(typeof(UserStatusJobDetailPage), payload);
        }
    }

    private void ViewSkillGap_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SkillGapPage));

  

    private void SkillInsightsButton_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SkillGapPage));

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
        => _userStatusViewModel.Refresh();

    private void GoToRecommendationsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(UserRecommendationPageView));
    }
}
