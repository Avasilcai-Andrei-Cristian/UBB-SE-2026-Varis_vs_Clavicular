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
    private readonly UserStatusViewModel _vm;

    public UserStatusPage()
    {
        InitializeComponent();

        _vm         = new UserStatusViewModel();
        DataContext = _vm;

        Loaded += (_, _) =>
        {
            SetActiveFilter(FilterAll);
            _ = _vm.LoadMatches();
        };
    }

  

    private void Filter_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        SetActiveFilter(btn);
        _vm.ApplyFilter(btn.Tag?.ToString() ?? "All");
    }

    private void SetActiveFilter(Button activeBtn)
    {
        foreach (var btn in new[] { FilterAll, FilterApplied, FilterAccepted, FilterRejected })
        {
            if (btn == activeBtn)
            {
                btn.Background  = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
                btn.Foreground  = new SolidColorBrush(Colors.White);
                btn.BorderBrush = new SolidColorBrush(Color.FromArgb(255, 30, 30, 30));
            }
            else
            {
                btn.Background  = new SolidColorBrush(Colors.White);
                btn.Foreground  = new SolidColorBrush(Colors.Black);
                btn.BorderBrush = new SolidColorBrush(Colors.Black);
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
                JobSkills = _vm.GetJobSkills(model.JobId)
            };

            Frame.Navigate(typeof(UserStatusJobDetailPage), payload);
        }
    }

    private void ViewSkillGap_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SkillGapPage));

  

    private void SkillInsightsButton_Click(object sender, RoutedEventArgs e)
        => Frame.Navigate(typeof(SkillGapPage));

    private void RefreshButton_Click(object sender, RoutedEventArgs e)
        => _vm.Refresh();

    private void GoToRecommendationsButton_Click(object sender, RoutedEventArgs e)
    {
        Frame?.Navigate(typeof(UserRecommendationPageView));
    }

   

}
